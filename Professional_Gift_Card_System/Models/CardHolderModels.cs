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
using System.Diagnostics;

namespace Professional_Gift_Card_System.Models
{
    #region Models


    [PropertiesMustMatch("Password", "ConfirmPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public class RegisterCardHolderModel
    {
        [Required]
        [StringLength(30, ErrorMessage = "First Name can not be more than 30 chars long")]
        [Name(ErrorMessage = "Invalid character in Name")]
        [Display(Name ="First Name*")]
        public String FirstName { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "Last Name can not be more than 30 chars long")]
        [Name(ErrorMessage = "Invalid character in Name")]
        [Display(Name ="Last Name*")]
        public String LastName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Address can not be more than 50 chars long")]
        [Address(ErrorMessage = "Invalid character in Address")]
        [Display(Name ="Address*")]
        public String Address1 { get; set; }

        [StringLength(50, ErrorMessage = "Address can not be more than 50 chars long")]
        [Address(ErrorMessage = "Invalid character in Address")]
        [Display(Name ="Address")]
        public String Address2 { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "City can not be more than 50 chars long")]
        [Name(ErrorMessage = "Invalid Character in City")]
        [Display(Name ="City*")]
        public String City { get; set; }

        [Required]
        [StringLength(2, ErrorMessage = "State can not be more than 2 chars long")]
        [State(ErrorMessage = "Invalid character in State")]
        [Display(Name ="State*")]
        public String State { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Postal Code can not be more than 10 chars long")]
        [PostalCode(ErrorMessage = "Invalid character in Postal Code")]
        [Display(Name ="Postal Code*")]
        public String PostalCode { get; set; }

        [StringLength(50, ErrorMessage = "Country can not be more than 50 chars long")]
        [Country(ErrorMessage = "Invalid character in Country")]
        [Display(Name ="Country")]
        public String Country { get; set; }

        [Required]
        [StringLength(25, ErrorMessage = "Phone number can not be more than 25 chars long")]
        [Phone(ErrorMessage = "Invalid character in phone")]
        [Display(Name ="Phone Number*")]
        public String CellPhoneNumber { get; set; }


        [Required]
        [StringLength(128, ErrorMessage = "Email Address can not be more than 128 chars long")]
        [Email(ErrorMessage = "Invalid character in Email address")]
        [DataType(DataType.EmailAddress)]
        [Display(Name ="Email address*")]
        public string email { get; set; }

        [Required]
        [ValidatePasswordLength]
        [PasswordContent(ErrorMessage = "Invalid character in password")]
        [DataType(DataType.Password)]
        [Display(Name ="Password*")]
        public string Password { get; set; }

        [Required]
        [ValidatePasswordLength]
        [PasswordContent(ErrorMessage = "Invalid character in password")]
        [DataType(DataType.Password)]
        [Display(Name ="Confirm password*")]
        public string ConfirmPassword { get; set; }

        [CardNumber(ErrorMessage = "Invalid character in card number")]
        [DataType(DataType.Text)]
        public string Card1 { get; set; }
        [CardNumber(ErrorMessage = "Invalid character in card number")]
        [DataType(DataType.Text)]
        public string Card2 { get; set; }
        [CardNumber(ErrorMessage = "Invalid character in card number")]
        [DataType(DataType.Text)]
        public string Card3 { get; set; }
        [CardNumber(ErrorMessage = "Invalid character in card number")]
        [DataType(DataType.Text)]
        public string Card4 { get; set; }
        [CardNumber(ErrorMessage = "Invalid character in card number")]
        [DataType(DataType.Text)]
        public string Card5 { get; set; }


    }

    public class CreateCardHolderModel
    {
        public int ID { get; set; }
        public Guid CardHolderGUID { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String UserName { get; set; }
        public String Address1 { get; set; }
        public String Address2 { get; set; }
        public String City { get; set; }
        public String State { get; set; }
        public String PostalCode { get; set; }
        public String Country { get; set; }
        public String CellPhoneNumber { get; set; }
        public String email { get; set; }
        public String Password { get; set; }
        public String passwordQuestion { get; set;}
        public String passwordAnswer{ get; set;}
        public bool isApproved{ get; set;}
        public object providerUserKey { get; set; }
        public string Card1 { get; set; }
        public Guid? Card1GUID { get; set; }
        public string Card2 { get; set; }
        public Guid? Card2GUID { get; set; }
        public string Card3 { get; set; }
        public Guid? Card3GUID { get; set; }
        public string Card4 { get; set; }
        public Guid? Card4GUID { get; set; }
        public string Card5 { get; set; }
        public Guid? Card5GUID { get; set; }
    }


    public class EditCardHolderModel
    {
        [Required]
        [StringLength(30, ErrorMessage = "First Name can not be more than 30 chars long")]
        [Name(ErrorMessage = "Invalid character in Name")]
        [Display(Name ="First Name*")]
        public String FirstName { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "Last Name can not be more than 30 chars long")]
        [Name(ErrorMessage = "Invalid character in Name")]
        [Display(Name ="Last Name*")]
        public String LastName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Address can not be more than 50 chars long")]
        [Address(ErrorMessage = "Invalid character in Address")]
        [Display(Name ="Address*")]
        public String Address1 { get; set; }

        [StringLength(50, ErrorMessage = "Address can not be more than 50 chars long")]
        [Address(ErrorMessage = "Invalid character in Address")]
        [Display(Name ="Address")]
        public String Address2 { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "City can not be more than 50 chars long")]
        [Name(ErrorMessage = "Invalid Character in City")]
        [Display(Name ="City*")]
        public String City { get; set; }

        [Required]
        [StringLength(2, ErrorMessage = "State can not be more than 2 chars long")]
        [State(ErrorMessage = "Invalid character in State")]
        [Display(Name ="State*")]
        public String State { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Postal Code can not be more than 10 chars long")]
        [PostalCode(ErrorMessage = "Invalid character in Postal Code")]
        [Display(Name ="Postal Code*")]
        public String PostalCode { get; set; }

        [StringLength(50, ErrorMessage = "Country can not be more than 50 chars long")]
        [Country(ErrorMessage = "Invalid character in Country")]
        [Display(Name ="Country")]
        public String Country { get; set; }

        [Required]
        [StringLength(25, ErrorMessage = "Phone number can not be more than 25 chars long")]
        [Phone(ErrorMessage = "Invalid character in phone")]
        [Display(Name ="Phone Number*")]
        public String CellPhoneNumber { get; set; }


        [Required]
        [StringLength(128, ErrorMessage = "Email Address can not be more than 128 chars long")]
        [Email(ErrorMessage = "Invalid character in Email address")]
        [DataType(DataType.EmailAddress)]
        [Display(Name ="Email address*")]
        public string Email { get; set; }
    }




    public class RegisterNewCardModel
    {
        [Required]
        [Display(Name ="Card Number")]
        [CardNumber(ErrorMessage = "Invalid character in card number")]
        public String CardNumber { get; set; }
    }
    public class UnregisterCardModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "ID")]
        public string ID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Card Number")]
        public String CardNumber { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Current Balance")]
        public String Balance { get; set; }
    }

    public class CardHolderHistoryItem
    {
        [Display(Name ="When")]
        public String When { get; set; }

        [Display(Name ="Where")]
        public String Where { get; set; }

        [Display(Name ="What Happened")]
        public String WhatHappened { get; set; }

        [Display(Name ="Amount")]
        public String Amount { get; set; }

        [Display(Name ="More Info")]
        public String Description { get; set; }

        public String PointsAwarded { get; set; }

    }

    public class CardHolderLookupModel
    {
        [StringLength(25, ErrorMessage = "Phone number can not be more than 25 chars long")]
        [Phone(ErrorMessage = "Invalid character in phone")]
        [Display(Name = "Phone Number*")]
        public String PhoneNumber { get; set; }

        [StringLength(128, ErrorMessage = "Email Address can not be more than 128 chars long")]
        [Email(ErrorMessage = "Invalid character in Email address")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address*")]
        public string Email { get; set; }

    }
    #endregion


}