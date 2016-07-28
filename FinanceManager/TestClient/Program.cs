using Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var securityToken = "7EE7CFD1D8F7405886D48E4C9F0585DF";
            var qe = new XigniteQuoter(securityToken: securityToken);
            var a = qe.GetQuote("MSFT", DateTime.Today);
            a = qe.GetQuote("MSFT", DateTime.Today.AddDays(-1));

            var filename = @"..\..\..\Portfolio\Portfolio.xml";
            var pr = PortfolioReader.ReadFromFile(filename);

            foreach(var p in pr.Positions)
            {
                //var dp = p.GetDailyProfit(qe);
                //var ip = p.GetInceptionToDateProfit(qe);
            }


        }
    }
}
