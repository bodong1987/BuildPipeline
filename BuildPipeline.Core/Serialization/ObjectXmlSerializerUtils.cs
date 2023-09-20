using BuildPipeline.Core.Utils;
using PropertyModels.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace BuildPipeline.Core.Serialization
{
    /// <summary>
    /// Class ObjectXmlSerializerUtils.
    /// </summary>
    internal static class ObjectXmlSerializerUtils
    {
        #region Internal Functions
        /// <summary>
        /// Creates the object.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        public static object CreateObject(Type type, params object[] parameters)
        {
            return ObjectCreator.Create(type, parameters);
        }

        /// <summary>
        /// Determines whether the specified type is array.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is array; otherwise, <c>false</c>.</returns>
        public static bool IsArray(Type type)
        {
            return type.IsArray;
        }

        /// <summary>
        /// Creates the array.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="count">The count.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.Object.</returns>
        public static object CreateArray(Type type, int count, IObjectXmlSerializerSchema schema)
        {   
            if(type.IsArray)
            {
                var etype = type.GetElementType();

                return Array.CreateInstance(etype, count);
            }

            Array Datas = schema.CreateObject(type, count) as Array;
            Assert(Datas.Length == count);

            return Datas;
        }

        /// <summary>
        /// Sets the array value.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        public static void SetArrayValue(object array, object value, int index)
        {
            Array Datas = array as Array;

            Datas.SetValue(value, index);
        }

        /// <summary>
        /// Throws the exception.
        /// </summary>
        /// <param name="e">The e.</param>
        public static void ThrowException(Exception e)
        {
            throw e;
        }

        /// <summary>
        /// Determines whether [is file exists] [the specified fullpath].
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <returns><c>true</c> if [is file exists] [the specified fullpath]; otherwise, <c>false</c>.</returns>
        public static bool IsFileExists(string fullpath)
        {
            return File.Exists(fullpath);
        }

        /// <summary>
        /// Opens the file read stream.
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <returns>Stream.</returns>
        public static Stream OpenFileReadStream(string fullpath)
        {
            return File.OpenRead(fullpath);
        }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>XmlDocument.</returns>
        public static XmlDocument CreateDocument(Stream stream)
        {
            XmlDocument document = new XmlDocument();

            try
            {
                document.Load(stream);
            }
            catch (Exception e)
            {
                ObjectXmlSerializerUtils.ThrowException(e);
                return null;
            }

            return document;
        }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>XmlDocument.</returns>
        public static XmlDocument CreateDocument(string text)
        {
            XmlDocument document = new XmlDocument();

            try
            {
                document.LoadXml(text);
            }
            catch (Exception e)
            {
                ObjectXmlSerializerUtils.ThrowException(e);
                return null;
            }

            return document;
        }

        /// <summary>
        /// Asserts the specified condition.
        /// </summary>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            Logger.Assert(condition);
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="fmt">The FMT.</param>
        /// <param name="params">The parameters.</param>
        public static void LogError(string fmt, params object[] @params)
        {
            Logger.LogError(fmt, @params);
        }

        /// <summary>
        /// Determines whether the specified inherit is defined.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns><c>true</c> if the specified inherit is defined; otherwise, <c>false</c>.</returns>
        public static bool IsDefined<T>(MemberInfo memberInfo, bool inherit = true)
            where T : Attribute
        {
            return memberInfo.IsDefined(typeof(T), inherit);
        }
        /// <summary>
        /// Gets any custom attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns>T.</returns>
        public static T GetAnyCustomAttribute<T>(MemberInfo memberInfo, bool inherit = true)
            where T : Attribute
        {
            var Attrs = memberInfo.GetCustomAttributes(typeof(T), inherit);

            if (Attrs != null && Attrs.Length > 0)
            {
                T attr = Attrs[0] as T;

                return attr;
            }

            return null;
        }


        #endregion
    }

}
