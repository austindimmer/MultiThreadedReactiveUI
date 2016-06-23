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

#endregion

namespace MultiThreadedReactiveUI.ViewModel
{

    public class MainViewModel : ReactiveObject{

        private readonly IFunctionDataProvider _dataProvider;
 

        public MainViewModel(IFunctionDataProvider dataProvider){
            _dataProvider = dataProvider;
            LoadData();
        }


        public ObservableCollection<Function> Functions { get; set; }

        public Function SelectedFunction { get; set; }


        private void LoadData(){
            var functions = _dataProvider.LoadFunctions();
            foreach (var function in functions)
            {
                Functions.Add(function);
            }

            SelectedFunction = Functions.Count > 0 ? Functions.First() : null;
        }

        [Reactive]public string ExecutionLabel{ get; set; }

}
}