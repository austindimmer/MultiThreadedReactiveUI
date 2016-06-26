using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MultiThreadedReactiveUI
{
    static class DispatcherExtensions
    {
        /// <summary>
        /// A simple threading extension method, to invoke a delegate
        /// on the correct thread if it is not currently on the correct thread
        /// which can be used with DispatcherObject types.
        /// </summary>
        /// <param name="dispatcher">The Dispatcher object on which to 
        /// perform the Invoke</param>
        /// <param name="action">The delegate to run</param>
        public static void InvokeIfRequired(this Dispatcher dispatcher, Action action)
        {
            //dispatcher.BeginInvoke(DispatcherPriority.Background, action);

            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(DispatcherPriority.Normal, action);
            }
            else
            {
                action();
            }
        }

        public static void InvokeIfRequired(this Dispatcher dispatcher, Action action, ILifetimeScope container)
        {
            using (var scope = container.BeginLifetimeScope("Autofac"))
            {
                var registeredUIDispatcher = scope.Resolve<Dispatcher>();
                registeredUIDispatcher.InvokeIfRequired(action);
            }
        }
    }
}
