// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Professional_Gift_Card_System;
using Professional_Gift_Card_System.Models;

namespace Professional_Gift_Card_System.Services
{
    interface IHistoryDAO
    {
        History GetHistoryItem(Int64 ID);
        void DeleteHistoryItem(int ID);
        List<CardHistory> GetCardHistory(int CardID);
        List<CardHistory> GetCardHistory(String CardToSearchFor);
        List<RawDailySalesItem> GetDailyHistory(DateTime? When);
        List<CardHistory> GetMerchantHistory(int MerchantToList, int Page, int PageSize);


        gp_DailyReport_Result DailyReport(String MerchantID, String ClerkID, String WhereFrom, String MerchSeqNum, String TerminalID, DateTime LocalTime);
        DetailHistory DetailReport(String MerchantID, String ClerkID, String WhereFrom, String MerchSeqNum, String TerminalID, DateTime LocalTime);
        bool CloseDay(String MerchantID, String ClerkID, String WhereFrom, String MerchSeqNum, String TerminalID, DateTime LocalTime);

        DateTime GetFirstTransactionDate(String MerchantID);
        LastHistoryItem GetLastHistoryItem(String MerchantID, long TransactionID);
        List<DateTime> GetPastCloses(String MerchantID);
        gp_DailyReport_Result PriorDailyReport(String MerchantID, String ClerkID, String WhereFrom, DateTime EndingDate);
        void DailyReportToWeb(gp_DailyReport_Result Res, DailySalesInformation Web, DateTime When);
        List<String> Dump();
    }
    public class HistoryDAO : BaseDAO<History>, IHistoryDAO
    {
        public HistoryDAO() { }
        public HistoryDAO(GiftEntities nGiftEntity)
        {
            GiftEntity = nGiftEntity;
        }

        #region IHistoryRepository Members


        History IHistoryDAO.GetHistoryItem(Int64 ID)
        {
            InitializeConnection();
            History DBHistory = (from c in GiftEntity.Histories
                                 where c.ID == ID
                                 select c).FirstOrDefault();
            return DBHistory;
        }


        void IHistoryDAO.DeleteHistoryItem(int ID)
        {
            using (var GiftEntity = new GiftEntities())
            {
                History DBHistory = (from c in GiftEntity.Histories
                                     where c.ID == ID
                                     select c).FirstOrDefault();
                GiftEntity.Histories.Remove(DBHistory);
                GiftEntity.SaveChanges();
            }

        }


        List<CardHistory> IHistoryDAO.GetCardHistory(int CardID)
        {
            InitializeConnection();
            List<CardHistory> Histories = new List<CardHistory>();
            ICardRepository CardData = new CardRepository(GiftEntity);
            Card WhichCard = CardData.GetCard(CardID);
            if (WhichCard == null)
                return Histories;
            var CardHistories = from c in GiftEntity.Histories
                                //join c2 in GiftEntity.Cards on c.Card2 equals c2.ID
                                where c.CardGUID == WhichCard.CardGUID &&
                                      c.ErrorCode == "APP  " &&
                                      (c.TransType == "ACTV" ||
                                       c.TransType == "SALE" ||
                                       c.TransType == "GTIP" ||
                                       c.TransType == "CRED" ||
                                       c.TransType == "BALN" ||
                                       c.TransType == "TRAN" ||
                                       c.TransType == "TPUP" ||
                                       c.TransType == "LYSL" ||
                                       c.TransType == "LYCR" ||
                                       c.TransType == "LYRM" ||
                                       c.TransType == "LYPR" ||
                                       c.TransType == "LEVT" ||
                                       c.TransType == "LVST" ||
                                       c.TransType == "CUCR" ||
                                       c.TransType == "CUDB" ||
                                       c.TransType == "DEPL")
                                //||
                                //c.Card2 == WhichCard.ID
                                select new
                                {
                                    ID = c.ID,
                                    CardGUID = c.CardGUID,
                                    TransType = c.TransType,
                                    When = c.WhenHappened,
                                    Amount = c.Amount,
                                    MerchWhere = c.WhichMerchantGUID,
                                    Clrk = c.Clerk,
                                    Text = c.TransactionText,
                                    //Card2 = c2.CardNumber,
                                    InvoiceNumber = c.InvoiceNumber
                                };

            // need to restructure what we return to the calling program
            if (CardHistories == null)
                return Histories;

            foreach (var Hist in CardHistories)
            {
                string CardNumber = (from c in GiftEntity.Cards
                                     where c.CardGUID == Hist.CardGUID
                                     orderby c.ID descending
                                     select c.CardNumLast4).FirstOrDefault();
                CardHistory Chist = new CardHistory();
                Chist.ID = Hist.ID;
                Chist.CardNumber = CardNumber;
                Chist.MerchWhere = (from m in GiftEntity.Merchants
                                    where m.MerchantGUID == Hist.MerchWhere
                                    select m.MerchantName).FirstOrDefault();
                Chist.Clerk = Hist.Clrk;
                Chist.When = Hist.When;
                Chist.TransType = Hist.TransType;
                Chist.Transaction = ConvertTransactionType(Hist.TransType);
                Chist.Amount = Hist.Amount;
                Chist.Text = Hist.Text;
                //Chist.Card2 = Hist.Card2;
                Chist.InvoiceNumber = Hist.InvoiceNumber;
                Histories.Add(Chist);
            }
            return (Histories);

        }


