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
using ReactiveUI;

namespace MultiThreadedReactiveUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, IViewFor<IMainViewModel>
    {

        public IMainViewModel ViewModel
        {
            get { return (IMainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IMainViewModel), typeof(MainWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IMainViewModel)value; }
        }

        public MainWindow(MainViewModel viewModel)
        {
            Contract.Requires(viewModel != null, "viewModel is null.");
            InitializeComponent();
            ViewModel = viewModel;
            this.DataContext = viewModel;
            //Setup two way binding with ViewModel
            this.Bind(ViewModel, x => x.SelectedTask, x => x.TasksSelectorList.SelectedItem);
            this.Bind(ViewModel, x => x.SelectedCategory, x => x.FunctionCategorySelector.SelectedValue);

            this.WhenActivated(d =>
                {
                    d(
                        this.BindCommand(
                        this.ViewModel,
                        x => x.CategoryFilterSelected,
                        x => x.FunctionCategorySelector,
                        nameof(FunctionCategorySelector.SelectionChanged)));
                });

        }


        private void FunctionsSelectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            ViewModel.SelectedFunctions.Clear();
            foreach (var selectedItem in lb.SelectedItems)
            {
                Function func = selectedItem as Function;
                ViewModel.SelectedFunctions.Add(func);
            }

        }

        //private void FunctionCategorySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    ComboBox combo = sender as ComboBox;
        //    var selectedItem = combo.SelectedValue.ToString();
        //    ViewModel.SelectedCategory = selectedItem;
        //}

    }
}
