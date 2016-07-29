
namespace TellusResourceAllocatorManagement.Config
{
    using System.Configuration;

    // Define a custom section containing an individual 
    // element and a collection of elements. 
    public class EnvironmentConfigSection : ConfigurationSection
    {       
        // Declare a collection element represented  
        // in the configuration file by the sub-section 
        //
        // Note: the "IsDefaultCollection = false"  
        // instructs the .NET Framework to build a nested  
        // section
        private const string ENVIRONMENTS_STRING = "environments";
        [ConfigurationProperty(ENVIRONMENTS_STRING, IsDefaultCollection = false)]
        public EnvironmentsCollection Environments
        {
            get
            {
                return base[ENVIRONMENTS_STRING] as EnvironmentsCollection;
            }
        }       
    }
}
