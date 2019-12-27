namespace Shared
{
    public struct Currency
    {
        public readonly string currencyName;
        public readonly string currencySymbol;
        public readonly string id;

        public Currency(string currencyName, string currencySymbol, string id)
        {
            this.currencyName = currencyName;
            this.currencySymbol = currencySymbol;
            this.id = id;
        }
    }
}