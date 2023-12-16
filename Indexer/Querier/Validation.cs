using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Indexer.Querier
{
    public class Validation
    {
        public Validation(Storage storage)
        {
            this.storage = storage;
        }

        public static Random RandomInstance = new Random();
        private readonly Storage storage;


        public void PremiumOrUnderLimit()
        {
            if (this.storage.TryGet("PremiumKey", out string premiumKey) && !string.IsNullOrEmpty(premiumKey))
            {
                // %20 percent of the time, go validate
                if (RandomInstance.Next(10) < 2)
                {
                    // Don't wait on it.
                    ValidatePremiumKey(premiumKey);
                }
                return;
            }

            if (!this.storage.Instance.Exists("maxdoc"))
            {
                return;
            }

            var maxdocNow = this.storage.Instance.Get<int>("maxdoc");

            if (maxdocNow < 130)
            {
                return;
            }

            throw new System.Exception("Indexed document count is over 100 for non-premium plan");
        } 

        public async Task ValidatePremiumKey(string key)
        {
            var client = new HttpClient();

            var response = await client.GetAsync($"https://wanshitong.azurewebsites.net/api/wanshitongbackend?api=validateKey&key={key}");

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            this.storage.Instance.Store<string>("PremiumKey", null);
            this.storage.Instance.Persist();

            throw new Exception("Key validation failed");
        }
    }
}