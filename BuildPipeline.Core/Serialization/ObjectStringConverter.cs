using BuildPipeline.Core.Utils;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace BuildPipeline.Core.Serialization
{
    /// <summary>
    /// Interface IStringSerializableObject
    /// </summary>
    public interface IStringSerializableObject
    {
        /// <summary>
        /// Froms the string.
        /// </summary>
        /// <param name="text">The in text.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool FromString(string text);

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>ICustomObject.</returns>
        IStringSerializableObject Clone();
    }

    /// <summary>
    /// Class ObjectSerializeHelper.
    /// </summary>
    public static class ObjectStringConverter
    {
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public static string ToString(Object target)
        {
            Object curValue = target;

            // convert enum to integer
            if (target.GetType().IsEnum)
            {
                curValue = (int)Enum.Parse(target.GetType(), curValue.ToString());
            }
            else if (target.GetType() == typeof(bool))
            {
                // convert bool to integer
                curValue = (int)((bool)curValue ? 1 : 0);
            }

            return curValue != null ? curValue.ToString() : "";
        }

        /// <summary>
        /// To the object.
        /// </summary>
        /// <param name="fieldInfo">The field information.</param>
        /// <param name="text">The text.</param>
        /// <returns>System.Object.</returns>
        public static object ToObject(FieldInfo fieldInfo, string text)
        {
            return ToObject(fieldInfo.FieldType, text, fieldInfo.Name);
        }

        /// <summary>
        /// To the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text">The text.</param>
        /// <param name="baseInformation">The base information.</param>
        /// <returns>T.</returns>
        public static T ToObject<T>(string text, string baseInformation = "")
        {
            return (T)ToObject(typeof(T), text, baseInformation);
        }

        /// <summary>
        /// To the object.
        /// </summary>
        /// <param name="binderType">Type of the binder.</param>
        /// <param name="text">The text.</param>
        /// <param name="baseInformation">The base information.</param>
        /// <returns>System.Object.</returns>
        public static object ToObject(Type binderType, string text, string baseInformation = "")
        {
#if !DEBUG
            try
            {
#endif
            object propValue = null;

            if (binderType.IsEnum)
            {
                propValue = Enum.GetName(binderType, int.Parse(text));

                // convert directly
                TypeConverter typeConverter = TypeDescriptor.GetConverter(binderType);

                Logger.Assert(typeConverter != null, "Failed find type converter for {0}", binderType);

                propValue = typeConverter.ConvertFromString((string)propValue);
            }
            else if (binderType == typeof(bool))
            {
                propValue = text == "1";
            }
            else if (binderType.GetInterfaces().Contains(typeof(IList)))
            {
                // skip list...
                return null;
            }
            else if (binderType.GetInterfaces().Contains(typeof(IStringSerializableObject)))
            {
                IStringSerializableObject val = Activator.CreateInstance(binderType) as IStringSerializableObject;

                if (val != null)
                {
                    val.FromString(text);
                }

                return val;
            }
            else
            {
                // convert directly
                TypeConverter typeConverter = TypeDescriptor.GetConverter(binderType);

                Logger.Assert(typeConverter != null, "Failed find type converter for {0}", binderType);
                propValue = typeConverter.ConvertFromString(text);
            }

            return propValue;
#if !DEBUG
            }
            catch (System.Exception e)
            {
                Logger.LogWarning("Skip property {0}, no vaild type converter. {1}-->{2}", baseInformation, e.Message, e.StackTrace);
            }

            return null;
#endif
        }

    }
}
