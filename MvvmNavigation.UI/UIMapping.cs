using BuildIt.Navigation;

namespace MvvmNavigation.UI
{
    [ViewModelMappingRegister(nameof(RegistarViewModelMappings))]

    public partial class UIMapping
    {
        partial void RegistarViewModelMappings(WindowsViewModelToViewMapping mappings);

        public static void RegisterMappings(WindowsViewModelToViewMapping mappings)
        {
            (new UIMapping()).RegistarViewModelMappings(mappings);
        }
    }
}
