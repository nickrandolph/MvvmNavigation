using BuildIt.Navigation;
using MvvmNavigation.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MvvmNavigation
{
    [ViewModel(typeof(SecondViewModel))]

    public sealed partial class SecondPage : Page
    {
        public SecondViewModel ViewModel => DataContext as SecondViewModel;
        public SecondPage()
        {
            this.InitializeComponent();
        }

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            (DataContext as SecondViewModel).Done();
        }
    }
}
