using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MvvmNavigation.ViewModels;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using Microsoft.Xaml.Interactivity;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MvvmNavigation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    base.OnNavigatedTo(e);
        //    (DataContext as MainViewModel).ViewModelDone += MainViewModelDone;
        //}

        //protected override void OnNavigatedFrom(NavigationEventArgs e)
        //{
        //    (DataContext as MainViewModel).ViewModelDone -= MainViewModelDone;
        //    base.OnNavigatedFrom(e);
        //}

        //private void MainViewModelDone(object sender, EventArgs e)
        //{
        //    Frame.Navigate(typeof(SecondPage));
        //}

        private void GoToSecondPageClick(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel)?.DoSomething();
        }

        private void GoToThirdPageClick(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel)?.DoSomethingDifferent();
        }
    }

    public class NavigationMessageRoutes
    {
        public IList<Tuple<Type,Type, Action<object,INavigationMessage,INavigationService>>> Behaviours { get; } = new List<Tuple<Type, Type,Action<object,INavigationMessage,INavigationService>>>();

        public NavigationMessageRoutes Register<TViewModel,TMessage>(
            Action<TViewModel, TMessage, INavigationService> actionFactory)
        {
            Behaviours.Add(
                new Tuple<Type,Type, Action<object, INavigationMessage, INavigationService>>(
                    typeof(TViewModel),
                    typeof(TMessage), 
                    (viewmodel,message,nav)=>actionFactory((TViewModel)viewmodel,(TMessage)message,nav)));
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

    public class NavigationRoutes
    {
        private IList<Tuple<Type, IApplicationBehaviour<INavigationService>>> Behaviours { get; } = new List<Tuple<Type, IApplicationBehaviour<INavigationService>>>();

        public NavigationRoutes Register<T, THandler>(Action<T, THandler> init, Action<T, THandler> deinit, Func<INavigationService,THandler> actionFactory)
        {
            Behaviours.Add(new Tuple<Type, IApplicationBehaviour<INavigationService>>(typeof(T), Behaviour<INavigationService, T,THandler>.Create(init, deinit, actionFactory)));
            return this;
        }

        public void Wire(INavigationService navigationService, object page)
        {
            var typeOfPage = page.GetType();
            var behaviours = Behaviours.Where(x => x.Item1 == typeOfPage).Select(x=>x.Item2);
            foreach (var behaviour in behaviours)
            {
                behaviour.Attach(navigationService, page);
            }
        }

        public void Unwire(object page)
        {
            var typeOfPage = page.GetType();
            var behaviours = Behaviours.Where(x => x.Item1 == typeOfPage).Select(x => x.Item2);
            foreach (var behaviour in behaviours)
            {
                behaviour.Detach();
            }
        }
    }

    public class NavigationEvents
    {
        private IList<Tuple<Type, IApplicationBehaviour<ISubject<INavigationMessage>>>> Behaviours { get; } = new List<Tuple<Type, IApplicationBehaviour<ISubject<INavigationMessage>>>>();

        public NavigationEvents Register<T, THandler>(Action<T, THandler> init, Action<T, THandler> deinit, Func<ISubject<INavigationMessage>, THandler> actionFactory)
        {
            Behaviours.Add(
                new Tuple<Type, IApplicationBehaviour<ISubject<INavigationMessage>>>(
                    typeof(T), 
                    Behaviour<ISubject< INavigationMessage > ,T, THandler>.Create(init, deinit, actionFactory)));
            return this;
        }

        public void Wire(ISubject<INavigationMessage> navigationEvents, object page)
        {
            var typeOfPage = page.GetType();
            var behaviours = Behaviours.Where(x => x.Item1 == typeOfPage).Select(x => x.Item2);
            foreach (var behaviour in behaviours)
            {
                behaviour.Attach(navigationEvents, page);
            }
        }

        public void Unwire(object page)
        {
            var typeOfPage = page.GetType();
            var behaviours = Behaviours.Where(x => x.Item1 == typeOfPage).Select(x => x.Item2);
            foreach (var behaviour in behaviours)
            {
                behaviour.Detach();
            }
        }
    }

    public class Behaviour<TService, T, THandler> :IApplicationBehaviour<TService>
    {
        private Action<T, THandler> Init { get; set; }
        private Action<T, THandler> Deinit { get; set; }

        private Func<TService, THandler> ActionFactory { get; set; }

        private T attachedEntity;
        private THandler action;

        public void Attach(TService service, object entity)
        {
            if(attachedEntity!=null)
            {
                Detach();
            }

            action = ActionFactory(service);
            attachedEntity = (T)entity;
            Init(attachedEntity, action);
        }

        public void Detach()
        {
            if (attachedEntity == null)
            {
                return;
            }

            Deinit(attachedEntity, action);
        }

        private Behaviour() { }

        public static Behaviour<TService, T, THandler> Create(
            Action<T, THandler> init, 
            Action<T, THandler> deinit,
            Func<TService, THandler> actionFactory)
        {
            return new Behaviour<TService, T, THandler>()
            {
                Init = init,
                Deinit = deinit,
                ActionFactory=actionFactory
            };
        }
    }

    public interface IApplicationBehaviour<TService>
    {
        void Attach(TService service, object entity);
        void Detach();
    }

    public interface INavigationService
    {
        Task Navigate<TViewModel>();
        Task GoBack();
    }


    public sealed class NavigationMessageAction : DependencyObject, IAction
    {
        public Type MessageType { get; set; }

        public object MessageParameter { get; set; }


        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="sender">The <see cref="System.Object"/> that is passed to the action by the behavior. Generally this is <seealso cref="Microsoft.Xaml.Interactivity.IBehavior.AssociatedObject"/> or a target object.</param>
        /// <param name="parameter">The value of this parameter is determined by the caller.</param>
        /// <returns>True if updating the property value succeeds; else false.</returns>
        public object Execute(object sender, object parameter)
        {
            var messageSender = (sender as FrameworkElement).DataContext;

            var args = new List<object>();
            args.Add(messageSender);
            if (MessageParameter != null)
            {
                var constructor = MessageType.GetConstructors().FirstOrDefault();
                var paraType = constructor.GetParameters().Skip(1).FirstOrDefault();
                if (paraType != null)
                {
                    if(paraType.ParameterType!=MessageParameter.GetType())
                    {
                        if (paraType.ParameterType.IsEnum)
                        {
                            MessageParameter = Enum.Parse(paraType.ParameterType, MessageParameter + "");
                        }
                        else
                        {
                            MessageParameter = Convert.ChangeType(MessageParameter, paraType.ParameterType);
                        }
                    }
                    args.Add(MessageParameter);
                }
            }

            var message = Activator.CreateInstance(MessageType,args.ToArray()) as INavigationMessage;

            (Application.Current as App).EventService.RaiseNavigationMessage(message);

            return true;
        }


    }
}
