using BuildIt.Navigation;
using MvvmNavigation.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MvvmNavigation
{
    [ViewModel(typeof(MainViewModel))]
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void GoToSecondPageClick(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel)?.DoSomething();
        }

        private void GoToThirdPageClick(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel)?.DoSomethingDifferent();
        }
    }

  

   

   

 


   
}
