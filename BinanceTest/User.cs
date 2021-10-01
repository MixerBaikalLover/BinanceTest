using System.Collections.Generic;

namespace BinanceTest
{
    public class User
    {
        private string _apiKey;
        private byte[] _secretKey;
        private string _userName;
        public int userId { get; set; }
        private static int _idCounter = 0;

        public string apiKey
        {
            get => _apiKey;
            set => _apiKey = value;
        }

        public byte[] secretKey
        {
            get => _secretKey;
            set => _secretKey = value;
        }
        public string userName
        {
            get => _userName;
            set => _userName = value;
        }

        public User(string apiKey, byte[] secretKey, string userName)
        {
            _apiKey = apiKey;
            _secretKey = secretKey;
            _userName = userName;
            _idCounter++;
            userId = _idCounter;
        }

        public User(string apiKey, byte[] secretKey, string userName, int userId)
        {
            _apiKey = apiKey;
            _secretKey = secretKey;
            _userName = userName;
            this.userId = userId;
        }

        public User(){}

    }
}