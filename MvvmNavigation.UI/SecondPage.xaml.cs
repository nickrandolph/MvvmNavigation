using BuildIt.Navigation;
using MvvmNavigation.ViewModels;
using Windows.UI.Xaml;

namespace MvvmNavigation
{
    [ViewModel(typeof(SecondViewModel), nameof(InitViewModel))]
    public sealed partial class SecondPage
    {
        partial void InitViewModel();
        public SecondViewModel ViewModel => this.ViewModel(() => DataContext as SecondViewModel, () => InitViewModel());

        public SecondPage()
        {
            InitializeComponent();
        }

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Done();
        }
    }
}
