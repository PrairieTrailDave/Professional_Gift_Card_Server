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


namespace Professional_Gift_Card_System.Models
{
    #region Models

    public class AddClerkModel
    {
        [ClerkID(ErrorMessage = "Invalid Character in Clerk ID")]
        [Required(ErrorMessage = "Clerk ID is required")]
        [StringLength(10, ErrorMessage = "Clerk ID can not be more than 10 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Clerk ID")]
        public string ClerkID { get; set; }

        [Name(ErrorMessage = "Invalid Characters in Clerk Name")]
        [StringLength(40, ErrorMessage = "Clerk Name can not be more than 40 chars long")]
        [Required(ErrorMessage = "Clerk Name is required")]
        [DataType(DataType.Text)]
        [Display(Name ="Clerk Name (for your records)")]
        public string ClerkName { get; set; }

        [PasswordContent(ErrorMessage = "Invalid Characters in Clerk Password")]
        [Required(ErrorMessage = "Clerk Password is required")]
        [DataType(DataType.Text)]
        [Display(Name ="Clerk Password")]
        public string ClerkPassword { get; set; }

    }

    public class SuggestClerkModel
    {
        public String DatabaseID { get; set; }

        [ClerkID(ErrorMessage = "Invalid Character in Clerk ID")]
        [Required(ErrorMessage = "Clerk ID is required")]
        [StringLength(10, ErrorMessage = "Clerk ID can not be more than 10 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Clerk ID")]
        public string ClerkID { get; set; }

        [Name(ErrorMessage = "Invalid Characters in Clerk Name")]
        [StringLength(40, ErrorMessage = "Clerk Name can not be more than 40 chars long")]
        [Required(ErrorMessage = "Clerk Name is required")]
        [DataType(DataType.Text)]
        [Display(Name ="Clerk Name (for your records)")]
        public string ClerkName { get; set; }

    }



    public class ClerkModel
    {
        public String DatabaseID { get; set; }

        [ClerkID(ErrorMessage = "Invalid Character in Clerk ID")]
        [Required(ErrorMessage = "Clerk ID is required")]
        [StringLength(10, ErrorMessage = "Clerk ID can not be more than 10 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Clerk ID")]
        public string ClerkID { get; set; }

        [Name(ErrorMessage = "Invalid Characters in Clerk Name")]
        [StringLength(40, ErrorMessage = "Clerk Name can not be more than 40 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Clerk Name")]
        public string ClerkName { get; set; }

    }

    public class EditClerkModel
    {

        public string DatabaseID { get; set; }

        [ClerkID(ErrorMessage = "Invalid Character in Clerk ID")]
        [Required(ErrorMessage = "Clerk ID is required")]
        [StringLength(10, ErrorMessage = "Clerk ID can not be more than 10 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Clerk ID")]
        public string ClerkID { get; set; }

        [StringLength(40, ErrorMessage = "Clerk Name can not be more than 40 chars long")]
        [Name(ErrorMessage = "Invalid Characters in Clerk Name")]
        [DataType(DataType.Text)]
        [Display(Name ="Clerk Name")]
        public string ClerkName { get; set; }

        [PasswordContent(ErrorMessage = "Invalid character in password")]
        [StringLength(50, ErrorMessage = "Password can not be more than 50 chars long")]
        [DataType(DataType.Text)]
        [Display(Name ="Password")]
        public string Password { get; set; }
    }


    #endregion



}