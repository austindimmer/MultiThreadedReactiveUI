using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadedReactiveUI.Model
{
    class RndFunction : Function
    {
        public override string Category
        {
            get
            {
                return Constants.CategorySpecial;
            }
        }

        public override string DisplayName
        {
            get
            {
                return Constants.FuncitonRnd;
            }
        }

        public override double FunctionToRun(double parameter)
        {
            int seed = (int)parameter;
            Random rnd = new Random(seed);
            return rnd.NextDouble();
        }
    }
}
