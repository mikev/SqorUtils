using System;
using Sqor.Utils.Encryption;

namespace Sqor.Utils.StatsInc
{
    public class StatsIncSignature
    {
        private string apiKey;
        private string apiSecret;

        public StatsIncSignature(string apiKey, string apiSecret)
        {
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
        }

        public string Encode()
        {
            var timestamp = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
            return SHA256Hash.Create(apiKey + apiSecret + timestamp);
        }
    }
}