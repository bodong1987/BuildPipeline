using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Utils;
using PropertyModels.ComponentModel;
using PropertyModels.Extensions;
using System.ComponentModel;
using System.Reflection;

namespace BuildPipeline.Core.CommandLine
{
    /// <summary>
    /// Class TypeInfo.
    /// </summary>
    public class TypeInfo
    {
        /// <summary>
        /// The type
        /// </summary>
        public readonly System.Type Type;

        /// <summary>
        /// The properties core
        /// </summary>
        readonly List<PropertyInfo> PropertiesCore = new List<PropertyInfo>();

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public PropertyInfo[] Properties => PropertiesCore.ToArray();

        /// <summary>
        /// Gets a value indicating whether this instance is command line object.
        /// </summary>
        /// <value><c>true</c> if this instance is command line object; otherwise, <c>false</c>.</value>
        public bool IsCommandLineObject => PropertiesCore.Count > 0;

        /// <summary>
        /// The default object
        /// </summary>
        public readonly object DefaultObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeInfo"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public TypeInfo(Type type)        
        {
            Type = type;

            Logger.Assert(!type.IsAbstract);

            try
            {
                DefaultObject = ObjectCreator.Create(type);
            }
            catch(Exception ex)
            {
                Logger.LogError("Failed create default for {0}, then this target will not support format command line in simplify mode.\n{1}", type, ex.Message);
            }

            ParseMeta();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeInfo" /> class.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="defaultObject">The default object.</param>
        public TypeInfo(ICustomTypeDescriptor descriptor, object defaultObject)
        {
            DefaultObject = defaultObject;

            foreach (PropertyDescriptor property in descriptor.GetProperties())
            {
                var attr = property.GetCustomAttribute<OptionAttribute>();

                if( attr != null )
                {
                    var info = new PropertyInfo(property, property.GetCustomAttribute<OptionAttribute>());
                    PropertiesCore.Add(info);
                }                
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Type.FullName;
        }

        private void ParseMeta()
        {
            var properties = TypeDescriptor.GetProperties(Type);

            // sort by meta data token
            foreach(PropertyDescriptor property in properties)
            {
                if(property.IsDefined<OptionAttribute>())
                {
                    var info = new PropertyInfo(property, property.GetCustomAttribute<OptionAttribute>());
                    PropertiesCore.Add(info);
                }
            }
        }

        /// <summary>
        /// Finds the short property.
        /// </summary>
        /// <param name="shortName">The short name.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns>PropertyInfo.</returns>
        public PropertyInfo FindShortProperty(string shortName, bool ignoreCase)
        {
            return PropertiesCore.Find(x => 
            { 
                if(x.Attribute.ShortName.IsNullOrEmpty())
                {
                    return false;
                }

                if(ignoreCase)
                {
                    return x.Attribute.ShortName.iEquals(shortName);
                }
                else
                {
                    return x.Attribute.ShortName.Equals(shortName);
                }
            });
        }

        /// <summary>
        /// Finds the long property.
        /// </summary>
        /// <param name="longName">The long name.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns>PropertyInfo.</returns>
        public PropertyInfo FindLongProperty(string longName, bool ignoreCase)
        {
            return PropertiesCore.Find(x =>
            {
                if (x.Attribute.LongName.IsNullOrEmpty())
                {
                    return false;
                }

                if (ignoreCase)
                {
                    return x.Attribute.LongName.iEquals(longName);
                }
                else
                {
                    return x.Attribute.LongName.Equals(longName);
                }
            });
        }
    }

    /// <summary>
    /// Class PropertyInfo.
    /// </summary>
    public class PropertyInfo
    {
        /// <summary>
        /// The property
        /// </summary>
        public readonly PropertyDescriptor Property;

        /// <summary>
        /// The attribute
        /// </summary>
        public readonly OptionAttribute Attribute;

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public System.Type Type => Property.PropertyType;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Attribute.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInfo"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="attribute">The attribute.</param>
        public PropertyInfo(PropertyDescriptor property, OptionAttribute attribute)
        {
            Property = property;
            Attribute = attribute;

            if(attribute.LongName.IsNullOrEmpty())
            {
                attribute.LongName = property.Name;
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>System.Object.</returns>
        public object GetValue(object target)
        {
            return Property.GetValue(target);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        public void SetValue(object target, object value)
        {
            Property.SetValue(target, value);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is array.
        /// </summary>
        /// <value><c>true</c> if this instance is array; otherwise, <c>false</c>.</value>
        public bool IsBindingList => Property.PropertyType.IsGenericType && Property.PropertyType.GetGenericTypeDefinition() == typeof(BindingList<>);

        /// <summary>
        /// Gets a value indicating whether this instance is flags.
        /// </summary>
        /// <value><c>true</c> if this instance is flags; otherwise, <c>false</c>.</value>
        public bool IsFlags => Property.PropertyType.IsEnum && Property.PropertyType.IsDefined<FlagsAttribute>();

        /// <summary>
        /// Gets the type of the element.
        /// </summary>
        /// <returns>Type.</returns>
        public Type GetElementType()
        {
            if(!IsBindingList)
            {
                return null;
            }

            if(Property.PropertyType.IsGenericType)
            {
                var etype = Type.GetGenericArguments()[0];
                return etype;
            }

            return Property.PropertyType.GetElementType();
        }
    }
}
