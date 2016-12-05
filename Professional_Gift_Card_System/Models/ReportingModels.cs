using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Professional_Gift_Card_System.Models
{
    public class MerchantIdentificationModel
    {
        public String MerchantName { get; set; }
        public String MerchantID { get; set; }
    }

    public class DailyCountModel
    {
        public int Level { get; set; }
        public String EntityName { get; set; }
        public String EntityID { get; set; }
        public int TotalCount { get; set; }
    }

    public class RawDailySalesItem
    {
        public long ID { get; set; }
        public DateTime WhenHappened { get; set; }
        public String MerchantID { get; set; }
        public String CardNumber { get; set; }
        public string TransType { get; set; }
        public decimal Amount { get; set; }
    }


    public class DailySalesInformation
    {
        public String MerchantName { get; set; }
        public char ResponseCode { get; set; }
        public String ErrorCode { get; set; }
        public DateTime When { get; set; }
        public DateTime LocalTime { get; set; }
        public Boolean GiftActive { get; set; }
        public int GiftActivations { get; set; }
        public Decimal GiftActivationAmount { get; set; }
        public int GiftSales { get; set; }
        public Decimal GiftSalesAmount { get; set; }
        public int GiftCredits { get; set; }
        public Decimal GiftCreditAmount { get; set; }
        public Decimal NetGiftAmount { get; set; }
    }


    public class SummaryInformation
    {
        public String GiftActivations { get; set; }
        public String GiftActivationAmount { get; set; }
        public String GiftSales { get; set; }
        public String GiftSalesAmount { get; set; }
        public String GiftCredits { get; set; }
        public String GiftCreditAmount { get; set; }
        public String GiftBalanceInquiries { get; set; }
        public String Voids { get; set; }
        public String NetGiftAmount { get; set; }

    }

    public class SummaryReportingData
    {

        public SummaryInformation ReportData { get; set; }
        public String Period { get; set; }
        [DateTime]
        public DateTime When { get; set; }
    }

    public class ReportTransactionCountModel
    {
        public String ByWhat { get; set; }
        public String Period { get; set; }
        [DateTime]
        public DateTime When { get; set; }
    }
}