        List<CardHistory> IHistoryDAO.GetCardHistory(String CardToSearchFor)
        {
            using (var GiftEntity = new GiftEntities())
            {
                List<CardHistory> Histories = new List<CardHistory>();
                ICardRepository CardData = new CardRepository(GiftEntity);
                Card WhichCard = CardData.GetCard(CardToSearchFor);
                if (WhichCard == null)
                    return Histories;
                var CardHistories = from c in GiftEntity.Histories
                                    //join c2 in GiftEntity.Cards on c.Card2 equals c2.ID
                                    where c.CardGUID == WhichCard.CardGUID //||
                                    //c.Card2 == WhichCard.ID
                                    select new
                                    {
                                        ID = c.ID,
                                        CardGUID = c.CardGUID,
                                        TransType = c.TransType,
                                        When = c.WhenHappened,
                                        Amount = c.Amount,
                                        MerchWhere = c.WhichMerchantGUID,
                                        Clrk = c.Clerk,
                                        Text = c.TransactionText,
                                        //Card2 = c2.CardNumber,
                                        InvoiceNumber = c.InvoiceNumber
                                    };

                // need to restructure what we return to the calling program
                if (CardHistories == null)
                    return Histories;

                foreach (var Hist in CardHistories)
                {
                    string CardNumber = (from c in GiftEntity.Cards
                                         where c.CardGUID == Hist.CardGUID
                                         orderby c.ID descending
                                         select c.CardNumLast4).FirstOrDefault();
                    CardHistory Chist = new CardHistory();
                    Chist.ID = Hist.ID;
                    Chist.CardNumber = CardNumber;
                    Chist.MerchWhere = (from m in GiftEntity.Merchants
                                        where m.MerchantGUID == Hist.MerchWhere
                                        select m.MerchantName).FirstOrDefault();
                    Chist.Clerk = Hist.Clrk;
                    Chist.When = Hist.When;
                    Chist.TransType = Hist.TransType;
                    Chist.Transaction = ConvertTransactionType(Hist.TransType);
                    Chist.Amount = Hist.Amount;
                    Chist.Text = Hist.Text;
                    //Chist.Card2 = Hist.Card2;
                    Chist.InvoiceNumber = Hist.InvoiceNumber;
                    Histories.Add(Chist);
                }
                return (Histories);
            }
        }


