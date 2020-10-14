using BuildIt.Navigation;
using MvvmNavigation.ViewModels;

namespace MvvmNavigation
{
    [ViewModel(typeof(FifthViewModel), nameof(InitViewModel))]
    public sealed partial class FifthPage 
    {
        partial void InitViewModel();
        public FifthViewModel ViewModel => this.ViewModel(() => DataContext as FifthViewModel, () => InitViewModel());

        public FifthPage()
        {
            this.InitializeComponent();
        }
    }
}
