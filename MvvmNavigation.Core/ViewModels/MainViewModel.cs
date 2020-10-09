using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace MvvmNavigation.ViewModels
{
    public class MainViewModel: ObservableObject
    {
        
        public event EventHandler ViewModelDone;
        public event EventHandler ViewModelAlsoDone;

        public string Title { get; } = "Main Page - VM";

        public async Task<int> DoSomething()
        {
            var rnd = new Random().Next(1000);
            await Task.Delay(rnd);
            if (rnd % 2 == 0)
            {
            }
            else
            {
            }
            ViewModelDone?.Invoke(this, EventArgs.Empty);
            return rnd;
        }

        public async Task<int> DoSomethingDifferent()
        {
            var rnd = new Random().Next(1000);
            await Task.Delay(rnd);
            if (rnd % 2 == 0)
            {
            }
            else
            {
            }
            ViewModelAlsoDone?.Invoke(this, EventArgs.Empty);
            return rnd;
        }
    }

   
}