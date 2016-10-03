using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VirtoCommerce.Storefront.Model.Common
{
    /// <summary>
    /// Abstract static type factory. With support of type overriding and sets special factories.
    /// </summary>
    /// <typeparam name="TBaseType"></typeparam>
    public static class AbstractTypeFactory<TBaseType>
    {
        private static readonly List<TypeInfo<TBaseType>> _typeInfos = new List<TypeInfo<TBaseType>>();

        /// <summary>
        /// All registered type mapping informations within current factory instance
        /// </summary>
        public static IEnumerable<TypeInfo<TBaseType>> AllTypeInfos
        {
            get
            {
                return _typeInfos;
            }
        }

        /// <summary>
        /// Register new  type (fluent method)
        /// </summary>
        /// <returns>TypeInfo instance to continue configuration through fluent syntax</returns>
        public static TypeInfo<TBaseType> RegisterType<T>() where T : TBaseType
        {
            var kowTypes = _typeInfos.SelectMany(x => x.AllSubclasses);
            if (kowTypes.Contains(typeof(T)))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type already registered: {0}", typeof(T).Name));
            }
            var retVal = new TypeInfo<TBaseType>(typeof(T));
            _typeInfos.Add(retVal);
            return retVal;
        }

        /// <summary>
        /// Override already registered  type to new 
        /// </summary>
        /// <returns>TypeInfo instance to continue configuration through fluent syntax</returns>
        public static TypeInfo<TBaseType> OverrideType<TOldType, TNewType>() where TNewType : TBaseType
        {
            var oldType = typeof(TOldType);
            var newType = typeof(TNewType);
            var existTypeInfo = _typeInfos.FirstOrDefault(x => x.Type == oldType);
            var newTypeInfo = new TypeInfo<TBaseType>(newType);
            if (existTypeInfo != null)
            {
                _typeInfos.Remove(existTypeInfo);
            }

            _typeInfos.Add(newTypeInfo);
            return newTypeInfo;
        }

        /// <summary>
        /// Create BaseType instance considering type mapping information
        /// </summary>
        /// <returns></returns>
        public static TBaseType TryCreateInstance()
        {
            return TryCreateInstance(typeof(TBaseType).Name);
        }

        /// <summary>
        /// Create derived from BaseType  specified type instance considering type mapping information
        /// </summary>
        /// <returns></returns>
        public static T TryCreateInstance<T>() where T : TBaseType
        {
            return (T)TryCreateInstance(typeof(T).Name);
        }

        public static TBaseType TryCreateInstance(string typeName)
        {
            TBaseType retVal;

            //Try find first direct type match from registered types
            var typeInfo = _typeInfos.FirstOrDefault(x => x.Type.Name.EqualsInvariant(typeName));

            //Then need to find in inheritance chain from registered types
            if (typeInfo == null)
            {
                typeInfo = _typeInfos.FirstOrDefault(x => x.IsAssignableTo(typeName));
            }

            if (typeInfo != null)
            {
                if (typeInfo.Factory != null)
                {
                    retVal = typeInfo.Factory();
                }
                else
                {
                    retVal = (TBaseType)Activator.CreateInstance(typeInfo.Type);
                }
            }
            else
            {
                retVal = (TBaseType)Activator.CreateInstance(typeof(TBaseType));
            }

            return retVal;
        }
    }

    /// <summary>
    /// Helper class contains  type mapping information
    /// </summary>
    public class TypeInfo<TBaseType>
    {
        public TypeInfo(Type type)
        {
            Services = new List<object>();
            Type = type;
        }

        public Func<TBaseType> Factory { get; private set; }
        public Type Type { get; private set; }
        public Type MappedType { get; set; }
        public ICollection<object> Services { get; set; }

        public T GetService<T>()
        {
            return Services.OfType<T>().FirstOrDefault();
        }

        public TypeInfo<TBaseType> WithService<T>(T service)
        {
            if (!Services.Contains(service))
            {
                Services.Add(service);
            }
            return this;
        }

        public TypeInfo<TBaseType> MapToType<T>()
        {
            MappedType = typeof(T);
            return this;
        }

        public TypeInfo<TBaseType> WithFactory(Func<TBaseType> factory)
        {
            Factory = factory;
            return this;
        }

        public bool IsAssignableTo(string typeName)
        {
            return Type.GetTypeInheritanceChainTo(typeof(TBaseType)).Concat(new[] { typeof(TBaseType) }).Any(t => typeName.EqualsInvariant(t.Name));
        }

        public IEnumerable<Type> AllSubclasses
        {
            get
            {
                return Type.GetTypeInheritanceChainTo(typeof(TBaseType)).ToArray();
            }
        }
    }
}
