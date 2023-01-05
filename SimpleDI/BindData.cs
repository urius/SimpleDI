using System;
using System.Collections.Generic;

namespace SimpleDI
{
    internal class BindData
    {
        private class SingleBindingData
        {
            public LinkedList<string> Aliases = new LinkedList<string>();
            public BindingInformationBase BindingInformation;
        }

        public static readonly BindData Instance = new BindData();

        private readonly Dictionary<string, string> _bindingNameByBindAsName = new Dictionary<string, string>();
        private readonly Dictionary<string, SingleBindingData> _data = new Dictionary<string, SingleBindingData>();

        private BindData()
        {

        }

        public void AddBindingAlias(string bindingName, string bindingAlias)
        {
            if (_bindingNameByBindAsName.ContainsKey(bindingAlias) == false)
            {
                _bindingNameByBindAsName[bindingAlias] = bindingName;
                _data[bindingName].Aliases.AddLast(bindingAlias);
            }
            else
            {
                throw new InvalidOperationException($"{nameof(AddBindingAlias)}: Can't set binding alias {bindingAlias} for {bindingName} because it's already binded");
            }
        }

        public bool RemoveBindingAlias(string bindingAlias)
        {
            if (_bindingNameByBindAsName.TryGetValue(bindingAlias, out var bindingName))
            {
                var singleBindingData = _data[bindingName];
                singleBindingData.Aliases.Remove(bindingAlias);
                _bindingNameByBindAsName.Remove(bindingAlias);

                if (singleBindingData.Aliases.Count <= 0)
                {
                    _data.Remove(bindingName);
                }
            }

            return bindingName != null;
        }

        public string GetBindingNameByAliasName(string bindingAliasName)
        {
            if (_bindingNameByBindAsName.TryGetValue(bindingAliasName, out var bindingName))
            {
                return bindingName;
            }

            return null;
        }

        public bool RemoveAllBindingsFor(string bindingName)
        {
            var containsBindingData = _data.ContainsKey(bindingName);

            if (containsBindingData)
            {
                var aliasesToUnbind = _data[bindingName].Aliases;

                foreach (var alias in aliasesToUnbind)
                {
                    _bindingNameByBindAsName.Remove(alias);
                }

                _data.Remove(bindingName);
            }

            return containsBindingData;
        }

        public void SetBindingInformation(string bindingName, BindingInformationBase data)
        {
            if (_data.ContainsKey(bindingName) == false)
            {
                _data[bindingName] = new SingleBindingData();
            }

            _data[bindingName].BindingInformation = data;
        }

        public BindingInformationBase Get(string bindingName)
        {
            if (_data.TryGetValue(bindingName, out var result))
            {
                return result.BindingInformation;
            }

            return null;
        }

        public BindingInformationBase GetByAliasName(string bindingAliasName)
        {
            var bindingName = GetBindingNameByAliasName(bindingAliasName);
            if (bindingName != null)
            {
                return Get(bindingName);
            }

            return null;
        }

        public bool HasBinding<T>()
        {
            return _data.ContainsKey(TypeOf<T>.Name);
        }

        public bool HasBindingTo<T>()
        {
            return _bindingNameByBindAsName.ContainsKey(TypeOf<T>.Name);
        }
    }

    public abstract class BindingInformationBase
    {
        public abstract BindingType BindingType { get; }

        public readonly Func<object> Factory;

        public BindingInformationBase(Func<object> factory)
        {
            Factory = factory;
        }
    }

    public class DefaultBindingInformation : BindingInformationBase
    {
        public override BindingType BindingType => BindingType.Default;

        public DefaultBindingInformation(Func<object> factory)
            : base(factory)
        {
        }
    }

    public class SingletonBindingInformation : BindingInformationBase
    {
        public override BindingType BindingType => BindingType.Singletone;

        private object _instance;

        public SingletonBindingInformation(Func<object> factory)
            : base(factory)
        {
        }

        public object Instance => _instance ??= Factory();
    }

    public enum BindingType
    {
        Default,
        Singletone,
    }
}