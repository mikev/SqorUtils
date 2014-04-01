using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sqor.Utils.Dictionaries;
using Sqor.Utils.Logging;
using System.Threading.Tasks;
using Sqor.Utils.Streams;

namespace Sqor.Utils.Net
{
    public class Http
    {
        private static readonly IHttpAdapter DefaultAdapter = 
#if MONOTOUCH // Todo, change to #if MONOTOUCH or Make this a settable property and set it somewhere else in the bootstrapping process
            new HttpClientAdapter();
#else
            new WebClientAdapter();
#endif

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
    
        private Http(string url, IHttpAdapter adapter)
        {
            if (url == null)
                throw new ArgumentNullException("url");
            this.adapter = adapter ?? DefaultAdapter;
            this.url = url;

            headers["Accept-Language"] = "en-us,en;q=0.5";
            headers["Accept-Encoding"] = "gzip,deflate";
            headers["Accept-Charset"] = "ISO-8859-1,utf-8;q=0.7,*;q=0.7";
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

        public static Http To(string url, IHttpAdapter adapter = null)
        {
            return new Http(url, adapter);
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
            if (key.Equals("accept", StringComparison.InvariantCultureIgnoreCase))
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
            var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(usernameAndPassword));
            var authorization = "Basic " + base64;
            headers["Authorization"] = authorization;
            
            return this;
        }

        public async Task<bool> Exists(int retryCount = 0)
        {
            var request = WebRequest.Create(Url);
            request.Method = "HEAD";
            try
            {
                await request.GetResponseAsync();
                return true;
            }
            catch (WebException e)
            {
                var response = (HttpWebResponse)e.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return false;
                else
                    throw;
            }
        }
        
        public RequestContext Get()
        {
            return new RequestContext(this, "GET");
        }
        
        public SendRequestContext Post()
        {
            return new SendRequestContext(this, "POST");
        }
        
        public SendRequestContext Put()
        {
            return new SendRequestContext(this, "PUT");
        }
        
        public Task Delete()
        {
            var context = new RequestContext(this, "DELETE");
            return context.Go();
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
                        if (list != null)
                        {
                            delimiter = "";
                            foreach (var value in list)
                            {
                                builder.Append(delimiter);
                                builder.Append(System.Web.HttpUtility.UrlEncode(item.Key.ToString()) + "=" + System.Web.HttpUtility.UrlEncode(value.ToString()));
                                delimiter = "&";
                            }
                        }
                        else
                        {
                            builder.Append(System.Web.HttpUtility.UrlEncode(item.Key.ToString()) + "=" + System.Web.HttpUtility.UrlEncode(item.Value.ToString()));
                        }
                        delimiter = "&";
                    }
                }
            }
            return builder.ToString();                
        }

        class HttpRequest : IHttpRequest
        {
            public string Url { get; set; }
            public string HttpMethod { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public byte[] Input { get; set; }
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
            protected byte[] binaryRequestData;
            
            public RequestContext(Http http, string method)
            {
                this.http = http;
                Method = method;
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
                
//                try
//                {
//                    try
//                    {

                // Upload data
                if (binaryRequestData == null && stringRequestData != null)
                {
                    binaryRequestData = Encoding.UTF8.GetBytes(stringRequestData);
                }
                
                var request = new HttpRequest
                {
                    Headers = http.headers.Merge(new Dictionary<string, string>
                    {
                        { "User-Agent", http.userAgent },
                        { "Accept", http.acceptHeader },
                        { "Content-Type", ContentType ?? "text/plain" }
                    }),
                    Url = http.Url,
                    HttpMethod = Method,
                    Input = binaryRequestData
                };
                
                var response = await http.adapter.Open(request);
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
                else
                {
                    if (http.onUnauthorized != null && response.Status == (int)HttpStatusCode.Unauthorized)
                    {
                        var onUnauthorized = http.onUnauthorized;
                        http.onUnauthorized = null;  // Clear out so we don't get an infinite loop
                        isExecuted = false;
                        await onUnauthorized(http);
                        await Execute();
                        return;
                    }
                    if (response.Output != null)
                    {
                        // Extract response
                        var exception = new InvalidOperationException("Error making " + Method + " request to: " + http.Url + "\n" + Encoding.UTF8.GetString(this.response));
                    
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
                await Execute();
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
                await Execute();
                if (isErrored) 
                    return null;

                return ResponseAsJson();
            }

            public async Task<JObject> AsJsonObject()
            {
                var response = await AsJson();
                return (JObject)response;
            }
            
            internal JToken ResponseAsJson()
            {
                using (var stream = new MemoryStream(response))
                using (var reader = new StreamReader(stream))
                {
                    var s = reader.ReadToEnd();
                    this.LogInfo("Response: " + s);
                    var result = JToken.Parse(s);
                    return result;
                }
            }
            
            public async Task<T> As<T>()
            {
                await Execute();
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
            
            public async Task<string> AsString()
            {
                await Execute();
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
                await Execute();
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
            public SendRequestContext(Http http, string method) : base(http, method)
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
            
            public RequestContext Binary(byte[] data, string contentType = "application/octet-stream")
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

