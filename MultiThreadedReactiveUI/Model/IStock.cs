namespace MultiThreadedReactiveUI.Model
{
    public interface IStock
    {
        decimal DividendYield { get; set; }
        decimal FixedDividend { get; set; }
        decimal LastDividend { get; set; }
        decimal ParValue { get; set; }
        decimal PERatio { get; set; }
        decimal Price { get; set; }
        string Symbol { get; set; }
        StockType Type { get; set; }
    }
}