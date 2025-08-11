using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace KindergartenSystem.Infrastructure
{
    public class SimpleContainer : IDependencyResolver
    {
        private readonly Dictionary<Type, Func<object>> _services = new Dictionary<Type, Func<object>>();
        private readonly IDependencyResolver _defaultResolver;

        public SimpleContainer(IDependencyResolver defaultResolver = null)
        {
            _defaultResolver = defaultResolver;
        }

        public void Register<TInterface, TImplementation>() 
            where TImplementation : class, TInterface, new()
        {
            _services[typeof(TInterface)] = () => new TImplementation();
        }

        public void Register<TInterface>(Func<TInterface> factory)
        {
            _services[typeof(TInterface)] = () => factory();
        }

        public object GetService(Type serviceType)
        {
            if (_services.ContainsKey(serviceType))
            {
                return _services[serviceType]();
            }

            return _defaultResolver?.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var service = GetService(serviceType);
            if (service != null)
            {
                yield return service;
            }

            var defaultServices = _defaultResolver?.GetServices(serviceType);
            if (defaultServices != null)
            {
                foreach (var defaultService in defaultServices)
                {
                    yield return defaultService;
                }
            }
        }
    }
}