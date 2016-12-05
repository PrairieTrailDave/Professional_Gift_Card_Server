// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;


namespace Professional_Gift_Card_System.Models
{
    #region Models


    public class MerchantSelectedModel
    {
        public String MerchantName { get; set; }
        public String MerchantAddress1 { get; set; }
        public String MerchantAddress2 { get; set; }
        public String MerchantCityState { get; set; }
        public String MerchantPhoneNumber { get; set; }
        public String PaidUpTo { get; set; }
        public int ID { get; set; }
    }



    public class MerchantPermissions
    {
        public bool IsRestaurant;
        public bool GiftAllowed;
        public bool LoyaltyAllowed;
        public Guid? LoyaltyProgram;
        public bool MemberOfChainWithLoyalty;
        public bool MemberOfGroupWithLoyalty;
    }

    public class MerchantLoyaltyPrograms
    {
        public Guid? LoyaltyProgram;
        public Guid? ChainLoyaltyProgram;
        public Guid? GroupLoyaltyProgram;
    }

    // base class for the merchant models
    public class MerchModel
    {
        [Name(ErrorMessage = "Invalid character in Merchant Name")]
        [Required(ErrorMessage = "Merchant Name is required")]
        [StringLength(50, ErrorMessage = "Merchant name can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Merchant Name")]
        public string MerchantName { get; set; }

