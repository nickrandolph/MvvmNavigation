using BuildIt.Navigation;
using MvvmNavigation.ViewModels;

namespace MvvmNavigation
{
    [ViewModel(typeof(FourthViewModel), nameof(InitViewModel))]
    public sealed partial class FourthPage
    {
        partial void InitViewModel();
        public FourthViewModel ViewModel => this.ViewModel(() => DataContext as FourthViewModel, () => InitViewModel());

        public FourthPage()
        {
            InitializeComponent();
        }
    }
}
