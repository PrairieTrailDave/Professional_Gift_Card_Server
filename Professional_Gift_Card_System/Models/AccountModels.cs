// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Professional_Gift_Card_System.Models
{

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Email(ErrorMessage = "Invalid Character in the Email Address")]
        [Display(Name = "eMail Address")]
        public string UserEmailAddress { get; set; }

        [Required]
        [PasswordContent(ErrorMessage = "Invalid character in Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }

    public class CardHolderLogOnModel
    {
        [Required]
        [StringLength(128, ErrorMessage = "Email Address can not be more than 128 chars long")]
        [Email(ErrorMessage = "Invalid character in Email address")]
        [Display(Name = "eMail Address")]
        public string UserEmailAddress { get; set; }

        [Required]
        [ValidatePasswordLength]
        [PasswordContent(ErrorMessage = "Invalid character in password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

    }


    public class AdministratorLogOnModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [PasswordContent(ErrorMessage = "Invalid character in Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

    }

    public class MerchantLogOnModel
    {
        [Required]
        [StringLength(46, ErrorMessage = "{0} can not be more than {1} chars long")]
        [MerchantID]
        [Display(Name = "Merchant ID")]
        public string MerchantID { get; set; }

        [Display(Name = "(Optional) Clerk ID")]
        public string ClerkID { get; set; }

        [Required]
        [PasswordContent(ErrorMessage = "Invalid character in Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

    }


    public class AdministratorSetupModel
    {
        [Required]
        [Display(Name = "Super User Name")]
        public string UserName { get; set; }

        [Required]
        [PasswordContent(ErrorMessage = "Invalid character in Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [PasswordContent(ErrorMessage = "Invalid character in Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Repeat Password")]
        public string RepeatPassword { get; set; }

        [Required]
        [PasswordContent(ErrorMessage = "Invalid character in Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Second Password")]
        public string SecondPassword { get; set; }

        [Required]
        [PasswordContent(ErrorMessage = "Invalid character in Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Repeat Second Password")]
        public string RepeatSecondPassword { get; set; }


    }


}
