using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
//using System.Web.Mvc;

namespace PermissionManagement.IoC
{
    public class Container : IContainer
    {
        private static HybridDictionary typeRegistry = new HybridDictionary();

        // Track whether Dispose has been called.
        private bool disposed;

        // null for the lifetime manager is the same as AlwaysNew, but slightly faster.
        private ILifetimeManager defaultLifetimeManager = null;

        public ILifetimeManager LifeTimeManager { get { return defaultLifetimeManager; } }

        //#region constructor

        //public Container()
        //{
        //    //this.Configure();
        //}

        //#endregion

        //#region Register Types in Web.config

        //private void Configure()
        //{
        //    var munqConfigure = MunqConfigurationReader.GetCurrent().RegisteredTypeList;

        //    foreach (var t in munqConfigure)
        //    {
        //        var registeredType = GetMapTo(t.HeaderInfo.MapTo);
        //        this.Register(t.HeaderInfo.MapTo, registeredType, c => Activator.CreateInstance(registeredType, GetParameters(registeredType, t.ParameterList)));
        //    }
        //}

        //private Type GetMapTo(string mapTo)
        //{
        //    var ty = Type.GetType(mapTo);
        //    return ty;
        //}

        //private string GetAssemblyName(string mapTo)
        //{
        //    var assemblyName = mapTo.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
        //    return assemblyName;
        //}

        //private object[] GetParameters(Type type, IList<MunqRegisteredTypeParameter> parameterList)
        //{
        //    object[] list = null;
        //    var c = type.GetConstructors()[0];
        //    if (c.GetParameters().Length == 0) { return null; }
        //    return list;
        //}

        //#endregion

        #region Register Members
        /// <summary>
        /// Registers a function to create instances of a type
        /// </summary>
        /// <typeparam name="TType">The type being registered</typeparam>
        /// <param name="func">The function that creates the type.  
        /// The function takes a single parameter of type Container</param>
        /// <returns>An IRegistration that can be used to configure the behavior of the registration</returns>
        public IRegistration Register<TType>(Func<IContainer, TType> func) where TType : class
        {
            if (func == null)
                throw new ArgumentNullException("func", "func is null.");
            return Register(typeof(TType), c => (func(c) as Object));
        }

        public IRegistration Register<TType>(string name, Func<IContainer, TType> func) where TType : class
        {
            if (func == null)
                throw new ArgumentNullException("func", "func is null.");
            return Register(name, typeof(TType), c => (func(c) as Object));
        }

        public IRegistration Register(Type type, Func<IContainer, object> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            var entry = new Registration(this, null, type, func);
            entry.WithLifetimeManager(defaultLifetimeManager);

            typeRegistry[new UnNamedRegistrationKey(type)] = entry;

            return entry;
        }

        public IRegistration Register(string name, Type type, Func<IContainer, object> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            var entry = new Registration(this, name ?? String.Empty, type, func);
            entry.WithLifetimeManager(defaultLifetimeManager);

            typeRegistry[new NamedRegistrationKey(name, type)] = entry;

            return entry;
        }

        #endregion

        #region RegisterInstance Members
        public IRegistration RegisterInstance<TType>(TType instance) where TType : class
        {
            return Register<TType>(c => instance);
        }

        public IRegistration RegisterInstance<TType>(string name, TType instance) where TType : class
        {
            return Register<TType>(name, c => instance);
        }

        public IRegistration RegisterInstance(Type type, object instance)
        {
            return Register(type, c => instance);
        }

        public IRegistration RegisterInstance(string name, Type type, object instance)
        {
            return Register(name, type, c => instance);
        }
        #endregion

        public void Remove(IRegistration ireg)
        {
            object key;
            if (ireg.Name == null)
                key = new UnNamedRegistrationKey(ireg.ResolvesTo);
            else
                key = new NamedRegistrationKey(ireg.Name, ireg.ResolvesTo);

            typeRegistry.Remove(key);

            ireg.InvalidateInstanceCache();
        }

        #region static Resolvers

        public static TType ResolveShared<TType>() where TType : class
        {
            return ResolveShared(typeof(TType)) as TType;
        }

        public static TType ResolveShared<TType>(string name) where TType : class
        {
            return ResolveShared(name, typeof(TType)) as TType;
        }

        public static object ResolveShared(Type type)
        {
            var entry = typeRegistry[new UnNamedRegistrationKey(type)] as Registration;

            if (entry != null)
                return entry.GetInstance();
            else
                throw new KeyNotFoundException();
        }

        public static object ResolveShared(string name, Type type)
        {
            var entry = typeRegistry[new NamedRegistrationKey(name, type)] as Registration;

