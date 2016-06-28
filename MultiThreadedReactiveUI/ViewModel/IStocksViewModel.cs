using System.Collections.Generic;
using Autofac;
using MultiThreadedReactiveUI.Model;
using ReactiveUI;

namespace MultiThreadedReactiveUI.ViewModel
{
    public interface IStocksViewModel
    {
        ReactiveCommand<List<StockTradeExecutionTaskViewModel>> AddStockToTradesToExecute { get; }
        ReactiveCommand<AsyncVoid> CancelRunningStocksToExecute { get; }
        CancelRunViewModel CancelRunViewModelCancel { get; set; }
        CancelRunViewModel CancelRunViewModelRun { get; set; }
        IContainer Container { get; set; }
        CancelRunViewModel CurrentCancelRunViewModel { get; set; }
        StockTradeExecutionTaskViewModel CurrentStockTradeExecutionTaskViewModel { get; set; }
        StockTradeExecutionTaskViewModel CurrentStockTradeExecutionTaskViewModelIndex { get; set; }
        string ExecutionLabel { get; set; }
        bool IsBusy { get; set; }
        int ProgressForAllTasks { get; set; }
        ReactiveCommand<List<StockTradeExecutionTaskViewModel>> RemoveStockFromStocksToExecute { get; }
        ReactiveCommand<AsyncVoid> RunStocksToExecute { get; }
        ReactiveList<Stock> SelectedStocks { get; set; }
        StockTradeExecutionTaskViewModel SelectedStockToTrade { get; set; }
        ReactiveCommand<AsyncVoid> StartExecutingTrades { get; }
        ReactiveList<Stock> Stocks { get; set; }
        Dictionary<string, ReactiveCommand<AsyncVoid>> ToggleExecutionDictionary { get; set; }
        int TotalIterationsForAllTasks { get; set; }
        ReactiveList<StockTradeExecutionTaskViewModel> TradesToExecute { get; set; }
    }
}