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
using System.Text;
using Professional_Gift_Card_System;
using Professional_Gift_Card_System.Models;

namespace Professional_Gift_Card_System.Services
{
    #region Services
    public interface ITransactionService
    {
        ReceiptInformation ActivateGiftCard(
            String Merchant, String ClerkID, 
            Char WebOrDial, String MerchantSequenceNumber, 
            String TerminalID, DateTime LocalTime,
            String CardToActivate, Decimal Amount, String InvoiceNumber);
        ReceiptInformation SellGiftCard(
            String MerchantID, String ClerkID, 
            Char WebOrDial, String MerchantSequenceNumber, 
            String TerminalID, DateTime LocalTime,
            String Card, Decimal Amount, String InvoiceNumber, String Description);
        ReceiptInformation GiftCardReturn(
            String MerchantID, String ClerkID, 
            Char WebOrDial, String MerchantSequenceNumber, 
            String TerminalID, DateTime LocalTime,
            String Card, Decimal Amount, String InvoiceNumber, String Description);
        ReceiptInformation GiftCardInquiry(
            String MerchantID, String ClerkID, 
            Char WebOrDial, String MerchantSequenceNumber, 
            String TerminalID, DateTime LocalTime,
            String Card);


        ReceiptInformation LastTransaction(String MerchantID, long TransactionID);

        Receipt FormatGiftReceipt(ReceiptInformation RecInfo);
        Receipt FormatLoyaltyReceipt(ReceiptInformation RecInfo);
        DailySalesInformation DailyReport(
            String MerchantID, String ClerkID,
            char WhereFrom, String MerchSeqNum,
            String TerminalID, DateTime LocalTime);
        List<PriorDays> GetPriorCloses(String MerchantID);
        DailySalesInformation PriorDailyReport(
            String MerchantID, String ClerkID,
            char WhereFrom, 
            String TerminalID, DateTime LocalTime, 
            String EndingDate);

        Receipt FormatDailyReport(DailySalesInformation ReportInfo);
        DetailReportInformation DetailReport(
            String MerchantID, String ClerkID, 
            char WhereFrom, String MerchSeqNum, 
            String TerminalID, DateTime LocalTime);
        Receipt FormatDetailReport(DetailReportInformation ReportInfo);
        bool CloseBatch(
            String MerchantID, String ClerkID,
            char WhereFrom, String MerchSeqNum, 
            String TerminalID, DateTime LocalTime);
        Receipt FormatCloseBatch(DateTime LocalTime);

    }

    public class TransactionService : ITransactionService
    {
        #region GiftTransactions


        //  A c t i v a t e G i f t C a r d

        ReceiptInformation ITransactionService.ActivateGiftCard(String MerchantID, String ClerkID, Char WebOrDial, String MerchantSequenceNumber, String TerminalID, DateTime LocalTime, String CardToActivate, Decimal Amount, String InvoiceNumber)
        {
            ICardRepository CardData = new CardRepository();
            gp_GiftActivateCard_Result Res = CardData.ActivateGiftCard(
                MerchantID, ClerkID, Convert.ToString(WebOrDial), MerchantSequenceNumber, TerminalID,
                LocalTime,
                CardToActivate, Amount, InvoiceNumber);

            ReceiptInformation ReceiptInfo = new ReceiptInformation();
            ReceiptInfo.ResponseCode = (char)Res.ResponseCode[0];
            ReceiptInfo.ErrorCode = Res.ErrorCode;
            if (ReceiptInfo.ResponseCode == 'A')
            {
                ReceiptInfo.Amount = Amount;
                ReceiptInfo.MerchantID = MerchantID;
                ReceiptInfo.ClerkID = ClerkID;
                ReceiptInfo.Description = "Card Activation"; // 
                ReceiptInfo.CardNumber = CardToActivate;
                ReceiptInfo.TransactionNumber = (long)Res.TranNumber;
                ReceiptInfo.TransactionType = "Activate";
                ReceiptInfo.When = (DateTime)Res.ReceiptTime;
                ReceiptInfo.LocalTime = LocalTime;
                ReceiptInfo.Balance = Amount;
            }
            return ReceiptInfo;
        }


        // S e l l G i f t C a r d

