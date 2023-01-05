using System;
using System.Collections.Generic;

namespace SimpleDI
{
    public interface IBinder
    {
        DefaultBindRegistration<T> Bind<T>() where T : new();
        DefaultBindRegistration<T> Bind<T, TInterface>() where T : TInterface, new();
        bool UnbindItem<T>();
        bool Unbind<T>() where T : new();
        bool HasBindingTo<T>();
        bool HasBinding<T>();
    }

    public class Binder : IBinder
    {
        private static readonly IBinder Instance = new Binder();

        private BindData _bindData = BindData.Instance;

        internal Binder()
        {

        }

        public static DefaultBindRegistration<T> Bind<T>()
            where T : new()
        {
            return Instance.Bind<T>();
        }

        public static DefaultBindRegistration<T> Rebind<T>()
            where T : new()
        {
            Unbind<T>();
            return Instance.Bind<T>();
        }

        public static DefaultBindRegistration<T> Bind<T, TInterface>()
            where T : TInterface, new()
        {
            return Instance.Bind<T, TInterface>();
        }

        public static bool UnbindItem<T>()
        {
            return Instance.UnbindItem<T>();
        }

        public static bool Unbind<T>()
            where T : new()
        {
            return Instance.Unbind<T>();
        }

        public static bool HasBindingTo<T>()
        {
            return Instance.HasBindingTo<T>();
        }

        public static bool HasBinding<T>()
        {
            return Instance.HasBinding<T>();
        }

        public static IDisposable OpenScope(Action<IBinder> scopeAction)
        {
            var result = new SubBinder();
            scopeAction(result);
            return result;
        }

        DefaultBindRegistration<T> IBinder.Bind<T>()
        {
            return BindInternal<T>();
        }

        DefaultBindRegistration<T> IBinder.Bind<T, TInterface>()            
        {
            return Bind<T>().As<TInterface>();
        }

        bool IBinder.UnbindItem<T>()            
        {
            if (HasBindingTo<T>())
            {
                return _bindData.RemoveBindingAlias(TypeOf<T>.Name);
            }
            else
            {
                return _bindData.RemoveAllBindingsFor(TypeOf<T>.Name);
            }
        }

        bool IBinder.HasBindingTo<T>()
        {
            return _bindData.HasBindingTo<T>();
        }

        bool IBinder.HasBinding<T>()
        {
            return _bindData.HasBinding<T>();
        }

        bool IBinder.Unbind<T>()
        {
            return _bindData.RemoveAllBindingsFor(TypeOf<T>.Name);
        }

        protected virtual DefaultBindRegistration<T> BindInternal<T>()
            where T : new()
        {
            var bindName = TypeOf<T>.Name;

            var hasBinding = _bindData.HasBinding<T>();

            if (hasBinding)
            {
                throw new InvalidOperationException($"Trying to Bind {bindName} when it was already binded");
            }

            var bindInfo = new DefaultBindingInformation(() => new T());
            _bindData.SetBindingInformation(bindName, bindInfo);

            return new DefaultBindRegistration<T>(_bindData);
        }
    }

    public class SubBinder : Binder, IDisposable
    {
        private readonly BindData _bindData = BindData.Instance;
        private readonly Queue<string> _bindNamesToRemove = new Queue<string>();

        public void Dispose()
        {
            foreach (var bindNameToRemove in _bindNamesToRemove)
            {
                _bindData.RemoveAllBindingsFor(bindNameToRemove);
            }

            _bindNamesToRemove.Clear();
        }

        protected override DefaultBindRegistration<T> BindInternal<T>()
        {
            var bindName = TypeOf<T>.Name;

            if (_bindData.HasBinding<T>())
            {
                throw new InvalidOperationException($"Trying to Bind {bindName} in {nameof(SubBinder)}, when it was already binded");
            }
            else
            {
                _bindNamesToRemove.Enqueue(bindName);
            }

            return base.BindInternal<T>();
        }
    }

    public abstract class BindRegistrationBase<T>
    {
        protected readonly string BindingName;
        internal readonly BindData BindData;

        internal BindRegistrationBase(BindData bindData)
        {
            BindingName = TypeOf<T>.Name;
            BindData = bindData;
        }

        public BindRegistrationBase<T> As<TInterface>()
        {
            var bindingName = BindingName;
            var aliasName = TypeOf<TInterface>.Name;

            if (TypeOf<TInterface>.Type.IsAssignableFrom(TypeOf<T>.Type) == false)
            {
                throw new ArgumentException($"Can't bind {bindingName} as {aliasName} because of {aliasName} is not assignable from {bindingName}");
            }
            else if (BindData.HasBindingTo<TInterface>())
            {
                throw new InvalidOperationException($"{aliasName} has already binded to {bindingName}. You may want to unbind it first");
            }

            BindData.AddBindingAlias(BindingName, aliasName);

            return this;
        }

        public BindRegistrationBase<T> AsSelf()
        {
            return As<T>();
        }
    }

    public class DefaultBindRegistration<T> : BindRegistrationBase<T>
        where T : new()
    {
        internal DefaultBindRegistration(BindData bindData) : base(bindData)
        {
        }

        public new DefaultBindRegistration<T> As<TInterface>()
        {
            return (DefaultBindRegistration<T>)base.As<TInterface>();
        }

        public new DefaultBindRegistration<T> AsSelf()
        {
            return (DefaultBindRegistration<T>)base.AsSelf();
        }

        public SingletonBindRegistration<T> AsSingleton()
        {
            var bindInfo = BindData.Get(BindingName);
            BindData.SetBindingInformation(BindingName, new SingletonBindingInformation(() => new T()));

            return new SingletonBindRegistration<T>(BindData);
        }

        public void AsDefault()
        {
            var bindInfo = BindData.Get(BindingName);
            BindData.SetBindingInformation(BindingName, new DefaultBindingInformation(() => new T()));
        }

        public void FromMethod(Func<T> method)
        {
            var bindInfo = BindData.Get(BindingName);
            BindData.SetBindingInformation(BindingName, new DefaultBindingInformation(() => method()));
        }
    }

    public class SingletonBindRegistration<T> : BindRegistrationBase<T>
        where T : new()
    {
        internal SingletonBindRegistration(BindData bindData) : base(bindData)
        {
        }

        public new SingletonBindRegistration<T> As<TInterface>()
        {
            return (SingletonBindRegistration<T>)base.As<TInterface>();
        }

        public new SingletonBindRegistration<T> AsSelf()
        {
            return (SingletonBindRegistration<T>)base.AsSelf();
        }

        public void FromInstance(T instance)
        {
            var bindInfo = BindData.Get(BindingName);
            BindData.SetBindingInformation(BindingName, new SingletonBindingInformation(() => instance));
        }

        public void FromMethod(Func<T> method)
        {
            var bindInfo = BindData.Get(BindingName);
            BindData.SetBindingInformation(BindingName, new SingletonBindingInformation(() => method()));
        }
    }
}
