namespace Shared
{
    public struct Currency
    {
        public readonly string CurrencyName;
        public readonly string CurrencySymbol;
        public readonly string Id;

        public Currency(string currencyName, string currencySymbol, string id)
        {
            CurrencyName = currencyName;
            CurrencySymbol = currencySymbol;
            Id = id;
        }
    }
}