            if (entry != null)
                return entry.GetInstance();
            else
                throw new KeyNotFoundException();
        }

        #endregion

        #region Resolve Members
        /// <summary>
        /// Returns an instance of a registered type
        /// </summary>
        /// <typeparam name="TType">The type to resolve</typeparam>
        /// <returns>An instance of the type.  Throws a KeyNoFoundException if not registered.</returns>
        public TType Resolve<TType>() where TType : class
        {
            return Resolve(typeof(TType)) as TType;
        }

        public TType Resolve<TType>(string name) where TType : class
        {
            return Resolve(name, typeof(TType)) as TType;
        }

        public object Resolve(Type type)
        {
            return ResolveShared(type);
        }

        public object Resolve(string name, Type type)
        {
            return ResolveShared(name, type);
        }

        #endregion

        #region LazyResolve Members
        //--------------------------------------------------------
        // Lazy Resolve methods returns a delegate that, when called
        // resolves the instance.  Used in case where you wish to delay
        // the actual instantiation of the class as it may use a scarce 
        // resource, or logic may not need to resolve it at all.
        public Func<TType> LazyResolve<TType>() where TType : class
        {
            Func<TType> lazyMethod;
            try
            {
                lazyMethod = Resolve<Func<TType>>();
            }
            catch (KeyNotFoundException)
            {
                lazyMethod = () => this.Resolve<TType>();
                RegisterInstance<Func<TType>>(lazyMethod);
            }
            return lazyMethod;
        }

        public Func<TType> LazyResolve<TType>(string name) where TType : class
        {
            Func<TType> lazyMethod;
            try
            {
                lazyMethod = Resolve<Func<TType>>(name);
            }
            catch (KeyNotFoundException)
            {
                lazyMethod = () => this.Resolve<TType>(name);
                RegisterInstance<Func<TType>>(lazyMethod);
            }
            return lazyMethod;
        }

        public Func<Object> LazyResolve(Type type)
        {
            var entry = typeRegistry[new UnNamedRegistrationKey(type)] as Registration;

            if (entry != null)
                return entry.LazyFactory;
            else
                throw new KeyNotFoundException();
        }

        public Func<Object> LazyResolve(string name, Type type)
        {
            var entry = typeRegistry[new NamedRegistrationKey(name, type)] as Registration;

            if (entry != null)
                return entry.LazyFactory;
            else
                throw new KeyNotFoundException();
        }
        #endregion

        #region GetRegistration Members
        /// <summary>
        /// Returns an Registration of a registered type
        /// </summary>
        /// <typeparam name="TType">The type to get the Registration for</typeparam>
        /// <returns>An Registration for the type.  Throws a KeyNoFoundException if not registered.</returns>
        public IRegistration GetRegistration<TType>() where TType : class
        {
            return GetRegistration(typeof(TType));
        }

        public IRegistration GetRegistration<TType>(string name) where TType : class
        {
            return GetRegistration(name, typeof(TType));
        }

        public IRegistration GetRegistration(Type type)
        {
            var reg = typeRegistry[new UnNamedRegistrationKey(type)] as IRegistration;
            if (reg == null)
                throw new KeyNotFoundException();

            return reg;
        }

        public IRegistration GetRegistration(string name, Type type)
        {
            var reg = typeRegistry[new NamedRegistrationKey(name, type)] as IRegistration;
            if (reg == null)
                throw new KeyNotFoundException();

            return reg;
        }

        public List<IRegistration> GetRegistrations<TType>() where TType : class
        {
            return GetRegistrations(typeof(TType));
        }

        public List<IRegistration> GetRegistrations(Type type)
        {
            List<IRegistration> registrations = new List<IRegistration>();
            foreach (IRegistrationKey key in typeRegistry.Keys)
            {
                if (key.GetInstanceType() == type)
                    registrations.Add((IRegistration)typeRegistry[key]);
            }
            return registrations;
        }

        #endregion

        #region Fluent Interface Members
        public IContainer UsesDefaultLifetimeManagerOf(ILifetimeManager lifetimeManager)
        {
            defaultLifetimeManager = lifetimeManager;
            return this;
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all ContainerLifetime instances
                if (disposing)
                {
                    foreach (Registration reg in typeRegistry.Values)
                    {
                        var instance = reg.Instance as IDisposable;
                        if (instance != null)
                        {
                            instance.Dispose();
                            reg.Instance = null;
                        }
                    }
                }
            }
            disposed = true;
        }

        ~Container() { Dispose(false); }

        #endregion
    }

}
