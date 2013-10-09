using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.IO;
using Sqor.Utils.Dictionaries;
using Sqor.Utils.Logging;
using Sqor.Utils.Json;
using System.Threading.Tasks;
using Sqor.Utils.Streams;

namespace Sqor.Utils.Net
{
    public class Http
    {
        private string url;
        private Action<Http> onUnauthorized;
        private Dictionary<string, object> queryString = new Dictionary<string, object>();
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        private Dictionary<string, string> cookies = new Dictionary<string, string>();
        private int readWriteTimeout = 60 * 1000;                   // 1 minute
        private int timeout = 60 * 1000;                            // 1 minute
        private DecompressionMethods automaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        private string userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_4) AppleWebKit/537.4 (KHTML, like Gecko) Chrome/22.0.1229.94 Safari/537.4";
        private string acceptHeader = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        private static bool isCurlLoggingEnabled = true;
        private Action<Http> onError;
        private bool synchronous;
    
        private Http(string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");
        
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
        
        public static Http To(string url)
        {
            return new Http(url);
        }
        
        public Http AsSynchronous()
        {
            synchronous = true;
            return this;
        }
            
        public Http OnError(Action<Http> onError)
        {
            this.onError = onError;
            return this;
        }
        
        public Http OnUnauthorized(Action<Http> onUnauthorized)
        {
            this.onUnauthorized = onUnauthorized;
            return this;
        }
        
        public Http WithHeader(string key, string value)
        {
            headers[key] = value;
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
        
        public Http WithReadWriteTimeout(int readWriteTimeout)
        {
            this.readWriteTimeout = readWriteTimeout;
            return this;
        }
        
        public Http WithTimeout(int timeout)
        {
            this.timeout = timeout;
            return this;
        }
        
        public Http WithAutomaticDecompression(DecompressionMethods automaticDecompression)
        {
            this.automaticDecompression = automaticDecompression;
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
        
        public Http WithLogin(string userName, string password)
        {
            var usernameAndPassword = userName + ":" + password;
            var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(usernameAndPassword));
            var authorization = "Basic " + base64;
            headers["Authorization"] = authorization;
            
            return this;
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
                        builder.Append(System.Web.HttpUtility.UrlEncode(item.Key.ToString()) + "=" + System.Web.HttpUtility.UrlEncode(item.Value.ToString()));
                        delimiter = "&";
                    }
                }
            }
            return builder.ToString();                
        }
        
        public class RequestContext 
        {
            public string Method { get; set; }
            public string ContentType { get; set; }
        
            private TimeoutWebClient client;
            private Http http;
            private bool ignoreErrors;
            private bool isErrored;
            private bool isExecuted;
            private string responseContentType;
            
            internal byte[] response;
            internal int statusCode;
            internal List<Tuple<Func<HttpStatusCode, bool>, Action>> statusCodeResponses = new List<Tuple<Func<HttpStatusCode, bool>, Action>>();
            
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
                    
                    foreach (var header in http.headers)
                    {
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
                    
                    builder.Append(http.Url.Replace("&", "\\&"));
                    this.LogInfo(builder.ToString());
                }
                
//                try
//                {
//                    try
//                    {

                client = new TimeoutWebClient();
                client.ReadWriteTimeout = http.readWriteTimeout;
                client.Timeout = http.timeout;
                client.AutomaticDecompression = http.automaticDecompression;
                client.Headers.Add("User-Agent", http.userAgent);
                client.Headers.Add("Accept", http.acceptHeader);
                client.Headers.Add("Content-Type", ContentType);
                
                // Process cookies            
                foreach (var cookie in http.cookies)
                {
                    client.Cookies.Add(new Cookie(cookie.Key, cookie.Value));
                }
                
                // Process headers
                foreach (var header in http.headers)
                {
                    client.Headers.Add(header.Key, header.Value);
                }
                
                // Upload data
                if (binaryRequestData == null && stringRequestData != null)
                {
                    binaryRequestData = Encoding.UTF8.GetBytes(stringRequestData);
                }
                
                try
                {
                    if (http.synchronous)
                    {
                        if (binaryRequestData == null)
                            response = client.DownloadData(new Uri(http.Url));
                        else if (binaryRequestData != null)
                            response = client.UploadData(new Uri(http.Url), Method, binaryRequestData);
                    }
                    else
                    {
                        if (binaryRequestData == null)
                            response = await client.DownloadDataTaskAsync(new Uri(http.Url)).ConfigureAwait(true);
                        else if (binaryRequestData != null)
                            response = await client.UploadDataTaskAsync(new Uri(http.Url), Method, binaryRequestData).ConfigureAwait(true);                        
                    }
                        
                    responseContentType = client.ResponseHeaders["Content-Type"];
                    foreach (var statusCodeResponse in statusCodeResponses)
                    {
                        if (statusCodeResponse.Item1(HttpStatusCode.OK))
                            statusCodeResponse.Item2();
                    }
                }
                catch (WebException e)
                {
                    if (http.onUnauthorized != null && ((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
                    {
                        http.onUnauthorized(http);
                        http.onUnauthorized = null;  // Clear out so we don't get an infinite loop
// ReSharper disable once CSharpWarnings::CS4014
                        Execute();
                    }
                    if (e.Response != null)
                    {
                        // Extract response
                        using (var stream = e.Response.GetResponseStream())
                        {
                            response = stream.ReadBytesToEnd();
                        }
                        responseContentType = e.Response.ContentType;
                    
                        if (ignoreErrors)
                        {
                            isErrored = true;
                            
                            foreach (var statusCodeResponse in statusCodeResponses)
                            {
                                if (statusCodeResponse.Item1(((HttpWebResponse)e.Response).StatusCode))
                                {
                                    statusCodeResponse.Item2();
                                    return;
                                }
                            }
                            
                            if (http.onError != null)
                                http.onError(http);
                            throw;
                        }
                        else
                        {
                            this.LogInfo("Error making " + Method + " request to: " + http.Url + "\n" + Encoding.UTF8.GetString(response), e);
                            if (http.onError != null)
                                http.onError(http);
                            throw;
                        }
                    }
                    else
                    {
                        if (http.onError != null)
                            http.onError(http);
                        throw;
                    }
                }
                client.Dispose();
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
            
            /// <summary>
            /// Returns the response as a JSON value.  If the response is in an errored state
            /// </summary>
            /// <returns>The json.</returns>
            public async Task<JsonValue> AsJson()
            {
                if (isErrored) 
                    return null;
                    
                await Execute();
                return ResponseAsJson();
            }

            public async Task<JsonObject> AsJsonObject()
            {
                var response = await AsJson();
                return (JsonObject)response;
            }
            
            internal JsonValue ResponseAsJson()
            {
                using (var stream = new MemoryStream(response))
                using (var reader = new StreamReader(stream))
                {
                    var s = reader.ReadToEnd();
                    this.LogInfo("Response: " + s);
                    var result = s.FromJson();
                    return result;
                }
            }
            
            public async Task<T> As<T>()
            {
                if (isErrored) 
                    return default(T);
                    
                await Execute();
                return ResponseAs<T>();
            }
            
            internal T ResponseAs<T>()
            {
                using (var stream = new MemoryStream(response))
                using (var reader = new StreamReader(stream))
                {
                    var s = reader.ReadToEnd();
                    this.LogInfo("Response: " + s);
                    var result = s.FromJson<T>();
                    return result;
                }
            }
            
            public async Task<string> AsString()
            {
                if (isErrored) 
                    return null;
                    
                await Execute();
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
                if (isErrored) 
                    return null;
                    
                await Execute();
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
            
            public RequestContext Json(object o)
            {
                var json = o.ToJson();
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
            
            public RequestContext Binary(byte[] data)
            {
                ContentType = "application/octet-stream";
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
            
            public RequestContext ReturnJson(Action<JsonValue> returnValue) 
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

