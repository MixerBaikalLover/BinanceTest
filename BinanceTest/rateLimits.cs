namespace BinanceTest
{
    public class rateLimits
    {
        private string _rateLimitType;
        private string _interval;
        private int _intervalNum;
        private int _limit;
        public  string rateLimitType { set => _rateLimitType = value; get => _rateLimitType; }
        public string interval { set => _interval = value; get => _interval; }
        public  int intervalNum { set => _intervalNum = value; get => _intervalNum; }
        public int limit { set => _limit = value; get => _limit; }

        public rateLimits(string rateLimitType, string interval, int intervalNum, int limit)
        {
            _rateLimitType = rateLimitType;
            _interval = interval;
            _intervalNum = intervalNum;
            _limit = limit;
        }

        public rateLimits() {}
        
    }
}