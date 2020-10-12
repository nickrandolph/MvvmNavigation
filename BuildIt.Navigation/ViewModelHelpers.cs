using System;

namespace BuildIt.Navigation
{
    public static class ViewModelHelpers
    {
        public static TViewModel ViewModel<TViewModel>(this object page, Func<TViewModel> dataContext, Action initDataContext)
        {
            var dc = dataContext();
            if (dc == null)
            {
                initDataContext();
                dc = dataContext();
            }
            return dc;
        }

    }
}