        ReceiptInformation ITransactionService.SellGiftCard(
            String MerchantID, String ClerkID, Char WebOrDial,
            String MerchantSequenceNumber, String TerminalID, DateTime LocalTime,
            String CardNumber, Decimal Amount, String InvoiceNumber, String Description)
        {
            ReceiptInformation ReceiptInfo = new ReceiptInformation();

            String CardToUse;
            if (!MapPhoneToCardNumber(MerchantID, CardNumber, out CardToUse))
            {
                ReceiptInfo.ErrorCode = "PHNER";
                ReceiptInfo.ResponseCode = 'E';
                ReceiptInfo.ApprovalMessage = "Phone Number not found";
                return ReceiptInfo;
            }
            CardNumber = CardToUse;
            ICardRepository CardData = new CardRepository();
            gp_GiftSellFromCard_Result Res = CardData.GiftCardSale(
                MerchantID, ClerkID, Convert.ToString(WebOrDial),
                MerchantSequenceNumber, TerminalID, LocalTime,
                CardNumber, Amount, InvoiceNumber, Description);

            ReceiptInfo.ResponseCode = (char)Res.ResponseCode[0];
            ReceiptInfo.ErrorCode = Res.ErrorCode;
            if (ReceiptInfo.ResponseCode == 'A')
            {
                ReceiptInfo.Amount = Res.TransactionAmount;
                ReceiptInfo.Remainder = Res.Remainder;
                ReceiptInfo.MerchantID = MerchantID;
                ReceiptInfo.ClerkID = ClerkID;
                ReceiptInfo.Balance = (Decimal)Res.Balance;
                ReceiptInfo.Description = Description; // 
                ReceiptInfo.CardNumber = CardNumber;
                ReceiptInfo.TransactionNumber = (long)Res.TranNumber;
                ReceiptInfo.TransactionType = "SALE";
                ReceiptInfo.When = (DateTime)Res.ReceiptTime;
                ReceiptInfo.LocalTime = LocalTime;
            }
            return ReceiptInfo;
        }



        // G i f t C a r d R e t u r n

        ReceiptInformation ITransactionService.GiftCardReturn(
            String MerchantID, String ClerkID, Char WebOrDial,
            String MerchantSequenceNumber, String TerminalID, DateTime LocalTime,
            String CardNumber, Decimal Amount, String InvoiceNumber, String Description)
        {
            ReceiptInformation ReceiptInfo = new ReceiptInformation();

            String CardToUse;
            if (!MapPhoneToCardNumber(MerchantID, CardNumber, out CardToUse))
            {
                ReceiptInfo.ErrorCode = "PHNER";
                ReceiptInfo.ResponseCode = 'E';
                ReceiptInfo.ApprovalMessage = "Phone Number not found";
                return ReceiptInfo;
            }
            CardNumber = CardToUse;

            ICardRepository CardData = new CardRepository();
            gp_GiftReturn_Result Res = CardData.GiftCardReturn(
                MerchantID, ClerkID,
                Convert.ToString(WebOrDial), MerchantSequenceNumber,
                TerminalID, LocalTime,
                CardNumber, Amount, Description, InvoiceNumber);

            ReceiptInfo.ResponseCode = (char)Res.ResponseCode[0];
            ReceiptInfo.ErrorCode = Res.ErrorCode;
            if (ReceiptInfo.ResponseCode == 'A')
            {
                ReceiptInfo.Amount = Amount;
                ReceiptInfo.MerchantID = MerchantID;
                ReceiptInfo.ClerkID = ClerkID;
                ReceiptInfo.Balance = (Decimal)Res.Balance;
                ReceiptInfo.Description = Description; // 
                ReceiptInfo.CardNumber = CardNumber;
                ReceiptInfo.TransactionNumber = (long)Res.TranNumber;
                ReceiptInfo.TransactionType = "RETURN";
                ReceiptInfo.When = (DateTime)Res.ReceiptTime;
                ReceiptInfo.LocalTime = LocalTime;
            }
            return ReceiptInfo;
        }


        // G i f t C a r d I n q u i r y

