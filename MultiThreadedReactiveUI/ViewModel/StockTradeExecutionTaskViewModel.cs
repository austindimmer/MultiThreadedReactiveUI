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
    public class StockTradeExecutionTaskViewModel : ReactiveObject
    {
        public StockTradeExecutionTaskViewModel()
        {
        }
        [Reactive]
        public string Symbol { get; set; }
        [Reactive]
        public DateTime Timestamp { get; set; }
        [Reactive]
        public int Quantity { get; set; }
        [Reactive]
        public TradeType TradeType { get; set; }
        [Reactive]
        public decimal Price { get; set; }
        public int Progress { get; set; }
        [Reactive]
        public bool IsIndeterminate { get; set; }
    }
}
