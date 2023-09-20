using BuildPipeline.Core.CommandLine;
using BuildPipeline.Core.Extensions;
using BuildPipeline.Core.Serialization;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;
using System.Collections;
using System.ComponentModel;
using System.Xml;

namespace BuildPipeline.Core.BuilderFramework.Scriptable
{
    /// <summary>
    /// Class ScriptableObject.
    /// </summary>
    public class ScriptableObject : ReactiveObject
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ScriptableObject"/> is require.
        /// </summary>
        /// <value><c>true</c> if require; otherwise, <c>false</c>.</value>
        public bool Require { get; set; }

        /// <summary>
        /// Gets or sets the type of the value.
        /// </summary>
        /// <value>The type of the value.</value>
        [DependsOnProperty(nameof(Value))]
        public Type ValueType { get; set; }

        object ValueCore;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get => ValueCore;
            set
            {
                if(value == null)
                {
                    ValueCore = null;
                    ValueType = null;
                    RaisePropertyChanged(nameof(Value));
                    return;
                }

                if(value is IList list)
                {
                    if(list.Count != 0)
                    {
                        Type type = null;
                        for(int i=0; i<list.Count; i++)
                        {
                            if (list[i] != null)
                            {
                                type = list[i].GetType();
                                break;
                            }
                        }

                        if(type != null)
                        {
                            ValueCore = Activator.CreateInstance(typeof(BindingList<>).MakeGenericType(type));

                            for(int i=0; i<list.Count;++i)
                            {
                                (ValueCore as IBindingList).Add(list[i]);
                            }

                            ValueType = ValueCore.GetType();

                            RaisePropertyChanged(nameof(Value));

                            return;
                        }
                    }

                    ValueCore = new BindingList<string>();
                    ValueType = ValueCore.GetType();
                }
                else
                {
                    ValueCore = value;
                    ValueType = ValueCore.GetType();
                }
                

                RaisePropertyChanged(nameof(Value));
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return (DisplayName.IsNotNullOrEmpty() ? DisplayName : Name) + "=" + (Value?.ToString()??"None");
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <returns>Attribute[].</returns>
        public Attribute[] GetAttributes()
        {
            OptionAttribute optionAttribute = new OptionAttribute(Name);
            optionAttribute.HelpText = this.Description.IsNotNullOrEmpty() ? this.Description : (this.DisplayName.IsNotNullOrEmpty()?DisplayName:Name);
            optionAttribute.Required = this.Require;

            DisplayNameAttribute displayNameAttribute = new DisplayNameAttribute(DisplayName.IsNotNullOrEmpty() ? DisplayName : Name);

            return new Attribute[] { optionAttribute, displayNameAttribute };
        }
    }

    #region Options
    /// <summary>
    /// Class ScriptableBuildTaskOptions.
    /// Implements the <see cref="BuildPipeline.Core.BuilderFramework.BuildTaskOptions" />
    /// </summary>
    /// <seealso cref="BuildPipeline.Core.BuilderFramework.BuildTaskOptions" />
    [ObjectXmlSerializePolicy(typeof(ScriptableBuildTaskOptionsSerializerPolicy))]    
    public class ScriptableBuildTaskOptions : BuildTaskOptions, ICustomTypeDescriptor, ICloneable
    {
        /// <summary>
        /// The properties
        /// </summary>
        public readonly List<ScriptableObject> Properties = new List<ScriptableObject>();

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="property">The property.</param>
        public void AddProperty(ScriptableObject property)
        {
            Properties.Add(property);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            string text = ObjectXmlSerializer.SerializeToText(this);

            ScriptableBuildTaskOptions options = new ScriptableBuildTaskOptions();
            ObjectXmlSerializer.OverrideFromText(ref options, text, null);

            return options;
        }