        ReceiptInformation ITransactionService.GiftCardInquiry(
            String MerchantID, String ClerkID, Char WebOrDial,
            String MerchantSequenceNumber, String TerminalID, DateTime LocalTime,
            String CardNumber)
        {
            ReceiptInformation ReceiptInfo = new ReceiptInformation();

            String CardToUse;
            if (!MapPhoneToCardNumber(MerchantID, CardNumber, out CardToUse))
            {
                ReceiptInfo.ErrorCode = "PHNER";
                ReceiptInfo.ResponseCode = 'E';
                ReceiptInfo.ApprovalMessage = "Phone Number not found";
                return ReceiptInfo;
            }
            CardNumber = CardToUse;


            ICardRepository CardData = new CardRepository();
            gp_GiftBalanceInq_Result Res = CardData.GiftCardInquiry(
                MerchantID, ClerkID,
                Convert.ToString(WebOrDial), MerchantSequenceNumber,
                TerminalID, LocalTime,
                CardNumber);

            ReceiptInfo.ResponseCode = (char)Res.ResponseCode[0];
            ReceiptInfo.ErrorCode = Res.ErrorCode;
            if (ReceiptInfo.ResponseCode == 'A')
            {
                ReceiptInfo.MerchantID = MerchantID;
                ReceiptInfo.ClerkID = ClerkID;
                ReceiptInfo.Balance = (Decimal)Res.Balance;
                ReceiptInfo.CardNumber = CardNumber;
                ReceiptInfo.TransactionNumber = (long)Res.TranNumber;
                ReceiptInfo.TransactionType = "BALANCE INQUIRY";
                ReceiptInfo.When = (DateTime)Res.ReceiptTime;
                ReceiptInfo.LocalTime = LocalTime;
            }
            return ReceiptInfo;
        }











        ReceiptInformation ITransactionService.LastTransaction(String MerchantID, long TransactionID)
        {
            ReceiptInformation Results = new ReceiptInformation();
            IHistoryDAO TransactionHistoryRepository = new HistoryDAO();
            LastHistoryItem LastHistory = TransactionHistoryRepository.GetLastHistoryItem(MerchantID, TransactionID);
            if (LastHistory.ResponseCode != 'A')
            {
                Results.ErrorCode = LastHistory.ErrorCode;
                return Results;
            }
            Results.TransactionType = LastHistory.LastItem.Transaction;
            Results.When = LastHistory.LastItem.When;
            Results.CardNumber = "XXXXXXXXXXXXX" + LastHistory.LastItem.CardNumber;
            Results.Amount = LastHistory.LastItem.Amount;
            Results.Description = LastHistory.LastItem.Text;
            return Results;
        }

        // Routine to map phone number in card number field into card
        // this uses simply the first card registered to that phone number
        // that is valid at this merchant

        private bool MapPhoneToCardNumber (String MerchantID, String EnteredCardNumber, out String CardNumber)
        {

            CardNumber = EnteredCardNumber;

            // first, if this does not look like a phone number, return it.

            if (EnteredCardNumber.Length != 13) return true;
            if (EnteredCardNumber.Substring(0, 3) != "000") return true;

            // ok, we have something that looks like it might be a phone number

            String PhoneNumber = EnteredCardNumber.Substring(3);
            ICardHolderRepository CardHolderData = new CardHolderRepository();
            String CardToUse;
            if (!CardHolderData.GetCardForTransaction(MerchantID, PhoneNumber, out CardToUse)) return false;
            if (CardToUse.Trim().Length > 0)
            {
                CardNumber = CardToUse.Trim();
                return true;
            }

            return true;
        }


        #endregion








        #region Reports

        //****************************************************************
        //
        //            R e p o r t s


        /// <summary>
        /// Daily Report
        /// </summary>
        /// <param name="MerchantID"></param>
        /// <param name="ClerkID"></param>
        /// <param name="MerchSeqNum"></param>
        /// <param name="WhereFrom"></param>
        /// <returns></returns>
        DailySalesInformation ITransactionService.DailyReport(
            String MerchantID, String ClerkID, char WhereFrom,
            String MerchSeqNum, String TerminalID, DateTime LocalTime)
        {
            DailySalesInformation DSI = new DailySalesInformation();
            IHistoryDAO IHR = new HistoryDAO();
            gp_DailyReport_Result DRR = IHR.DailyReport(MerchantID, ClerkID, Convert.ToString(WhereFrom), MerchSeqNum, TerminalID, LocalTime);

            IHR.DailyReportToWeb(DRR, DSI, LocalTime);
            return DSI;
        }


