﻿using BuildIt.Navigation;
using MvvmNavigation.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MvvmNavigation
{
    [ViewModel(typeof(ThirdViewModel), nameof(InitViewModel))]
    public sealed partial class ThirdPage : Page
    {
        partial void InitViewModel();
        public ThirdViewModel ViewModel => this.ViewModel(() => DataContext as ThirdViewModel, () => InitViewModel());

        public ThirdPage()
        {
            InitializeComponent();
        }

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Done();
        }
    }
}
