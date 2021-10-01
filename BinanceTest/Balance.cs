namespace BinanceTest
{
    public class Balance
    {
        private string _asset;
        private string _free;
        private string _locked;
        public string asset
        {
            set => _asset = value;
            get => _asset;
        }

        public string free
        {
            set => _free = value;
            get => _free;
        }

        public string locked
        {
            set => _locked = value;
            get => _locked;
        }

        public Balance()
        {
        }

        public Balance(string asset, string free, string locked)
        {
            _asset = asset;
            _free = free;
            _locked = locked;
        }
    }
}