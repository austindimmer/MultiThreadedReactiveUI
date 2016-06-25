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

    public class MainViewModel : ReactiveObject
    {

        private readonly IFunctionDataProvider _DataProvider;

        private CancellationToken _CancellationToken { get; set; }
        private CancellationTokenSource _CancellationTokenSource { get; set; }




        public MainViewModel(IFunctionDataProvider dataProvider)
        {
            _DataProvider = dataProvider;
            _CancellationTokenSource = new CancellationTokenSource();
            _CancellationToken = _CancellationTokenSource.Token;
            Functions = new ReactiveList<Function>();
            SelectedFunctions = new ReactiveList<Function>();
            SelectedTask = new ReactiveList<Function>();
            TasksToExecute = new ReactiveList<ComputationTaskViewModel>();
            LoadData();
            FunctionCategories = (List<string>)Functions.CreateDerivedCollection(x => x.Category).Distinct().ToList();
            //LoadedLocalGrammars = (ReactiveList<SapiGrammarComId>)LoadedSapiGrammars.Values.CreateDerivedCollection(x => x, x => x.IsGlobal == false, (l, r) => l.GrammarName.CompareTo(r.GrammarName));
            FunctionCategories.Add("All Categories");
            SelectedCategory = FunctionCategories.Last();


            StartAsyncCommand = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return StartAsyncWork(); });
            AddFunctionToFunctionsToExecute = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return AddFunctionToFunctionsToExecuteAsync(); });
            RemoveFunctionFromFunctionsToExecute = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return RemoveFunctionFromFunctionsToExecuteAsync(); });
            RunFunctionsToExecute = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return RunFunctionsToExecuteAsync(); });
            CancelRunningFunctionsToExecute = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return CancelRunningFunctionsToExecuteAsync(); });
            CategoryFilterSelected = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ => { return CategoryFilterSelectedAsync(SelectedCategory); });

        }

        private Task<AsyncVoid> AddFunctionToFunctionsToExecuteAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    foreach (var function in SelectedFunctions)
                    {
                        ComputationTaskViewModel viewModel = new ComputationTaskViewModel((x,y) => function.FunctionToRun(1));
                        TasksToExecute.Add(viewModel);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                }

                return AsyncVoid.Default;
            });
        }

        private Task<AsyncVoid> CancelRunningFunctionsToExecuteAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    _CancellationTokenSource.Cancel();
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                }

                return AsyncVoid.Default;
            });
        }


        private Task<AsyncVoid> CategoryFilterSelectedAsync(string category)
        {
            return Task.Run(() =>
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                }

                return AsyncVoid.Default;
            });
        }




        private void LoadData()
        {
            var functions = _DataProvider.LoadFunctions();
            foreach (var function in functions)
                Functions.Add(function);
        }

        private Task<AsyncVoid> RemoveFunctionFromFunctionsToExecuteAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                }

                return AsyncVoid.Default;
            });
        }

        private Task<AsyncVoid> RunFunctionsToExecuteAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                }

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
                    Progress += 10;
                    Thread.Sleep(100);
                }

                return AsyncVoid.Default;
            });
        }


        public ReactiveCommand<AsyncVoid> AddFunctionToFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> CancelRunningFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> CategoryFilterSelected { get; protected set; }
        [Reactive]
        public string ExecutionLabel { get; set; }
        public ReactiveList<Function> FilteredFunctions { get; set; }
        public List<string> FunctionCategories { get; set; }



        public ReactiveList<Function> Functions { get; set; }
        public ReactiveList<ComputationTaskViewModel> TasksToExecute { get; set; }
        [Reactive]
        public int Progress { get; set; }
        public ReactiveCommand<AsyncVoid> RemoveFunctionFromFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> RunFunctionsToExecute { get; protected set; }
        [Reactive]
        public string SelectedCategory { get; set; }

        public ReactiveList<Function> SelectedFunctions { get; set; }
        public ReactiveList<Function> SelectedTask { get; set; }

        public ReactiveCommand<AsyncVoid> StartAsyncCommand { get; protected set; }
        public ReactiveCommand<AsyncVoid> ToggleRunCancelCommand { get; protected set; }

    }
}