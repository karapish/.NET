using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xignite.Sdk.Api;
using Xignite.Sdk.Api.Models.XigniteGlobalHistorical;

namespace Core
{
    public class XigniteQuoter
    {
        private XigniteGlobalHistorical dataProvider;

        public XigniteQuoter(string securityToken)
        {
            this.dataProvider = new XigniteGlobalHistorical(securityToken);
        }

        public GlobalHistoricalQuote GetQuote(string symbol, DateTime asOf)
        {
            // TBD: Paid API after getting agreement from NASDAQ, but gives API to query HH:MM:SS
            // 
            //var result = this.dataProvider.GetBar(
            //    identifier: symbol,
            //    identifierType: IdentifierTypes.Symbol,
            //    endTime: asOf,
            //    useHttps: true);

            // Can only query symbol price at the closing
            var result = this.dataProvider.GetGlobalHistoricalQuote(
                identifier: symbol,
                identifierType: IdentifierTypes.Symbol,
                adjustmentMethod: AdjustmentMethods.DefaultValue,
                asOfDate: asOf.ToString("MM/dd/yyyy"),
                useHttps: true);

            return result;
        }
    }
}
