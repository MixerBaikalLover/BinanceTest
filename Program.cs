using System;
using RestSharp;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;


namespace BinanceTest
{
    class Program
    {
        static string baseurl = "https://api.binance.com";
        private static readonly Dictionary<string, string> Keys = new Dictionary<string, string>() //api key and secret key
            {
                {"apiKey", "KK6sfVa9BW6tCUsN9eghRHMK28DOGSB8MyBfmZgDyVD64po0wDSH66qUmPuTvhnq"},
                {"secretKey", "Iyf3cFri9TL5EX66T6cCqJmqi2dejo7Rgi8acVseJTYDG6DDINZPzo2mV1th0kFD"}
            };
        private static void Main()
        {
            try
            {
                while (true)
                {
                    IRestResponse btcCost = Request(new Dictionary<string, dynamic>(){{"symbol", "BTCUSDT"}}, "/api/v3/avgPrice", Method.GET); // getting BTCUSDT price
                    string[] btcost = btcCost.Content.Split('"');
                    Console.WriteLine("BTCUSDT "+ btcost[5]);
                    IRestResponse limit = Request(new Dictionary<string, dynamic>(), "/api/v3/exchangeInfo", Method.GET); //getting request limits
                    btcost = limit.Content.Split('"');
                    String limits = "";
                    bool needed = false;
                    foreach (var s in btcost)
                    {
                        if (s == "rateLimits") needed = true;
                        if (s == "exchangeFilters") break;
                        if (needed) limits += s;
                    }
                    Console.WriteLine("request limits: " + limits);
                    var account = GetAccount();
                    Console.WriteLine("Account" + account.Content);
                    Thread.Sleep(6000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        static IRestResponse Request(Dictionary<string, dynamic> @params, string api, Method method)
        {
            RestRequest request = new RestRequest(method);
            try
            {
                RestClient client = new RestClient(baseurl + api);
                foreach (KeyValuePair<string, string> key in Keys) request.AddHeader(key.Key, key.Value);
                foreach (KeyValuePair<string, dynamic> param in @params) request.AddParameter(param.Key, param.Value);
                return client.Execute(request);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error " + e);
                return null;
            }
        }

        private static IRestResponse GetAccount() //getting account info
        {
            RestRequest request = new RestRequest(Method.GET);
            try
            {
                RestClient client = new RestClient(baseurl + "/api/v3/account");
                request.AddHeader("X-MBX-APIKEY", Keys["apiKey"]);
                request.AddParameter("timestamp", DateTimeOffset.Now.ToUnixTimeMilliseconds());
                request.AddQueryParameter("signature", CreateSignature(request.Parameters,Keys["secretKey"]));
                return client.Execute(request);
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
                foreach (var item in parameters) if (item.Name != "X-MBX-APIKEY") signature += $"{item.Name}={item.Value}&";
                signature = signature.Substring(0, signature.Length - 1);
            }
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] queryStringBytes = Encoding.UTF8.GetBytes(signature);
            HMACSHA256 hmac = new HMACSHA256(keyBytes);
            byte[] bytes = hmac.ComputeHash(queryStringBytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
} 