        public List<RawDailySalesItem> GetDailyHistory(DateTime? When)
        {
            List<RawDailySalesItem> Results = new List<RawDailySalesItem>();

            DateTime Today;
            if (When.HasValue)
                Today = When.Value.Date;
            else
                Today = DateTime.Now.Date.AddDays(-1);
            DateTime Tomorrow = Today.AddDays(1);

            InitializeConnection();
            // need to resolve any merchant sequence number issues


                // now get the details



                var DailyHistories = from h in GiftEntity.Histories
                                     where h.WhenHappened > Today &&
                                           h.WhenHappened < Tomorrow &&
                                           h.ErrorCode == "APP  " &&
                                           h.TransType != "DYRP" && // keep the daily report out of this
                                           h.TransType != "SHIP"    // keep the card shipments out of the report
                                     orderby h.ID
                                     //c.Card2 == WhichCard.ID
                                     select new
                                     {
                                         ID = h.ID,
                                         CardGUID = h.CardGUID,
                                         TransType = h.TransType,
                                         When = h.WhenHappened,
                                         Amount = h.Amount,
                                         PointsGranted = 0,
                                         MerchWhere = h.WhichMerchantGUID,
                                         Clrk = h.Clerk,
                                         Text = h.TransactionText,
                                         CardGUID2 = h.CardGUID2,
                                         CouponUsed = "",
                                         CouponIssued = "",
                                         PrizeAwardedGUID = "",
                                         InvoiceNumber = h.InvoiceNumber
                                     };
                foreach (var his in DailyHistories)
                {
                    RawDailySalesItem nHistory = new RawDailySalesItem();
                    nHistory.ID = his.ID;
                    nHistory.CardNumber = (from c in GiftEntity.Cards
                                           where c.CardGUID == his.CardGUID
                                           select c.CardNumLast4).FirstOrDefault();
                    nHistory.Amount = his.Amount;
                    Merchant tMerchant =  (from m in GiftEntity.Merchants
                                           where m.MerchantGUID == his.MerchWhere
                                           select m).FirstOrDefault();
                    if (tMerchant != null)
                    {
                        nHistory.MerchantID = tMerchant.MerchantID;
                    }
                    
                    nHistory.TransType = his.TransType;
                    nHistory.WhenHappened = his.When;

                    Results.Add(nHistory);
                }




            return Results;
        }


        public List<History> GetHistoryForPeriod (DateTime When, DateTime StartDate, String Period, String Reseller, String Chain, String MerchantGroup, String Merchant)
        {
            InitializeConnection();

            var query = from his in GiftEntity.Histories
                        select his;


            DateTime EndDate = When.AddDays(1);
            query = query.Where(his => his.WhenHappened < EndDate);

            DateTime ToStart = StartDate;
            switch (Period)
            {
                case null: break;
                case "Daily": break;
                case "Monthly": ToStart = new DateTime(When.Year, When.Month, 1); break;
                case "Quarter":
                    if (When.Month < 4) ToStart = new DateTime(When.Year, 1, 1);
                    if ((When.Month > 3) && (When.Month < 7)) ToStart = new DateTime(When.Year, 4, 1);
                    if ((When.Month > 6) && (When.Month < 10)) ToStart = new DateTime(When.Year, 7, 1);
                    if (When.Month > 9) ToStart = new DateTime(When.Year, 10, 1);
                    break;
                case "Annual": ToStart = new DateTime(When.Year, 1, 1);  break;
            }
            query = query.Where(his => his.WhenHappened > ToStart);

            return query.ToList();
        }


