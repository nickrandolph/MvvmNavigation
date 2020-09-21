using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using BuildIt.Navigation.Messages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BuildIt.Navigation
{
    

   

    public class WindowsNavigationEventService : INavigationService
    {
        private Frame NavigationFrame { get; }
        public IDictionary<Type, Type> ViewModelToPageMap { get; } = new Dictionary<Type, Type>();

        private INavigationEventService EventService { get; }


        public WindowsNavigationEventService(
            Frame navigationFrame,
            INavigationEventService eventService)
        {
            NavigationFrame = navigationFrame;
            NavigationFrame.Navigated += OnNavigated;
            EventService = eventService;
        }


        public async Task GoBack()
        {
            NavigationFrame.GoBack();
        }

        public async Task Navigate<TViewModel>()
        {
            NavigationFrame.Navigate(ViewModelToPageMap[typeof(TViewModel)]);
        }

        public WindowsNavigationEventService RegisterForNavigation<TPage, TViewModel>() where TPage : Page
        {
            ViewModelToPageMap[typeof(TViewModel)] = typeof(TPage);
            return this;
        }

        private object PreviousPage { get; set; }
        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (PreviousPage != null)
            {
                EventService.Unwire((PreviousPage as Page).DataContext);
                PreviousPage = null;
            }

            PreviousPage = e.Content;
            EventService.Wire(EventService.RaiseNavigationMessage, (PreviousPage as Page).DataContext);
        }
    }
}