        [Address(ErrorMessage = "Invalid character in Address")]
        [Required(ErrorMessage = "An address is required")]
        [StringLength(50, ErrorMessage = "Address can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }

        [Address(ErrorMessage = "Invalid character in Address")]
        [StringLength(50, ErrorMessage = "Address can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }

        [Name(ErrorMessage = "Invalid character in City")]
        [Required(ErrorMessage = "The city is required")]
        [StringLength(50, ErrorMessage = "City can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(2, ErrorMessage = "State can not be more than 2 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "State")]
        public string State { get; set; }

        [StringLength(50, ErrorMessage = "Country can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(10, ErrorMessage = "Postal code can not be more than 10 chars long")]
        [PostalCode(ErrorMessage = "Invalid characters in Postal Code")]
        [DataType(DataType.Text)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(25, ErrorMessage = "Phone number can not be more than 25 chars long")]
        [Phone(ErrorMessage = "Invalid characters in phone number")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "An eMail address is required")]
        [StringLength(50, ErrorMessage = "Email address can not be more than 50 chars long")]
        [Email(ErrorMessage = "Invalid character in email address")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "eMail")]
        public string EMail { get; set; }

        [Required(ErrorMessage = "Tax ID is required")]
        [StringLength(25, ErrorMessage = "Tax ID can not be more than 25 chars long")]
        [TaxID(ErrorMessage = "Invalid characters in Tax ID")]
        [DataType(DataType.Text)]
        [Display(Name = "Tax ID")]
        public string TaxID { get; set; }


        [Address(ErrorMessage = "Invalid character in Address")]
        [StringLength(40, ErrorMessage = "Address can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Shipping Address Line 1")]
        public string ShippingAddressLine1 { get; set; }

        [Address(ErrorMessage = "Invalid character in Address")]
        [StringLength(40, ErrorMessage = "Address can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Shipping Address Line 2")]
        public string ShippingAddressLine2 { get; set; }

        [Address(ErrorMessage = "Invalid character in Address")]
        [StringLength(40, ErrorMessage = "Address can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Shipping Address Line 3")]
        public string ShippingAddressLine3 { get; set; }

        [Address(ErrorMessage = "Invalid character in Address")]
        [StringLength(40, ErrorMessage = "Address can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name = "Shipping Address Line 4")]
        public string ShippingAddressLine4 { get; set; }

    }





    public class MerchantModel : MerchModel
    {
        [Count(ErrorMessage = "Invalid character in ID")]
        [DataType(DataType.Text)]
        [Display(Name = "Which Merchant")]
        public string ID { get; set; }

        [StringLength(50, ErrorMessage = "Chain ID can not be more than 50 chars long")]
        [ChainID(ErrorMessage = "Invalid character in Chain ID")]
        [DataType(DataType.Text)]
        [Display(Name = "Chain")]
        public string ChainID { get; set; }

        [StringLength(46, ErrorMessage = "{0} can not be more than {1} chars long")]
        [MerchantID]
        [Required(ErrorMessage = "Merchant ID is required")]
        [DataType(DataType.Text)]
        [Display(Name = "Merchant ID")]
        public string MerchantID { get; set; }




        [Required(ErrorMessage = "A contact person is required")]
        [StringLength(50, ErrorMessage = "Name can not be more than 50 chars long")]
        [Name(ErrorMessage = "Invalid character in name")]
        [DataType(DataType.Text)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Required(ErrorMessage = "Contact Phone number is required")]
        [StringLength(25, ErrorMessage = "Phone number can not be more than 25 chars long")]
        [Phone(ErrorMessage = "Invalid characters in phone number")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; }

        [StringLength(8, ErrorMessage = "Group Code can not be more than 8 chars long")]
        [GroupCode(ErrorMessage = "Invalid characters in group code")]
        [DataType(DataType.Text)]
        [Display(Name = "Group Code")]
        public string GroupCode { get; set; }

        [StringLength(8, ErrorMessage = "MultiStore Code can not be more than 8 chars long")]
        [MultiStoreCode(ErrorMessage = "Invalid characters in MultiStore code")]
        [RejectValue("NOGO")]
        [DataType(DataType.Text)]
        [Display(Name = "Multi-Store Code")]
        public string MultiStoreCode { get; set; }

        [StringLength(40, ErrorMessage = "Time Zone can not be more than 40 chars long")]
        [Name(ErrorMessage = "Invalid characters in time zone")]
        [DataType(DataType.Text)]
        [Display(Name = "Time Zone")]
        public string TimeZone { get; set; }

        [Required]
        [Time]
        [DataType(DataType.Time)]
        [Display(Name = "Close Time")]
        public String CloseTime { get; set; }

        [YesNo]
        [DataType(DataType.Text)]
        [Display(Name ="Gift Processing Active")]
        public string GiftActive { get; set; }

        [YesNo]
        [DataType(DataType.Text)]
        [Display(Name ="Loyalty Processing Active")]
        public string LoyaltyActive { get; set; }

        //[YesNo]
        //[DataType(DataType.Text)]
        //[Display(Name = "Uses Discounts")]
        //public string UsesDiscounts { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Merchant Billing Cycle")]
        public String MerchantBillingCycle { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DisplayOnly]
        [DataType(DataType.Date)]
        [Display(Name ="Last Billing Date")]
        public string LastBillingDate { get; set; }

        [DisplayOnly]
        [DataType(DataType.Text)]
        public String Pricing { get; set; }

        [DisplayOnly]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DataType(DataType.Date)]
        [Display(Name = "Paid to Date")]
        public DateTime PaidToDate { get; set; }

        [YesNo]
        [DataType(DataType.Text)]
        [Display(Name = "Allow Split Tender")]
        public string AllowSplitTender { get; set; }

        [RestaurantRetail]
        [DataType(DataType.Text)]
        [Display(Name = "Type of Merchant")]
        public string RestaurantRetail { get; set; }

        [YesNo]
        [DataType(DataType.Text)]
        [Display(Name ="Ask for Clerk/Server")]
        public string AskForClerkServer { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name ="Last Shift Close")]
        public string LastShiftClose { get; set; }



        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 1")]
        public string ReceiptHeaderLine1 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 2")]
        public string ReceiptHeaderLine2 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 3")]
        public string ReceiptHeaderLine3 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 4")]
        public string ReceiptHeaderLine4 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 5")]
        public string ReceiptHeaderLine5 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Footer Line 1")]
        public string ReceiptFooterLine1 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Footer Line 2")]
        public string ReceiptFooterLine2 { get; set; }

        public List<SelectListItem> ChainList { get; set; }
        public List<SelectListItem> GroupList { get; set; }
    }



    public class MerchantPaidUpModel
    {
        [Count(ErrorMessage = "Invalid character in ID")]
        [DataType(DataType.Text)]
        [Display(Name = "Which Merchant")]
        public string ID { get; set; }

        [StringLength(46, ErrorMessage = "{0} can not be more than {1} chars long")]
        [MerchantID]
        [Required(ErrorMessage = "Merchant ID is required")]
        [DataType(DataType.Text)]
        [Display(Name = "Merchant ID")]
        public string MerchantID { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Paid to Date")]
        public DateTime PaidToDate { get; set; }

    }


    public class SuggestMerchantModel
    {
        [Name(ErrorMessage = "Invalid character in Merchant Name")]
        [Required(ErrorMessage = "Merchant Name is required")]
        [StringLength(50, ErrorMessage = "Merchant name can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant Name")]
        public string MerchantName { get; set; }

        [ChainID(ErrorMessage = "Invalid character in Chain")]
        [DataType(DataType.Text)]
        [Display(Name ="I am a member of this chain")]
        public string ChainID { get; set; }

        [StringLength(8, ErrorMessage = "Group Code can not be more than 8 chars long")]
        [GroupCode(ErrorMessage = "Invalid characters in group code")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant Group Code")]
        public string GroupCode { get; set; }
    }

    public class SignUpMerchantModel : MerchModel
    {
        [ChainID(ErrorMessage = "Invalid character in Chain")]
        [DataType(DataType.Text)]
        [Display(Name ="I am a member of this chain")]
        public string ChainID { get; set; }

        [StringLength(8, ErrorMessage = "Group Code can not be more than 8 chars long")]
        [GroupCode(ErrorMessage = "Invalid characters in group code")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant Group Code")]
        public string GroupCode { get; set; }

        [Required(ErrorMessage = "A contact person is required")]
        [StringLength(50, ErrorMessage = "Name can not be more than 50 chars long")]
        [Name(ErrorMessage = "Invalid character in name")]
        [DataType(DataType.Text)]
        [Display(Name ="Contact Person *")]
        public string ContactPerson { get; set; }

        [Required(ErrorMessage = "Contact Phone number is required")]
        [StringLength(25, ErrorMessage = "Phone number can not be more than 25 chars long")]
        [Phone(ErrorMessage = "Invalid characters in phone number")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name ="Contact Phone *")]
        public string ContactPhone { get; set; }



        [StringLength(15, ErrorMessage = "Time Zone can not be more than 15 chars long")]
        [Name(ErrorMessage = "Invalid characters in time zone")]
        [DataType(DataType.Text)]
        [Display(Name ="Time Zone")]
        public string TimeZone { get; set; }


        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 1")]
        public string ReceiptHeaderLine1 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 2")]
        public string ReceiptHeaderLine2 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 3")]
        public string ReceiptHeaderLine3 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 4")]
        public string ReceiptHeaderLine4 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 5")]
        public string ReceiptHeaderLine5 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Footer Line 1")]
        public string ReceiptFooterLine1 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Footer Line 2")]
        public string ReceiptFooterLine2 { get; set; }

        public List<SelectListItem> ChainList { get; set; }
        public List<SelectListItem> GroupList { get; set; }
    }


    public class AddMerchantModel:SignUpMerchantModel
    {

        [StringLength(46, ErrorMessage = "{0} can not be more than {1} chars long")]
        [MerchantID]
        [Required(ErrorMessage = "Merchant ID is required")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant ID")]
        public string MerchantID { get; set; }

        [Name(ErrorMessage = "Invalid Character in Merchant 'user name'")]
        [Required(ErrorMessage = "Merchant 'user name' is required")]
        [StringLength(50, ErrorMessage = "Merchant 'user name' can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant 'user name'")]
        public string MerchantUserName { get; set; }

        [PasswordContent(ErrorMessage = "Invalid character in Password")]
        [Required(ErrorMessage = "Merchant password is required")]
        [StringLength(30, ErrorMessage = "Merchant password can not be more than 30 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant Access Password")]
        public string MerchantPassword { get; set; }

        [YesNo]
        [DataType(DataType.Text)]
        [Display(Name ="Ask for Clerk/Server")]
        public string AskForClerkServer { get; set; }

        [YesNo]
        [DataType(DataType.Text)]
        [Display(Name = "Allow Split Tender")]
        public string AllowSplitTender { get; set; }

        [YesNo]
        [DataType(DataType.Text)]
        [Display(Name = "Gift Processing Active")]
        public string GiftActive { get; set; }

        [YesNo]
        [DataType(DataType.Text)]
        [Display(Name = "Loyalty Processing Active")]
        public string LoyaltyActive { get; set; }

        [RestaurantRetail]
        [DataType(DataType.Text)]
        [Display(Name = "Type of Merchant")]
        public string RestaurantRetail { get; set; }
        //[YesNo]
        //[DataType(DataType.Text)]
        //[Display(Name = "Uses Discounts")]
        //public string UsesDiscounts { get; set; }

        [StringLength(8, ErrorMessage = "MultiStore Code can not be more than 8 chars long")]
        [MultiStoreCode(ErrorMessage = "Invalid characters in MultiStore code")]
        [RejectValue("NOGO")]
        [DataType(DataType.Text)]
        [Display(Name = "Multi-Store Code")]
        public string MultiStoreCode { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Merchant Billing Cycle")]
        public String MerchantBillingCycle { get; set; }

        [Required]
        [Time]
        [DataType(DataType.Time)]
        [Display(Name ="Close Time")]
        public String CloseTime { get; set; }
    }

    // used for merchant to edit his own information
    public class EditMerchDataModel: MerchModel
    {


        [Required(ErrorMessage = "A contact person is required")]
        [StringLength(50, ErrorMessage = "Name can not be more than 50 chars long")]
        [Name(ErrorMessage = "Invalid character in name")]
        [DataType(DataType.Text)]
        [Display(Name ="Contact Person")]
        public string ContactPerson { get; set; }

        [Required(ErrorMessage = "Contact Phone number is required")]
        [StringLength(25, ErrorMessage = "Phone number can not be more than 25 chars long")]
        [Phone(ErrorMessage = "Invalid characters in phone number")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name ="Contact Phone")]
        public string ContactPhone { get; set; }

        [StringLength(15, ErrorMessage = "Time Zone can not be more than 15 chars long")]
        [Name(ErrorMessage = "Invalid characters in time zone")]
        [DataType(DataType.Text)]
        [Display(Name ="Time Zone")]
        public string TimeZone { get; set; }

        [YesNo]
        [DataType(DataType.Text)]
        [Display(Name ="Ask for Clerk/Server")]
        public string AskForClerkServer { get; set; }

        [YesNo]
        [DataType(DataType.Text)]
        [Display(Name = "Allow Split Tender")]
        public string AllowSplitTender { get; set; }


        [Required]
        [Time]
        [DataType(DataType.Time)]
        [Display(Name = "Close Time")]
        public String CloseTime { get; set; }
    }


    public class EditReceiptHeaderModel
    {
        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 1")]
        public string ReceiptHeaderLine1 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 2")]
        public string ReceiptHeaderLine2 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 3")]
        public string ReceiptHeaderLine3 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 4")]
        public string ReceiptHeaderLine4 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Header Line 5")]
        public string ReceiptHeaderLine5 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Footer Line 1")]
        public string ReceiptFooterLine1 { get; set; }

        [ReceiptText(ErrorMessage = "Invalid character in Receipt Text")]
        [StringLength(40, ErrorMessage = "Text can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Receipt Footer Line 2")]
        public string ReceiptFooterLine2 { get; set; }

    }





    public class MerchantShipModel
    {
        [Count(ErrorMessage = "Invalid character in ID")]
        [DataType(DataType.Text)]
        [Display(Name ="Which Merchant")]
        public string ID { get; set; }

        [StringLength(46, ErrorMessage = "{0} can not be more than {1} chars long")]
        [MerchantID]
        [Required(ErrorMessage = "Merchant ID is required")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant ID")]
        public string MerchantID { get; set; }

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


    }

    public class MerchantLogonModel
    {
        [StringLength(46, ErrorMessage = "{0} can not be more than {1} chars long")]
        [MerchantID]
        [Required(ErrorMessage = "Merchant ID is required")]
        [DataType(DataType.Text)]
        [Display(Name ="Merchant ID")]
        public string MerchantID { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="Password")]
        public string Password { get; set; }
    }



    public class MerchantSignUpResult
    {
        public bool AddResults;
        public String MerchantID;
        public String MerchantSignUpName;
        public String MerchantPassword;
        public String ErrorMessage;
        public String Pricing;
    }



    public class MerchantResetPasswordModel
    {
        public string ID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name ="Proposed new Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="Retype Password")]
        public string SecondPassword { get; set; }
    }



    public class MerchantInvoicingModel
    {
        public String MerchantID { get; set; }
        public String MerchantName { get; set; }
        public String Address1 { get; set; }
        public String Address2 { get; set; }
        public String CityStateZip { get; set; }
        public String MerchantBillingCycle { get; set; }
        public String GiftActive { get; set; }
        public String LoyaltyActive { get; set; }
        public String LoyaltyLevel { get; set; }
        public String MerchantGiftMonthlyFee { get; set; }
        public String MerchantLoyaltyMonthlyFee { get; set; }
        public String MerchantGiftTransactionCount { get; set; }
        public String MerchantLoyaltyTransactionCount { get; set; }
        public String PaidUpToDate { get; set; }
        public String LastBillingDate { get; set; }
        public String InvoiceAmount { get; set; }
    }




    public class MerchantHistoryModel
    {
        public List<WebHistory> MHistory { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int PageIndex { get; set; }
    }

    public class MultiStoreDisplayModel
    {
        public List<String> MerchantMultiStore { get; set; }
        public List<String> CardMultiStore { get; set; }
    }

    public class MerchantPricingModel
    {
        [Count]
        public String ID { get; set; }

        public String MerchantName { get; set; }
        [Required]
        [Count]
        [DataType(DataType.Text)]
        [Display(Name = "Which Price")]
        public String PricingID { get; set; }
    }
    public class MerchantBalancesModel
    {
        public String MerchantName { get; set; }
        public String MerchantID { get; set; }
        public String GiftTransactionCount { get; set; }
        public String LoyaltyTransactionCount { get; set; }
        public String Balance { get; set; }
        public String PaidUpToDate { get; set; }
    }

    public class MerchantTransferModel
    {
        public String FromMerchantName { get; set; }
        public String FromMerchantID { get; set; }
        public String ToMerchantName { get; set; }
        public String ToMerchantID { get; set; }
        public String Amount { get; set; }
        public String CloseDate { get; set; }
    }

    public class MerchantTransferSelectModel
    {
        [Required]
        [DateTime]
        [DataType(DataType.Date)]
        [Display(Name="Start Date")]
        public DateTime StartDate { get; set; }
        [Required]
        [DateTime]
        [DataType(DataType.Date)]
        [Display(Name="End Date")]
        public DateTime EndDate { get; set; }
    }
    #endregion



   
}