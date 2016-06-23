using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

        public App()
        {
            app = this;
            EnsureXamlResources(app);
            InitializeComponent();
        }

     

        private static void EnsureXamlResources(Application app)
        {

            var globalStylesDictionary = new ResourceDictionary();
            var dataTemplatesDictionary = new ResourceDictionary();



            globalStylesDictionary.Source = new Uri("pack://application:,,,/MultiThreadedReactiveUI;component/Resources/Styles/GlobalStyles.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(globalStylesDictionary);

            dataTemplatesDictionary.Source = new Uri("pack://application:,,,/MultiThreadedReactiveUI;component/Resources/DataTemplates.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(dataTemplatesDictionary);

            var mahAppsControls = new ResourceDictionary();
            var mahAppsFonts = new ResourceDictionary();
            var mahAppsColors = new ResourceDictionary();
            var mahAppsAccentsBlack = new ResourceDictionary();
            var mahAppsAccentsBaseLight = new ResourceDictionary();
            var mahAppsIcons = new ResourceDictionary();



            mahAppsControls.Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsControls);

            mahAppsFonts.Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsFonts);

            mahAppsColors.Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Colors.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsColors);

            mahAppsAccentsBlack.Source = new Uri("pack://application:,,,/MultiThreadedReactiveUI;component/Resources/Styles/Accents/Black.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsAccentsBlack);

            mahAppsAccentsBaseLight.Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseDark.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsAccentsBaseLight);


            mahAppsIcons.Source = new Uri("pack://application:,,,/MultiThreadedReactiveUI;component//Resources/Icons.xaml", UriKind.RelativeOrAbsolute);
            app.Resources.MergedDictionaries.Add(mahAppsIcons);


        }
    }
}
