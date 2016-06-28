using System;
using System.ComponentModel;
using System.Windows;
using Autofac;
using MultiThreadedReactiveUI.DataProvider;
using MultiThreadedReactiveUI.DesignTimeData;

namespace MultiThreadedReactiveUI.ViewModel
{
    public class ViewModelLocator
    {
        private StocksViewModel _stocksViewModel;
        public StocksViewModel StocksViewModel
        {
            get
            {
                bool isDisplayedInDesigner = DesignerProperties.GetIsInDesignMode(new FrameworkElement());
                if (_stocksViewModel == null)
                {
                    if (isDisplayedInDesigner)
                    {
                        IStocksDataProvider dataProvider = (IStocksDataProvider)new DesignFunctionDataProvider();
                        _stocksViewModel = new StocksViewModel(dataProvider, null);
                    }
                }
                return _stocksViewModel;
            }
        }
    }
}
