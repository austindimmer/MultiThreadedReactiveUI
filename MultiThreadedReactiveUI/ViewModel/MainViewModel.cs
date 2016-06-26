#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using MultiThreadedReactiveUI.DataProvider;
using MultiThreadedReactiveUI.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Windows;
using System.Windows.Threading;
using Autofac;

#endregion

namespace MultiThreadedReactiveUI.ViewModel
{

    public class MainViewModel : ReactiveObject, IMainViewModel
    {

        private readonly IFunctionDataProvider _DataProvider;
        int CurrentLoopCounter;
        public IContainer Container { get; set; }

        public MainViewModel(IFunctionDataProvider dataProvider, IContainer container)
        {
            _DataProvider = dataProvider;
            Container = container;
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
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
            CancelRunningFunctionsToExecute = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return CancelRunningFunctionsToExecuteAsync(); });
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
                CancellationTokenSource.Cancel();
                ToggleRunCancelCommand();
                return AsyncVoid.Default;
            });
        }

        private void ExecuteFunctionLoop(ComputationTaskViewModel computationTaskViewModel)
        {

            // set cancel on the token somewhere in the workers to make the loop stop


            int[] nums = Enumerable.Range(0, computationTaskViewModel.NumberOfIterations).ToArray();
            CancellationTokenSource cts = new CancellationTokenSource();

            // Use ParallelOptions instance to store the CancellationToken
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;


            try
            {
                Parallel.ForEach(nums, po, (num) =>
                {

                    //double d = Math.Sqrt(num);
                    double d = computationTaskViewModel.FunctionToRun.Invoke(computationTaskViewModel.InputValue);
                    Console.WriteLine("{0} on {1}", d, Thread.CurrentThread.ManagedThreadId);
                    po.CancellationToken.ThrowIfCancellationRequested();
                    Interlocked.Increment(ref this.CurrentLoopCounter);
                    var currentProgress = (CurrentLoopCounter / nums.Length) * 100;
                    computationTaskViewModel.Progress = currentProgress;

                });
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                cts.Dispose();
            }
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
            CancellationTokenSource cts = new CancellationTokenSource();
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;


            var data = TasksToExecute.ToList();
            try
            {
                Parallel.ForEach(TasksToExecute, po, computationTaskViewModel =>
                {

                    //double d = Math.Sqrt(num);
                    double d = computationTaskViewModel.FunctionToRun.Invoke(computationTaskViewModel.InputValue);
                    Console.WriteLine("{0} on {1}", d, Thread.CurrentThread.ManagedThreadId);
                    po.CancellationToken.ThrowIfCancellationRequested();
                    Interlocked.Increment(ref this.CurrentLoopCounter);
                    var currentProgress = (CurrentLoopCounter / TotalIterationsForAllTasks) * 100;
                    computationTaskViewModel.Progress = currentProgress;

                });
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                cts.Dispose();
            }
        }

        /// <summary>
        /// This method will set UI on all tasks to indeterminate and then iterate through all the work to be done.
        /// </summary>
        /// <returns></returns>
        private Task<AsyncVoid> RunFunctionsToExecuteAsync()
        {
            ToggleRunCancelCommand();
            //Calculate total iterations to perform at start of work
            TotalIterationsForAllTasks = TasksToExecute.Select(x => x.NumberOfIterations).Sum();
            //Set all tasks to indeterminate first
            foreach (var computationTaskViewModel in TasksToExecute)
                computationTaskViewModel.IsIndeterminate = true;
            return Task.Run(() =>
            {
                //foreach (var computationTaskViewModel in TasksToExecute)
                //{
                //    ExecuteFunctionLoop(computationTaskViewModel);
                //}
                // Use ParallelOptions instance to store the CancellationToken
                RunFuncitonsToExecuteInSeperateThreads();

                return AsyncVoid.Default;

            });
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
            //Dispatcher.CurrentDispatcher.InvokeIfRequired(() => GrammarViewModel.Grammars.Clear(), Facade.Container);
            Application.Current.Dispatcher.InvokeIfRequired(() => {
                bool hasToggled = false;
                if (CurrentCancelRunViewModel.DisplayText == Constants.RunButtonDisplayText) {
                    CurrentCancelRunViewModel = CancelRunViewModelCancel;
                    hasToggled = true;
                }
                if (CurrentCancelRunViewModel.DisplayText == Constants.CancelButtonDisplayText && hasToggled==false )
                {
                    CurrentCancelRunViewModel = CancelRunViewModelRun;
                }
            }, Container);
        }

        private CancellationToken CancellationToken { get; set; }
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