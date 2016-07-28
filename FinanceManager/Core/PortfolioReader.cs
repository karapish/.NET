using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Core
{
    public class PortfolioReader
    {
        public static Portfolio ReadFromFile(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                return (new XmlSerializer(typeof(Portfolio))).Deserialize(reader) as Portfolio;
            }
        }
    }
}
