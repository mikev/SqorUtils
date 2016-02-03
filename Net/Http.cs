using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sqor.Utils.Dictionaries;
using Sqor.Utils.Injection;
using Sqor.Utils.Logging;
using System.Threading.Tasks;
using Sqor.Utils.Streams;

namespace Sqor.Utils.Net
{
    public class Http
    {
        public static IHttpAdapter DefaultAdapter = new HttpClientAdapter();

        private IHttpAdapter adapter;
        private string url;
        private Func<Http, Task> onUnauthorized;
        private Dictionary<string, object> queryString = new Dictionary<string, object>();
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        private Dictionary<string, string> cookies = new Dictionary<string, string>();
        private List<Action<IDictionary<string, string>>> responseHeaders = new List<Action<IDictionary<string, string>>>();
        private string userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_4) AppleWebKit/537.4 (KHTML, like Gecko) Chrome/22.0.1229.94 Safari/537.4";
        private string acceptHeader = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        private static bool isCurlLoggingEnabled = true;
        private Action<Http> onError;
        private bool preserveAsyncContext;

        // CWN
        private int? currentUserId;
        private string AccessToken;  

        private Http(string url, IHttpAdapter adapter, bool messWithQuerystring = true, bool preserveAsyncContext = false)
        {
            this.preserveAsyncContext = preserveAsyncContext;
            if (url == null)
                throw new ArgumentNullException("url");

            if (messWithQuerystring)
            {
                // If the url already contains a query string, we want to decompose it into our query string
                // dictionary so we can seamlessly add on other query string arguments if necessary.
                var queryIndex = url.IndexOf('?');
                if (queryIndex != -1)
                {
                    var baseUrl = url.Substring(0, queryIndex);
                    var queryString = url.Substring(queryIndex + 1);
                    foreach (var pair in queryString.Split('&'))
                    {
                        var parts = pair.Split('=');
                        WithQueryString(parts[0], Uri.UnescapeDataString(parts[1]));
                    }
                    url = baseUrl;
                }
            }

            this.adapter = adapter ?? DefaultAdapter;
            this.url = url;

            headers["User-Id"] = this.currentUserId.Value.ToString();
            headers["Access-Token"] = this.AccessToken;

            //headers["Accept-Language"] = "en-us,en;q=0.5";
            //headers["Accept-Encoding"] = "gzip,deflate";
            //headers["Accept-Charset"] = "utf-8;q=0.7,*;q=0.7";
        }

        public string BaseUrl
        {
            get { return url; }
        }

        public string Url
        {
            get
            {
                var url = new StringBuilder(this.url);

                // Build query string
                if (queryString.Any())
                {
                    url.Append("?");
                    url.Append(ConvertDictionaryToQueryString(queryString));
                }

                return url.ToString();
            }
        }

        public string AcceptHeader
        {
            get { return acceptHeader; }
            set { acceptHeader = value; }
        }

        public Dictionary<string, object> QueryString
        {
            get { return queryString; }
        }

        public static Http To(string url, bool messWithQuerystring = true, IHttpAdapter adapter = null, bool preserveAsyncContext = false)
        {
            return new Http(url, adapter, messWithQuerystring, preserveAsyncContext);
        }

        public Http OnError(Action<Http> onError)
        {
            this.onError = onError;
            return this;
        }

        public Http OnUnauthorized(Func<Http, Task> onUnauthorized)
        {
            this.onUnauthorized = onUnauthorized;
            return this;
        }

        public Http WithHeader(string key, string value)
        {
            if (key.Equals("accept", StringComparison.OrdinalIgnoreCase))
            {
                acceptHeader = value;
            }
            else
            {
                headers[key] = value;
            }
            return this;
        }

        public Http WithCookie(string key, string value)
        {
            cookies[key] = value;
            return this;
        }

        public Http WithQueryString(Dictionary<string, object> dictionary)
        {
            foreach (var item in dictionary)
                queryString[item.Key] = item.Value;
            return this;
        }

        public Http WithQueryString(string key, string value)
        {
            queryString[key] = value;
            return this;
        }

        public Http WithQueryString(object obj)
        {
            var dictionary = obj.ToDictionary();
            foreach (var item in dictionary)
                queryString[item.Key] = item.Value;
            return this;
        }

