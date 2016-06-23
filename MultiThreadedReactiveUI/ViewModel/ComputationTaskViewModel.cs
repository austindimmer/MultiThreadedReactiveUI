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
        public ComputationTaskViewModel(double inputValue = 1.0, int numberOfIterations = 1000000)
        {
            this.InputValue = inputValue;
            this.NumberOfIterations = numberOfIterations;
        }
        [Reactive]
        public double InputValue { get; set; }
        [Reactive]
        public int NumberOfIterations { get; set; }
        [Reactive]
        public Func<double, int, double> FunctionToRun { get; set; }

        [Reactive]
        public int PercentComplete { get; set; }
    }
}
