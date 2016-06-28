using System.Collections.Generic;

namespace MultiThreadedReactiveUI.DataProvider
{
    public interface IStocksDataProvider
    {
        IEnumerable<Model.Stock> LoadStocks();
    }
}