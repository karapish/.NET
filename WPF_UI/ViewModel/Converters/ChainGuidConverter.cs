
namespace TellusResourceAllocatorManagement.ViewModels.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using TellusResourceAllocatorManagement.Data;

    /// <summary>
    /// Extracts info about the chain off the Chain ID
    /// </summary>
    public class ChainGuidConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {            
            if (values == null || values.Length != 2)
                throw new ArgumentException(
                    "Expect two values to perform the conversion", 
                    "values");            

            Guid guid = new Guid(values[0].ToString());            
            IEnumerable<object> requests = values[1] as IEnumerable<object>;

            IEnumerable<object> enumerable = requests as IList<object> ?? requests.ToList();

            if (!enumerable.Any())
            {
                throw new ArgumentException(
                    string.Format("No requests are associated with chain id {0}", guid));
            }

            foreach(Request r in enumerable.AsEnumerable())
            {
                if(guid == r.ChainId)
                {
                    return string.Format(
                        "\tRoleSetsCount={0}  RequestsCount={1}  LastProcessedTime={2}  CreationTime={3}", 
                        r.Chain.RoleSetsCount,
                        r.Chain.RequestCount,
                        r.Chain.LastProcessedTime.HasValue ? (r.Chain.LastProcessedTime.Value.Equals(DateTime.MinValue) ? "N/A" : r.Chain.LastProcessedTime.ToString()) : null,
                        r.Chain.CreationTime);
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