        //   Get prior day close times for the last X days
        List<PriorDays> ITransactionService.GetPriorCloses (String MerchantID)
        {
            IHistoryDAO IHR = new HistoryDAO();
            List<PriorDays> Results = new List<PriorDays>();
            List<DateTime> OldCloses = IHR.GetPastCloses(MerchantID);
            foreach(DateTime OC in OldCloses)
            {
                PriorDays tDate = new PriorDays();
                tDate.DisplayDate = OC.Month.ToString() + "/" + OC.Day.ToString("00") + "/" + OC.Year.ToString() + "." +
                            OC.Hour.ToString() + ":" + OC.Minute.ToString("00");
                tDate.RequestDate = OC.Year.ToString() + "-" + OC.Month.ToString("00") + "-" + OC.Day.ToString("00") + "." +
                            OC.Hour.ToString("00") + OC.Minute.ToString("00");
                Results.Add(tDate);
            }
            return (Results);
        }


        // do the prior day's summary report
        DailySalesInformation ITransactionService.PriorDailyReport(
            String MerchantID, String ClerkID, char WhereFrom, String TerminalID, DateTime LocalTime,
            String EndingDate)
        {
            DailySalesInformation DSI = new DailySalesInformation();
            IHistoryDAO IHR = new HistoryDAO();
            gp_DailyReport_Result DRR = IHR.PriorDailyReport(MerchantID, ClerkID, Convert.ToString(WhereFrom), Convert.ToDateTime(EndingDate));

            IHR.DailyReportToWeb(DRR, DSI, Convert.ToDateTime(EndingDate));

            return DSI;
        }





        // D e t a i l   R e p o r t


        DetailReportInformation ITransactionService.DetailReport(
            String MerchantID, String ClerkID, char WhereFrom,
            String MerchSeqNum, String TerminalID, DateTime LocalTime)
        {
            DetailReportInformation DRI = new DetailReportInformation();
            IHistoryDAO HistoryRepositoryInstance = new HistoryDAO();

            DetailHistory DHI = HistoryRepositoryInstance.DetailReport(MerchantID, ClerkID,
                 Convert.ToString(WhereFrom), MerchSeqNum,
                TerminalID, LocalTime);
            DRI.When = DateTime.Now;
            DRI.ResponseCode = (char)DHI.ResponseCode;
            DRI.ErrorCode = (string)DHI.ErrorCode;
            DRI.SummaryInformation = DHI.Summary;


            DRI.Details = new List<TransactionDetailInformation>();
            foreach (CardHistory ch in DHI.DetailItems)
            {
                TransactionDetailInformation TDI = new TransactionDetailInformation();
                TDI.CardNumber = ch.CardNumber;
                TDI.Amount = ch.Amount.ToString();
                TDI.ID = ch.ID.ToString();
                if (ch.LocalTime != null)
                    TDI.When = ch.LocalTime.ToString();
                else
                    TDI.When = ch.When.ToString();
                TDI.Clerk = ch.Clerk;
                TDI.TransType = ch.TransType;
                TDI.Transaction = ch.Transaction;
                TDI.MerchWhere = ch.MerchWhere;
                if (ch.Text != null)
                    if (ch.Text.Trim().Length > 0)
                        TDI.Text = ch.Text;
                if (ch.InvoiceNumber != null)
                    TDI.InvoiceNumber = ch.InvoiceNumber.Trim();
                else
                    TDI.InvoiceNumber = "";
                DRI.Details.Add(TDI);
            }
            return DRI;
        }


        bool ITransactionService.CloseBatch(
            String MerchantID, String ClerkID, char WhereFrom,
            String MerchSeqNum, String TerminalID, DateTime LocalTime)
        {
            IHistoryDAO HistoryRepositoryInstance = new HistoryDAO();

            bool CloseBatchResults = HistoryRepositoryInstance.CloseDay(MerchantID, ClerkID, 
                Convert.ToString(WhereFrom),  MerchSeqNum,
                TerminalID, LocalTime);
            return CloseBatchResults;
        }


        #endregion





        #region Formatting
        // F o r m a t R e c e i p t

