using System.Collections.Generic;
using MultiThreadedReactiveUI.DataProvider;
using MultiThreadedReactiveUI.Model;

namespace MultiThreadedReactiveUI.DataProvider
{
    public class FunctionDataProvider : IFunctionDataProvider
    {
        public IEnumerable<Model.Function> LoadFunctions()
        {
            List<Function> functionsToReturn = new List<Function>();
            functionsToReturn.Add(new SinFunction());
            functionsToReturn.Add(new CosFunction());
            functionsToReturn.Add(new TanFunction());
            functionsToReturn.Add(new RndFunction());
            functionsToReturn.Add(new SqrtFunction());
            functionsToReturn.Add(new LnFunction());
            return functionsToReturn;

        }
    }
}
