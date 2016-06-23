using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Whitebox.Containers.Autofac;
using MultiThreadedReactiveUI.DataProvider;
using MultiThreadedReactiveUI.ViewModel;

namespace MultiThreadedReactiveUI
{
    public static class IoCContainer
    {
        public static IContainer BaseContainer { get; private set; }

        public static void Build()
        {
            if (BaseContainer == null)
            {
                var builder = new ContainerBuilder();
                builder.RegisterModule<WhiteboxProfilingModule>();
                builder.RegisterType<FunctionDataProvider>().As<IFunctionDataProvider>();
                builder.RegisterType<MainViewModel>();
                builder.RegisterType<MainWindow>();
                BaseContainer = builder.Build();
            }
        }

        public static TService Resolve<TService>()
        {
            return BaseContainer.Resolve<TService>();
        }
    }
}