        Receipt ITransactionService.FormatGiftReceipt(ReceiptInformation RecInfo)
        {
            Merchant Merch;
            Receipt aRec = new Receipt();
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                IMerchantDAO MerchantData = new MerchantDAO(GiftEntity);
                Merch = MerchantData.GetMerchant(RecInfo.MerchantID);


                // format the header lines

                aRec.AddCentered(Merch.ReceiptHeaderLine1);
                aRec.AddCentered(Merch.ReceiptHeaderLine2);
                aRec.AddCentered(Merch.ReceiptHeaderLine3);
                aRec.AddCentered(Merch.ReceiptHeaderLine4);
                aRec.AddCentered(Merch.ReceiptHeaderLine5);

                // format the date & time line

                String TransactionDate;
                String TransactionTime;
                if (RecInfo.LocalTime != null)
                {
                    TransactionDate = RecInfo.LocalTime.ToShortDateString();
                    TransactionTime = RecInfo.LocalTime.ToShortTimeString();
                    aRec.JustifyBoth(TransactionDate, TransactionTime);
                    //aRec.AddCentered("System time: " + RecInfo.When.ToShortDateString() + " " + RecInfo.When.ToShortTimeString());
                }
                else
                {
                    TransactionDate = RecInfo.When.ToShortDateString();
                    TransactionTime = RecInfo.When.ToShortTimeString();
                    aRec.JustifyBoth(TransactionDate, TransactionTime);
                }
                aRec.AddCentered("#: " + RecInfo.TransactionNumber.ToString());

                // format the transaction type

                aRec.Add("");
                aRec.AddCentered(RecInfo.TransactionType);//Utils.getMappedMessage("Activate"));

                if (RecInfo.ApprovalMessage != null)
                {
                    if (RecInfo.ApprovalMessage.Length > 0)
                        aRec.AddCentered(RecInfo.ApprovalMessage);
                }

                aRec.Add("");
                aRec.AddCentered("Account " + //Utils.getMappedMessage("Account") + " "
                    aRec.GetPrintableCardNumber(RecInfo.CardNumber));

                // format the amount line

                if (RecInfo.Amount != null)
                {
                    aRec.Add("");
                    aRec.AddCentered("Amount: " + RecInfo.Amount); //Utils.getMappedMessage("Amount") + amount);
                }
                if (RecInfo.Remainder != null)
                {
                    if (RecInfo.Remainder > 0.00M)
                    {
                        aRec.Add("");
                        aRec.AddCentered("Remainder: " + RecInfo.Remainder); 
                    }
                }
                if (RecInfo.Description != null)
                {
                    if (RecInfo.Description.Length > 0)
                    {
                        aRec.Add("");
                        aRec.AddCentered(RecInfo.Description);
                    }
                }
                if (RecInfo.Balance != null)
                {
                    aRec.Add("");
                    aRec.AddCentered("Remaining Balance:" + RecInfo.Balance);
                }
                // add the footer lines

                aRec.Add("");
                aRec.Add(Merch.ReceiptFooterLine1);
                aRec.Add(Merch.ReceiptFooterLine2);
            }
            return aRec;
        }


        // F o r m a t  L o y a l t y  R e c e i p t

