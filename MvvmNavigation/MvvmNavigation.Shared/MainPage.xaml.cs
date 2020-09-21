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
using BuildIt.Navigation;
using BuildIt.Navigation.Messages;

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

        private void GoToSecondPageClick(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel)?.DoSomething();
        }

        private void GoToThirdPageClick(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel)?.DoSomethingDifferent();
        }
    }

  

   

   

 


   
}
