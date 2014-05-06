using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Sqor.Utils.Streams;
using System.Net.Http;
using System.Threading;


namespace Sqor.Utils.Net
{
	public class HttpClientAdapter : IHttpAdapter
	{
		private int readWriteTimeout = 60 * 1000;                   // 1 minute
		private int timeout = 60 * 1000;                            // 1 minute


		public HttpClientAdapter()
		{

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
//            if (cancelToken.IsCancellationRequested)
//                return;
            	
            var httpClient = HttpClientFactory.Get();
			
			httpClient.Timeout = TimeSpan.FromSeconds(timeout);

			WebException error = null;
			try
			{

				var httpClientRequest = new HttpRequestMessage(new HttpMethod(request.HttpMethod), request.Url);

                if (request.Input != null)
                {        
                    httpClientRequest.Content = new StreamContent(request.Input);
                }

                // Process headers
                foreach (var header in request.Headers)
                {
                    httpClientRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);

                    if (httpClientRequest.Content != null)
                        httpClientRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                var responseMessage = await httpClient.SendAsync(httpClientRequest).ConfigureAwait(true);
				var responseContent = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(true);

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
