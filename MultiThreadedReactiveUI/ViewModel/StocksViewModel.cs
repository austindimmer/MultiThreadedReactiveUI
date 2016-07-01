#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using MultiThreadedReactiveUI.DataProvider;
using MultiThreadedReactiveUI.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MultiThreadedReactiveUI.Extensions;

#endregion

namespace MultiThreadedReactiveUI.ViewModel
{

    public class StocksViewModel : ReactiveObject, IStocksViewModel
    {

        private readonly IStocksDataProvider _DataProvider;
        int CurrentLoopCounter;


        public StocksViewModel(IStocksDataProvider dataProvider, IContainer container)
        {

            _DataProvider = dataProvider;
            Container = container;
            CancellationTokenSource = new CancellationTokenSource();
            SelectedStocks = new ReactiveList<Stock>();
            TradesToExecute = new ReactiveList<StockTradeExecutionTaskViewModel>();
            TradesExecuted = new ReactiveList<StockTradeExecutionTaskViewModel>();
            var window = TimeSpan.FromSeconds(60);
            TradesExecutedStream = new ReplaySubject<StockTradeExecutionTaskViewModel>(window);

            TradesExecutedStream.Subscribe(TradeOnNext, TradeOnError, TradeOnCompleted);

            ResetStocksData();


            StartExecutingTrades = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return RunTradesToExecuteAsync(); });

            AddStockToTradesToExecute = ReactiveCommand.CreateAsyncTask<List<StockTradeExecutionTaskViewModel>>(_ => { return AddStockToTradesToExecuteAsync(); });
            AddStockToTradesToExecute
                    .Subscribe(p =>
                    {
                        using (TradesToExecute.SuppressChangeNotifications())
                        {
                            TradesToExecute.Clear();
                            TradesToExecute.AddRange(p);
                            SelectedStockToTrade = TradesToExecute.LastOrDefault();
                        }
                    });
            AddStockToTradesToExecute.ThrownExceptions.Subscribe(
                ex => Console.WriteLine("Error whilst adding Stocks! Err: {0}", ex.Message));

            RemoveStockFromStocksToExecute = ReactiveCommand.CreateAsyncTask<List<StockTradeExecutionTaskViewModel>>(_ => { return RemoveStockFromStocksToExecuteAsync(); });
            RemoveStockFromStocksToExecute
                    .Subscribe(p =>
                    {
                        using (TradesToExecute.SuppressChangeNotifications())
                        {
                            TradesToExecute.Clear();
                            TradesToExecute.AddRange(p);
                            SelectedStockToTrade = TradesToExecute.FirstOrDefault();
                        }
                    });
            RemoveStockFromStocksToExecute.ThrownExceptions.Subscribe(
                ex => Console.WriteLine("Error whilst removing Stocks! Err: {0}", ex.Message));

