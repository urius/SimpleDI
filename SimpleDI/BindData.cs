using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleDI
{
    public class BindData
    {
        public static readonly BindData Instance = new BindData();

        private readonly Dictionary<string, string> _bindingNameByBindAsName = new Dictionary<string, string>();
        private readonly Dictionary<string, BindingInformationBase> _data = new Dictionary<string, BindingInformationBase>();

        public void SetBindingAlias(string bindingName, string bindingAlias)
        {
            _bindingNameByBindAsName[bindingAlias] = bindingName;
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
                var aliasesToUnbind = _bindingNameByBindAsName
                    .Where(kvp => kvp.Value == bindingName)
                    .Select(kvp => kvp.Key)
                    .ToArray();

                foreach (var alias in aliasesToUnbind)
                {
                    _bindingNameByBindAsName.Remove(alias);
                }

                _data.Remove(bindingName);
            }

            return containsBindingData;
        }

        public void Set(string bindingName, BindingInformationBase data)
        {
            _data[bindingName] = data;
        }

        public BindingInformationBase Get(string bindingName)
        {
            if (_data.TryGetValue(bindingName, out var result))
            {
                return result;
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