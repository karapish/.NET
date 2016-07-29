
namespace TellusResourceAllocatorManagement.Config
{
    using System.Configuration;

    /// <summary>
    /// Allows custom config sections in app.config
    /// </summary>
    public class EnvironmentConfigElement : ConfigurationElement
    {
        private const string NAME_STRING = "name";
        private const string RAM_FILE_LOCATION_STRING = "ramFileLocation";
        private const string TMS_BINDING_NAME_STRING = "tmsBindingName";
        private const string WEB_PORTAL_STRING = "webPortal";

        #region Ctors
        /// <summary>
        /// Constructor allowing name, ram file, and binding to be specified. 
        /// </summary>
        /// <param name="name">Environment alias</param>
        /// <param name="ramFileLocation">RAM file location</param>
        /// <param name="tmsBindingName">Binding name which is defined in app.config</param>
        public EnvironmentConfigElement(string name, string ramFileLocation, string tmsBindingName)
        {
            this.Name = name;
            this.RamFileLocation = ramFileLocation;
            this.TmsBindingName = tmsBindingName;
        }

        /// <summary>
        /// Requirement to have the default constructor.                
        /// </summary>
        public EnvironmentConfigElement()
        {
            // Left empty intentionally.
        }

        /// <summary>
        /// Overloaded ctor
        /// </summary>
        /// <param name="name">Environment alias</param>
        public EnvironmentConfigElement(string name)
        {
            this.Name = name;
        }
        #endregion

        #region Config properties
        [ConfigurationProperty(NAME_STRING, IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return this[NAME_STRING] as string;
            }
            set
            {
                this[NAME_STRING] = value;
            }
        }

        [ConfigurationProperty(RAM_FILE_LOCATION_STRING)]
        public string RamFileLocation
        {
            get
            {
                return this[RAM_FILE_LOCATION_STRING] as string;
            }
            set
            {
                this[RAM_FILE_LOCATION_STRING] = value;
            }
        }

        [ConfigurationProperty(TMS_BINDING_NAME_STRING)]
        public string TmsBindingName
        {
            get
            {
                return this[TMS_BINDING_NAME_STRING] as string;
            }
            set
            {
                this[TMS_BINDING_NAME_STRING] = value;
            }
        }

        [ConfigurationProperty(WEB_PORTAL_STRING)]
        public string WebPortal
        {
            get
            {
                return this[WEB_PORTAL_STRING] as string;
            }
            set
            {
                this[WEB_PORTAL_STRING] = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// Supports a list of settings in the config sections.
    /// </summary>
    public class EnvironmentsCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new EnvironmentConfigElement();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new EnvironmentConfigElement(elementName);
        }
        
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as EnvironmentConfigElement).Name;
        }

        public new string AddElementName
        {
            get
            {
                return base.AddElementName;
            }

            set
            {
                base.AddElementName = value;
            }
        }       

        public new int Count
        {
            get
            {
                return base.Count;
            }
        }

        public EnvironmentConfigElement this[int index]
        {
            get
            {
                return this.BaseGet(index) as EnvironmentConfigElement;
            }

            set
            {
                if (this.BaseGet(index) != null)
                {
                    this.BaseRemoveAt(index);
                }

                this.BaseAdd(index, value);
            }
        }

        new public EnvironmentConfigElement this[string name]
        {
            get
            {
                return this.BaseGet(name) as EnvironmentConfigElement;
            }
        }

        public int IndexOf(EnvironmentConfigElement url)
        {
            return this.BaseIndexOf(url);
        }

        public void Add(EnvironmentConfigElement url)
        {
            this.BaseAdd(url);            
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            this.BaseAdd(element, false);
        }     
    }
}