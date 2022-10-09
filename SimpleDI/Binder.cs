using System;

namespace SimpleDI
{
    public class Binder
    {
        public static bool ThrowIfBindOnExistingBinding = false;

        public static DefaultBindRegistration<T> Bind<T>()
            where T : new()
        {
            var bindName = TypeOf<T>.Name;

            var unbindResult = Unbind<T>();

            if (unbindResult && ThrowIfBindOnExistingBinding)
            {
                throw new InvalidOperationException($"Trying to Bind {bindName} when it was already binded (this exception can be skipped via {nameof(ThrowIfBindOnExistingBinding)})");
            }

            var bindInfo = new DefaultBindingInformation(() => new T());
            BindData.Instance.Set(bindName, bindInfo);

            return new DefaultBindRegistration<T>();
        }

        public static DefaultBindRegistration<T> Bind<T, TInterface>()
            where T : TInterface, new()
        {
            return Bind<T>().As<TInterface>();
        }

        public static bool Unbind<T>()
            where T : new()
        {
            return BindData.Instance.RemoveAllBindingsFor(TypeOf<T>.Name);
        }
    }

    public abstract class BindRegistrationBase<T>
    {
        protected readonly string BindingName;

        public BindRegistrationBase()
        {
            BindingName = TypeOf<T>.Name;
        }

        public BindRegistrationBase<T> As<TInterface>()
        {
            var bindingName = BindingName;
            var aliasName = TypeOf<TInterface>.Name;

            if (TypeOf<TInterface>.Type.IsAssignableFrom(TypeOf<T>.Type) == false)
            {
                throw new ArgumentException($"Can not bind {bindingName} as {aliasName} because of {aliasName} is not assignable from {bindingName}");
            }

            BindData.Instance.SetBindingAlias(BindingName, aliasName);

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
            var bindData = BindData.Instance;
            var bindInfo = bindData.Get(BindingName);
            bindData.Set(BindingName, new SingletonBindingInformation(() => new T()));

            return new SingletonBindRegistration<T>();
        }

        public void AsDefault()
        {
            var bindData = BindData.Instance;
            var bindInfo = bindData.Get(BindingName);
            bindData.Set(BindingName, new DefaultBindingInformation(() => new T()));
        }

        public void FromMethod(Func<T> method)
        {
            var bindData = BindData.Instance;
            var bindInfo = bindData.Get(BindingName);
            bindData.Set(BindingName, new DefaultBindingInformation(() => method()));
        }
    }

    public class SingletonBindRegistration<T> : BindRegistrationBase<T>
        where T : new()
    {
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
            var bindData = BindData.Instance;
            var bindInfo = bindData.Get(BindingName);
            bindData.Set(BindingName, new SingletonBindingInformation(() => instance));
        }

        public void FromMethod(Func<T> method)
        {
            var bindData = BindData.Instance;
            var bindInfo = bindData.Get(BindingName);
            bindData.Set(BindingName, new SingletonBindingInformation(() => method()));
        }
    }
}
