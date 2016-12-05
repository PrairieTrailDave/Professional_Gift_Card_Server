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
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Security;

namespace Professional_Gift_Card_System.Models
{
    public class LocalizedValidation
    {
    }

    // one place says that all we need to do is to put a line in the web.config
    // in <system.web>
    // <globalization culture="auto" uiCulture="auto" />
    // and the display attribute will be internationalized
    // enableclientbasedculture="true"  is not supported per MSDN
/*    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class LocalDisplayAttribute : DisplayAttribute
    {
        private const string _defaultErrorMessage = "";
        public LocalDisplayAttribute(String name)
            : base(name)//, ResourceType = typeof(Resources.Resources))
        {
        }
        public override string FormatErrorMessage(string name)
        {
            // want to check for the existance of an error name
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name);
        }

        // valid if null entry
        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString == null);
        }
    }

*/

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class LocalXAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "";
        public LocalXAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            // want to check for the existance of an error name
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name);
        }

        // valid if null entry
        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString == null);
        }
    }

}