using BuildPipeline.Core.Extensions;
using System.Collections;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml;
using PropertyModels.Extensions;

namespace BuildPipeline.Core.Serialization
{
    #region Policies Structure
    /// <summary>
    /// Interface IObjectXmlSerializerPolicy
    /// </summary>
    public interface IObjectXmlSerializerPolicy
    {
        /// <summary>
        /// Serializes to document.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="target">The target.</param>
        /// <param name="serializationPolicy">The serialization policy.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool SerializeToDocument(XmlElement root, object target, XmlSerializationPolicy serializationPolicy, IObjectXmlSerializerSchema schema);

        /// <summary>
        /// Des the serialize from document.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="target">The target.</param>
        /// <param name="serializationPolicy">The serialization policy.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool DeSerializeFromDocument(XmlElement root, ref object target, XmlSerializationPolicy serializationPolicy, IObjectXmlSerializerSchema schema);
    }

    /// <summary>
    /// Enum XmlSerializationPolicy
    /// </summary>
    [Flags]
    public enum XmlSerializationPolicy
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The public only
        /// </summary>
        PublicOnly = 1 << 0,
        /// <summary>
        /// The declare only
        /// </summary>
        DeclareOnly = 1 << 1,
        /// <summary>
        /// The ignore fields
        /// </summary>
        IgnoreFields = 1 << 2,
        /// <summary>
        /// The ignore properties
        /// </summary>
        IgnoreProperties = 1 << 3
    }

    /// <summary>
    /// Class SerializationIgnoreAttribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public class SerializationIgnoreAttribute : Attribute
    {
    }

    /// <summary>
    /// Class AlwaysSerializeAttribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AlwaysSerializeAttribute : Attribute
    {
    }

    /// <summary>
    /// Class ObjectXmlSerializePolicyAttribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class ObjectXmlSerializePolicyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type of the policy.
        /// </summary>
        /// <value>The type of the policy.</value>
        public Type PolicyType { get; protected set; }
        /// <summary>
        /// Gets or sets the policy.
        /// </summary>
        /// <value>The policy.</value>
        public IObjectXmlSerializerPolicy Policy { get; protected set; }

        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>The attribute.</value>
        public XmlSerializationPolicy Flag { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectXmlSerializePolicyAttribute" /> class.
        /// </summary>
        /// <param name="serializationPolicy">The serialization policy.</param>
        public ObjectXmlSerializePolicyAttribute(XmlSerializationPolicy serializationPolicy) :
            this(typeof(DefaultObjectXmlSerializePolicy), serializationPolicy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectXmlSerializePolicyAttribute" /> class.
        /// </summary>
        /// <param name="policyType">Type of the policy.</param>
        public ObjectXmlSerializePolicyAttribute(Type policyType) :
            this(policyType, XmlSerializationPolicy.PublicOnly | XmlSerializationPolicy.DeclareOnly)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectXmlSerializePolicyAttribute" /> class.
        /// </summary>
        /// <param name="policyType">Type of the policy.</param>
        /// <param name="serializationPolicy">The serialization policy.</param>
        public ObjectXmlSerializePolicyAttribute(Type policyType, XmlSerializationPolicy serializationPolicy)
        {
            Flag = serializationPolicy;
            PolicyType = policyType;

            ObjectXmlSerializerUtils.Assert(PolicyType.IsClass && !policyType.IsAbstract);

            Policy = Activator.CreateInstance(policyType) as IObjectXmlSerializerPolicy;

            ObjectXmlSerializerUtils.Assert(Policy != null);
        }
    }

    /// <summary>
    /// Class DefaultXmlSerializePolicyAttribute.
    /// </summary>    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class DefaultXmlSerializePolicyAttribute : ObjectXmlSerializePolicyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultXmlSerializePolicyAttribute"/> class.
        /// </summary>
        public DefaultXmlSerializePolicyAttribute() :
            base(typeof(DefaultObjectXmlSerializePolicy), XmlSerializationPolicy.DeclareOnly | XmlSerializationPolicy.PublicOnly)
        {
        }
    }
    #endregion

    #region Schema  
    /// <summary>
    /// Interface IObjectXmlSerializerSchema
    /// </summary>
    public interface IObjectXmlSerializerSchema
    {
        /// <summary>
        /// Finds the type.
        /// </summary>
        /// <param name="fullname">The fullname.</param>
        /// <returns>Type.</returns>
        Type GetType(string fullname);
        /// <summary>
        /// Finds the name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        string GetName(Type type);

        /// <summary>
        /// Creates the object.
        /// </summary>
        /// <param name="fullname">The fullname.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        object CreateObject(string fullname, params object[] parameters);
        /// <summary>
        /// Creates the object.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        object CreateObject(Type type, params object[] parameters);

        /// <summary>
        /// Gets the policy.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>ObjectXmlSerializePolicyAttribute.</returns>
        ObjectXmlSerializePolicyAttribute GetPolicyAttribute(Type type);

        /// <summary>
        /// Accepts the member information.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool AcceptMemberInfo(MemberInfo info);
    }
    #endregion

    #region Default Schema
    /// <summary>
    /// Class DefaultObjectXmlSerializerSchema.
    /// Implements the <see cref="IObjectXmlSerializerSchema" />
    /// </summary>
    /// <seealso cref="IObjectXmlSerializerSchema" />
    public class DefaultObjectXmlSerializerSchema : IObjectXmlSerializerSchema
    {
        /// <summary>
        /// The policies
        /// </summary>
        private Dictionary<Type, ObjectXmlSerializePolicyAttribute> Policies = new Dictionary<Type, ObjectXmlSerializePolicyAttribute>();

        /// <summary>
        /// The default attribute
        /// </summary>
        private DefaultXmlSerializePolicyAttribute DefaultAttr = new DefaultXmlSerializePolicyAttribute();

        /// <summary>
        /// Finds the type.
        /// </summary>
        /// <param name="fullname">The fullname.</param>
        /// <returns>Type.</returns>
        public virtual Type GetType(string fullname)
        {
            System.Type type = System.Type.GetType(fullname);

            if (null != type)
            {
                return type;
            }

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(fullname);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
        /// <summary>
        /// Finds the name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        public virtual string GetName(Type type)
        {
            return type.FullName;
        }

        /// <summary>
        /// Creates the object.
        /// </summary>
        /// <param name="fullname">The fullname.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        public virtual object CreateObject(string fullname, params object[] parameters)
        {
            var type = GetType(fullname);

            return type != null ? CreateObject(type, parameters) : null;
        }

        /// <summary>
        /// Creates the object.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.Object.</returns>
        public virtual object CreateObject(Type type, params object[] parameters)
        {
            return ObjectXmlSerializerUtils.CreateObject(type, parameters);
        }

        /// <summary>
        /// Gets the policy.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>ObjectXmlSerializePolicyAttribute.</returns>
        public virtual ObjectXmlSerializePolicyAttribute GetPolicyAttribute(Type type)
        {
            ObjectXmlSerializePolicyAttribute attribute;
            if (Policies.TryGetValue(type, out attribute))
            {
                return attribute;
            }

            object[] attrs = type.GetCustomAttributes(typeof(ObjectXmlSerializePolicyAttribute), true);

            if (attrs != null && attrs.Length > 0)
            {
                ObjectXmlSerializePolicyAttribute attr = attrs[0] as ObjectXmlSerializePolicyAttribute;

                Policies.Add(type, attr);

                return attr;
            }

            Policies.Add(type, DefaultAttr);

            return DefaultAttr;
        }

        /// <summary>
        /// Registers the policy.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="policyType">Type of the policy.</param>
        /// <param name="policy">The policy.</param>
        public virtual void RegisterPolicy(Type type, Type policyType, XmlSerializationPolicy policy = XmlSerializationPolicy.PublicOnly | XmlSerializationPolicy.DeclareOnly)
        {
            ObjectXmlSerializerUtils.Assert(!Policies.ContainsKey(type));

            Policies.Add(type, new ObjectXmlSerializePolicyAttribute(policyType, policy));
        }

        /// <summary>
        /// Uns the register policy.
        /// </summary>
        /// <param name="type">The type.</param>
        public virtual void UnRegisterPolicy(Type type)
        {
            Policies.Remove(type);
        }

        /// <summary>
        /// Accepts the member information.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public virtual bool AcceptMemberInfo(MemberInfo info)
        {
            return true;
        }
    }
    #endregion

    #region Default Serialize Policy
    /// <summary>
    /// Class DefaultObjectXmlSerializePolicy.
    /// </summary>    
    public class DefaultObjectXmlSerializePolicy : IObjectXmlSerializerPolicy
    {
        #region Serialize
        /// <summary>
        /// Class SerializedMeta.
        /// </summary>
        private class SerializedMeta
        {
            /// <summary>
            /// The member infos
            /// </summary>
            public List<MemberInfo> MemberInfos = new List<MemberInfo>();
        }
        /// <summary>
        /// The meta data
        /// </summary>
        Dictionary<Type, SerializedMeta> MetaData = new Dictionary<Type, SerializedMeta>();

        /// <summary>
        /// Caches the serialized meta data.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="serializationPolicy">The serialization policy.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>SerializedMeta.</returns>
        private SerializedMeta CacheSerializedMetaData(object target, XmlSerializationPolicy serializationPolicy, IObjectXmlSerializerSchema schema)
        {
            SerializedMeta Meta;
            var type = target.GetType();

            if (!MetaData.TryGetValue(type, out Meta))
            {
                Meta = new SerializedMeta();
                MetaData.Add(type, Meta);

                MemberInfo[] infos = type.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                // sort 
                infos = infos.OrderBy(field => field.MetadataToken).ToArray();

                foreach (var i in infos)
                {
                    // only serialize fields or properties
                    if (i.CanSerialize() && (schema == null || schema.AcceptMemberInfo(i)))
                    {
                        if ((serializationPolicy & XmlSerializationPolicy.DeclareOnly) != 0 && i.DeclaringType != type)
                        {
                            continue;
                        }

                        if (!i.IsDefined(typeof(AlwaysSerializeAttribute), true))
                        {
                            if ((serializationPolicy & XmlSerializationPolicy.PublicOnly) != 0 && !i.IsPublic())
                            {
                                continue;
                            }

                            if ((serializationPolicy & XmlSerializationPolicy.IgnoreFields) != 0 && i.MemberType == MemberTypes.Field)
                            {
                                continue;
                            }

                            if ((serializationPolicy & XmlSerializationPolicy.IgnoreProperties) != 0 && i.MemberType == MemberTypes.Property)
                            {
                                continue;
                            }

                        }


                        Meta.MemberInfos.Add(i);
                    }
                }
            }

            return Meta;
        }

        /// <summary>
        /// Serializes to document.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="target">The target.</param>
        /// <param name="serializationPolicy">The serialization policy.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool SerializeToDocument(XmlElement root, object target, XmlSerializationPolicy serializationPolicy, IObjectXmlSerializerSchema schema)
        {
            if (target == null || root == null)
            {
                return false;
            }

            var element = root;
            var type = target.GetType();

            if (type.IsEnum)
            {
                element.InnerText = ((int)target).ToString();
            }
            else if (type.IsNumericType() || type == typeof(string))
            {
                element.InnerText = target.ToString();
            }
            else if (type == typeof(bool))
            {
                element.InnerText = ((bool)target) ? "1" : "0";
            }
            else
            {
                // custom object
                var typeName = schema.GetName(type);

                element.SetAttribute("Type", typeName);

                var Meta = CacheSerializedMetaData(target, serializationPolicy, schema);

                foreach (var i in Meta.MemberInfos)
                {
                    SerializeMemberInfo(element, target, i, schema);
                }
            }

            return true;
        }

        /// <summary>
        /// Serializes the member information.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="target">The target.</param>
        /// <param name="info">The information.</param>
        /// <param name="schema">The schema.</param>
        private static void SerializeMemberInfo(XmlElement element, object target, MemberInfo info, IObjectXmlSerializerSchema schema)
        {
            object value = info.GetUnderlyingValue(target);

            if (value == null)
            {
                return;
            }

            IEnumerable enumerable = value is string ? null : value as IEnumerable;

            if (enumerable != null)
            {
                var childElement = element.OwnerDocument.CreateElement(info.GetSerializeName());
                element.AppendChild(childElement);
                var childTypeName = schema.GetName(value.GetType());
                childElement.SetAttribute("Type", childTypeName);

                // this is a list or array ...
                foreach (var i in enumerable)
                {
                    var ele = childElement.OwnerDocument.CreateElement("Item");

                    childElement.AppendChild(ele);

                    if (i != null)
                    {
                        var eleTypeName = schema.GetName(i.GetType());

                        ele.SetAttribute("Type", eleTypeName);

                        ObjectXmlSerializer.SerializeToDocument(ele, i, schema);
                    }
                }
            }
            else
            {
                var ele = element.OwnerDocument.CreateElement(info.GetSerializeName());
                element.AppendChild(ele);

                ObjectXmlSerializer.SerializeToDocument(ele, value, schema);
            }
        }
        #endregion

        #region Deserialize
        /// <summary>
        /// Des the serialize from document.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="target">The target.</param>
        /// <param name="serializationPolicy">The serialization policy.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool DeSerializeFromDocument(XmlElement root, ref object target, XmlSerializationPolicy serializationPolicy, IObjectXmlSerializerSchema schema)
        {
            if (root == null)
            {
                return false;
            }

            if (target == null)
            {
                var typeName = root.GetAttribute("Type");
                target = schema.CreateObject(typeName);

                ObjectXmlSerializerUtils.Assert(target != null);
            }

            var element = root;
            var type = target.GetType();

            if (type.IsEnum)
            {
                int value = int.Parse(element.InnerText);
                target = Enum.ToObject(type, value);
            }
            else if (type.IsNumericType() || type == typeof(string))
            {
                target = Convert.ChangeType(element.InnerText, type);
            }
            else if (type == typeof(bool))
            {
                target = element.InnerText == "1";
            }
            else
            {
                // custom object
                var Meta = CacheSerializedMetaData(target, serializationPolicy, schema);

                foreach (var i in Meta.MemberInfos)
                {
                    DeSerializeMemberInfo(element, ref target, i, schema);
                }
            }

            return true;
        }

        /// <summary>
        /// Des the serialize member information.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="target">The target.</param>
        /// <param name="info">The information.</param>
        /// <param name="schema">The schema.</param>
        private static void DeSerializeMemberInfo(XmlElement element, ref object target, MemberInfo info, IObjectXmlSerializerSchema schema)
        {
            var propElement = element.SelectSingleNode(info.GetSerializeName()) as XmlElement;

            if (propElement == null)
            {
                return;
            }

            Type type = info.GetUnderlyingType();
            bool IsEnumerable = ObjectXmlSerializerUtils.IsArray(type) || 
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) || 
                type.GetInterfaces().Contains(typeof(ICollection));

            if (IsEnumerable)
            {
                // deserialize collection
                var Items = propElement.SelectNodes("Item");

                bool IsArray = ObjectXmlSerializerUtils.IsArray(type) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (IsArray)
                {
                    var Datas = ObjectXmlSerializerUtils.CreateArray(type, Items.Count, schema);

                    for (int i = 0; i < Items.Count; ++i)
                    {
                        var item = Items[i] as XmlElement;
                        string typeName = item.GetAttribute("Type");
                        var obj = string.IsNullOrEmpty(typeName) ? null : schema.CreateObject(typeName);
                        ObjectXmlSerializer.DeSerializeFromDocument(item, ref obj);

                        ObjectXmlSerializerUtils.SetArrayValue(Datas, obj, i);
                    }

                    info.SetUnderlyingValue(target, Datas);
                }
                else
                {
                    IList Datas = schema.CreateObject(type) as IList;

                    foreach (XmlElement item in Items)
                    {
                        string typeName = item.GetAttribute("Type");

                        var obj = schema.CreateObject(typeName);
                        ObjectXmlSerializer.DeSerializeFromDocument(item, ref obj);

                        Datas.Add(obj);
                    }

                    info.SetUnderlyingValue(target, Datas);
                }
            }
            else
            {
                var objTarget = info.GetUnderlyingValue(target);

                if (objTarget == null)
                {
                    var typeName = propElement.GetAttribute("Type");

                    if (!string.IsNullOrEmpty(typeName))
                    {
                        var dynamicType = schema.GetType(typeName);

                        if (dynamicType != null)
                        {
                            objTarget = schema.CreateObject(dynamicType);
                        }
                    }

                    if (objTarget == null)
                    {
                        objTarget = schema.CreateObject(type);
                    }
                }

                if (objTarget != null)
                {
                    ObjectXmlSerializer.DeSerializeFromDocument(propElement, ref objTarget);
                    info.SetUnderlyingValue(target, objTarget);
                }
            }
        }
        #endregion
    }
    #endregion

    #region Object Serializer
    /// <summary>
    /// Class ObjectXmlSerializer.
    /// </summary>
    public static class ObjectXmlSerializer
    {
        #region Open Serialize APIs
        /// <summary>
        /// The default schema private
        /// </summary>
        private static DefaultObjectXmlSerializerSchema DefaultSchemaPrivate = new DefaultObjectXmlSerializerSchema();
        /// <summary>
        /// Gets the default schema.
        /// </summary>
        /// <value>The default schema.</value>
        public static DefaultObjectXmlSerializerSchema DefaultSchema { get { return DefaultSchemaPrivate; } }

        #region Serialization
        /// <summary>
        /// Serializes to document.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="target">The target.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool SerializeToDocument(XmlElement root, object target, IObjectXmlSerializerSchema schema = null)
        {
            if (root == null || target == null)
            {
                return false;
            }

            schema = schema != null ? schema : DefaultSchema;

            var Attr = schema.GetPolicyAttribute(target.GetType());

            return Attr.Policy != null && Attr.Policy.SerializeToDocument(root, target, Attr.Flag, schema);
        }

        /// <summary>
        /// Serializes to text.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.String.</returns>
        public static string SerializeToText(object target, IObjectXmlSerializerSchema schema = null)
        {
            if (target == null)
            {
                return null;
            }

            schema = schema != null ? schema : DefaultSchema;

            XmlDocument doc = new XmlDocument();

            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));

            XmlElement rootElement = doc.CreateElement("Root");
            doc.AppendChild(rootElement);

            try
            {
                if (!SerializeToDocument(rootElement, target, schema))
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                ObjectXmlSerializerUtils.ThrowException(e);
            }

            using (var stringWriter = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = Environment.NewLine,
                    NewLineHandling = NewLineHandling.Replace
                };

                using (var xmlTextWriter = XmlWriter.Create(stringWriter, settings))
                {
                    doc.WriteTo(xmlTextWriter);

                    xmlTextWriter.Flush();

                    string Text = stringWriter.GetStringBuilder().ToString();

                    return Text;
                }
            }
        }

        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <param name="target">The target.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool SerializeToFile(string fullpath, object target, IObjectXmlSerializerSchema schema = null)
        {
            if (target == null)
            {
                return false;
            }

            schema = schema != null ? schema : DefaultSchema;
            string text = SerializeToText(target, schema);

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }

            File.WriteAllText(fullpath, text, System.Text.Encoding.UTF8);

            return true;
        }
        #endregion

        #region Deserialization
        /// <summary>
        /// Des the serialize from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullpath">The fullpath.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>T.</returns>
        public static T DeSerializeFromFile<T>(string fullpath, IObjectXmlSerializerSchema schema = null)
            where T : new()
        {
            var obj = DeSerializeFromFile(typeof(T), fullpath, schema != null ? schema : DefaultSchema);

            if (obj != null && obj is T)
            {
                return (T)obj;
            }

            return default(T);
        }

        /// <summary>
        /// Des the serialize from file.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fullpath">The fullpath.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.Object.</returns>
        public static object DeSerializeFromFile(Type type, string fullpath, IObjectXmlSerializerSchema schema = null)
        {
            if (!ObjectXmlSerializerUtils.IsFileExists(fullpath))
            {
                return null;
            }

            using (var stream = ObjectXmlSerializerUtils.OpenFileReadStream(fullpath))
            {
                return stream != null ? DeSerializeFromStream(type, stream, schema != null ? schema : DefaultSchema) : null;
            }
        }

        /// <summary>
        /// Des the serialize from file.
        /// </summary>
        /// <param name="fullpath">The fullpath.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.Object.</returns>
        public static object DeSerializeFromFile(string fullpath, IObjectXmlSerializerSchema schema = null)
        {
            if (!ObjectXmlSerializerUtils.IsFileExists(fullpath))
            {
                return null;
            }

            using (var stream = ObjectXmlSerializerUtils.OpenFileReadStream(fullpath))
            {
                return stream != null ? DeSerializeFromStream(stream, schema != null ? schema : DefaultSchema) : null;
            }
        }

        /// <summary>
        /// Des the serialize from text.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text">The text.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>T.</returns>
        public static T DeSerializeFromText<T>(string text, IObjectXmlSerializerSchema schema = null)
            where T : new()
        {
            var obj = DeSerializeFromText(typeof(T), text, schema != null ? schema : DefaultSchema);

            if (obj != null && obj is T)
            {
                return (T)obj;
            }

            return default(T);
        }

        /// <summary>
        /// Des the serialize from text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.Object.</returns>
        public static object DeSerializeFromText(string text, IObjectXmlSerializerSchema schema = null)
        {
            XmlDocument document = ObjectXmlSerializerUtils.CreateDocument(text);

            return document != null ? DeSerializeFromDocument(document, schema) : null; //-V3022
        }

        /// <summary>
        /// Des the serialize from stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.Object.</returns>
        public static object DeSerializeFromStream(Stream stream, IObjectXmlSerializerSchema schema = null)
        {
            XmlDocument document = ObjectXmlSerializerUtils.CreateDocument(stream);

            return document != null ? DeSerializeFromDocument(document, schema) : null; //-V3022
        }

        /// <summary>
        /// Des the serialize from document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.Object.</returns>
        public static object DeSerializeFromDocument(XmlDocument document, IObjectXmlSerializerSchema schema = null)
        {
            XmlElement RootElement = document.SelectSingleNode("Root") as XmlElement;

            if (RootElement == null)
            {
                ObjectXmlSerializerUtils.LogError("Failed find root element");

                return null;
            }

            schema = schema != null ? schema : DefaultSchema;

            string typeName = RootElement.HasAttribute("Type") ? RootElement.Attributes["Type"].Value : null;

            if (string.IsNullOrEmpty(typeName))
            {
                ObjectXmlSerializerUtils.LogError("Failed find root type");

                return null;
            }

            var type = schema.GetType(typeName);

            if (type == null)
            {
                ObjectXmlSerializerUtils.LogError("Failed find root type {0}", typeName);

                return null;
            }

            object target = schema.CreateObject(type);

            if (!DeSerializeFromDocument(RootElement, ref target, schema))
            {
                return null;
            }

            return target;
        }

        /// <summary>
        /// Des the serialize from document.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.Object.</returns>
        public static object DeSerializeFromDocument(XmlElement root, IObjectXmlSerializerSchema schema = null)
        {
            schema = schema != null ? schema : DefaultSchema;

            string typeName = root.HasAttribute("Type") ? root.Attributes["Type"].Value : null;

            if (string.IsNullOrEmpty(typeName))
            {
                ObjectXmlSerializerUtils.LogError("Failed find root type");

                return null;
            }

            var type = schema.GetType(typeName);

            if (type == null)
            {
                ObjectXmlSerializerUtils.LogError("Failed find root type {0}", typeName);

                return null;
            }

            object target = schema.CreateObject(type);

            if (!DeSerializeFromDocument(root, ref target, schema))
            {
                return null;
            }

            return target;
        }


        /// <summary>
        /// Des the serialize from text.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="text">The text.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.Object.</returns>
        public static object DeSerializeFromText(Type type, string text, IObjectXmlSerializerSchema schema = null)
        {
            ObjectXmlSerializerUtils.Assert(type != null && !type.IsAbstract);

            if (type == null || type.IsAbstract)
            {
                return null;
            }

            XmlDocument document = ObjectXmlSerializerUtils.CreateDocument(text);

            return document != null ? DeSerializeFromDocument(type, document, schema) : null; //-V3022
        }

        /// <summary>
        /// Des the serialize from stream.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.Object.</returns>
        public static object DeSerializeFromStream(Type type, Stream stream, IObjectXmlSerializerSchema schema = null)
        {
            ObjectXmlSerializerUtils.Assert(type != null && !type.IsAbstract);

            if (type == null || type.IsAbstract)
            {
                return null;
            }

            XmlDocument document = ObjectXmlSerializerUtils.CreateDocument(stream);

            return document != null ? DeSerializeFromDocument(type, document, schema) : null; //-V3022
        }

        /// <summary>
        /// Des the serialize from document.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="document">The document.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>System.Object.</returns>
        public static object DeSerializeFromDocument(Type type, XmlDocument document, IObjectXmlSerializerSchema schema = null)
        {
            XmlElement RootElement = document.SelectSingleNode("Root") as XmlElement;

            if (RootElement == null)
            {
                return null;
            }

            schema = schema != null ? schema : DefaultSchema;

            object target = schema.CreateObject(type);

            if (!DeSerializeFromDocument(RootElement, ref target, schema))
            {
                return null;
            }

            return target;
        }

        /// <summary>
        /// Des the serialize from document.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="target">The target.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool DeSerializeFromDocument(XmlElement root, ref object target, IObjectXmlSerializerSchema schema = null)
        {
            if (root == null || target == null)
            {
                return false;
            }

            schema = schema != null ? schema : DefaultSchema;

            var Attr = schema.GetPolicyAttribute(target.GetType());

            return Attr.Policy != null && Attr.Policy.DeSerializeFromDocument(root, ref target, Attr.Flag, schema);
        }

        #endregion

        #region Override
        /// <summary>
        /// Overrides from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="fullpath">The fullpath.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool OverrideFromFile<T>(ref T target, string fullpath, IObjectXmlSerializerSchema schema = null)
        {
            using (var stream = ObjectXmlSerializerUtils.OpenFileReadStream(fullpath))
            {
                return stream != null && OverrideFromStream(ref target, stream, schema);
            }
        }

        /// <summary>
        /// Overrides from text.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="text">The text. should be xml format text</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool OverrideFromText<T>(ref T target, string text, IObjectXmlSerializerSchema schema)
        {
            XmlDocument document = ObjectXmlSerializerUtils.CreateDocument(text);

            XmlElement RootElement = document.SelectSingleNode("Root") as XmlElement;

            if (RootElement == null)
            {
                return false;
            }

            schema = schema != null ? schema : DefaultSchema;

            return OverrideFromDocument(RootElement, ref target, schema);
        }

        /// <summary>
        /// Overrides from stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool OverrideFromStream<T>(ref T target, Stream stream, IObjectXmlSerializerSchema schema)
        {
            XmlDocument document = ObjectXmlSerializerUtils.CreateDocument(stream);

            XmlElement RootElement = document.SelectSingleNode("Root") as XmlElement;

            if (RootElement == null)
            {
                return false;
            }

            schema = schema != null ? schema : DefaultSchema;

            return OverrideFromDocument(RootElement, ref target, schema);
        }

        /// <summary>
        /// Overrides from document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root">The root.</param>
        /// <param name="target">The target.</param>
        /// <param name="schema">The schema.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool OverrideFromDocument<T>(XmlElement root, ref T target, IObjectXmlSerializerSchema schema = null)
        {
            if (root == null || target == null)
            {
                return false;
            }

            schema = schema != null ? schema : DefaultSchema;

            var Attr = schema.GetPolicyAttribute(target.GetType());
            object obj = target;

            if (Attr.Policy != null && Attr.Policy.DeSerializeFromDocument(root, ref obj, Attr.Flag, schema))
            {
                target = (T)obj;
                return true;
            }

            return false;
        }

        #endregion

        #endregion
    }
    #endregion

    #region Extensions
    /// <summary>
    /// Class MemberInfoExtensions.
    /// </summary>
    internal static class MemberInfoExtensions
    {
        /// <summary>
        /// Determines whether this instance can serialize the specified information.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns><c>true</c> if this instance can serialize the specified information; otherwise, <c>false</c>.</returns>
        public static bool CanSerialize(this MemberInfo info)
        {
            if (info.MemberType == MemberTypes.Field || info.MemberType == MemberTypes.Property)
            {
                if (info.IsStatic() ||
                    info.IsConstant() ||
                    info.Name.EndsWith("k__BackingField")/* Skip backing Fields*/ ||
                    info.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true) ||
                    (!info.IsDefined(typeof(AlwaysSerializeAttribute), true) &&
                    (
                    info.IsDefined(typeof(XmlIgnoreAttribute), true) ||
                    info.IsDefined(typeof(NonSerializedAttribute), true) ||
                    info.IsDefined(typeof(SerializationIgnoreAttribute), true)
                    )
                    )
                )
                {
                    return false;
                }

                if (info.MemberType == MemberTypes.Property)
                {
                    PropertyInfo pi = info as PropertyInfo;
                    return pi.CanRead && pi.CanWrite;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the name of the serialize.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns>System.String.</returns>
        public static string GetSerializeName(this MemberInfo info)
        {
            if (!ObjectXmlSerializerUtils.IsDefined<XmlElementAttribute>(info))
            {
                return info.Name;
            }

            XmlElementAttribute attr = ObjectXmlSerializerUtils.GetAnyCustomAttribute<XmlElementAttribute>(info);

            return attr!= null && !string.IsNullOrEmpty(attr.ElementName) ? attr.ElementName : info.Name;
        }
    }
    #endregion
}