        public Http WithUserAgent(string userAgent)
        {
            this.userAgent = userAgent;
            return this;
        }

        public Http WithAcceptHeader(string acceptHeader)
        {
            this.acceptHeader = acceptHeader;
            return this;
        }

        public Http OnResponse(Action<IDictionary<string, string>> response)
        {
            responseHeaders.Add(response);
            return this;
        }

        public Http WithLogin(string userName, string password)
        {
            var usernameAndPassword = userName + ":" + password;
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameAndPassword));
            var authorization = "Basic " + base64;
            headers["Authorization"] = authorization;

            return this;
        }

        public RequestContext Get()
        {
            return new RequestContext(this, "GET");
        }

        public RequestContext Head()
        {
            return new RequestContext(this, "HEAD");
        }
        
        public SendRequestContext Post()
        {
            return new SendRequestContext(this, "POST");
        }

        public SendRequestContext Put()
        {
            return new SendRequestContext(this, "PUT");
        }

        public SendRequestContext Patch()
        {
            return new SendRequestContext(this, "PATCH");
        }

        public RequestContext Delete()
        {
            var context = new RequestContext(this, "DELETE");
            return context;
            //return context.Go();
        }

        public static string ConvertDictionaryToQueryString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            var builder = new StringBuilder();
            if (dictionary.Any())
            {
                var delimiter = "";
                foreach (var item in dictionary)
                {
                    if (item.Value != null)
                    {
                        builder.Append(delimiter);
                        // Handle passing arrays through query string e.g. ?list=5&list=6&list=7
                        // not checking for any given IEnumerable because strings implement that, but should be treated the normal way (?str=string)
                        var list = item.Value as IList;
                        var dateTime = item.Value as DateTime?;
                        if (list != null)
                        {
                            delimiter = "";
                            foreach (var value in list)
                            {
                                builder.Append (delimiter);
                                builder.Append (UrlEncoder.UrlEncode (item.Key.ToString ()) + "=" + UrlEncoder.UrlEncode (value.ToString ()));
                                delimiter = "&";
                            }
                        }
                        else
                            if (dateTime != null)
                            {
                                builder.Append(UrlEncoder.UrlEncode(item.Key.ToString()) + "=" + UrlEncoder.UrlEncode(dateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                builder.Append(UrlEncoder.UrlEncode(item.Key.ToString()) + "=" + UrlEncoder.UrlEncode(item.Value.ToString()));
                            }
                        delimiter = "&";
                    }
                }
            }
            return builder.ToString();
        }

        class HttpRequest : IHttpRequest
        {
            public bool PreserveAsyncContext { get; set; }
            public string Url { get; set; }
            public string HttpMethod { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public Stream Input { get; set; }
        }

        public class RequestContext
        {
            public string Method { get; set; }
            public string ContentType { get; set; }

            private Http http;
            private bool ignoreErrors;
            private bool isErrored;
            private bool isExecuted;
            private string responseContentType;

            internal byte[] response;
            //            internal int statusCode;
            internal List<Tuple<Func<HttpStatusCode, bool>, Action>> statusCodeResponses = new List<Tuple<Func<HttpStatusCode, bool>, Action>>();
            internal Action<HttpStatusCode> onStatus;

            protected string stringRequestData;
            protected Stream binaryRequestData;
            
            public RequestContext(Http http, string method)
            {
                this.http = http;
                Method = method;
            }
            
            public RequestContext WithContentType(string contentType)
            {
                ContentType = contentType;
                return this;
            }

            public Http Http
            {
                get { return http; }
            }

            public string ResponseContentType
            {
                get { return responseContentType; }
            }

            public async Task Execute()
            {
                if (isExecuted)
                    return;

                isExecuted = true;
                if (isCurlLoggingEnabled)
                {
                    LogCurl();
                }

                // Upload data
                if (binaryRequestData == null && stringRequestData != null)
                {
                    binaryRequestData = new MemoryStream(Encoding.UTF8.GetBytes(stringRequestData));
                }

                var request = new HttpRequest
                {
                    Headers = http.headers.Merge(new Dictionary<string, string>
                    {
                        { "User-Agent", http.userAgent },
                        { "Accept", http.acceptHeader },
                        { "Access-Token", http.AccessToken },
                        { "User-Id", http.currentUserId.Value.ToString()},
                        { "Content-Type", ContentType ?? "text/plain" }
                    }),
                    PreserveAsyncContext = http.preserveAsyncContext,
                    Url = http.Url,
                    HttpMethod = Method,
                    Input = binaryRequestData
                };
                
                var response = await http.adapter.Open(request).ConfigureAwait(http.preserveAsyncContext);
                this.response = response.Output;
                responseContentType = response.Headers.Get("Content-Type") ?? "text/plain";
                if (response.Status >= 200 && response.Status < 300)
                {
                    if (onStatus != null)
                        onStatus(HttpStatusCode.OK);
                    foreach (var statusCodeResponse in statusCodeResponses)
                    {
                        if (statusCodeResponse.Item1(HttpStatusCode.OK))
                            statusCodeResponse.Item2();
                    }
                    foreach (var responseHeader in http.responseHeaders)
                    {
                        responseHeader(response.Headers);
                    }
                }
                else // an error status code was received
                {
                    if (http.onUnauthorized != null && response.Status == (int)HttpStatusCode.Unauthorized)
                    {
                        var onUnauthorized = http.onUnauthorized;
                        http.onUnauthorized = null;  // Clear out so we don't get an infinite loop
                        isExecuted = false;
                        await onUnauthorized(http).ConfigureAwait(http.preserveAsyncContext);
                        await Execute().ConfigureAwait(http.preserveAsyncContext);
                        return;
                    }
                    if (response.Output != null)
                    {
                        // Extract response
                        var exception = new InvalidOperationException("Error making " + Method + " request to: " + http.Url + "\n" + Encoding.UTF8.GetString(this.response, 0, this.response.Length));
                    
                        if (ignoreErrors)
                        {
                            isErrored = true;

                            var statusCode = (HttpStatusCode)response.Status;
                            if (onStatus != null)
                                onStatus(statusCode);
                            foreach (var statusCodeResponse in statusCodeResponses)
                            {
                                if (statusCodeResponse.Item1(statusCode))
                                {
                                    statusCodeResponse.Item2();
                                    return;
                                }
                            }

                            if (http.onError != null)
                                http.onError(http);
                            throw new Exception(response.StatusMessage, exception);
                        }
                        else
                        {
                            this.LogInfo(exception.Message + ": " + response.StatusMessage);
                            if (http.onError != null)
                                http.onError(http);
                            throw new Exception(response.StatusMessage, exception);
                        }
                    }
                    else
                    {
                        if (http.onError != null)
                            http.onError(http);
                        throw new InvalidOperationException("Error making " + Method + " request to: " + http.Url + ": " + response.StatusMessage);
                    }
                }
            }

            private void LogCurl()
            {
                var builder = new StringBuilder();
                builder.Append("curl -v -X " + Method + " ");

                if (ContentType != null)
                    builder.Append("-H \"Content-Type: " + ContentType + "\" ");

                foreach (var header in http.headers)
                {
                    if (header.Value != null)
                        builder.Append("-H \"" + header.Key + ": " + header.Value + "\" ");
                }

                if (stringRequestData != null)
                {
                    //                        if (stringRequestData.Contains("'") && stringRequestData.Contains("\""))
                    //                            throw new InvalidOperationException("Cannot debug curl output because data contains two kinds of quotes.");
                    var quote = stringRequestData.Contains("'") ? "\"" : "'";
                    builder.Append("--data " + quote);
                    builder.Append(stringRequestData);
                    builder.Append(quote);
                    builder.Append(" ");
                }

                //                    builder.Append(http.Url.Replace("&", "\\&"));
                builder.Append(http.Url);
                this.LogInfo(builder.ToString());
            }

            public WhenStatusContext WhenStatusIs(HttpStatusCode responseCode)
            {
                return WhenStatusIs(x => x == responseCode);
            }

            public WhenStatusContext WhenStatusIs(Func<HttpStatusCode, bool> responseCode)
            {
                ignoreErrors = true;
                return new WhenStatusContext(this, responseCode);
            }

            public WhenStatusContext Else()
            {
                return new WhenStatusContext(this, statuCode => true);
            }

            public async Task Go()
            {
                await Execute().ConfigureAwait(http.preserveAsyncContext);
            }

            public RequestContext OnStatus(Action<HttpStatusCode> onStatus)
            {
                ignoreErrors = true;
                this.onStatus = onStatus;
                return this;
            }

            /// <summary>
            /// Returns the response as a JSON value.  If the response is in an errored state
            /// </summary>
            /// <returns>The json.</returns>
            public async Task<JToken> AsJson()
            {
                await Execute().ConfigureAwait(http.preserveAsyncContext);
                if (isErrored) 
                    return null;

                return ResponseAsJson();
            }

            public async Task<JObject> AsJsonObject()
            {
                var response = await AsJson().ConfigureAwait(http.preserveAsyncContext);
                return (JObject)response;
            }

            internal JToken ResponseAsJson()
            {
                using (var stream = new MemoryStream(response))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var s = reader.ReadToEnd();
                    this.LogInfo("Response: " + s);
                    try
                    {
                        var result = JToken.Parse(s);
                        return result;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error parsing JSON: " + s, e);
                    }
                }
            }

            public async Task<T> As<T>()
            {
                await Execute().ConfigureAwait(http.preserveAsyncContext);
                if (isErrored) 
                    return default(T);

                return ResponseAs<T>();
            }

            internal T ResponseAs<T>()
            {
                using (var stream = new MemoryStream(response))
                using (var reader = new StreamReader(stream))
                {
                    var s = reader.ReadToEnd();
                    this.LogInfo("Response: " + s);
                    var result = JsonConvert.DeserializeObject<T>(s);
                    return result;
                }
            }

            public async Task<IDictionary<string, string>> AsForm()
            {
                await Execute().ConfigureAwait(http.preserveAsyncContext);
                if (isErrored)
                    return new Dictionary<string, string>();

                return ResponseAsForm();
            }

            internal IDictionary<string, string> ResponseAsForm()
            {
                using (var stream = new MemoryStream(response))
                using (var reader = new StreamReader(stream))
                {
                    var s = reader.ReadToEnd();
                    this.LogInfo("Response: " + s);
                    var result = s
                        .Split('&')
                        .Select(x => x.Split('='))
                        .Select(x => new { Key = x[0], Value = x[1] })
                        .ToDictionary(x => x.Key, x => Uri.UnescapeDataString(x.Value));
                    return result;
                }
            }

            public async Task<string> AsString()
            {
                await Execute().ConfigureAwait(http.preserveAsyncContext);
                if (isErrored) 
                    return null;

                return ResponseAsString();
            }

            internal string ResponseAsString()
            {
                using (var stream = new MemoryStream(response))
                using (var reader = new StreamReader(stream))
                {
                    var s = reader.ReadToEnd();
                    this.LogInfo("Response: " + s);
                    return s;
                }
            }

            public async Task<byte[]> AsBinary()
            {
                await Execute().ConfigureAwait(http.preserveAsyncContext);
                if (isErrored) 
                    return null;

                return ResponseAsBinary();
            }

            internal byte[] ResponseAsBinary()
            {
                using (var stream = new MemoryStream(response))
                {
                    var result = stream.ReadBytesToEnd();
                    return result;
                }
            }
        }

        public class SendRequestContext : RequestContext
        {
            public SendRequestContext(Http http, string method)
                : base(http, method)
            {
            }

            public new SendRequestContext OnStatus(Action<HttpStatusCode> onStatus)
            {
                return (SendRequestContext)base.OnStatus(onStatus);
            }

            public RequestContext Json(string json)
            {
                ContentType = "application/json";
                stringRequestData = json;
                return this;
            }

            public RequestContext Json(JToken json)
            {
                ContentType = "application/json";
                stringRequestData = json.ToString();
                return this;
            }

            public RequestContext Json(object o)
            {
                var json = JsonConvert.SerializeObject(o);
                ContentType = "application/json";
                stringRequestData = json;
                return this;
            }

            public RequestContext Form(object o)
            {
                ContentType = "application/x-www-form-urlencoded";

                var dictionary = o.ToDictionary();
                var s = Http.ConvertDictionaryToQueryString(dictionary);
                stringRequestData = s;
                return this;
            }

            public RequestContext Multipart(object o)
            {
                var boundary = "----" + DateTime.Now.Ticks;
                ContentType = "multipart/form-data; boundary=" + boundary;

                var dictionary = o.ToDictionary();
                var multipart = new MultipartFormDataContent(boundary);
                foreach (var item in dictionary)
                {
                    HttpContent content;
                    if (item.Value is byte[] || item.Value is Tuple<string, byte[]>)
                    {
                        var fileData = item.Value is byte[] ? (byte[])item.Value : ((Tuple<string, byte[]>)item.Value).Item2;
                        var fileName = item.Value is byte[] ? item.Key : ((Tuple<string, byte[]>)item.Value).Item1;
                        content = new ByteArrayContent(fileData);
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        multipart.Add(content, item.Key, fileName);
                    }
                    else
                    {
                        content = new StringContent(item.Value.ToString());
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
                        multipart.Add(content, item.Key);
                    }
                }
                var stream = new MemoryStream();
                multipart.CopyToAsync(stream).Wait();

                // Hack around weird behavior where WebApi demands a trailing newline, and MultipartFormDataContent doesn't supply it
                var writer = new StreamWriter(stream);
                writer.WriteLine();
                writer.Flush();
                
                stream.Position = 0;
                binaryRequestData = stream;
                return this;
            }

            public RequestContext Binary(byte[] data, string contentType = "application/octet-stream")
            {
                ContentType = contentType;
                binaryRequestData = new MemoryStream(data);
                return this;
            }
            
            public RequestContext Binary(Stream data, string contentType = "application/octet-stream")
            {
                ContentType = contentType;
                binaryRequestData = data;
                return this;
            }

            public RequestContext String(string s, string contentType = "text/plain")
            {
                ContentType = contentType;
                stringRequestData = s;
                return this;
            }
        }

        public class WhenStatusContext
        {
            private RequestContext requestContext;
            private Func<HttpStatusCode, bool> responseCode;

            public WhenStatusContext(RequestContext requestContext, Func<HttpStatusCode, bool> responseCode)
            {
                this.requestContext = requestContext;
                this.responseCode = responseCode;
            }

            public RequestContext ReturnJson(Action<JToken> returnValue)
            {
                requestContext.statusCodeResponses.Add(Tuple.Create<Func<HttpStatusCode, bool>, Action>(responseCode, () => returnValue(requestContext.ResponseAsJson())));
                return requestContext;
            }

            public RequestContext ReturnString(Action<string> returnValue)
            {
                requestContext.statusCodeResponses.Add(Tuple.Create<Func<HttpStatusCode, bool>, Action>(responseCode, () => returnValue(requestContext.ResponseAsString())));
                return requestContext;
            }

            public RequestContext ReturnBinary(Action<byte[]> returnValue)
            {
                requestContext.statusCodeResponses.Add(Tuple.Create<Func<HttpStatusCode, bool>, Action>(responseCode, () => returnValue(requestContext.ResponseAsBinary())));
                return requestContext;
            }

            public RequestContext Return<T>(Action<T> returnValue)
            {
                requestContext.statusCodeResponses.Add(Tuple.Create<Func<HttpStatusCode, bool>, Action>(responseCode, () => returnValue(requestContext.ResponseAs<T>())));
                return requestContext;
            }

            public RequestContext ReturnConstant<T>(T value, Action<T> returnValue)
            {
                requestContext.statusCodeResponses.Add(Tuple.Create<Func<HttpStatusCode, bool>, Action>(responseCode, () => returnValue(value)));
                return requestContext;
            }

            public RequestContext ReturnTrue(Action<bool> returnValue)
            {
                requestContext.statusCodeResponses.Add(Tuple.Create<Func<HttpStatusCode, bool>, Action>(responseCode, () => returnValue(true)));
                return requestContext;
            }

            public RequestContext ReturnFalse(Action<bool> returnValue)
            {
                requestContext.statusCodeResponses.Add(Tuple.Create<Func<HttpStatusCode, bool>, Action>(responseCode, () => returnValue(false)));
                return requestContext;
            }
        }
    }
}
