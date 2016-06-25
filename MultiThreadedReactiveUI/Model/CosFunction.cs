using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadedReactiveUI.Model
{
    class CosFunction : Function
    {
        public override string Category
        {
            get
            {
                return Constants.CategoryTrig;
            }
        }

        public override string DisplayName
        {
            get
            {
                return Constants.FuncitonCos;
            }
        }

        public override double FunctionToRun(double parameter)
        {
            return Math.Cos(parameter);
        }
    }
}
