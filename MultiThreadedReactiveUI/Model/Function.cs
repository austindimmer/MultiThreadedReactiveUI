using System;
using System.Threading.Tasks;

namespace MultiThreadedReactiveUI.Model
{
    public abstract class Function : IFunction
    {
        public abstract string Category { get; }


        public abstract string DisplayName { get; }

        public virtual double FunctionToRun(double parameter)
        {
            return Math.Sin(parameter);
        }
    }
}
