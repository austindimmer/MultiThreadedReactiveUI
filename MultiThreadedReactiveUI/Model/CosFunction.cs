using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadedReactiveUI.Model
{
    class CosFunction : Function
    {
        public string Category
        {
            get
            {
                return Constants.CategoryTrig;
            }
        }

        public string DisplayName
        {
            get
            {
                return Constants.FuncitonCos;
            }
        }

        public double FunctionToRun(double parameter)
        {
            return Math.Cos(parameter);
        }
    }
}
