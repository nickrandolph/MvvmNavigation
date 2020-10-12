using BuildIt.Navigation;
using MvvmNavigation.ViewModels;
using Windows.UI.Xaml;

namespace MvvmNavigation
{
    [ViewModel(typeof(MainViewModel), nameof(InitViewModel))]
    public sealed partial class MainPage
    {
        partial void InitViewModel();
        public MainViewModel ViewModel => this.ViewModel(() => DataContext as MainViewModel, () => InitViewModel());

        public MainPage()
        {
            InitializeComponent();
        }

        private async void GoToSecondPageClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.DoSomething();
        }

        private async void GoToThirdPageClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.DoSomethingDifferent();
        }
    }

  

   

   

 


   
}
