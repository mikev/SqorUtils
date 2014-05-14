using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Sqor.Utils.Streams;
using System.Net.Http;

namespace Sqor.Utils.Net
{
	public class HttpClientAdapter : IHttpAdapter
	{
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
            	
            var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });
			
			httpClient.Timeout = TimeSpan.FromSeconds(timeout);

			WebException error;
			try
			{

				var httpClientRequest = new HttpRequestMessage(new HttpMethod(request.HttpMethod), request.Url);

                if (request.Input != null)
                {        
                    httpClientRequest.Content = new StreamContent(request.Input);
                }
                else if (request.HttpMethod != "GET")
                {
                    httpClientRequest.Content = new ByteArrayContent(new byte[0]);
                }

                if (httpClientRequest.Content != null && request.Headers.ContainsKey("Content-Type"))
                {
                    httpClientRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(request.Headers["Content-Type"]);
                }

                // Process headers
                foreach (var header in request.Headers)
                {
                    if (header.Key == "Content-Type")
                        continue;

                    httpClientRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);

                    if (httpClientRequest.Content != null)
                        httpClientRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                var responseMessage = await httpClient.SendAsync(httpClientRequest).ConfigureAwait(false);
				var responseContent = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

				var httpResponse = new HttpResponse
				{
					Headers = responseMessage.Headers.ToDictionary(x => x.Key,
						x => string.Join(", ", Array.ConvertAll(x.Value.ToArray(), s => s.ToString() ) ) ),
					Output = responseContent,
                    Status = (int)responseMessage.StatusCode,
                    StatusMessage = responseMessage.ReasonPhrase
				};

                // Add content headers, headers are stored in two places when using HttpClient
                foreach (var head in responseMessage.Content.Headers)
                {
                    if (!httpResponse.Headers.ContainsKey(head.Key))
                        httpResponse.Headers.Add(head.Key, string.Join(", ", Array.ConvertAll(head.Value.ToArray(), s => s.ToString())));
                }

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