            RunStocksToExecute = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return RunTradesToExecuteAsync(); });
            RunStocksToExecute.ThrownExceptions.Subscribe(
                ex => Console.WriteLine("Error whilst running Stocks! Err: {0}", ex.Message));


            CancelRunningStocksToExecute = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return CancelRunningStocksToExecuteAsync(); });
            CancelRunningStocksToExecute.ThrownExceptions.Subscribe(
                ex => Console.WriteLine("Error whilst cancelling running Stocks! Err: {0}", ex.Message));

            SetupCancelRunViewModels();
            SetComputationViewModelBusyIndicator(false);

        }

        private Task<List<StockTradeExecutionTaskViewModel>> AddStockToTradesToExecuteAsync()
        {
            return Task.Run(() =>
            {
                List<StockTradeExecutionTaskViewModel> updatedList = TradesToExecute.ToList();
                foreach (var Stock in SelectedStocks)
                {
                    StockTradeExecutionTaskViewModel viewModel = new StockTradeExecutionTaskViewModel();
                    viewModel.Symbol = Stock.Symbol;
                    viewModel.Price = Stock.Price;
                    viewModel.Quantity = 10;
                    viewModel.TradeType = TradeType.Buy;
                    updatedList.Add(viewModel);
                }

                return updatedList;
            });
        }


        private Task<AsyncVoid> CancelRunningStocksToExecuteAsync()
        {
            return Task.Run(() =>
            {

                try
                {
                    CancellationTokenSource.Cancel();
                }
                catch (OperationCanceledException e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    ResetStockTradeExecutionTaskViewModel();
                    ToggleRunCancelCommand();
                    SetComputationViewModelBusyIndicator(false);
                    CancellationTokenSource.Dispose();
                }
                return AsyncVoid.Default;
            });
        }

        private Stock ComputeStockStatistics(Stock stock)
        {
            if (stock.LastDividend > 0)
            {
                var peRatio = Math.Round((stock.Price / stock.LastDividend), 2);
                stock.PERatio = peRatio;
            }
            if (stock.Price > 0)
            {
                decimal dividendYield = 0;
                switch (stock.Type)
                {
                    case StockType.Common:
                        dividendYield = Math.Round((stock.LastDividend / stock.Price), 2);
                        break;
                    case StockType.Preferred:
                        dividendYield = Math.Round((((stock.FixedDividend / 100) * stock.ParValue) / stock.Price), 2);
                        break;
                }
                stock.DividendYield = dividendYield;
            }
            return stock;
        }

        private void RemoveExecutedTradesFromTradesToExecute(StockTradeExecutionTaskViewModel trade)
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
             {
                 int index = TradesToExecute.IndexOf(trade);
                 TradesToExecute.RemoveAt(index);
             }, Container);
        }



        private Task<List<StockTradeExecutionTaskViewModel>> RemoveStockFromStocksToExecuteAsync()
        {
            return Task.Run(() =>
            {
                List<StockTradeExecutionTaskViewModel> updatedList = TradesToExecute.ToList();
                updatedList.Remove(SelectedStockToTrade);
                return updatedList;
            });
        }


        private void ResetStocksData()
        {
            var inMemoryStocks = _DataProvider.LoadStocks();
            Stocks = new ReactiveList<Stock>();
            Stocks.Clear();
            foreach (var stock in inMemoryStocks)
            {
                ComputeStockStatistics(stock);
                Stocks.Add(stock);
            }

            UpdateIndex();
        }

        private void ResetStockTradeExecutionTaskViewModel()
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
             {
                 foreach (var StockTradeExecutionTaskViewModel in TradesToExecute)
                 {
                     StockTradeExecutionTaskViewModel.IsIndeterminate = false;
                     StockTradeExecutionTaskViewModel.Progress = 0;
                 }
             }, Container);
        }

        /// <summary>
        /// This method will set UI on all tasks to indeterminate and then iterate through all the work to be done.
        /// </summary>
        /// <returns></returns>
        private Task<AsyncVoid> RunTradesToExecuteAsync()
        {
            ResetStockTradeExecutionTaskViewModel();
            SetProgress(0);
            CurrentLoopCounter = 0;
            ToggleRunCancelCommand();
            //Calculate total iterations to perform at start of work
            TotalIterationsForAllTasks = TradesToExecute.Count;
            SetComputationViewModelBusyIndicator(true);
            return Task.Run(() =>
            {
                RunTradesToExecuteInSeperateThreads();

                return AsyncVoid.Default;

            });
        }

        private void RunTradesToExecuteInSeperateThreads()
        {
            CancellationTokenSource = new CancellationTokenSource();
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = CancellationTokenSource.Token;
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;

            //There is a difference between specifying Scheduler.Default and not specifying a scheduler. Using Scheduler.Default will introduce asynchronous and possibly concurrent behavior, while not supplying a scheduler leaves it up to the discretion of the operator. Some operators will choose to execute synchronously while others will execute asynchronously, while others will choose to jump threads.
            //TradesExecutedValuesBuffer.Buffer(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15), Scheduler.Default).Subscribe(TradeBufferOnNext, TradeBufferOnError, TradeBufferOnCompleted);

            //TradesExecutedValuesBuffer.Buffer(TimeSpan.FromSeconds(20), Scheduler.Default).Subscribe(TradeBufferOnNext, TradeBufferOnError, TradeBufferOnCompleted);

            //TradesExecutedValuesBuffer.Buffer(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15)).Subscribe(TradeBufferOnNext, TradeBufferOnError, TradeBufferOnCompleted);
            //TradesExecutedValuesBuffer.Buffer(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)).Subscribe(TradeBufferOnNext, TradeBufferOnError, TradeBufferOnCompleted);
            //TradesExecutedValuesBuffer.Buffer(TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(60)).Subscribe(TradeBufferOnNext, TradeBufferOnError, TradeBufferOnCompleted);
            //TradesExecutedValuesBuffer.Buffer(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20)).Subscribe(TradeBufferOnNext, TradeBufferOnError, TradeBufferOnCompleted);

            //TradesExecutedValuesBuffer.Buffer(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)).Select(CurrentTradeBufferList => CurrentTradeBufferList).Subscribe(TradeBufferOnNext, TradeBufferOnError, TradeBufferOnCompleted);




            ConcurrentDictionary<int, StockTradeExecutionTaskViewModel> tasks = new ConcurrentDictionary<int, StockTradeExecutionTaskViewModel>();
            for (int i = 0; i < TradesToExecute.Count; i++)
                tasks.TryAdd(i, TradesToExecute[i]);
            TotalIterationsForAllTasks = tasks.Count;
            try
            {
                Parallel.ForEach(tasks, po, StockTradeExecutionTaskViewModel =>
                {
                    SimulateTrades(StockTradeExecutionTaskViewModel);
                    UpdateTradesExecuted(StockTradeExecutionTaskViewModel.Value);
                    TradesExecutedStream.OnNext(StockTradeExecutionTaskViewModel.Value);




                    int percentCompleteAllTasks = (int)Math.Round((double)(100 * CurrentLoopCounter) / TotalIterationsForAllTasks);
                    SetProgress(percentCompleteAllTasks);
                    Interlocked.Increment(ref this.CurrentLoopCounter);
                    if (CurrentLoopCounter == TotalIterationsForAllTasks)
                    {
                        SetComputationViewModelBusyIndicator(false);
                        SetProgress(100);
                        ToggleRunCancelCommand();

                        List<StockTradeExecutionTaskViewModel> snapshot = new List<StockTradeExecutionTaskViewModel>();
                        using (TradesExecutedStream.Subscribe(item => snapshot.Add(item)))
                        {
                            TradeBufferOnNext(snapshot);
                            foreach (var item in snapshot)
                            {
                                Debug.WriteLine(String.Format("Traded: {0} Price:{1} Quantity:{2}", item.Symbol, item.Price, item.Quantity));
                            }

                        }
                    }

                });
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                //TradesExecutedStream.OnCompleted();
            }

        }

        private void SetComputationViewModelBusyIndicator(bool value)
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
               {
                   if (value == false)
                       IsBusy = true;
                   if (value == true)
                       IsBusy = false;
                   foreach (var StockTradeExecutionTaskViewModel in TradesToExecute)
                       StockTradeExecutionTaskViewModel.IsIndeterminate = value;
               }, Container);

        }

        private void SetProgress(int percentComplete)
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
               {
                   ProgressForAllTasks = percentComplete;
               }, Container);
        }

        private void SetupCancelRunViewModels()
        {
            CancelRunViewModel cancelViewModel = new CancelRunViewModel();
            cancelViewModel.DisplayText = Constants.CancelButtonDisplayText;
            cancelViewModel.ReactiveCommandToExecute = CancelRunningStocksToExecute;
            CancelRunViewModelCancel = cancelViewModel;
            CancelRunViewModel runViewModel = new CancelRunViewModel();
            runViewModel.DisplayText = Constants.RunButtonDisplayText;
            runViewModel.ReactiveCommandToExecute = RunStocksToExecute;
            CancelRunViewModelRun = runViewModel;
            CurrentCancelRunViewModel = runViewModel;
        }

        private void SimulateTrades(KeyValuePair<int, StockTradeExecutionTaskViewModel> stockTradeExecutionTaskViewModel)
        {
            int progress = 0;
            while (progress <= 100)
            {
                //Interlocked.Increment(ref progress);
                progress += 10;
                Debug.WriteLine(String.Format("Stock: {0} Progress: {1}", stockTradeExecutionTaskViewModel.Value.Symbol, progress));
                UpdateStockTradeExecutionTaskViewModel(stockTradeExecutionTaskViewModel, progress);
                Thread.Sleep(20);
            }
            if (progress > 100) RemoveExecutedTradesFromTradesToExecute(stockTradeExecutionTaskViewModel.Value);



        }

        private void ToggleRunCancelCommand()
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
            {
                bool hasToggled = false;
                if (CurrentCancelRunViewModel.DisplayText == Constants.RunButtonDisplayText)
                {
                    CurrentCancelRunViewModel = CancelRunViewModelCancel;
                    hasToggled = true;
                }
                if (CurrentCancelRunViewModel.DisplayText == Constants.CancelButtonDisplayText && hasToggled == false)
                    CurrentCancelRunViewModel = CancelRunViewModelRun;
            }, Container);
        }


        private void TradeBufferOnNext(IList<StockTradeExecutionTaskViewModel> trades)
        {
            if (trades.Count > 0)
            {
                var currentSymbols = trades.Select(x => x.Symbol).Distinct();
                foreach (var symbol in currentSymbols)
                {
                    var stock = Stocks.Select(x => x).Where(x => x.Symbol == symbol).FirstOrDefault();
                    var tradesInCurrentStock = trades.Select(x => x).Where(y => y.Symbol == symbol);
                    var totalQuantiy = tradesInCurrentStock.Select(x => x.Quantity).Sum();
                    var priceTimesQuantity = tradesInCurrentStock.Select(x => x.Quantity * x.Price).Sum();
                    var currentPrice = Math.Round(priceTimesQuantity / totalQuantiy, 2);

                    stock.Price = currentPrice;
                    Debug.WriteLine(String.Format("TradeBufferOnNext Average Price: {0} StockPrice:{1} Quantity:{2}", currentPrice, stock.Price, totalQuantiy));
                    stock = ComputeStockStatistics(stock);
                    UpdateUIStockPrice(stock);
                    UpdateIndex();
                }
            }
            var viewModel = trades.FirstOrDefault();
            if (viewModel == null)
                Debug.WriteLine(String.Format("TradeBufferOnNext: viewModelList.Count:{0} Buffer now empty!", trades.Count));
        }

        private void TradeOnCompleted()
        {
            Debug.WriteLine(String.Format("Trade Completed"));
        }

        private void TradeOnError(Exception ex)
        {
            Debug.WriteLine(String.Format("Trade Error: {0}", ex.Message));
        }

        private void TradeOnNext(StockTradeExecutionTaskViewModel viewModel)
        {
            Debug.WriteLine(String.Format("Traded: {0} Price:{1} Quantity:{2}", viewModel.Symbol, viewModel.Price, viewModel.Quantity));
        }


        private void UpdateIndex()
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
            {
                var index = Stocks.Select(x => x.Price).Average();
                AllShareIndex = index;
            }, Container);
        }

        private void UpdateStockTradeExecutionTaskViewModel(KeyValuePair<int, StockTradeExecutionTaskViewModel> StockTradeExecutionTaskViewModel, int progress)
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
             {
                 var viewModelToUpdate = TradesToExecute.ElementAtOrDefault(StockTradeExecutionTaskViewModel.Key);
                 if (viewModelToUpdate != null)
                 {
                     Debug.WriteLine(String.Format("Updating ViewModel for Stock: {0} Key: {1} Progress: {2}", viewModelToUpdate.Symbol, viewModelToUpdate.Timestamp, progress));
                     viewModelToUpdate.IsIndeterminate = false;
                     viewModelToUpdate.Progress = progress;
                 }
             }, Container);
        }

        private void UpdateTradesExecuted(StockTradeExecutionTaskViewModel executedTrade)
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
             {
                 executedTrade.Timestamp = DateTime.Now;
                 TradesExecuted.Add(executedTrade);

             }, Container);
        }

        private void UpdateUIStockPrice(Stock stock)
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
             {
                 int index = Stocks.IndexOf(stock);
                 Stocks.RemoveAt(index);
                 Stocks.Insert(index, stock);
             }, Container);
        }

        private CancellationTokenSource CancellationTokenSource { get; set; }



        public ReactiveCommand<List<StockTradeExecutionTaskViewModel>> AddStockToTradesToExecute { get; protected set; }

        [Reactive]
        public decimal AllShareIndex { get; set; }
        public ReactiveCommand<AsyncVoid> CancelRunningStocksToExecute { get; protected set; }

        [Reactive]
        public CancelRunViewModel CancelRunViewModelCancel { get; set; }
        [Reactive]
        public CancelRunViewModel CancelRunViewModelRun { get; set; }
        public IContainer Container { get; set; }
        [Reactive]
        public CancelRunViewModel CurrentCancelRunViewModel { get; set; }
        [Reactive]
        public StockTradeExecutionTaskViewModel CurrentStockTradeExecutionTaskViewModel { get; set; }
        [Reactive]
        public StockTradeExecutionTaskViewModel CurrentStockTradeExecutionTaskViewModelIndex { get; set; }
        public Func<IList<StockTradeExecutionTaskViewModel>, object> CurrentTradeBufferList { get; private set; }
        [Reactive]
        public string ExecutionLabel { get; set; }
        [Reactive]
        public bool IsBusy { get; set; }
        [Reactive]
        public int ProgressForAllTasks { get; set; }

        public ReactiveCommand<List<StockTradeExecutionTaskViewModel>> RemoveStockFromStocksToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> RunStocksToExecute { get; protected set; }

        public ReactiveList<Stock> SelectedStocks { get; set; }
        [Reactive]
        public StockTradeExecutionTaskViewModel SelectedStockToTrade { get; set; }

        public ReactiveCommand<AsyncVoid> StartExecutingTrades { get; protected set; }

        [Reactive]
        public ReactiveList<Stock> Stocks { get; set; }

        public Dictionary<string, ReactiveCommand<AsyncVoid>> ToggleExecutionDictionary { get; set; }
        [Reactive]
        public int TotalIterationsForAllTasks { get; set; }
        public int TotalProgressCounter { get; private set; }

        [Reactive]
        public ReactiveList<StockTradeExecutionTaskViewModel> TradesExecuted { get; set; }

        public ReplaySubject<StockTradeExecutionTaskViewModel> TradesExecutedStream { get; set; }
        [Reactive]
        public ReactiveList<StockTradeExecutionTaskViewModel> TradesToExecute { get; set; }
    }
}