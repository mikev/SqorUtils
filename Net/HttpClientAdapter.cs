using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using ModernHttpClient;
using Sqor.Utils.Streams;
using System.Net.Http;

namespace Sqor.Utils.Net
{
    public class HttpClientAdapter : IHttpAdapter
    {
        static HttpClientAdapter()
        {
            UseAutomaticDecompression = true;
        }

        public HttpClientAdapter()
        {
        }

        private HttpMessageHandler handler = null;
        public HttpClientAdapter(HttpMessageHandler msgHandler)
        {
            handler = msgHandler;
        }

        public static bool UseAutomaticDecompression { get; set; }

        private int timeout = 60 * 1000;                            // 1 minute

        class HttpResponse : IHttpResponse
        {
            public IDictionary<string, string> Headers { get; set; }
            public byte[] Output { get; set; }
            public int Status { get; set; }
            public string StatusMessage { get; set; }
        }

        public async Task<IHttpResponse> Open(IHttpRequest request)
        {
            //            if (cancelToken.IsCancellationRequested)
            //                return;

            if(handler == null) {
                if (UseAutomaticDecompression) 
                {
                    // temporarily using httpClientHandler instead of ModernHttps NativeHttpClient
                    // because there were some bugs in the OKHttp implementation. New version
                    handler = new NativeMessageHandler();
                } 
                else 
                {
                    handler = new HttpClientHandler();
                }
                var decompProperty = handler.GetType ().GetRuntimeProperty ("AutomaticDecompression");
                if (decompProperty != null) {
                    decompProperty.SetValue(handler, 1 | 2);
                }
            }

            var httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(timeout)
            };

            WebException error;
            try
            {

                var httpClientRequest = new HttpRequestMessage(new HttpMethod(request.HttpMethod), request.Url);

                if (request.Input != null)
                {
                    httpClientRequest.Content = new StreamContent(request.Input);
                }
                else if (!new[] { "GET", "HEAD" }.Contains(request.HttpMethod))
                {
                    httpClientRequest.Content = new ByteArrayContent(new byte[0]);
                }

                if (httpClientRequest.Content != null && request.Headers.ContainsKey("Content-Type"))
                {
                    var contentType = request.Headers["Content-Type"];
                    MediaTypeHeaderValue contentTypeHeader;
                    if (contentType.Contains("; "))
                    {
                        var prefix = contentType.Substring(0, contentType.IndexOf("; "));
                        var suffix = contentType.Substring(contentType.IndexOf("; ") + 2);
                        contentTypeHeader = new MediaTypeHeaderValue(prefix);
                        var parameters = suffix.Split('=');
                        contentTypeHeader.Parameters.Add(new NameValueHeaderValue(parameters[0], parameters[1]));
                    }
                    else
                    {
                        contentTypeHeader = new MediaTypeHeaderValue(contentType);
                    }
                    httpClientRequest.Content.Headers.ContentType = contentTypeHeader;
                    request.Headers.Remove("Content-Type");
                }

                if (request.Headers.ContainsKey("Accept-Language"))
                {
                    httpClientRequest.Headers.AcceptLanguage.ParseAdd(request.Headers["Accept-Language"]);
                    request.Headers.Remove("Accept-Language");
                }

                if (request.Headers.ContainsKey("Accept-Encoding"))
                {
                    httpClientRequest.Headers.AcceptEncoding.ParseAdd(request.Headers["Accept-Encoding"]);
                    request.Headers.Remove("Accept-Encoding");
                }

                if (request.Headers.ContainsKey("Accept-Charset"))
                {
                    httpClientRequest.Headers.AcceptCharset.ParseAdd((request.Headers["Accept-Charset"]));
                    request.Headers.Remove("Accept-Charset");
                }

                if (request.Headers.ContainsKey("User-Agent"))
                {
                    httpClientRequest.Headers.UserAgent.ParseAdd(request.Headers["User-Agent"]);
                    request.Headers.Remove("User-Agent");
                }

                if (request.Headers.ContainsKey("Content-Type"))
                {
                    httpClientRequest.Headers.UserAgent.ParseAdd(request.Headers["Content-Type"]);
                    request.Headers.Remove("Content-Type");
                }

                // Process headers
                foreach (var header in request.Headers)
                {
                    httpClientRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                var responseMessage = await httpClient.SendAsync(httpClientRequest).ConfigureAwait(request.PreserveAsyncContext);
                var responseContent = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(request.PreserveAsyncContext);

                var httpResponse = new HttpResponse
                {
                    Headers = responseMessage.Headers.ToDictionary(x => x.Key,
                        x => string.Join(", ", x.Value.Select(s => s.ToString()))),
                    Output = responseContent,
                    Status = (int)responseMessage.StatusCode,
                    StatusMessage = responseMessage.ReasonPhrase
                };

                return httpResponse;

            }
            catch (WebException e)
            {
                if (e.Response == null)
                    throw new InvalidOperationException("Unable to make request to " + request.Url, e);
                // We can't do async stuff in an exception handler, so perform logic below
                error = e;
            }

            return new HttpResponse
            {
                Status = (int)((HttpWebResponse)error.Response).StatusCode,
                StatusMessage = ((HttpWebResponse)error.Response).StatusDescription,
                Output = error.Response.GetResponseStream().ReadBytesToEnd(),
                Headers = error.Response.Headers.Cast<string>().ToDictionary(x => x, x => error.Response.Headers[x])
            };
        }
    }
}
