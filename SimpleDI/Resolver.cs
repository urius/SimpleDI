using System;

namespace SimpleDI
{
    public class Resolver
    {
        private const int MAX_RECURSIVE_RESOLVES_COUNT = 1000;

        private static int _recursiveResolvesCounter = 0;

        public static T Resolve<T>()
        {
            var bindData = BindData.Instance;

            var bindingName = TypeOf<T>.Name;

            var bindingInfo = bindData.GetByAliasName(bindingName);

            if (bindingInfo == null)
            {
                throw new ArgumentException($"Can not resolve {bindingName} ( need to Bind it first via Binder.Bind<>().As<{bindingName}>() )");
            }

            if (_recursiveResolvesCounter > MAX_RECURSIVE_RESOLVES_COUNT)
            {
                throw new InvalidOperationException($"Too many recursive resolves detected (probably circular dependecy) while resolving {bindingName}");
            }

            _recursiveResolvesCounter++;

            T instance = GetInstance<T>(bindingInfo);

            _recursiveResolvesCounter--;

            return instance;
        }

        private static T GetInstance<T>(BindingInformationBase bindingInfo)
        {
            if (bindingInfo.BindingType == BindingType.Singletone)
            {
                return (T)(bindingInfo as SingletonBindingInformation).Instance;
            }

            return (T)bindingInfo.Factory();
        }
    }
}
