using System;
using System.Windows;

namespace MultiThreadedReactiveUI.DesignTimeData
{
    public class DesignFunction : Model.Function
    {
        public DesignFunction()
        {

        }

        public override string Category
        {
            get
            {
                return "Trig";
            }
        }

        public override string DisplayName
        {
            get
            {
                return "Sin";
            }

        }
    }
}
