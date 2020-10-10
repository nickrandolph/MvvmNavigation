using BuildIt.Navigation;
using MvvmNavigation.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MvvmNavigation
{
    [ViewModel(typeof(ThirdViewModel))]
    public sealed partial class ThirdPage : Page
    {
        public ThirdPage()
        {
            this.InitializeComponent();
        }

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            (DataContext as ThirdViewModel).Done();
        }
    }
}
