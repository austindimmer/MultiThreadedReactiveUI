using System;
using System.Threading.Tasks;

namespace MultiThreadedReactiveUI.Model
{
    public class Function : IFunction
    {
        public string Category { get; }


        public string DisplayName { get; }

        public double FunctionToRun(double parameter) {
            return Math.Sin(parameter);
         }
    }
}