        List<CardHistory> IHistoryDAO.GetMerchantHistory(int MerchantToList, int Page, int PageSize)
        {
            using (var GiftEntity = new GiftEntities())
            {
                IMerchantDAO MerchantData = new MerchantDAO();
                List<CardHistory> Histories = new List<CardHistory>();
                ICardRepository CardData = new CardRepository(GiftEntity);
                Merchant MerchToList = MerchantData.GetMerchant(MerchantToList);
                if (MerchToList == null) return Histories;
                Guid MerchantToListGUID = MerchToList.MerchantGUID;

                var MerchHistories = (from m in GiftEntity.Histories
                                      where m.WhichMerchantGUID == MerchantToListGUID
                                      orderby m.ID descending
                                      select m
                                    ).Skip(Page * PageSize).Take(PageSize);

                // need to restructure what we return to the calling program
                if (MerchHistories == null)
                    return Histories;

                foreach (var Hist in MerchHistories)
                {
                    string CardNumber = (from c in GiftEntity.Cards
                                         where c.CardGUID == Hist.CardGUID
                                         select c.CardNumLast4).FirstOrDefault();
                    CardHistory Chist = new CardHistory();
                    Chist.ID = Hist.ID;
                    Chist.CardNumber = CardNumber;
                    Chist.MerchWhere = (from m in GiftEntity.Merchants
                                        where m.MerchantGUID == Hist.WhichMerchantGUID
                                        select m.MerchantName).FirstOrDefault();
                    Chist.Clerk = Hist.Clerk;
                    Chist.When = Hist.WhenHappened;
                    Chist.TransType = Hist.TransType;
                    Chist.Transaction = ConvertTransactionType(Hist.TransType);
                    Chist.Amount = Hist.Amount;
                    Chist.Text = Hist.TransactionText;
                    //Chist.Card2 = Hist.Card2;
                    Chist.InvoiceNumber = Hist.InvoiceNumber;
                    Histories.Add(Chist);
                }
                return (Histories);
            }
        }












        gp_DailyReport_Result IHistoryDAO.DailyReport(String MerchantID, String ClerkID, String WhereFrom, String MerchSeqNum, String TerminalID, DateTime LocalTime)
        {
            InitializeConnection();
            gp_DailyReport_Result Res = GiftEntity.gp_DailyReport(MerchantID, ClerkID, "A", WhereFrom, MerchSeqNum,TerminalID, LocalTime).FirstOrDefault();
            return Res;


        }




        // Yes, there was a stored procedure to do the detail report, 
        // but it returns two result sets and the second one can have 
        // a bunch of records in it. 
        DetailHistory IHistoryDAO.DetailReport(String MerchantID, String ClerkID, String WhereFrom, String MerchSeqNum, String TerminalID, DateTime LocalTime)
        {
            InitializeConnection();
            // need to resolve any merchant sequence number issues



            DetailHistory Results = new DetailHistory();
            Results.DetailItems = new List<CardHistory>();


            gp_DailyReport_Result Res = GiftEntity.gp_DailyReport(MerchantID, ClerkID, "E", WhereFrom, MerchSeqNum, TerminalID, LocalTime).FirstOrDefault();

            if (Res.ResponseCode == "A")
            {
                Results.When = DateTime.Now;
                Results.ResponseCode = Res.ResponseCode[0];
                Results.ErrorCode = (string)Res.ErrorCode;
                Results.Summary = new DailySalesInformation();
                DailyReportToWeb(Res, Results.Summary, LocalTime);


                // now get the details

                Merchant Merch = GiftEntity.Merchants.FirstOrDefault(d => d.MerchantID == MerchantID);

                var LastRun = (from h in GiftEntity.Histories
                               where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                     h.TransType == "CLOS"
                               orderby h.ID descending
                               select h.ID).FirstOrDefault();
                //if (LastRun == null)
                //    LastRun = 0;

                var DailyHistories = from h in GiftEntity.Histories
                                     where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                           h.ID > LastRun &&
                                           h.ErrorCode == "APP  " &&
                                           h.TransType != "DYRP" && // keep the daily report out of this
                                           h.TransType != "SHIP"    // keep the card shipments out of the report
                                     orderby h.ID
                                     //c.Card2 == WhichCard.ID
                                     select new
                                     {
                                         ID = h.ID,
                                         CardGUID = h.CardGUID,
                                         TransType = h.TransType,
                                         When = h.WhenHappened,
                                         LocalTime = h.LocalTime,
                                         Amount = h.Amount,
                                         PointsGranted = 0,
                                         MerchWhere = h.WhichMerchantGUID,
                                         Clrk = h.Clerk,
                                         Text = h.TransactionText,
                                         CardGUID2 = h.CardGUID2,
                                         CouponUsed = 0,
                                         CouponIssued = 0,
                                         PrizeAwardedGUID = Guid.NewGuid(),
                                         InvoiceNumber = h.InvoiceNumber
                                     };
                foreach (var his in DailyHistories)
                {
                    CardHistory nHistory = new CardHistory();
                    nHistory.ID = his.ID;
                    nHistory.CardNumber = (from c in GiftEntity.Cards
                                           where c.CardGUID == his.CardGUID
                                           select c.CardNumLast4).FirstOrDefault().ToString();
                    nHistory.Amount = his.Amount;
                    nHistory.Clerk = his.Clrk;
                    nHistory.MerchWhere = (from m in GiftEntity.Merchants
                                           where m.MerchantGUID == his.MerchWhere
                                           select m.MerchantName).FirstOrDefault();
                    nHistory.TransType = his.TransType;
                    nHistory.Transaction = ConvertTransactionType(his.TransType);
                    nHistory.When = his.When;
                    if (his.LocalTime.HasValue)
                        nHistory.LocalTime = his.LocalTime.Value;
                    nHistory.Text = his.Text;
                    nHistory.PointsGranted = his.PointsGranted;
                    if (his.CardGUID2 != null)
                        nHistory.Card2 = GiftEntity.Cards
                            .FirstOrDefault(c => c.CardGUID == his.CardGUID2).CardNumLast4;
                    nHistory.InvoiceNumber = his.InvoiceNumber;

                    Results.DetailItems.Add(nHistory);
                }



            }
            else
            {
                Results.ResponseCode = Res.ResponseCode[0];
                Results.ErrorCode = (string)Res.ErrorCode;
            }
            return Results;
        }



