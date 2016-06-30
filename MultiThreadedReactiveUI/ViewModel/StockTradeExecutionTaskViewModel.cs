using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiThreadedReactiveUI.Model;

namespace MultiThreadedReactiveUI.ViewModel
{
    public class StockTradeExecutionTaskViewModel : ReactiveObject, IStock
    {
        public StockTradeExecutionTaskViewModel()
        {
        }

        [Reactive]
        public DateTime Timestamp { get; set; }
        [Reactive]
        public int Quantity { get; set; }
        [Reactive]
        public TradeType TradeType { get; set; }
        [Reactive]
        public int Progress { get; set; }
        [Reactive]
        public bool IsIndeterminate { get; set; }
        [Reactive]
        public decimal AveragePrice { get; set; }
        [Reactive]
        public decimal DividendYield { get; set; }
        [Reactive]
        public decimal FixedDividend { get; set; }
        [Reactive]
        public decimal LastDividend { get; set; }
        [Reactive]
        public decimal ParValue { get; set; }
        [Reactive]
        public decimal PERatio { get; set; }
        [Reactive]
        public decimal Price { get; set; }
        [Reactive]
        public string Symbol { get; set; }
        [Reactive]
        public StockType Type { get; set; }
    }
}
