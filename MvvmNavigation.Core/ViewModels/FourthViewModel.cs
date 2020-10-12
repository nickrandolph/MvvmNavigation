using BuildIt.Navigation;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MvvmNavigation.ViewModels
{
    [Register]
    public class FourthViewModel : ObservableObject
    {
        public string Title { get; } = "Fourth Page - VM";
    }
}
