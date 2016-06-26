#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using MultiThreadedReactiveUI.DataProvider;
using MultiThreadedReactiveUI.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

#endregion

namespace MultiThreadedReactiveUI.ViewModel
{

    public class MainViewModel : ReactiveObject, IMainViewModel
    {

        private readonly IFunctionDataProvider _DataProvider;

        private CancellationToken _CancellationToken { get; set; }
        private CancellationTokenSource _CancellationTokenSource { get; set; }


        public MainViewModel(IFunctionDataProvider dataProvider)
        {
            _DataProvider = dataProvider;
            _CancellationTokenSource = new CancellationTokenSource();
            _CancellationToken = _CancellationTokenSource.Token;
            SelectedFunctions = new ReactiveList<Function>();
            TasksToExecute = new ReactiveList<ComputationTaskViewModel>();
            ResetFunctionsData();
            FunctionCategories = (List<string>)Functions.CreateDerivedCollection(x => x.Category).Distinct().ToList();
            //LoadedLocalGrammars = (ReactiveList<SapiGrammarComId>)LoadedSapiGrammars.Values.CreateDerivedCollection(x => x, x => x.IsGlobal == false, (l, r) => l.GrammarName.CompareTo(r.GrammarName));
            FunctionCategories.Add("All Categories");
            SelectedCategory = FunctionCategories.Last();


            StartAsyncCommand = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return StartAsyncWork(); });
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

        }


        private Task<List<ComputationTaskViewModel>> AddFunctionToFunctionsToExecuteAsync()
        {
            return Task.Run(() =>
            {
                List<ComputationTaskViewModel> updatedList = TasksToExecute.ToList();
                foreach (var function in SelectedFunctions)
                {
                    ComputationTaskViewModel viewModel = new ComputationTaskViewModel((x, y) => function.FunctionToRun(1));
                    viewModel.DisplayName = function.DisplayName;
                    updatedList.Add(viewModel);
                }

                return updatedList;
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



        private Task<AsyncVoid> CancelRunningFunctionsToExecuteAsync()
        {
            return Task.Run(() =>
            {
                _CancellationTokenSource.Cancel();
                return AsyncVoid.Default;
            });
        }




        private void ResetFunctionsData()
        {
            var functions = _DataProvider.LoadFunctions();
            Functions = null;
            Functions = new ReactiveList<Function>();
            Functions.Clear();


            foreach (var function in functions)
                {
                    Functions.Add(function);
                }
            SelectedCategory = Constants.CategoryAll;
            CategoryFilterSelectedAsync();
        }

        public Task<IEnumerable<Function>> CategoryFilterSelectedAsync()
        {
            return Task.Run(() =>
            {
                if (Functions == null || Functions.Count == 0)
                    ResetFunctionsData();
                if (SelectedCategory == null || SelectedCategory == String.Empty)
                    SelectedCategory = Constants.CategoryAll;

                var FunctionsToReturn = Functions.Select(x => x).Where(y => y.Category == SelectedCategory);

                return FunctionsToReturn;
            });

        }

        private Task<AsyncVoid> RunFunctionsToExecuteAsync()
        {
            return Task.Run(() =>
            {
                return AsyncVoid.Default;
            });
        }

        private Task<AsyncVoid> StartAsyncWork()
        {
            return Task.Run(() =>
            {
                Progress = 0;
                while (Progress <= 100)
                {
                    Progress += 1;
                    Thread.Sleep(50);
                }

                return AsyncVoid.Default;
            });
        }


        public ReactiveCommand<List<ComputationTaskViewModel>> AddFunctionToFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> CancelRunningFunctionsToExecute { get; protected set; }
        public ReactiveCommand<IEnumerable<Function>> CategoryFilterSelected { get; protected set; }
        [Reactive]
        public string ExecutionLabel { get; set; }
        public ReactiveList<Function> FilteredFunctions { get; set; }
        public List<string> FunctionCategories { get; set; }


        [Reactive]
        public ReactiveList<Function> Functions { get; set; }
        [Reactive]
        public ReactiveList<ComputationTaskViewModel> TasksToExecute { get; set; }
        [Reactive]
        public int Progress { get; set; }
        public ReactiveCommand<List<ComputationTaskViewModel>> RemoveFunctionFromFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> RunFunctionsToExecute { get; protected set; }
        [Reactive]
        public string SelectedCategory { get; set; }

        public ReactiveList<Function> SelectedFunctions { get; set; }
        [Reactive]
        public ComputationTaskViewModel SelectedTask { get; set; }

        public ReactiveCommand<AsyncVoid> StartAsyncCommand { get; protected set; }
        public ReactiveCommand<AsyncVoid> ToggleRunCancelCommand { get; protected set; }

        public Dictionary<string, ReactiveCommand<AsyncVoid>> ToggleExecutionDictionary { get; set; }

    }
}