        public void DailyReportToWeb(gp_DailyReport_Result Res, DailySalesInformation Web, DateTime When)
        {
            Web.When = When;
            Web.ResponseCode = (char)Res.ResponseCode[0];
            Web.ErrorCode = (string)Res.ErrorCode;
            Web.GiftActive = false;
            if (Res.GiftActive.Length > 0)
                if (Res.GiftActive[0] == 'A')
                    Web.GiftActive = true;
            Web.GiftActivations = 0;
            if (Res.GiftActivates.HasValue)
                Web.GiftActivations = Res.GiftActivates.Value;
            Web.GiftActivationAmount = 0.00M;
            if (Res.GiftActiveTotal.HasValue)
                Web.GiftActivationAmount = Res.GiftActiveTotal.Value;
            Web.GiftSales = 0;
            if (Res.GiftSales.HasValue)
                Web.GiftSales = Res.GiftSales.Value;
            Web.GiftSalesAmount = 0.00M;
            if (Res.GiftSalesTotal.HasValue)
                Web.GiftSalesAmount = Res.GiftSalesTotal.Value;
            Web.GiftCredits = 0;
            if (Res.GiftCredits.HasValue)
                Web.GiftCredits = Res.GiftCredits.Value;
            Web.GiftCreditAmount = 0.00M;
            if (Res.GiftCreditTotal.HasValue)
                Web.GiftCreditAmount = Res.GiftCreditTotal.Value;
            Web.NetGiftAmount = 0;
            if (Res.NetGiftTotal.HasValue)
                Web.NetGiftAmount = Res.NetGiftTotal.Value;

        }





        bool IHistoryDAO.CloseDay(String MerchantID, String ClerkID, String WhereFrom, String MerchSeqNum, String TerminalID, DateTime LocalTime)
        {
            InitializeConnection();

           
                gp_Close_Result Res = GiftEntity.gp_Close(MerchantID, ClerkID, WhereFrom, MerchSeqNum, TerminalID, LocalTime).FirstOrDefault();

                if (Res.ResponseCode == "A")
                    return true;
                return false;
        }


        // get the list of past day's closes

        List<DateTime> IHistoryDAO.GetPastCloses(String MerchantID)
        {
            InitializeConnection();
            Merchant Merch = GiftEntity.Merchants.FirstOrDefault(d => d.MerchantID == MerchantID);

            var LastRun = (from h in GiftEntity.Histories
                           where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                 h.TransType == "CLOS"
                           orderby h.ID descending
                           select new
                           {
                               h.ID,
                               h.WhenHappened,
                               h.LocalTime
                           }
                               ).Take(20);

            List<DateTime> Results = new List<DateTime>();
            foreach (var LR in LastRun)
            {
                if (LR.LocalTime.HasValue)
                    Results.Add(LR.LocalTime.Value);
                else
                    Results.Add(LR.WhenHappened);
            }
            return Results;
        }



