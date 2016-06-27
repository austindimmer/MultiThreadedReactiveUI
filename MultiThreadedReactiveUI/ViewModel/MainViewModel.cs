#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using MultiThreadedReactiveUI.DataProvider;
using MultiThreadedReactiveUI.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Concurrent;

#endregion

namespace MultiThreadedReactiveUI.ViewModel
{

    public class MainViewModel : ReactiveObject, IMainViewModel
    {

        private readonly IFunctionDataProvider _DataProvider;
        int CurrentLoopCounter;



        public MainViewModel(IFunctionDataProvider dataProvider, IContainer container)
        {

            _DataProvider = dataProvider;
            Container = container;
            CancellationTokenSource = new CancellationTokenSource();
            SelectedFunctions = new ReactiveList<Function>();
            TasksToExecute = new ReactiveList<ComputationTaskViewModel>();
            ResetFunctionsData();
            FunctionCategories = (List<string>)Functions.CreateDerivedCollection(x => x.Category).Distinct().ToList();
            //LoadedLocalGrammars = (ReactiveList<SapiGrammarComId>)LoadedSapiGrammars.Values.CreateDerivedCollection(x => x, x => x.IsGlobal == false, (l, r) => l.GrammarName.CompareTo(r.GrammarName));
            FunctionCategories.Add(Constants.CategoryAll);
            SelectedCategory = FunctionCategories.Last();


            StartRunningTasks = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return RunFunctionsToExecuteAsync(); });
            AddFunctionToFunctionsToExecute = ReactiveCommand.CreateAsyncTask<List<ComputationTaskViewModel>>(_ => { return AddFunctionToFunctionsToExecuteAsync(); });
            AddFunctionToFunctionsToExecute
                    .Subscribe(p =>
                    {
                        using (TasksToExecute.SuppressChangeNotifications())
                        {
                            TasksToExecute.Clear();
                            TasksToExecute.AddRange(p);
                            SelectedTask = TasksToExecute.LastOrDefault();
                        }
                    });
            AddFunctionToFunctionsToExecute.ThrownExceptions.Subscribe(
                ex => Console.WriteLine("Error whilst adding functions! Err: {0}", ex.Message));

            RemoveFunctionFromFunctionsToExecute = ReactiveCommand.CreateAsyncTask<List<ComputationTaskViewModel>>(_ => { return RemoveFunctionFromFunctionsToExecuteAsync(); });
            RemoveFunctionFromFunctionsToExecute
                    .Subscribe(p =>
                    {
                        using (TasksToExecute.SuppressChangeNotifications())
                        {
                            TasksToExecute.Clear();
                            TasksToExecute.AddRange(p);
                            SelectedTask = TasksToExecute.FirstOrDefault();
                        }
                    });
            RemoveFunctionFromFunctionsToExecute.ThrownExceptions.Subscribe(
                ex => Console.WriteLine("Error whilst removing functions! Err: {0}", ex.Message));

