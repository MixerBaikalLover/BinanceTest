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
    class Program
    {
        private const string Baseurl = "https://api.binance.com";
        private static async Task Main()
        {
            // занесение в json файл всех юзеров
            /* User user = new User("KK6sfVa9BW6tCUsN9eghRHMK28DOGSB8MyBfmZgDyVD64po0wDSH66qUmPuTvhnq",
               "Iyf3cFri9TL5EX66T6cCqJmqi2dejo7Rgi8acVseJTYDG6DDINZPzo2mV1th0kFD", "Mixer");
            User secondUser = new User("G5iVYWMjyMjNcQrS5t1OZxOLSlDSX58G2Yyrd9Dr9OSSeMk5leqzOcFvMBv1GdDx",
                "1NsMrpKDjeKRhkc10KZGD3tOkBh4vGJtLzRTKSlRCKLGApdHTGMNobW32YZ44kyn", "Vyacheslav");
            User thirdUser = new User("m7qslS4h9oJMD6Exp0kySTR6cZGMUgvHCmdYvG3rIYlWJu7jyLRTkfkfyCZ6jwX9",
                "puR7xnTCfG4YQI9BJPSgoJE3em2EKALUxKHxiRJVdjYHLGHmr2gUFlZuKDXwjlPO", "Sergey");
               var users = new List<User>
               {
                   user, secondUser, thirdUser
               };
               var jsonString = JsonSerializer.Serialize(users);
               File.WriteAllText("userbI.json", jsonString); */

         List<User> readUsers; //считывание юзеров из json-файла
         await using (var fs = new FileStream("userbI.json", FileMode.OpenOrCreate))
            {
                readUsers = await JsonSerializer.DeserializeAsync<List<User>>(fs);
            }
            try
            {
                while (true)
                {
                    IRestResponse btcCost = Request(new Dictionary<string, dynamic>() {{"symbol", "BTCUSDT"}},
                        "/api/v3/avgPrice", Method.GET); // getting BTCUSDT price
                    var btcost = btcCost.Content.Split('"');
                    Console.WriteLine("BTCUSDT " + btcost[5]);
                    IRestResponse
                        limit = Request(new Dictionary<string, dynamic>(), "/api/v3/exchangeInfo",
                            Method.GET); //getting request limits
                    btcost = limit.Content.Split('"');
                    string limits = "";
                    var needed = false;
                    foreach (var s in btcost)
                    {
                        if (s == "rateLimits") needed = true;
                        if (s == "exchangeFilters") break;
                        if (needed) limits += s;
                    }

                    Console.WriteLine("request limits: " + limits);
                    if (readUsers != null)
                        foreach (var t in readUsers.Where(t => t != null))
                        {
                            Console.WriteLine(t.userName + " " + GetAccount(t));
                        }
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

        private static string GetAccount(User user) //getting account info
        {
            var request = new RestRequest(Method.GET);
            try
            {
                RestClient client = new RestClient(Baseurl + "/api/v3/account");
                request.AddHeader("X-MBX-APIKEY", user.apiKey);
                request.AddParameter("timestamp", DateTimeOffset.Now.ToUnixTimeMilliseconds());
                request.AddQueryParameter("signature", CreateSignature(request.Parameters, user.secretKey));
                var account = client.Execute(request).Content;
                string balances = account.Substring(account.IndexOf("\"balances\":[", StringComparison.Ordinal));
                balances = balances.Remove(balances.IndexOf("\"perm", StringComparison.Ordinal)-1, 24);
                return balances;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error " + e);
                return null;
            }
        }

        private static string CreateSignature(List<Parameter> parameters, string secretKey) //encoding api key
        {
            var signature = "";
            if (parameters.Count > 0)
            {
                foreach (var item in parameters)
                    if (item.Name != "X-MBX-APIKEY")
                        signature += $"{item.Name}={item.Value}&";
                signature = signature.Substring(0, signature.Length - 1);
            }

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var queryStringBytes = Encoding.UTF8.GetBytes(signature);
            var hmac = new HMACSHA256(keyBytes);
            var bytes = hmac.ComputeHash(queryStringBytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}