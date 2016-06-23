using System.Collections.Generic;

namespace MultiThreadedReactiveUI.DataProvider
{
    public interface IFunctionDataProvider
    {
        IEnumerable<Model.Function> LoadFunctions();
    }
}