using Autofac;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MultiThreadedReactiveUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App app { get; set; } 


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            WireUpApplicationEvents();
            app = this;
            EnsureXamlResources(app);

            IoCContainer.Build();
            var mainWindow = IoCContainer.BaseContainer.Resolve<StocksWindow>();
            MainWindow = mainWindow;
            Application.Current.MainWindow = mainWindow;
            Application.Current.MainWindow.Show();
        }

        private void WireUpApplicationEvents()
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.Exception.Message);
        }


        private static void EnsureXamlResources(Application app)
        {

            var mahAppsControls = new ResourceDictionary();
            var mahAppsFonts = new ResourceDictionary();
            var mahAppsColors = new ResourceDictionary();
            var mahAppsAccent = new ResourceDictionary();
            var mahAppsAccentsBaseLight = new ResourceDictionary();
            var mahAppsIcons = new ResourceDictionary();

            mahAppsControls.Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsControls);

            mahAppsFonts.Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsFonts);

            mahAppsColors.Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Colors.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsColors);

            mahAppsAccent.Source = new Uri("pack://application:,,,/MultiThreadedReactiveUI;component/Resources/Styles/Accents/Black.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsAccent);

            //mahAppsAccent.Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Cobalt.xaml", UriKind.RelativeOrAbsolute);
            //app.Resources.MergedDictionaries.Add(mahAppsAccent);

            mahAppsAccentsBaseLight.Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseDark.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsAccentsBaseLight);


            mahAppsIcons.Source = new Uri("pack://application:,,,/MultiThreadedReactiveUI;component//Resources/Icons.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsIcons);

            var globalStylesDictionary = new ResourceDictionary();
            globalStylesDictionary.Source = new Uri("pack://application:,,,/MultiThreadedReactiveUI;component/Resources/Styles/GlobalStyles.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(globalStylesDictionary);

            var dataTemplatesDictionary = new ResourceDictionary();
            dataTemplatesDictionary.Source = new Uri("pack://application:,,,/MultiThreadedReactiveUI;component/Resources/DataTemplates.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(dataTemplatesDictionary);


        }
    }
}
