using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadedReactiveUI.Model
{
    class LnFunction : Function
    {
        public override string Category
        {
            get
            {
                return Constants.CategoryGeneral;
            }
        }

        public override string DisplayName
        {
            get
            {
                return Constants.FuncitonLn;
            }
        }

        public override double FunctionToRun(double parameter)
        {
            return Math.Log(parameter);
        }
    }
}
