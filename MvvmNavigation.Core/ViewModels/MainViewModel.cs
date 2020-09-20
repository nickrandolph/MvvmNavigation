using System;
using System.Threading.Tasks;

namespace MvvmNavigation.ViewModels
{
    public class MainViewModel
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

    public class SecondViewModel
    {
        public event EventHandler ViewModelDone;
        public string Title { get; } = "Second Page - VM";

        public void Done()
        {
            ViewModelDone?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ThirdViewModel
    {
        public event EventHandler ViewModelDone;
        public string Title { get; } = "Third Page - VM";

        public void Done()
        {
            ViewModelDone?.Invoke(this, EventArgs.Empty);
        }
    }
}