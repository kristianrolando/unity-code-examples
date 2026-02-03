using System;
using System.Collections.Generic;

namespace Game.Core.ServiceLocator
{
    /// <summary>
    /// Global service locator for registering and retrieving service instances by type.
    /// Promotes decoupling by avoiding hard references between systems.
    /// </summary>
    public static class ServiceLocator
    {
        /// <summary>
        /// Stores registered service instances mapped to their respective types.
        /// </summary>
        private static readonly Dictionary<Type, object> _services = new();

        /// <summary>
        /// Registers a service instance to be globally accessible.
        /// If the service type already exists, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">Type of the service.</typeparam>
        /// <param name="service">Instance of the service.</param>
        /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the service is already registered.</exception>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var type = typeof(T);

            if (_services.ContainsKey(type))
                throw new InvalidOperationException($"Service of type {type} is already registered.");

            _services[type] = service;
        }

        /// <summary>
        /// Replaces an existing service, or registers a new one if not present.
        /// </summary>
        public static void Override<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Retrieves the registered service of the given type.
        /// </summary>
        /// <typeparam name="T">Type of the service.</typeparam>
        /// <returns>The service instance.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if service is not found.</exception>
        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return service as T;

            throw new KeyNotFoundException($"Service of type {typeof(T)} not found.");
        }

        /// <summary>
        /// Tries to retrieve a service without throwing an exception.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var instance))
            {
                service = instance as T;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Unregisters a previously registered service.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        /// <summary>
        /// Clears all registered services.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
