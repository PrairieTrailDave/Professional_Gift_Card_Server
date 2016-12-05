// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text;

namespace Professional_Gift_Card_System.Models
{
    #region Models

    public class TransactionModel
    {
        [StringLength(46)]
        [MerchantID]
        [gRequired]
        [DataType(DataType.Text)]
        [Display(Name = "Merchant ID")]
        public string MerchantID { get; set; }

        [Name]
        [StringLength(10)]
        [DataType(DataType.Text)]
        [Display(Name = "Clerk ID")]
        public string ClerkID { get; set; }

        public String TerminalID { get; set; }


        [DateTime(ErrorMessage = "Invalid character in Local Time")]
        public DateTime LocalTime { get; set; }
    }
    public class GiftCardActivateModel : TransactionModel
    {


        [gRequired]
        [CardSwipe(ErrorMessage = "Invalid character in {0}")]
        [DataType(DataType.Text)]
        [Display(Name ="Card Swipe")]
        public string CardSwipe { get; set; }

        [gRequired]
        [Amount(ErrorMessageResourceName = "AmountFormat", ErrorMessageResourceType=typeof(Resources.ValidationMessages))]
        [StringLength(10, ErrorMessage = "Too many digits in Amount")]
        [DataType(DataType.Currency)]
        [Display(Name = "Amount")]
        public string Amount { get; set; }

    }

    public class GiftInquiryModel : TransactionModel 
    {

        [gRequired]
        [CardSwipe(ErrorMessage = "Invalid character in {0}")]
        [DataType(DataType.Text)]
        [Display(Name ="Card Swipe")]
        public string CardSwipe { get; set; }

        [SeqNum]
        [StringLength(10)]
        [DataType(DataType.Text)]
        public String SequenceNumber { get; set; }
    }

    public class GiftCardReturnModel : TransactionModel
    {
        [gRequired]
        [CardSwipe(ErrorMessage = "Invalid Character in Card Swipe")]
        [DataType(DataType.Text)]
        [Display(Name ="Card Swipe")]
        public string CardSwipe { get; set; }

        [gRequired]
        [Amount(ErrorMessageResourceName = "AmountFormat", ErrorMessageResourceType=typeof(Resources.ValidationMessages))]
        [StringLength(10, ErrorMessage = "Too many digits in Amount")]
        [DataType(DataType.Currency)]
        [Display(Name ="Amount")]
        public string AmountOfReturn { get; set; }

        [gRequired]
        [Description]
        [StringLength(40)]
        [DataType(DataType.Text)]
        [Display(Name ="Return Reason")]
        public string ReturnReason { get; set; }

    }

    public class GiftCardSaleModel : TransactionModel
    {

        [Required]
        [CardSwipe]
        [DataType(DataType.Text)]
        [Display(Name ="Card Swipe")]
        public string CardSwipe { get; set; }

        [Required]
        [Amount(ErrorMessageResourceName = "AmountFormat", ErrorMessageResourceType=typeof(Resources.ValidationMessages))]
        [StringLength(10, ErrorMessage = "Too many digits in Amount")]
        [DataType(DataType.Currency)]
        [Display(Name ="Amount")]
        public string Amount { get; set; }

        [Description]
        [StringLength(40)]
        [DataType(DataType.Text)]
        [Display(Name ="Description")]
        public string SalesDescription { get; set; }

    }

    
    


    



    
    public class DailySalesModel
    {
        [Required]
        [MerchantID]
        [StringLength(46, ErrorMessage = "Merchant ID can not be more than 46 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant ID")]
        public string MerchantID { get; set; }

        [StringLength(10, ErrorMessage = "Clerk ID can not be more than 10 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Clerk ID")]
        public string ClerkID { get; set; }
    }


    public class CloseModel
    {
        [Required]
        [MerchantID]
        [StringLength(46, ErrorMessage = "Merchant ID can not be more than 46 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Merchant ID")]
        public string MerchantID { get; set; }

        [StringLength(10, ErrorMessage = "Clerk ID can not be more than 10 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Clerk ID")]
        public string ClerkID { get; set; }

        [DateTime(ErrorMessage = "Invalid character in Local Time")]
        public string LocalTime { get; set; }
    }

    public class PriorDays
    {
        public String DisplayDate { get; set; }
        public String RequestDate { get; set; }
    }


    public class ReceiptInformation
    {
        public char      ResponseCode { get; set; }
        public String    ErrorCode { get; set; }
        public String    ApprovalMessage { get; set; }
        public String    MerchantID { get; set; }
        public String    ClerkID { get; set; }
        public DateTime  When { get; set; }
        public DateTime  LocalTime { get; set; }
        public String    TransactionType { get; set; }
        public Int64     TransactionNumber { get; set; }
        public String    CardNumber { get; set; }
        public String    CardHolderName { get; set; }
        public Guid      CardGUID { get; set; }
        public String    PhoneNumber { get; set; }
        public Decimal?  Amount { get; set; }
        public Decimal?  Balance { get; set; }
        public Decimal?  Remainder { get; set; }
        public String    Description { get; set; }
        public String    TransactionMessage { get; set; }
        public String    InvoiceNumber { get; set; }
        public String    AdditionalMessage { get; set; }
    }




    public class TransactionDetailInformation
    {
        public String ID;
        public String CardNumber;
        public String TransType;
        public String Transaction;
        public String When;
        public String Amount;
        public String MerchWhere;
        public String Clerk;
        public String Text;
        public String Description;
        public String InvoiceNumber;

    }

    public class DetailReportInformation
    {
        public char ResponseCode { get; set; }
        public String ErrorCode { get; set; }
        public DateTime When { get; set; }
        public DateTime LocalTime { get; set; }

        public List<TransactionDetailInformation> Details { get; set; }
        public DailySalesInformation SummaryInformation { get; set; }
    }

    public class PriorSummariesModel
    {
        public List<PriorDays> PriorDaysCloseTimes { get; set; }
    }

    #endregion Models



}