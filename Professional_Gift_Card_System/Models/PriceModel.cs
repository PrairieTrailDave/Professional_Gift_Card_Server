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

namespace Professional_Gift_Card_System.Models
{
    #region Models



    public class AddPriceModel
    {

        [Name(ErrorMessage = "Invalid character in pricing name")]
        [StringLength(50, ErrorMessage = "Can not have more than 50 characters in the pricing name")]
        [Required(ErrorMessage = "Pricing name is required")]
        [DataType(DataType.Text)]
        [Display(Name ="What to call this price set")]
        public string PricingName { get; set; }

        [Amount(ErrorMessage = "Invalid character in card price")]
        [Required(ErrorMessage = "Card price is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="Price per card")]
        public string CardPrice { get; set; }

        [Amount(ErrorMessage = "Invalid character in transaction price")]
        [Required(ErrorMessage = "Transaction Price is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="Price per transaction")]
        public string TransactionPrice { get; set; }

        [Amount(ErrorMessage = "Invalid character in transaction price")]
        [Required(ErrorMessage = "Support Transaction Price is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="Price per support call")]
        public string SupportTransactionPrice { get; set; }

        [Amount(ErrorMessage = "Invalid character in transaction price")]
        [Required(ErrorMessage = "Dial Up Transaction Price is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="Price per Dial Transaction")]
        public string DialupTransactionPrice { get; set; }

        // going to pull this from the model so that we can not have it on the screen
        // the add will have to put a constant value into the database.
        //[Amount(ErrorMessage = "Invalid character in transaction price")]
        //[Required(ErrorMessage = "Cell Phone Transaction Price is required")]
        //[DataType(DataType.Currency)]
        //[Display(Name ="Price per cell phone Transaction")]
        //public string CellPhoneTransactionPrice { get; set; }

        [Amount(ErrorMessage = "Invalid character in gift monthly fee")]
        [Required(ErrorMessage = "Gift Monthly Fee is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="GiftMonthly Fee")]
        public string GiftMonthlyFee { get; set; }

        [Amount(ErrorMessage = "Invalid character in loyalty monthly fee")]
        [Required(ErrorMessage = "Loyalty Monthly Fee is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="LoyaltyMonthly Fee")]
        public string LoyaltyMonthlyFee { get; set; }

        [Amount(ErrorMessage = "Invalid character in monthly fee")]
        [DataType(DataType.Currency)]
        [Display(Name ="Card Inactivity Monthly Fee")]
        public string CardholderMonthlyFee { get; set; }

        [Count(ErrorMessage = "Invalid character in percent fee")]
        [DataType(DataType.Text)]
        [Display(Name ="Card Inactivity percent fee")]
        public string CardHolderPercentageCharge { get; set; }

        [Count(ErrorMessage= "Invalid character in Month count")]
        [DataType(DataType.Text)]
        [Display(Name ="Card Inactivity after X months")]
        public string AfterXMonths { get; set; }
    }


    public class EditPriceModel
    {

        [DataType(DataType.Text)]
        [Display(Name ="ID")]
        public string DatabaseID { get; set; }

        [Name(ErrorMessage = "Invalid character in pricing name")]
        [StringLength(50, ErrorMessage = "Can not have more than 50 characters in the pricing name")]
        [Required(ErrorMessage = "Pricing name is required")]
        [DataType(DataType.Text)]
        [Display(Name ="What to call this price set")]
        public string PricingName { get; set; }

        [Amount(ErrorMessage = "Invalid character in card price")]
        [Required(ErrorMessage = "Card price is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="Price per card")]
        public string CardPrice { get; set; }

        [Amount(ErrorMessage = "Invalid character in transaction price")]
        [Required(ErrorMessage = "Transaction Price is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="Price per transaction")]
        public string TransactionPrice { get; set; }

        [Amount(ErrorMessage = "Invalid character in transaction price")]
        [Required(ErrorMessage = "Support Transaction Price is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="Price per support call")]
        public string SupportTransactionPrice { get; set; }

        [Amount(ErrorMessage = "Invalid character in transaction price")]
        [Required(ErrorMessage = "Dial Up Transaction Price is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="Price per Dial Transaction")]
        public string DialupTransactionPrice { get; set; }

        //[Amount(ErrorMessage = "Invalid character in transaction price")]
        //[Required(ErrorMessage = "Cell Phone Transaction Price is required")]
        //[DataType(DataType.Currency)]
        //[Display(Name ="Price per cell phone Transaction")]
        //public string CellPhoneTransactionPrice { get; set; }

        [Amount(ErrorMessage = "Invalid character in gift monthly fee")]
        [Required(ErrorMessage = "Gift Monthly Fee is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="GiftMonthly Fee")]
        public string GiftMonthlyFee { get; set; }

        [Amount(ErrorMessage = "Invalid character in loyalty monthly fee")]
        [Required(ErrorMessage = "Loyalty Monthly Fee is required")]
        [DataType(DataType.Currency)]
        [Display(Name ="LoyaltyMonthly Fee")]
        public string LoyaltyMonthlyFee { get; set; }

        [Amount(ErrorMessage = "Invalid character in monthly fee")]
        [DataType(DataType.Currency)]
        [Display(Name ="Card Inactivity Monthly Fee")]
        public string CardholderMonthlyFee { get; set; }

        [Count(ErrorMessage = "Invalid character in percent fee")]
        [DataType(DataType.Text)]
        [Display(Name ="Card Inactivity percent fee")]
        public string CardHolderPercentageCharge { get; set; }

        [Count(ErrorMessage = "Invalid character in Month count")]
        [DataType(DataType.Text)]
        [Display(Name ="Card Inactivity after X months")]
        public string AfterXMonths { get; set; }
    }

    #endregion Models

    #region Validation
    #endregion

}