using System;
using RestSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;


namespace BinanceTest
{
    internal static class Program
    {
        private const string Baseurl = "https://api.binance.com";
        private static async Task Main()
        {

            try
            {
                List<User> readUsers; //считывание юзеров из json-файла
                await using (var fs = new FileStream("userbI.json", FileMode.OpenOrCreate))
                {
                    readUsers = await JsonSerializer.DeserializeAsync<List<User>>(fs);
                }
                while (true)
                {
                    IRestResponse btcCost = Request(new Dictionary<string, dynamic>() {{"symbol", "BTCUSDT"}},
                        "/api/v3/avgPrice", Method.GET); // getting BTCUSDT price
                    var btcost = btcCost.Content.Split('"');
                    Console.WriteLine("BTCUSDT " + btcost[5]);
                    GetLimits();
                    GetBalances(readUsers);
                    Thread.Sleep(6000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static IRestResponse Request(Dictionary<string, dynamic> @params, string api, Method method)
        {
            var request = new RestRequest(method);
            try
            {
                RestClient client = new RestClient(Baseurl + api);
                foreach (KeyValuePair<string, dynamic> param in @params) request.AddParameter(param.Key, param.Value);
                return client.Execute(request);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error " + e);
                return null;
            }
        }

        private static List<Balance>? GetAccount(User user) //getting account info
        {
            var request = new RestRequest(Method.GET);
            try
            {
                RestClient client = new RestClient(Baseurl + "/api/v3/account");
                request.AddHeader("X-MBX-APIKEY", user.apiKey);
                request.AddParameter("timestamp", DateTimeOffset.Now.ToUnixTimeMilliseconds());
                request.AddQueryParameter("signature", CreateSignature(request.Parameters, user.secretKey));
                var account = client.Execute(request).Content;
                string balances = account.Substring(account.IndexOf("\"balances\":[", StringComparison.Ordinal)+11);
                balances = balances.Remove(balances.IndexOf("\"perm", StringComparison.Ordinal)-1, 24);
                var balancesDeserialized = JsonSerializer.Deserialize<List<Balance>>(balances);
                return balancesDeserialized;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error " + e);
                return null;
            }
        }

        private static string CreateSignature(List<Parameter> parameters, byte[] secretKey) //encoding api key
        {
            var signature = "";
            if (parameters.Count > 0)
            {
                foreach (var item in parameters)
                    if (item.Name != "X-MBX-APIKEY")
                        signature += $"{item.Name}={item.Value}&";
                signature = signature.Substring(0, signature.Length - 1);
            }
            var queryStringBytes = Encoding.UTF8.GetBytes(signature);
            var hmac = new HMACSHA256(secretKey);
            var bytes = hmac.ComputeHash(queryStringBytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private static void GetLimits() //получение лимитов
        {
            try
            {
                var limit = Request(new Dictionary<string, dynamic>(), "/api/v3/exchangeInfo",
                        Method.GET); //getting request limits
                var prettyLimits = limit.Content;
                prettyLimits = prettyLimits.Substring(prettyLimits.IndexOf("\"rateLimits\"" )+13);
                prettyLimits = prettyLimits.Remove(prettyLimits.IndexOf("\"exchangeFilters\"")-1);
                var limitsDeserialized = JsonSerializer.Deserialize<List<rateLimits>>(prettyLimits);
                if (limitsDeserialized != null)
                    foreach (var t in limitsDeserialized)
                    {
                        Console.WriteLine("Rate limit type - " + t.rateLimitType + " Interval - " + t.interval +
                                          " Limit - " + t.limit);
                    } else Console.WriteLine("Didn't get any limits 404");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void GetBalances(List<User> users)
        {
            try
            {
                foreach (var t in users.Where(t => t != null))
                {
                    Console.WriteLine(t.userName + " Balances :");
                    var accBalances = GetAccount(t);
                    if (accBalances == null) continue;
                            
                    foreach (var t1 in accBalances)
                    {
                        if ((t1.free == "0.00000000" & t1.locked == "0.00000000") || (t1.free == "0.00" & t1.locked == "0.00"))
                            continue;
                        Console.WriteLine(" "  + t1.asset + " free - " + t1.free + " locked- " + t1.locked);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}