        // Do the prior day's report again


        gp_DailyReport_Result IHistoryDAO.PriorDailyReport(String MerchantID, String ClerkID, String WhereFrom, DateTime EndingDate)
        {
            char GiftActive = 'N';
            char LoyaltyActive = 'N';

            int GiftActivations = 0;
            Decimal GiftActivationsTotal = 0.00M;
            int GiftSales = 0;
            Decimal GiftSalesTotal = 0.00M;
            int GiftCredits = 0;
            Decimal GiftCreditsTotal = 0.00M;
            Decimal NetGiftTotal = 0.00M;

            InitializeConnection();
            gp_DailyReport_Result Res = new gp_DailyReport_Result();

            Res.ReceiptTime = DateTime.Now;
            Res.ErrorCode = "APP";

            Merchant Merch = GiftEntity.Merchants.FirstOrDefault(d => d.MerchantID == MerchantID);

            // default to zero and the first transaction from this merchant
            long StartingID = 0;
            DateTime StartingTime = (from h in GiftEntity.Histories
                                     where h.WhichMerchantGUID == Merch.MerchantGUID
                                     orderby h.ID
                                     select h.WhenHappened).First();

            // although we have an ending date, 
            // that is lacking the seconds and will cause problems.
            // find the ID of the close in question
            // Yes, if someone did two closes within a minute, this will give wrong results
            // but that is not something that happens often

            DateTime CompareTime = EndingDate.AddMinutes(1);
            long EndingID = (from h in GiftEntity.Histories
                             where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                   h.TransType == "CLOS" &&
                                   h.WhenHappened <= CompareTime
                             orderby h.ID descending
                             select h.ID).First();
                           
                          

            var LastRun = (from h in GiftEntity.Histories
                           where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                 h.TransType == "CLOS" &&
                                 h.WhenHappened <= CompareTime
                           orderby h.ID descending
                           select new
                           {
                               h.ID,
                               h.WhenHappened
                           }
                               ).Take(2).ToList();

            // see if we are to start within a range

            if (LastRun.Count() == 2)
            {
                StartingID = LastRun[1].ID;
                StartingTime = LastRun[1].WhenHappened;
            }


            // get what services are active

            GiftActive = Merch.GiftActive[0];

            // do the summations

            if (GiftActive == 'A')
            {
                GiftActivations = (from h in GiftEntity.Histories
                                           where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                           h.WhenHappened > StartingTime &&
                                           h.ID < EndingID &&
                                           h.ErrorCode == "APP  " &&
                                           h.TransType == "ACTV"
                                           select h.Amount).Count();

                GiftActivationsTotal = (from h in GiftEntity.Histories
                                           where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                           h.WhenHappened > StartingTime &&
                                           h.ID < EndingID &&
                                           h.ErrorCode == "APP  " &&
                                           h.TransType == "ACTV"
                                        select h).Sum(h => (decimal?)(h.Amount)) ?? 0;

                GiftSales = (from h in GiftEntity.Histories
                                           where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                           h.WhenHappened > StartingTime &&
                                           h.ID < EndingID &&
                                           h.ErrorCode == "APP  " &&
                                           h.TransType == "SALE"
                                           select h.Amount).Count();

                GiftSalesTotal = (from h in GiftEntity.Histories
                                           where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                           h.WhenHappened > StartingTime &&
                                           h.ID < EndingID &&
                                           h.ErrorCode == "APP  " &&
                                           h.TransType == "SALE"
                                        select h).Sum(h => (decimal?)(h.Amount)) ?? 0;


                GiftCredits = (from h in GiftEntity.Histories
                                           where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                           h.WhenHappened > StartingTime &&
                                           h.ID < EndingID &&
                                           h.ErrorCode == "APP  " &&
                                           h.TransType == "CRED"
                                           select h.Amount).Count();

                GiftCreditsTotal = (from h in GiftEntity.Histories
                                           where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                           h.WhenHappened > StartingTime &&
                                           h.ID < EndingID &&
                                           h.ErrorCode == "APP  " &&
                                           h.TransType == "CRED"
                                        select h).Sum(h => (decimal?)(h.Amount)) ?? 0;

                NetGiftTotal = GiftActivationsTotal - GiftSalesTotal + GiftCreditsTotal;
            }


            int Voids = (from h in GiftEntity.Histories
                                           where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                           h.WhenHappened > StartingTime &&
                                           h.ID < EndingID &&
                                           h.ErrorCode == "APP  " &&
                                           h.TransType == "VOID"
                                           select h.ID).Count();


            Res.GiftActive = GiftActive.ToString();
            Res.GiftSales = GiftSales;
            Res.GiftSalesTotal = GiftSalesTotal;
            Res.GiftCredits = GiftCredits;
            Res.GiftCreditTotal = GiftCreditsTotal;
            Res.GiftActivates = GiftActivations;
            Res.GiftActiveTotal = GiftActivationsTotal;
            Res.NetGiftTotal = NetGiftTotal;
            Res.Voids = Voids;
            Res.LoyaltyActive = LoyaltyActive.ToString();

            Res.ResponseCode = "A";
            return Res;
        }


