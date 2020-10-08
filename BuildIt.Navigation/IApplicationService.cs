using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BuildIt.Navigation
{
    public class ApplicationService : IApplicationService
    {
        private IServiceCollection ServiceRegistrations { get; } = new ServiceCollection();

        private readonly SemaphoreSlim servicesLock = new SemaphoreSlim(1);

        public ApplicationService()
        {
            InitializeApplicationServices(ServiceRegistrations);
        }

        private IServiceProvider services;
        public IServiceProvider Services
        {
            get
            {
                servicesLock.Wait();
                try
                {
                    if (services == null)
                    {
                        services = ServiceRegistrations.BuildServiceProvider();
                    }

                    return services;
                }
                finally
                {
                    servicesLock.Release();
                }
            }
        }

        public async Task ConfigureServices(Action<IServiceCollection> setup)
        {
            await servicesLock.WaitAsync().ConfigureAwait(false);
            try
            {
                services = null;
                setup(ServiceRegistrations);
            }
            finally
            {
                servicesLock.Release();
            }
        }

        protected virtual void InitializeApplicationServices(IServiceCollection serviceRegistration)
        {

        }

    }


    public interface IApplicationService
    {
        IServiceProvider Services { get; }

        Task ConfigureServices(Action<IServiceCollection> setup);
    }
}
