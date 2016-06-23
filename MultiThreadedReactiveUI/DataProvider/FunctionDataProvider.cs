using System.Collections.Generic;
using MultiThreadedReactiveUI.DataProvider;

namespace MultiThreadedReactiveUI.DataProvider
{
    public class FunctionDataProvider : IFunctionDataProvider
    {
        public IEnumerable<Model.Function> LoadFunctions()
        {
            return new List<Model.Function>
            {
                new Model.Function {DisplayName = "Sin", Category = "Trig"},
                new Model.Function {DisplayName = "Cos", Category = "Trig"},
                new Model.Function {DisplayName = "Tan", Category = "Trig" },
                new Model.Function {DisplayName = "Ln", Category = "General"},
                new Model.Function {DisplayName = "Sort", Category = "General" },
                new Model.Function {DisplayName = "Rnd", Category = "Special"},
            };
        }
    }
}