        DateTime IHistoryDAO.GetFirstTransactionDate(String MerchantID)
        {
            DateTime Results = DateTime.Now;

            using (var GiftEntity = new GiftEntities())
            {
                // start by validating the merchant id

                Merchant Merch = GiftEntity.Merchants.FirstOrDefault(d => d.MerchantID == MerchantID);
                if (Merch == null) return Results;

                // get the last history item

                History LastHistory = (from h in GiftEntity.Histories
                                       where h.WhichMerchantGUID == Merch.MerchantGUID
                                       orderby h.ID
                                       select h).FirstOrDefault();
                if (LastHistory == null)
                    return Results;

                Results = LastHistory.WhenHappened;
            }
            return Results;
        }


        LastHistoryItem IHistoryDAO.GetLastHistoryItem(String MerchantID, long TransactionID)
        {
            LastHistoryItem Results = new LastHistoryItem();
            Results.LastItem = new CardHistory();
            Results.ResponseCode = 'E';
            Results.ErrorCode = "MIDER";  // - merchant id error

            using (var GiftEntity = new GiftEntities())
            {
                // start by validating the merchant id

                Merchant Merch = GiftEntity.Merchants.FirstOrDefault(d => d.MerchantID == MerchantID);
                if (Merch == null) return Results;

                // get the last history item

                History LastHistory;
                if (TransactionID != 0L)
                {
                    LastHistory = (from h in GiftEntity.Histories
                                   where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                         h.ErrorCode == "APP  " &&
                                         h.ID == TransactionID
                                   select h).FirstOrDefault();
                }
                else
                {
                    LastHistory = (from h in GiftEntity.Histories
                                   where h.WhichMerchantGUID == Merch.MerchantGUID &&
                                         h.ErrorCode == "APP  "
                                   orderby h.ID descending
                                   select h).FirstOrDefault();
                }
                if (LastHistory == null)
                {
                    Results.ErrorCode = "NOTRN";
                    return Results;
                }

                // get the rest of the information

                Results.LastItem.CardNumber = (from c in GiftEntity.Cards
                                               where c.CardGUID == LastHistory.CardGUID
                                               select c.CardNumLast4).FirstOrDefault();
                Results.LastItem.TransType = LastHistory.TransType;
                Results.LastItem.Transaction = ConvertTransactionType(LastHistory.TransType);
                Results.LastItem.When = LastHistory.WhenHappened;
                Results.LastItem.Amount = LastHistory.Amount;
                Results.LastItem.Text = LastHistory.TransactionText;
                Results.ResponseCode = 'A';
            }
            return Results;
        }



