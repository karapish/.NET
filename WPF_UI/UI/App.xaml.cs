
namespace TellusResourceAllocatorManagement.UI
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// Indicates if test mode is request
        /// </summary>
        private static bool? s_isTestModeEnabled;
        private const string TEST_MODE_TEXT = "TestMode";

        /// <summary>
        /// Returns whether execution is configured to TestMode. The value is set
        /// in the App.Config in TestMode.
        /// </summary>
        public static bool? TestMode
        {
            get
            {
                if (s_isTestModeEnabled != null) 
                    return s_isTestModeEnabled;

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[TEST_MODE_TEXT]))
                {
                    string value = ConfigurationManager.AppSettings[TEST_MODE_TEXT];
                    
                    if (value != null)
                    {
                        s_isTestModeEnabled = Convert.ToBoolean(value);
                    }
                }
                else
                {
                    s_isTestModeEnabled = false;
                }

                return s_isTestModeEnabled;
            }
        }       
    }
}