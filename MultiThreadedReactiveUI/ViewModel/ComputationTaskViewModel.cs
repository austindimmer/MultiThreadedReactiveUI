using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadedReactiveUI.ViewModel
{
    public class ComputationTaskViewModel : ReactiveObject
    {
        //public ComputationTaskViewModel(Func<double, double> functionToRun, double inputValue = 1.0, int numberOfIterations = 1000000)
            public ComputationTaskViewModel(Func<double, double> functionToRun, double inputValue = 1.0, int numberOfIterations = 1000000)
        {
            this.FunctionToRun = functionToRun;
            this.InputValue = inputValue;
            this.NumberOfIterations = numberOfIterations;
        }
        [Reactive]
        public string DisplayName { get; set; }
        [Reactive]
        public double InputValue { get; set; }
        [Reactive]
        public int NumberOfIterations { get; set; }

        public Func<double, double> FunctionToRun;

        [Reactive]
        public int Progress { get; set; }
        [Reactive]
        public bool IsIndeterminate{ get; set; }
}
}
