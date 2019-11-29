using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Indexer.Querier
{
    public class Validation
    {
        public static Random RandomInstance = new Random();

        public static void PremiumOrUnderLimit()
        {
            var premiumKey = Storage.Instance.Get<string>("PremiumKey");

            if (string.IsNullOrEmpty(premiumKey))
            {
                // %20 percent of the time, go validate
                if (RandomInstance.Next(10) < 2)
                {
                    // Don't wait on it.
                    ValidatePremiumKey(premiumKey);
                }
                return;
            }

            if (!Storage.Instance.Exists("maxdoc"))
            {
                return;
            }

            var maxdocNow = Storage.Instance.Get<int>("maxdoc");

            if (maxdocNow < 130)
            {
                return;
            }

            throw new System.Exception("Indexed document count is over 100 for non-premium plan");
        } 

        public static async Task ValidatePremiumKey(string key)
        {
            var client = new HttpClient();

            var response = await client.GetAsync($"https://wanshitong.azurewebsites.net/api/wanshitongbackend?api=validateKey&key={key}");

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            Storage.Instance.Store<string>("PremiumKey", null);
            Storage.Instance.Persist();

            throw new Exception("Key validation failed");
        }
    }
}