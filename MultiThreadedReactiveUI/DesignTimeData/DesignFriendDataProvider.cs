using System.Collections.Generic;
using MultiThreadedReactiveUI.DataProvider;

namespace MultiThreadedReactiveUI.DesignTimeData
{
    class DesignFunctionDataProvider : IFunctionDataProvider
    {
        public IEnumerable<Model.Function> LoadFunctions()
        {
            yield return new DesignFunction();
            yield return new MultiThreadedReactiveUI.DesignTimeData.DesignFunction { DisplayName = "Sin", Category="Trig" };
            yield return new MultiThreadedReactiveUI.DesignTimeData.DesignFunction { DisplayName = "Cos", Category = "Trig" };
            yield return new MultiThreadedReactiveUI.DesignTimeData.DesignFunction { DisplayName = "Tan", Category = "Trig" };
            yield return new MultiThreadedReactiveUI.DesignTimeData.DesignFunction { DisplayName = "Ln", Category = "General" };
            yield return new MultiThreadedReactiveUI.DesignTimeData.DesignFunction { DisplayName = "Sort", Category = "General" };
            yield return new MultiThreadedReactiveUI.DesignTimeData.DesignFunction { DisplayName = "Rnd", Category = "General" };
        }
    }
}