            RunFunctionsToExecute = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return RunFunctionsToExecuteAsync(); });
            RunFunctionsToExecute.ThrownExceptions.Subscribe(
                ex => Console.WriteLine("Error whilst running functions! Err: {0}", ex.Message));


            CancelRunningFunctionsToExecute = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return CancelRunningFunctionsToExecuteAsync(); });
            CancelRunningFunctionsToExecute.ThrownExceptions.Subscribe(
                ex => Console.WriteLine("Error whilst cancelling running functions! Err: {0}", ex.Message));


            CategoryFilterSelected = ReactiveCommand.CreateAsyncTask<IEnumerable<Function>>(_ => { return CategoryFilterSelectedAsync(); });
            CategoryFilterSelected
                    .Subscribe(p =>
                    {
                        if (Functions != null || Functions.Count > 0)
                        {
                            ReactiveList<Function> newFunctions = new ReactiveList<Function>();
                            newFunctions.AddRange(p);
                            Functions = newFunctions;
                        }
                        //using (Functions.SuppressChangeNotifications())
                        //{


                        //}
                    });
            CategoryFilterSelected.ThrownExceptions.Subscribe(
                    ex => Console.WriteLine("Error whilst filtering Categories! Err: {0}", ex.Message));

            SetupCancelRunViewModels();
            SetComputationViewModelBusyIndicator(false);

        }

        private Task<List<ComputationTaskViewModel>> AddFunctionToFunctionsToExecuteAsync()
        {
            return Task.Run(() =>
            {
                List<ComputationTaskViewModel> updatedList = TasksToExecute.ToList();
                foreach (var function in SelectedFunctions)
                {
                    ComputationTaskViewModel viewModel = new ComputationTaskViewModel(x => function.FunctionToRun(1));
                    viewModel.DisplayName = function.DisplayName;
                    updatedList.Add(viewModel);
                }

                return updatedList;
            });
        }



        private Task<AsyncVoid> CancelRunningFunctionsToExecuteAsync()
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
                    ToggleRunCancelCommand();
                    SetComputationViewModelBusyIndicator(false);
                    CancellationTokenSource.Dispose();
                }
                return AsyncVoid.Default;
            });
        }


        private Task<List<ComputationTaskViewModel>> RemoveFunctionFromFunctionsToExecuteAsync()
        {
            return Task.Run(() =>
            {
                List<ComputationTaskViewModel> updatedList = TasksToExecute.ToList();
                updatedList.Remove(SelectedTask);
                return updatedList;
            });
        }


        private void ResetFunctionsData()
        {
            var functions = _DataProvider.LoadFunctions();
            Functions = null;
            Functions = new ReactiveList<Function>();
            Functions.Clear();

            SelectedCategory = Constants.CategoryAll;

            foreach (var function in functions)
                Functions.Add(function);
        }

        private void RunFuncitonsToExecuteInSeperateThreads()
        {
            CancellationTokenSource = new CancellationTokenSource();
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = CancellationTokenSource.Token;
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            ConcurrentDictionary<int, ComputationTaskViewModel> tasks = new ConcurrentDictionary<int, ComputationTaskViewModel>();
            for (int i = 0; i < TasksToExecute.Count; i++)
            {
                tasks.TryAdd(i, TasksToExecute[i]);
            }
            try
            {
                Parallel.ForEach(tasks, po, computationTaskViewModel =>
                {
                    for (int i = 0; i < computationTaskViewModel.Value.NumberOfIterations; i++)
                    {
                        double d = computationTaskViewModel.Value.FunctionToRun.Invoke(computationTaskViewModel.Value.InputValue);
                        Debug.WriteLine("Running {0} with result {1} on {2}", computationTaskViewModel.Value.DisplayName, d, Thread.CurrentThread.ManagedThreadId);
                        po.CancellationToken.ThrowIfCancellationRequested();
                        Interlocked.Increment(ref this.CurrentLoopCounter);
                        Debug.WriteLine("CurrentLoopCounter {0}", CurrentLoopCounter);
                        int percentCompleteIndividualTask = (int)Math.Round((double)(100 * i) / computationTaskViewModel.Value.NumberOfIterations);
                        computationTaskViewModel.Value.Progress = percentCompleteIndividualTask;
                        if ((percentCompleteIndividualTask % 1) == 0)
                        {
                            UpdateComputationTaskViewModel(computationTaskViewModel);
                        }
                        int percentCompleteAllTasks = (int)Math.Round((double)(100 * CurrentLoopCounter) / TotalIterationsForAllTasks);
                        Debug.WriteLine("percentComplete {0}", percentCompleteAllTasks);
                        if ((percentCompleteAllTasks % 1) == 0) {
                            SetProgress(percentCompleteAllTasks);
                        }

                        if (CurrentLoopCounter == TotalIterationsForAllTasks)
                        {
                            SetComputationViewModelBusyIndicator(false);
                            ToggleRunCancelCommand();
                        }
                    }


                });
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private void UpdateComputationTaskViewModel(KeyValuePair<int, ComputationTaskViewModel> computationTaskViewModel)
        {
              Application.Current.Dispatcher.InvokeIfRequired(() =>
               {
                   var viewModelToUpdate = TasksToExecute.ElementAt(computationTaskViewModel.Key);
                   viewModelToUpdate.IsIndeterminate = false;
                   viewModelToUpdate.Progress = computationTaskViewModel.Value.Progress;
               }, Container);
        }

        /// <summary>
        /// This method will set UI on all tasks to indeterminate and then iterate through all the work to be done.
        /// </summary>
        /// <returns></returns>
        private Task<AsyncVoid> RunFunctionsToExecuteAsync()
        {
            CurrentLoopCounter = 0;
            ToggleRunCancelCommand();
            //Calculate total iterations to perform at start of work
            TotalIterationsForAllTasks = TasksToExecute.Select(x => x.NumberOfIterations).Sum(x => x);
            SetComputationViewModelBusyIndicator(true);
            return Task.Run(() =>
            {
                RunFuncitonsToExecuteInSeperateThreads();

                return AsyncVoid.Default;

            });
        }

        private void SetComputationViewModelBusyIndicator(bool value)
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
               {
                   if (value == false)
                       IsBusy = true;
                   if (value == true)
                       IsBusy = false;
                   foreach (var computationTaskViewModel in TasksToExecute)
                       computationTaskViewModel.IsIndeterminate = value;
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
            cancelViewModel.ReactiveCommandToExecute = CancelRunningFunctionsToExecute;
            CancelRunViewModelCancel = cancelViewModel;
            CancelRunViewModel runViewModel = new CancelRunViewModel();
            runViewModel.DisplayText = Constants.RunButtonDisplayText;
            runViewModel.ReactiveCommandToExecute = RunFunctionsToExecute;
            CancelRunViewModelRun = runViewModel;
            CurrentCancelRunViewModel = runViewModel;
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

        private CancellationTokenSource CancellationTokenSource { get; set; }

        public Task<IEnumerable<Function>> CategoryFilterSelectedAsync()
        {
            return Task.Run(() =>
            {
                if (Functions == null || Functions.Count == 0)
                    ResetFunctionsData();
                if (SelectedCategory == null || SelectedCategory == String.Empty)
                    SelectedCategory = Constants.CategoryAll;

                if (SelectedCategory != Constants.CategoryAll)
                {
                    var FunctionsToReturn = Functions.Select(x => x).Where(y => y.Category == SelectedCategory);
                    return FunctionsToReturn;
                }
                else
                {
                    ResetFunctionsData();
                    return Functions;
                }

            });

        }


        public ReactiveCommand<List<ComputationTaskViewModel>> AddFunctionToFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> CancelRunningFunctionsToExecute { get; protected set; }

        [Reactive]
        public CancelRunViewModel CancelRunViewModelCancel { get; set; }
        [Reactive]
        public CancelRunViewModel CancelRunViewModelRun { get; set; }
        public ReactiveCommand<IEnumerable<Function>> CategoryFilterSelected { get; protected set; }
        public IContainer Container { get; set; }
        [Reactive]
        public CancelRunViewModel CurrentCancelRunViewModel { get; set; }
        [Reactive]
        public ComputationTaskViewModel CurrentComputationTaskViewModel { get; set; }
        [Reactive]
        public ComputationTaskViewModel CurrentComputationTaskViewModelIndex { get; set; }
        [Reactive]
        public string ExecutionLabel { get; set; }
        public List<string> FunctionCategories { get; set; }

        [Reactive]
        public ReactiveList<Function> Functions { get; set; }
        [Reactive]
        public bool IsBusy { get; set; }
        [Reactive]
        public int ProgressForAllTasks { get; set; }

        public ReactiveCommand<List<ComputationTaskViewModel>> RemoveFunctionFromFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> RunFunctionsToExecute { get; protected set; }
        [Reactive]
        public string SelectedCategory { get; set; }

        public ReactiveList<Function> SelectedFunctions { get; set; }
        [Reactive]
        public ComputationTaskViewModel SelectedTask { get; set; }

        public ReactiveCommand<AsyncVoid> StartRunningTasks { get; protected set; }
        [Reactive]
        public ReactiveList<ComputationTaskViewModel> TasksToExecute { get; set; }

        public Dictionary<string, ReactiveCommand<AsyncVoid>> ToggleExecutionDictionary { get; set; }
        [Reactive]
        public int TotalIterationsForAllTasks { get; set; }

    }
}