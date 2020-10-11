using BuildIt.Navigation;
using MvvmNavigation.ViewModels;
using Windows.UI.Xaml.Controls;


namespace MvvmNavigation
{
    [ViewModel(typeof(FourthViewModel))]
    public sealed partial class FourthPage : Page
    {
        public FourthPage()
        {
            this.InitializeComponent();
        }
    }
}
