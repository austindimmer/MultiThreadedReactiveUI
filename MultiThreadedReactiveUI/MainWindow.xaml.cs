using MahApps.Metro.Controls;
using MultiThreadedReactiveUI.ViewModel;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics.Contracts;
using MultiThreadedReactiveUI.Model;

namespace MultiThreadedReactiveUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainViewModel _MainViewModel { get; set; }
        public MainWindow(MainViewModel viewModel)
        {
            Contract.Requires(viewModel != null, "viewModel is null.");
            InitializeComponent();
            _MainViewModel = viewModel;
            this.DataContext = viewModel;
        }


        private void FunctionsSelectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            _MainViewModel.SelectedFunctions.Clear();
            foreach (var selectedItem in lb.SelectedItems)
            {
                Function func = selectedItem as Function;
                _MainViewModel.SelectedFunctions.Add(func);
            }
            
        }

        private void FunctionCategorySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            var selectedItem = combo.SelectedValue.ToString();
            _MainViewModel.SelectedCategory = selectedItem;
        }

        private void TasksSelectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            _MainViewModel.SelectedTasks.Clear();
            foreach (var selectedItem in lb.SelectedItems)
            {
                Function func = selectedItem as Function;
                _MainViewModel.SelectedTasks.Add(func);
            }
        }
    }
}
