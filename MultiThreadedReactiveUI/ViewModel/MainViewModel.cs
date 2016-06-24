#region

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using ReactiveUI;
using MultiThreadedReactiveUI.DataProvider;
using WPF_Task.ViewModel;
using MultiThreadedReactiveUI.Model;
using ReactiveUI.Fody.Helpers;
using System.Threading.Tasks;
using System.Collections.Generic;

#endregion

namespace MultiThreadedReactiveUI.ViewModel
{

    public class MainViewModel : ReactiveObject{

        private readonly IFunctionDataProvider _dataProvider;
 

        public MainViewModel(IFunctionDataProvider dataProvider){
            _dataProvider = dataProvider;
            Functions = new ReactiveList<Function>();
            LoadData();
            FunctionCategories = (List<string>)Functions.CreateDerivedCollection(x => x.Category).Distinct().ToList();
            FunctionCategories.Add("All Categories");
            SelectedCategory = FunctionCategories.Last();
            //LoadedLocalGrammars = (ReactiveList<SapiGrammarComId>)LoadedSapiGrammars.Values.CreateDerivedCollection(x => x, x => x.IsGlobal == false, (l, r) => l.GrammarName.CompareTo(r.GrammarName));

            StartAsyncCommand = ReactiveCommand.CreateAsyncTask<AsyncVoid>(_ =>
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
            });

        }


        public ReactiveList<Function> Functions { get; set; }
        public ReactiveList<Function> FunctionsToExecute { get; set; }
        public ReactiveList<Function> FilteredFunctions { get; set; }
        public List<string> FunctionCategories { get; set; }

        [Reactive]
        public Function SelectedFunction { get; set; }
        [Reactive]
        public string SelectedCategory { get; set; }

        public ReactiveCommand<AsyncVoid> AddFunctionToFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> RemoveFunctionFromFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> RunFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> CancelRunningFunctionsToExecute { get; protected set; }
        public ReactiveCommand<AsyncVoid> CategoryFilterSelected { get; protected set; }

        public ReactiveCommand<AsyncVoid> StartAsyncCommand { get; protected set; }


        private void LoadData(){
            var functions = _dataProvider.LoadFunctions();
            foreach (var function in functions)
            {
                Functions.Add(function);
            }

            SelectedFunction = Functions.Count > 0 ? Functions.First() : null;
        }

        [Reactive]public string ExecutionLabel{ get; set; }
        [Reactive]
        public int Progress { get; set; }

    }
}