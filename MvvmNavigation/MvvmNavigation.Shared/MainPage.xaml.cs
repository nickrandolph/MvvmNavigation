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
    }

    public class NavigationRoutes
    {
        private IList<Tuple<Type, IApplicationBehaviour>> Behaviours { get; } = new List<Tuple<Type, IApplicationBehaviour>>();

        public NavigationRoutes Register<T, THandler>(Action<T, THandler> init, Action<T, THandler> deinit, Func<INavigationService,THandler> actionFactory)
        {
            Behaviours.Add(new Tuple<Type, IApplicationBehaviour>(typeof(T), Behaviour<T,THandler>.Create(init, deinit, actionFactory)));
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

    public class Behaviour<T, THandler> :IApplicationBehaviour
    {
        private Action<T, THandler> Init { get; set; }
        private Action<T, THandler> Deinit { get; set; }

        private Func<INavigationService, THandler> ActionFactory { get; set; }

        private T attachedEntity;
        private THandler action;

        public void Attach(INavigationService navService, object entity)
        {
            if(attachedEntity!=null)
            {
                Detach();
            }

            action = ActionFactory(navService);
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

        public static Behaviour<T, THandler> Create(Action<T, THandler> init, Action<T, THandler> deinit,Func<INavigationService, THandler> actionFactory)
        {
            return new Behaviour<T, THandler>()
            {
                Init = init,
                Deinit = deinit,
                ActionFactory=actionFactory
            };
        }
    }

    public interface IApplicationBehaviour
    {
        void Attach(INavigationService navService, object entity);
        void Detach();
    }

    public interface INavigationService
    {
        Task Navigate<TViewModel>();
        Task GoBack();
    }
}
