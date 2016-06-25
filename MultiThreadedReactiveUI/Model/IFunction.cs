using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadedReactiveUI.Model
{
    interface IFunction
    {
        string DisplayName { get; }
        string Category { get; }

        double FunctionToRun(double parameter);
    }
}
