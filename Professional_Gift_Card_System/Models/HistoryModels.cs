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

namespace Professional_Gift_Card_System.Models
{
    #region Models

    public class WebHistory
    {
        [DataType(DataType.Text)]
        [Display(Name ="TransactionID")]
        public string ID { get; set; }


        [DataType(DataType.Text)]
        [Display(Name ="Which Merchant")]
        public string MerchantName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="When")]
        public string When { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Which Card")]
        public string CardNumber { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Transaction Type")]
        public string TransType { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Amount")]
        public string Amount { get; set; }

        public String PointsGranted { get; set; }

        public String Reward { get; set; }
        public String Text { get; set; }

    }




    public class CardHistory
    {
        public long ID;
        public String CardNumber;
        public String TransType;
        public String Transaction;
        public DateTime When;
        public DateTime LocalTime;
        public Decimal Amount;
        public String MerchWhere;
        public String Clerk;
        public String Text;
        public String Card2;
        public int?   PointsGranted;
        public String CouponIssued;
        public String CouponUsed;
        public String RewardGranted;
        public String RewardUsed;
        public String PrizeAwarded;
        public String PrizeUsed;
        public String InvoiceNumber;
    }


    public class DetailHistory
    {
        public char ResponseCode { get; set; }
        public String ErrorCode { get; set; }
        public DateTime When { get; set; }

        public List<CardHistory> DetailItems { get; set; }

        public DailySalesInformation Summary { get; set; }
    }

    public class LastHistoryItem
    {
        public char ResponseCode { get; set; }
        public String ErrorCode { get; set; }
        public CardHistory LastItem { get; set; }
    }


    public class LogHistoryModel
    {
        public int id { get; set; }
        public String FileName { get; set; }
    }


    #endregion Models
}