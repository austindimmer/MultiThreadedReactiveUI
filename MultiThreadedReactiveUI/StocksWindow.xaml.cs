using MahApps.Metro.Controls;
using MultiThreadedReactiveUI.Model;
using MultiThreadedReactiveUI.ViewModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MultiThreadedReactiveUI
{
    /// <summary>
    /// Interaction logic for StocksWindow.xaml
    /// </summary>
    public partial class StocksWindow : MetroWindow, IViewFor<IStocksViewModel>
    {
        public StocksWindow(StocksViewModel viewModel)
        {
            Contract.Requires(viewModel != null, "viewModel is null.");
            InitializeComponent();
            ViewModel = viewModel;
            this.DataContext = viewModel;
            //Setup two way binding with ViewModel
            this.Bind(ViewModel, x => x.SelectedStockToTrade, x => x.TradeSelectorList.SelectedItem);
            this.Bind(ViewModel, x => x.SelectedStockToTrade.TradeType, x => x.CurrentTradeTypeCombo.SelectedValue);
        }

        public IStocksViewModel ViewModel
        {
            get { return (IStocksViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IStocksViewModel), typeof(StocksWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IStocksViewModel)value; }
        }

        private void StocksSelectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            ViewModel.SelectedStocks.Clear();
            foreach (var selectedItem in lb.SelectedItems)
            {
                Stock stock = selectedItem as Stock;
                ViewModel.SelectedStocks.Add(stock);
            }

        }
    }
}
