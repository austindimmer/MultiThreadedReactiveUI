using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadedReactiveUI.Model
{
    public class Stock : IStock
    {
        [Reactive]
        public string Symbol { get; set; }
        [Reactive]
        public StockType Type { get; set; }
        [Reactive]
        public decimal LastDividend { get; set; }
        [Reactive]
        public decimal FixedDividend { get; set; }
        [Reactive]
        public decimal ParValue { get; set; }
        [Reactive]
        public decimal Price { get; set; }
        [Reactive]
        public decimal PERatio { get; set; }
        [Reactive]
        public decimal DividendYield { get; set; }
    }
}
