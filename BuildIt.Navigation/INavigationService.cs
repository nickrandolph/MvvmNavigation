using System.Threading.Tasks;

namespace BuildIt.Navigation
{
    public interface INavigationService
    {
        Task Navigate<TViewModel>();
        Task GoBack();
    }
}
