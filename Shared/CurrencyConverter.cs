#region

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using Shared.Config;

#endregion

namespace Shared
{
    public static class CurrencyConverter
    {
        private static Dictionary<string, Currency> _currencies = new Dictionary<string, Currency>();

        public static Dictionary<string, Currency> Currencies
        {
            get
            {
                if (_currencies.Count == 0)
                {
                    WebClient client = new WebClient();
                    string str = client.DownloadString(
                        $"https://free.currconv.com/api/v7/currencies?apiKey={TokenManager.CurrencyconverterapiToken}");
                    client.Dispose();
                    JToken o = JObject.Parse(str)["results"];
                    _currencies = o.Select(s => s.First()).ToDictionary(s => s.Value<string>("id"),
                        s => new Currency(
                            s.Value<string>("currencyName"),
                            s.Value<string>("currencySymbol"),
                            s.Value<string>("id")));
                }
                return _currencies;
            }
        }

        public static double GetRatio(Currency inCurrency, Currency outCurrency)
        {
            WebClient client = new WebClient();
            string str = client.DownloadString(
                $"https://free.currconv.com/api/v7/convert?q={inCurrency.id}_{outCurrency.id}&apiKey={TokenManager.CurrencyconverterapiToken}");
            client.Dispose();
            JToken o = JObject.Parse(str)["results"][$"{inCurrency.id}_{outCurrency.id}"];
            return o.Value<float>("val");
        }

        public static double Convert(double value, Currency inCurrency, Currency outCurrency) =>
            GetRatio(inCurrency, outCurrency) * value;
    }
}