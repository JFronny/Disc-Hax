namespace Shared
{
    public struct Currency
    {
        public readonly string CurrencyName;
        public readonly string CurrencySymbol;
        public readonly string Id;

        public Currency(string currencyName, string currencySymbol, string id)
        {
            this.CurrencyName = currencyName;
            this.CurrencySymbol = currencySymbol;
            this.Id = id;
        }
    }
}