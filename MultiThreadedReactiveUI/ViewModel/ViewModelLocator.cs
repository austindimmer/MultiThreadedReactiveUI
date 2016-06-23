using System;
using System.ComponentModel;
using System.Windows;
using Autofac;
using MultiThreadedReactiveUI.DataProvider;
using MultiThreadedReactiveUI.DesignTimeData;
using MultiThreadedReactiveUI.ViewModel;

namespace MultiThreadedReactiveUI.ViewModel
{
    public class ViewModelLocator
    {
        private MainViewModel _mainViewModel;
        public MainViewModel MainViewModel
        {
            get
            {
                bool isDisplayedInDesigner = DesignerProperties.GetIsInDesignMode(new FrameworkElement());
                if (_mainViewModel == null)
                {
                    if (isDisplayedInDesigner)
                    {
                        IFunctionDataProvider dataProvider = (IFunctionDataProvider)new DesignFunctionDataProvider();
                        _mainViewModel = new MainViewModel(dataProvider);
                    }

                }
                return _mainViewModel;
            }
        }
    }
}
