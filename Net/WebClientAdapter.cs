using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Sqor.Utils.Dictionaries;
using Sqor.Utils.Streams;
using Sqor.Utils.Strings;
using System.Threading;

namespace Sqor.Utils.Net
{
    public class WebClientAdapter : IHttpAdapter
    {
        private int readWriteTimeout = 60 * 1000;                   // 1 minute
        private int timeout = 60 * 1000;                            // 1 minute
        private DecompressionMethods automaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        private bool isSynchronous;

        public WebClientAdapter(bool isSynchronous = false)
        {
            this.isSynchronous = isSynchronous;
        }

        class HttpResponse : IHttpResponse
        {
            public IDictionary<string, string> Headers { get; set; }
            public byte[] Output { get; set; }
            public int Status { get; set; }
            public string StatusMessage { get; set; }
        }

        public async Task<IHttpResponse> Open(IHttpRequest request)
        {
            using (var client = new TimeoutWebClient())
            {
                client.ReadWriteTimeout = readWriteTimeout;
                client.Timeout = timeout;
                client.AutomaticDecompression = automaticDecompression;

                // Process headers
                foreach (var header in request.Headers)
                {
                    client.Headers.Add(header.Key, header.Value);
                }

                WebException error = null;
                byte[] response;
                try
                {
                    if (isSynchronous)
                    {
                        if (request.Input == null && request.HttpMethod == "GET")
                            response = client.DownloadData(new Uri(request.Url));
                        else
                        {
                            var stream = client.OpenWrite(new Uri(request.Url), request.HttpMethod);
                            request.Input.CopyTo(stream, 10 * 1024);
                            var output = new MemoryStream();
                            stream.CopyTo(output);
                            response = output.ToArray();
                        } 
                    }
                    else
                    {
                        if (request.Input == null && request.HttpMethod == "GET")
                            response = await client.DownloadDataTaskAsync(new Uri(request.Url)).ConfigureAwait(true);
                        else
                        {
                            var stream = await client.OpenWriteTaskAsync(new Uri(request.Url), request.HttpMethod);
                            await request.Input.CopyToAsync(stream);
                            var output = new MemoryStream();
                            await stream.CopyToAsync(output);
                            response = output.ToArray();
                        }
                    }
                    
                    var httpResponse = new HttpResponse
                    {
                        Headers = client.ResponseHeaders.Cast<string>().ToDictionary(x => x, x => client.ResponseHeaders[x]),
                        Output = response,
                        Status = 200,
                        StatusMessage = "OK"
                    };
                    return httpResponse;
                }
                catch (WebException e)
                {
                    if (e.Response == null)
                        throw;

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
}
