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

    public class WebLookupCard
    {
        [Required]
        [CardSwipe(ErrorMessage="Invalid characters in card number")]
        [DataType(DataType.Text)]
        [Display(Name ="Card Number")]
        public string CardNumber { get; set; }
    }

    public class WebCard
    {
        [DataType(DataType.Text)]
        [Display(Name ="ID")]
        public string ID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name ="Card Number")]
        public string CardNumber { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Last 4 digits of Card Number")]
        public string CardNumLast4 { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Which Chain")]
        public string ChainID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Which Merchant")]
        public string MerchantID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Which Merchant Group")]
        public string GroupCode { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Cardholder Name")]
        public string CardholderName { get; set; }
        public string CardHolder { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Shipped")]
        public string Shipped { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Activated")]
        public string Activated { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name ="Gift Balance")]
        public string GiftBalance { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Loyalty Balance")]
        public string LoyaltyBalance { get; set; }

        [DataType(DataType.Text)]
        [Display(Name ="Loyalty Visits")]
        public string LoyaltyVisits { get; set; }

        [DataType(DataType.Date)]
        [Display(Name ="Date Shipped")]
        public string DateShipped { get; set; }

        [DataType(DataType.Date)]
        [Display(Name ="Date Activated")]
        public string DateActivated { get; set; }


    }



    public class ReceiveCards
    {
        [Required(ErrorMessage = "Card Swipe is required")]
        [CardSwipe(ErrorMessage = "Invalid Character in Card Swipe")]
        [DataType(DataType.Text)]
        [Display(Name ="Swipe First Card to add")]
        public string FirstCardNumber { get; set; }

        [CardSwipe(ErrorMessage = "Invalid Character in Card Swipe")]
        [DataType(DataType.Text)]
        [Display(Name ="Swipe Last Card to add")]
        public string LastCardNumber { get; set; }

        [Count(ErrorMessage = "Invalid character in count")]
        [DataType(DataType.Text)]
        [Display(Name ="Or type the number of cards to add")]
        public string NumberOfCards { get; set; }



    }
    public class ShipCards
    {

        [Name(ErrorMessage = "Invalid character in Merchant Name")]
        [Required(ErrorMessage = "Merchant Name is required")]
        [StringLength(50, ErrorMessage = "Merchant name can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant Name")]
        public string MerchantName { get; set; }

        [Required(ErrorMessage = "Card Swipe is required")]
        [CardSwipe(ErrorMessage = "Invalid Character in Card Swipe")]
        [DataType(DataType.Text)]
        [Display(Name ="Swipe First Card to ship")]
        public string FirstCardNumber { get; set; }

        [CardSwipe(ErrorMessage = "Invalid Character in Card Swipe")]
        [DataType(DataType.Text)]
        [Display(Name ="Swipe Last Card to ship")]
        public string LastCardNumber { get; set; }

        [Count(ErrorMessage = "Invalid character in count")]
        [DataType(DataType.Text)]
        [Display(Name ="Or type the number of cards to ship")]
        public string NumberOfCards { get; set; }

        public List<SelectListItem> MerchantList { get; set; }
    }


    public class ReceiveAndShipCardModel
    {

        [Name(ErrorMessage = "Invalid character in Merchant Name")]
        [Required(ErrorMessage = "Merchant Name is required")]
        [StringLength(50, ErrorMessage = "Merchant name can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant Name")]
        public string MerchantName { get; set; }

        [Required(ErrorMessage = "Card Batch is required")]
        [BatchCard(ErrorMessage = "Invalid Character in Card Batch")]
        [DataType(DataType.MultilineText)]
        [Display(Name ="Cut and Paste Batch into here")]
        public string BatchCards { get; set; }

        [StringLength(8, ErrorMessage = "MultiStore Code can not be more than 8 chars long")]
        [MultiStoreCode(ErrorMessage = "Invalid characters in MultiStore code")]
        [RejectValue("NOGO")]
        [DataType(DataType.Text)]
        [Display(Name = "Multi-Store Code")]
        public string MultiStoreCode { get; set; }


        public List<SelectListItem> MerchantList { get; set; }
    }

    public class IssueCardsModel
    {

        [Name(ErrorMessage = "Invalid character in Merchant Name")]
        [Required(ErrorMessage = "Merchant Name is required")]
        [StringLength(50, ErrorMessage = "Merchant name can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Merchant Name")]
        public string MerchantName { get; set; }

        [Required(ErrorMessage = "Number of Cards is required")]
        [Count(ErrorMessage = "Invalid Character in Number of Cards")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Number of Cards")]
        public string NumberOfCards { get; set; }

        [Required(ErrorMessage = "First Card Number is required")]
        [CardSwipe(ErrorMessage = "Invalid Character in First Card Number")]
        [DataType(DataType.Text)]
        [Display(Name = "First Card Number to Issue")]
        public string FirstCardNumber { get; set; }



        public List<SelectListItem> MerchantList { get; set; }
    }





    public class CardHistoryModel
    {
        List<WebHistory> CardHistory { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int PageIndex { get; set; }
    }


    #endregion Models


}