        Receipt ITransactionService.FormatLoyaltyReceipt(ReceiptInformation RecInfo)
        {
            Merchant Merch;
            Receipt aRec = new Receipt();
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                IMerchantDAO MerchantData = new MerchantDAO(GiftEntity);
                Merch = MerchantData.GetMerchant(RecInfo.MerchantID);


                // format the header lines

                aRec.AddCentered(Merch.ReceiptHeaderLine1);
                aRec.AddCentered(Merch.ReceiptHeaderLine2);
                aRec.AddCentered(Merch.ReceiptHeaderLine3);
                aRec.AddCentered(Merch.ReceiptHeaderLine4);
                aRec.AddCentered(Merch.ReceiptHeaderLine5);

                // format the date & time line

                String TransactionDate;
                String TransactionTime;

                if (RecInfo.LocalTime != null)
                {
                    TransactionDate = RecInfo.LocalTime.ToShortDateString();
                    TransactionTime = RecInfo.LocalTime.ToShortTimeString();
                }
                else
                {
                    TransactionDate = RecInfo.When.ToShortDateString();
                    TransactionTime = RecInfo.When.ToShortTimeString();
                }
                aRec.JustifyBoth(TransactionDate, TransactionTime);
                if (RecInfo.TransactionNumber > 0)
                    aRec.AddCentered("#: " + RecInfo.TransactionNumber.ToString());

                // format the transaction type

                aRec.Add("");
                aRec.AddCentered(RecInfo.TransactionType);//Utils.getMappedMessage("Activate"));
                if (RecInfo.ApprovalMessage != null)
                {
                    if (RecInfo.ApprovalMessage.Length > 0)
                        aRec.AddCentered(RecInfo.ApprovalMessage);
                }

                aRec.Add("");
                if (RecInfo.CardNumber != null) 
                {
                    if (RecInfo.CardNumber.Length > 0)
                    aRec.AddCentered("Account " + //Utils.getMappedMessage("Account") + " "
                        aRec.GetPrintableCardNumber(RecInfo.CardNumber));
                    else
                        aRec.AddCentered("Phone: " + //Utils.getMappedMessage("Phone:") + " "
                            RecInfo.PhoneNumber);
                }
                else
                    aRec.AddCentered("Phone: " + //Utils.getMappedMessage("Phone:") + " "
                        RecInfo.PhoneNumber);


                // format the amount line

                if (RecInfo.Amount != null)
                {
                    aRec.Add("");
                    aRec.AddCentered("Amount: " + RecInfo.Amount); //Utils.getMappedMessage("Amount") + amount);
                }
                if (RecInfo.Description != null)
                {
                    if (RecInfo.Description.Length > 0)
                    {
                        aRec.Add("");
                        aRec.AddCentered(RecInfo.Description);
                    }
                }
                if (RecInfo.AdditionalMessage != null)
                {
                    if (RecInfo.AdditionalMessage.Length > 0)
                    {
                        aRec.Add("");
                        aRec.AddCentered(RecInfo.AdditionalMessage);
                    }
                }

                // add the footer lines

                aRec.Add("");
                aRec.Add(Merch.ReceiptFooterLine1);
                aRec.Add(Merch.ReceiptFooterLine2);
            }
            return aRec;
        }


        public void FormatSummarySection (Receipt aRep, DailySalesInformation ReportInfo)
        {
            int linelen = ("cardActivations" + " -     " + ReportInfo.GiftActivationAmount.ToString()).Length + 12;
            int ValueLen = ReportInfo.GiftSalesAmount.ToString().Length + 3;

            aRep.Add(" ");
            if (ReportInfo.GiftActive)
            {
                aRep.Add(formatReportLine("Card Activations", " -     ", ReportInfo.GiftActivations.ToString(), linelen, ValueLen));
                aRep.Add(formatReportLine("Amount", " $", formatAmount(ReportInfo.GiftActivationAmount.ToString()), linelen, ValueLen));
                aRep.Add(formatReportLine("Sales From Cards", " -     ", ReportInfo.GiftSales.ToString(), linelen, ValueLen));
                aRep.Add(formatReportLine("Amount", " $", formatAmount(ReportInfo.GiftSalesAmount.ToString()), linelen, ValueLen));
                aRep.Add(formatReportLine("Credits To Cards", " -     ", ReportInfo.GiftCredits.ToString(), linelen, ValueLen));
                aRep.Add(formatReportLine("Amount", " $", formatAmount(ReportInfo.GiftCreditAmount.ToString()), linelen, ValueLen));
                aRep.Add(formatReportLine("netTotal", " $", formatAmount(ReportInfo.NetGiftAmount.ToString()), linelen, ValueLen));
                aRep.Add(" ");
            }

        }


        Receipt ITransactionService.FormatDailyReport(DailySalesInformation ReportInfo)
        {
            Receipt aRep = new Receipt();


            // format the date & time line

            String TransactionDate = ReportInfo.When.ToShortDateString();
            String TransactionTime = ReportInfo.When.ToShortTimeString();
            aRep.JustifyBoth(TransactionDate, TransactionTime);

            // format the transaction type

            aRep.Add("");
            aRep.AddCentered("Daily Sales Report");//Utils.getMappedMessage("Activate"));

            aRep.Add(" ");
            FormatSummarySection(aRep, ReportInfo);
            return (aRep);
        }


        Receipt ITransactionService.FormatDetailReport(DetailReportInformation ReportInfo)
        {
            Receipt aRep = new Receipt();

            // format the date & time line

            String TransactionDate;
            String TransactionTime;
            if (ReportInfo.LocalTime != null)
            {
                TransactionDate = ReportInfo.LocalTime.ToShortDateString();
                TransactionTime = ReportInfo.LocalTime.ToShortTimeString();
                aRep.JustifyBoth(TransactionDate, TransactionTime);
                //aRep.AddCentered("System time: " + ReportInfo.When.ToShortDateString() + " " + ReportInfo.When.ToShortTimeString());
            }
            else
            {
                TransactionDate = ReportInfo.When.ToShortDateString();
                TransactionTime = ReportInfo.When.ToShortTimeString();
                aRep.JustifyBoth(TransactionDate, TransactionTime);
            }

            // format the transaction type

            aRep.Add("");
            aRep.AddCentered("Detail Report");//Utils.getMappedMessage("Activate"));


            int linelen = ("cardActivations" + " -       " + ReportInfo.SummaryInformation.GiftActivationAmount.ToString()).Length + 9;

            aRep.Add("");
                aRep.Add(String.Format(
                    "{0,5} {1,5} {2,4}  {3,10}   {4, 10} {5,8}",
                    " ID  ", "Clerk", "Type", "Card Num", "Inv#", "Amount"));
            aRep.Add("--------------------------------------------------");

            foreach (TransactionDetailInformation TDI in ReportInfo.Details)
            {
                aRep.Add(String.Format(
                    "{0,5} {1,4}  {2,4}  ******{3,4}   {4, 10} {5,8:C}",
                    TDI.ID, TDI.Clerk, TDI.TransType, TDI.CardNumber, TDI.InvoiceNumber, TDI.Amount));
                //aRep.Add(transactionnumber + Clerk + transactionType + cardnumber +  amount);
                if (TDI.TransType == "LEVT")
                {
                    aRep.Add(TDI.Text);
                }

            }




            aRep.Add(" ");
            FormatSummarySection(aRep, ReportInfo.SummaryInformation);

            return (aRep);
        }







        private String formatReportLine(String prefix, String lead, String value, int length, int ValueLen)
        {
            String results;

            results = prefix;
            while (value.Length < ValueLen) value = " " + value;
            while (results.Length + lead.Length + value.Length < length)
                results = results + " ";
            return results + lead + value;
        }

        private String formatAmount(String Value)
        {
            StringBuilder results = new StringBuilder();
            int DecimalPositions = -1;

            foreach (char ch in Value)
            {
                if (ch == '.')
                {
                    DecimalPositions = 0;
                    results.Append(ch);
                }
                else
                    if (Char.IsDigit(ch))
                    {
                        if (DecimalPositions > -1)
                            DecimalPositions ++;
                        if (DecimalPositions > 2)
                            break;
                        results.Append(ch);
                    }
            }
            if (DecimalPositions == -1)
            {
                results.Append('.');
                DecimalPositions = 0;
            }
            while (DecimalPositions < 2)
            {
                results.Append('0');
                DecimalPositions++;
            }
            return results.ToString();
        }

        string formatCardNumber(String Card)
        {
            StringBuilder Results = new StringBuilder();
            int count = Card.Length - 4;
            if (count < 1) count = 0;
            while (Results.Length < count)
                Results.Append('*');

            Results.Append(Card.Substring(count));
            return Results.ToString();
        }


        string RightJustify(String Value, int length)
        {
            while (Value.Length < length)
                Value = " " + Value;
            return Value;
        }


        Receipt ITransactionService.FormatCloseBatch(DateTime LocalTime)
        {
            Receipt aRep = new Receipt();


            // format the date & time line

            String TransactionDate;
            String TransactionTime;
            if (LocalTime != null)
            {
                TransactionDate = LocalTime.ToShortDateString();
                TransactionTime = LocalTime.ToShortTimeString();
                aRep.JustifyBoth(TransactionDate, TransactionTime);
                //aRep.AddCentered("System time: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            }
            else
            {
                TransactionDate = DateTime.Now.ToShortDateString();
                TransactionTime = DateTime.Now.ToShortTimeString();
                aRep.JustifyBoth(TransactionDate, TransactionTime);
            }

            // format the transaction type

            aRep.Add("");
            aRep.AddCentered("Close Batch");//Utils.getMappedMessage("Activate"));

            return (aRep);
        }




        String PullShorterDate(String tDate)
        {
            return (tDate.Substring(0, 5) + tDate.Substring(7));
        }

        #endregion Formatting










    }
    #endregion
}