        List<String> IHistoryDAO.Dump()
        {
            List<String> Results = new List<string>();
            using (var GiftEntity = new GiftEntities())
            {
                var Hist = (from h in GiftEntity.Histories
                            select h);
                foreach (History h in Hist)
                {
                    String Guid2 = "";
                    if (h.CardGUID2 != null)
                        Guid2 = h.CardGUID2.ToString();
                    String HistLine = "INSERT INTO HISTORY " +
                        "(WhenHappened, CardGUID, WhichMerchantGUID, MerchSeqNumber, Clerk, WebCellOrDialup, " +
                        "TransType, TransactionText, Amount, CardGUID2, PointsGranted," +
                        "RewardGivenGUID, RewardUsedGUID, CouponIssued, CouponUsed, PrizeAwardedGUID, PrizeTakenGUID, InvoiceNumber) " +
                        Environment.NewLine +
                        "Values (" +
                        "'" + h.WhenHappened.ToString() + "'," +
                        "'" + h.CardGUID.ToString() + "'," +
                        "'" + h.WhichMerchantGUID.ToString() + "'," +
                        "'" + h.MerchSeqNumber.ToString() + "'," +
                        "'" + h.Clerk + "'," +
                        "'" + h.WebCellOrDialup + "'," +
                        "'" + h.TransType + "'," +
                        "'" + h.TransactionText + "'," +
                        "'" + h.Amount.ToString() + "'," +
                        "'" + Guid2 + "'," +
                        "'" + h.InvoiceNumber + "'"
                        + ");"
                        ;

                    Results.Add(HistLine);
                }
            }
            return Results;
        }


        string ConvertTransactionType(String TransType)
        {
            switch (TransType)
            {
                case ("ADDC"): return ("add card");
                case ("SHIP"): return ("ship card to merchant");

                case ("ACTV"): return ("activate");
                case ("SALE"): return ("sell item using this card");
                case ("GTIP"): return ("tip from this card");
                case ("CRED"): return ("credit to the card");
                case ("DECC"): return ("deactivate");
                case ("BALN"): return ("balance inquiry");
                case ("TRAN"): return ("balance transfer");
                case ("TPUP"): return ("top up");

                case ("LYSL"): return ("loyalty sale");
                case ("LYCR"): return ("loyalty credit");
                case ("LYPR"): return ("loyalty purchase reward");
                case ("LYRM"): return ("loyalty redeem");
                case ("LEVT"): return ("loyalty event activity");
                case ("LVST"): return ("loyalty visit");

                case ("DYRP"): return ("daily report");
                case ("DTRP"): return ("detail report");
                case ("CLOS"): return ("a daily close");
                case ("CLSH"): return ("close shift");      // not implemented
                case ("VOID"): return ("a void transaction");
                case ("VDAC"): return ("a voided activate");
                case ("VDSL"): return ("a voided sale");
                case ("VDCR"): return ("a voided credit");
                case ("VDUP"): return ("a voided top up");
                case ("VDSH"): return ("a voided shipment");
                case ("VDEC"): return ("a voided deactivate");
                case ("VDTR"): return ("a voided balance transfer");
                case ("VDLS"): return ("a voided loyalty sale");
                case ("VDLC"): return ("a voided loyalty credit");
                case ("VDLR"): return ("a voided loyalty redemption");
                case ("VDLP"): return ("a voided loyalty purchase reward");
                case ("VEVT"): return ("a voided loyalty event activity");
                case ("VVST"): return ("a voided loyalty visit");

                case ("RVAC"): return ("a reversed activate");           //not implemented
                case ("RVSL"): return ("a reversed sale");               //not implemented
                case ("RVCR"): return ("a reversed credit");             // not implemented
                case ("RVUP"): return ("a reversed top up");             // not implemented
                case ("RVSH"): return ("a reversed shipment");           // not implemented
                case ("RVDC"): return ("a reversed deactivate");         // not implemented
                case ("RVTR"): return ("a reversed balance transfer");   // not implemented
                case ("RVCL"): return ("a reversed close");              // not implemented
                case ("CUHI"): return ("a customer history inquiry");
                case ("CUCR"): return ("a customer support initiated credit");
                case ("CUDB"): return ("a customer support initiated debit");
                case ("DEPL"): return ("a depletion action");

            }
            return "";
        }


        #endregion
    }
}