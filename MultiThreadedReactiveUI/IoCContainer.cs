﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Whitebox.Containers.Autofac;
using MultiThreadedReactiveUI.DataProvider;
using MultiThreadedReactiveUI.ViewModel;
using System.Windows.Threading;
using System.Windows;

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
                ConfigureContainer();
            }
        }

        private static void ConfigureContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(BaseContainer)
                .As<IContainer>()
                .SingleInstance();

            /* By registering the UI thread dispatcher 
* we are able to invoke controls from anywhere. */

            var dispatcher = Application.Current.Dispatcher;
            var dispatcher2 = Dispatcher.CurrentDispatcher;
            builder.RegisterInstance(dispatcher)
                .As<Dispatcher>()
                .SingleInstance();

            builder.Update(BaseContainer);
        }

        public static TService Resolve<TService>()
        {
            return BaseContainer.Resolve<TService>();
        }

    }
}
