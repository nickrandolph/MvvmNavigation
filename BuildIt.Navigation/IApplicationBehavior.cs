using System;
using System.Collections.Generic;
using System.Text;

namespace BuildIt.Navigation
{
    public interface IApplicationBehavior<TService>
    {
        void Attach(TService service, object entity);
        void Detach();
    }
}
