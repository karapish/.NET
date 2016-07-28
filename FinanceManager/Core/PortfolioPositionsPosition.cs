using Core;
using System;
using System.ComponentModel;
using Xignite.Sdk.Api.Models.XigniteGlobalHistorical;

namespace Core
{
    public partial class PortfolioPositionsPosition
    {
        private GlobalHistoricalQuote latestQuote;
        private double? valueAtInceptionDay = null;

        public GlobalHistoricalQuote GetLatestQuote(XigniteQuoter quoter)
        {
            return quoter.GetQuote(this.tickerField, DateTime.Now);
        }

        public void Update(XigniteQuoter quoter)
        {
            if (!valueAtInceptionDay.HasValue)
                this.valueAtInceptionDay = quoter.GetQuote(this.tickerField, DateTime.Parse(inceptionDateField)).Last.Value;

            this.latestQuote = this.GetLatestQuote(quoter);
        }

        public double PricePerShare
        {
            get
            {
                return this.latestQuote.Last.Value;
            }
        }
        public double DailyProfit
        {
            get
            {
                return this.latestQuote.ChangeFromOpen.Value;
            }
        }

        public double InceptionPricePerShare
        {
            get
            {
                return this.valueAtInceptionDay.Value;
            }
        }

        public double InceptionToDateProfit
        {
            get
            {
                return double.Parse(this.sharesField) * (this.latestQuote.Last.Value - this.valueAtInceptionDay.Value);
            }
        }
    }
}