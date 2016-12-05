using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Professional_Gift_Card_System.Models;

namespace Professional_Gift_Card_System.Services
{
    public class HistoryServices
    {
        GiftEntities GiftEntity;

        public HistoryServices (GiftEntities GE)
        {
            GiftEntity = GE;
        }

        public SummaryInformation GetSummaryReport (DateTime EndDate, DateTime StartDate, String Period, String Reseller, String Chain, String MerchantGroup, String Merchant)
        {

            int GiftActivations = 0;
            decimal GiftActivationAmount = 0;
            int GiftSales = 0;
            decimal GiftSalesAmount = 0;
            int GiftCredits = 0;
            decimal GiftCreditAmount = 0;
            int GiftBalanceInquiries = 0;
            int Voids = 0;
            decimal NetGiftAmount = 0;
            SummaryInformation Results = new SummaryInformation();

            List<History> RawDailySales = GetHistoryData(EndDate, StartDate, Period, Reseller, Chain, MerchantGroup, Merchant);

            foreach (History RawDailySalesItem in RawDailySales)
            {
                if (RawDailySalesItem.ErrorCode != "APP  ") continue;  // ignore transactions with errors

                if (RawDailySalesItem.TransType == "ACTV") { GiftActivations++; GiftActivationAmount = GiftActivationAmount + RawDailySalesItem.Amount; }
                if (RawDailySalesItem.TransType == "SALE") { GiftSales++; GiftSalesAmount = GiftSalesAmount + RawDailySalesItem.Amount; }
                if (RawDailySalesItem.TransType == "GTIP") { GiftSales++; GiftSalesAmount = GiftSalesAmount + RawDailySalesItem.Amount; }
                if (RawDailySalesItem.TransType == "TPUP") { GiftSales++; GiftSalesAmount = GiftSalesAmount + RawDailySalesItem.Amount; }
                if (RawDailySalesItem.TransType == "CRED") { GiftCredits++; GiftCreditAmount = GiftCreditAmount + RawDailySalesItem.Amount; }
                if (RawDailySalesItem.TransType == "BALN") { GiftBalanceInquiries++; }


            }
            Results.GiftActivations = GiftActivations.ToString();
            Results.GiftActivationAmount = GiftActivationAmount.ToString();
            Results.GiftSales = GiftSales.ToString();
            Results.GiftSalesAmount = GiftSalesAmount.ToString();
            Results.GiftCredits = GiftCredits.ToString();
            Results.GiftCreditAmount = GiftCreditAmount.ToString();
            Results.GiftBalanceInquiries = GiftBalanceInquiries.ToString();
            Results.Voids = Voids.ToString();
            Results.NetGiftAmount = NetGiftAmount.ToString();

            return Results;
        }



        public List<DailyCountModel> GetTransactionCounts(DateTime EndDate, DateTime StartDate, String Period, String ByWhat)
        {

            
            List<DailyCountModel> Results = new List<DailyCountModel>();


            // get the raw history data

            List<History> RawDailySales = GetHistoryData(EndDate, StartDate, Period, "", "", "", "");


            // add up the data for each requested style

            DailyCountModel Totals = new DailyCountModel();
            Totals.EntityID = "";
            Totals.EntityName = "Over All";
            Totals.Level = 0;
            Totals.TotalCount = 0;

            foreach (History RawDailySalesItem in RawDailySales)
            {
                if (IsCountableTransaction(RawDailySalesItem)) Totals.TotalCount++;
            }

            Results.Add(Totals);
            return Results;
        }


        private bool IsCountableTransaction(History RawDailySalesItem)
        {
            if (RawDailySalesItem.TransType == "ACTV") return true;
            if (RawDailySalesItem.TransType == "SALE") return true;
            if (RawDailySalesItem.TransType == "GTIP") return true;
            if (RawDailySalesItem.TransType == "TPUP") return true;
            if (RawDailySalesItem.TransType == "CRED") return true;
            if (RawDailySalesItem.TransType == "BALN") return true;

            if (RawDailySalesItem.TransType == "LYSL") return true;
            if (RawDailySalesItem.TransType == "LYCR") return true;
            if (RawDailySalesItem.TransType == "LYRM") return true;
            if (RawDailySalesItem.TransType == "LYPR") return true;
            if (RawDailySalesItem.TransType == "LEVT") return true;
            if (RawDailySalesItem.TransType == "LVST") return true;
            if (RawDailySalesItem.TransType == "LYCL") return true;

            if (RawDailySalesItem.TransType == "VOID") return true;
            if (RawDailySalesItem.TransType == "DYRP") return true;
            if (RawDailySalesItem.TransType == "DTRP") return true;
            return false;
        }

        private List<History> GetHistoryData(DateTime EndDate, DateTime StartDate, String Period, String Reseller, String Chain, String MerchantGroup, String Merchant)
        {
            List<History> Results = new List<History>();
            HistoryDAO HistoryRepositoryInstance = new HistoryDAO(GiftEntity);
            return HistoryRepositoryInstance.GetHistoryForPeriod (EndDate, StartDate, Period, Reseller, Chain, MerchantGroup, Merchant);
        }
    }

}