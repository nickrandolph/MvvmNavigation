using System;
using System.Collections.Generic;
using System.Text;
using BuildIt.Navigation.Messages;

namespace BuildIt.Navigation
{
    public interface INavigationMessageRoutes
    {
        IList<Tuple<Type, Type, Action<object, INavigationMessage, INavigationService>>> Behaviors { get; } 

    }

    public class NavigationMessageRoutes: INavigationMessageRoutes
    {
        public IList<Tuple<Type, Type, Action<object, INavigationMessage, INavigationService>>> Behaviors { get; } = new List<Tuple<Type, Type, Action<object, INavigationMessage, INavigationService>>>();

        public NavigationMessageRoutes Register<TViewModel, TMessage>(
            Action<TViewModel, TMessage, INavigationService> actionFactory)
        {
            Behaviors.Add(
                new Tuple<Type, Type, Action<object, INavigationMessage, INavigationService>>(
                    typeof(TViewModel),
                    typeof(TMessage),
                    (viewmodel, message, nav) => actionFactory((TViewModel)viewmodel, (TMessage)message, nav)));
            return this;
        }

        public NavigationMessageRoutes RegisterNavigate<TViewModel, TMessage, TNewViewModel>()
        {
            return Register<TViewModel, TMessage>((vm, msg, nav) => nav.Navigate<TNewViewModel>());
        }
        public NavigationMessageRoutes RegisterGoBack<TViewModel, TMessage>()
        {
            return Register<TViewModel, TMessage>((vm, msg, nav) => nav.GoBack());
        }

        public NavigationMessageRoutes RegisterGoBack<TMessage>()
        {
            return Register<object, TMessage>((vm, msg, nav) => nav.GoBack());
        }
    }
}