        #region Custom Type Descriptor Interfaces
        /// <summary>
        /// Returns a collection of custom attributes for this instance of a component.
        /// </summary>
        /// <returns>An <see cref="T:System.ComponentModel.AttributeCollection" /> containing the attributes for this object.</returns>
        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        /// <summary>
        /// Returns the class name of this instance of a component.
        /// </summary>
        /// <returns>The class name of the object, or <see langword="null" /> if the class does not have a name.</returns>
        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        /// <summary>
        /// Returns the name of this instance of a component.
        /// </summary>
        /// <returns>The name of the object, or <see langword="null" /> if the object does not have a name.</returns>
        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        /// <summary>
        /// Returns a type converter for this instance of a component.
        /// </summary>
        /// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> that is the converter for this object, or <see langword="null" /> if there is no <see cref="T:System.ComponentModel.TypeConverter" /> for this object.</returns>
        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        /// <summary>
        /// Returns the default event for this instance of a component.
        /// </summary>
        /// <returns>An <see cref="T:System.ComponentModel.EventDescriptor" /> that represents the default event for this object, or <see langword="null" /> if this object does not have events.</returns>
        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        /// <summary>
        /// Returns the default property for this instance of a component.
        /// </summary>
        /// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptor" /> that represents the default property for this object, or <see langword="null" /> if this object does not have properties.</returns>
        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        /// <summary>
        /// Returns an editor of the specified type for this instance of a component.
        /// </summary>
        /// <param name="editorBaseType">A <see cref="T:System.Type" /> that represents the editor for this object.</param>
        /// <returns>An <see cref="T:System.Object" /> of the specified type that is the editor for this object, or <see langword="null" /> if the editor cannot be found.</returns>
        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        /// <summary>
        /// Returns the events for this instance of a component.
        /// </summary>
        /// <returns>An <see cref="T:System.ComponentModel.EventDescriptorCollection" /> that represents the events for this component instance.</returns>
        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);            
        }

        /// <summary>
        /// Returns the events for this instance of a component using the specified attribute array as a filter.
        /// </summary>
        /// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that is used as a filter.</param>
        /// <returns>An <see cref="T:System.ComponentModel.EventDescriptorCollection" /> that represents the filtered events for this component instance.</returns>
        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        /// <summary>
        /// Returns the properties for this instance of a component.
        /// </summary>
        /// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that represents the properties for this component instance.</returns>
        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        /// <summary>
        /// Returns the properties for this instance of a component using the attribute array as a filter.
        /// </summary>
        /// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that is used as a filter.</param>
        /// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that represents the filtered properties for this component instance.</returns>
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

            foreach(var property in Properties)
            {
                var pd = new ScriptableObjectPropertyDescriptor(property);

                pds.Add(pd);
            }

            return pds;
        }

        /// <summary>
        /// Returns an object that contains the property described by the specified property descriptor.
        /// </summary>
        /// <param name="pd">A <see cref="T:System.ComponentModel.PropertyDescriptor" /> that represents the property whose owner is to be found.</param>
        /// <returns>An <see cref="T:System.Object" /> that represents the owner of the specified property.</returns>
        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
        #endregion
    }
    #endregion

    #region XmlSerialization
    internal class ScriptableBuildTaskOptionsSerializerPolicy : IObjectXmlSerializerPolicy
    {
        public bool DeSerializeFromDocument(XmlElement root, ref object target, XmlSerializationPolicy serializationPolicy, IObjectXmlSerializerSchema schema)
        {
            ScriptableBuildTaskOptions options = target as ScriptableBuildTaskOptions;

            if(options == null)
            {
                return false;
            }

            foreach(XmlElement node in root.SelectNodes("Properties/Property"))
            {
                ScriptableObject property = new ScriptableObject();
                property.Name = node.GetAttribute("Name");
                property.DisplayName = node.GetAttribute("DisplayName");
                property.Description = node.GetAttribute("Description");
                string typeName = node.GetAttribute("Type");

                var type = schema.GetType(typeName);

                if(type == null)
                {
                    continue;
                }

                var obj = schema.CreateObject(type);
                                
                DeserializePropertyValue(node, ref obj, schema);

                property.Value = obj;

                options.AddProperty(property);
            }

            return true;
        }

        public bool SerializeToDocument(XmlElement root, object target, XmlSerializationPolicy serializationPolicy, IObjectXmlSerializerSchema schema)
        {
            ScriptableBuildTaskOptions options = target as ScriptableBuildTaskOptions;

            if(options == null)
            {
                return false;
            }

            var childElement = root.OwnerDocument.CreateElement("Properties");
            root.AppendChild(childElement);

            foreach(var property in options.Properties)
            {
                var ele = childElement.OwnerDocument.CreateElement("Property");
                childElement.AppendChild(ele);

                ele.SetAttribute("Name", property.Name);
                ele.SetAttribute("DisplayName", property.DisplayName ?? "");
                ele.SetAttribute("Description", property.Description ?? "");
                ele.SetAttribute("Type", property.Value != null? property.Value.GetType().FullName : "");

                SerializeProeprtyValue(ele, property.Value, schema);                
            }

            return true;
        }

        private void DeserializePropertyValue(XmlElement root, ref object target, IObjectXmlSerializerSchema schema)
        {
            if(target is IList list)
            {
                foreach(XmlElement node in root.SelectNodes("Item"))
                {
                    string typeName = node.GetAttribute("Type");

                    var type = schema.GetType(typeName);

                    if (type == null)
                    {
                        continue;
                    }

                    var obj = schema.CreateObject(type);

                    DeserializePropertyValue(node, ref obj, schema);

                    list.Add(obj);
                }
            }
            else
            {
                ObjectXmlSerializer.DeSerializeFromDocument(root, ref target, schema);
            }            
        }

        private void SerializeProeprtyValue(XmlElement root, object target, IObjectXmlSerializerSchema schema)
        {
            if(target is IList list)
            {               
                // this is a list or array ...
                foreach (var i in list)
                {
                    var ele = root.OwnerDocument.CreateElement("Item");

                    root.AppendChild(ele);

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
                ObjectXmlSerializer.SerializeToDocument(root, target, schema);
            }
        }
    }
    #endregion

    #region Property Descriptor
    /// <summary>
    /// Class ScriptableObjectPropertyDescriptor.
    /// Implements the <see cref="PropertyDescriptor" />
    /// </summary>
    /// <seealso cref="PropertyDescriptor" />
    internal class ScriptableObjectPropertyDescriptor : PropertyDescriptor
    {
        Type _Type;
        string _DisplayName;
        string _Description;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptableObjectPropertyDescriptor" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public ScriptableObjectPropertyDescriptor(ScriptableObject target) :
            base(target.Name, target.GetAttributes())
        {
            _Type = target.ValueType;
            _DisplayName = target.DisplayName;
            _Description = target.Description;
        }

        public override Type ComponentType => typeof(ScriptableBuildTaskOptions);

        public override bool IsReadOnly => false;

        public override Type PropertyType => _Type;

        public override string DisplayName => _DisplayName;

        public override string Name => base.Name;

        public override string Description => _Description;

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            if(component is ScriptableBuildTaskOptions options)
            {
                var obj = options.Properties.Find(x => x.Name == this.Name);

                return obj?.Value;
            }

            return null;
        }

        public override void ResetValue(object component)
        {
            
        }

        public override void SetValue(object component, object value)
        {
            if(component is ScriptableBuildTaskOptions options)
            {
                var obj = options.Properties.Find(x => x.Name == this.Name);

                if(obj != null)
                {
                    obj.Value = value;

                    options.RaisePropertyChanged(this.Name);
                }
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
