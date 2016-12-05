// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;  
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Security;

namespace Professional_Gift_Card_System.Models
{

    public class ReAttribute : RegularExpressionAttribute
    {
        public ReAttribute(String Expression)
            : base(Expression)
        {
            ErrorMessageResourceName = "InvalidCharacter";
            ErrorMessageResourceType = typeof(Resources.ValidationMessages);
        }
    }
    public class gRequiredAttribute : RequiredAttribute
    {
        public gRequiredAttribute()
            : base()
        {
            ErrorMessageResourceName = "RequiredError";
            ErrorMessageResourceType = typeof(Resources.ValidationMessages);
        }
    }

    // Custom Validation Attributes that can be used on any screen

    public sealed class AddressAttribute : RegularExpressionAttribute
    {
        public AddressAttribute()
            : base("^[a-zA-Z0-9_\\-\\ \\.\\,]+$") { }
        // [a-zA-Z0-9_\\-\\ \\.]+  allow alpha, digit, underscore, dash, space, period, comma : of any length
    }

    // AmountAttribute - see below

    public sealed class BatchCardAttribute : RegularExpressionAttribute
    {
        public BatchCardAttribute()
            : base("^[0-9/\r/\n/\t]+$") { }
        // [0-9]+  allow digit: of any length
        // /\r - allow CR
        // /\n - allow LF
        // /\t - allow TAB
    }

    public sealed class BonusActivityNameAttribute : RegularExpressionAttribute
    {
        public BonusActivityNameAttribute()
            : base("^[a-zA-Z0-9 \\-]+$") { }
        // [a-zA-Z0-9 \\-\\.]+  allow alpha, digit, space, dash : of any length
    }

    // CardAvailableToRegisterAttribute - see below

    public sealed class CardNumberAttribute : RegularExpressionAttribute
    {
        public CardNumberAttribute()
            : base("^[0-9]{4,20}$")
        { }
        // [0-9]{4-20}    allow digits but 4-20 digits
    }

    // CardSwipeAttribute - see below

    public sealed class ChainIDAttribute : RegularExpressionAttribute
    {
        public ChainIDAttribute()
            : base("^[0-9]+$") { }
        // [0-9]+  allow digit : of any length
    }

    public sealed class ClerkIDAttribute : RegularExpressionAttribute
    {
        public ClerkIDAttribute()
            : base("^[a-zA-Z0-9\\-\\.]+$") { }
        // [a-zA-Z0-9\\-\\.]+  allow alpha, digit, dash, period : of any length
    }

    public sealed class CouponIDAttribute : RegularExpressionAttribute
    {
        public CouponIDAttribute()
            : base("^[a-zA-Z0-9\\-\\.]+$") { }
        // [a-zA-Z0-9\\-\\.]+  allow alpha, digit, dash, dot : of any length
    }
    public sealed class CouponDescriptionAttribute : RegularExpressionAttribute
    {
        public CouponDescriptionAttribute()
            : base("^[a-zA-Z0-9 \\-\\$\\.\\%]+$") { }
        // [a-zA-Z0-9 \\-\\$\\.]+  allow alpha, digit, space, dash, period, $ : of any length
    }

    
    public sealed class CountAttribute : RegularExpressionAttribute
    {
        public CountAttribute()
            : base("^[0-9]+$") { }
        // [0-9]+  allow digit: of any length
    }

    public sealed class CountryAttribute : RegularExpressionAttribute
    {
        public CountryAttribute()
            : base("^[A-Z][0-9a-zA-Z \\-\\.]+$") { }
        // [A-Z]                must start with uppercase letter
        // [0-9a-zA-Z \\-\\.]+  allow alpha, digit, dash, space, period : of any length
    }

    public sealed class DescriptionAttribute : RegularExpressionAttribute
    {
        public DescriptionAttribute()
            : base("^[a-zA-Z0-9 \\-]+$") { }
        // [a-zA-Z0-9 \\-\\.]+  allow alpha, digit, space, dash : of any length
    }

    public sealed class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute()
            : base ("^[a-zA-Z0-9_\\+\\-\\.]+(\\.[a-zA-Z0-9_\\+\\-\\.])*@[a-zA-Z0-9_\\-]+(\\.[a-zA-Z0-9_\\-]+)*\\.([a-zA-Z]{2,4})$") { }
        // [a-zA-Z0-9_\\+-]+      allow alpha, digit, underscore, plus, dash : of any length
        //(\\.[a-zA-Z0-9_\\+-])*  allow period, alpha, digit, underscore, plus, dash : zero or any length
        //@                       force @
        //[a-zA-Z0-9_-]+          allow alpha, digit, underscore, dash : of any length
        //(\\.[a-zA-Z0-9_-]+)*    allow period, alpha, digit, underscore, dash : of any length or no length
        //\\.([a-zA_Z]{2,4})      allow alpha 2-4 chars
    }


    public sealed class InvoiceNumberAttribute : ReAttribute
    {
        public InvoiceNumberAttribute()
            :base("^[a-zA-Z0-9 \\-]+$") { }
        // [a-zA-Z0-9\\-\\.]+  allow alpha, digit, space, dash : of any length
    }
    public sealed class GroupCodeAttribute : RegularExpressionAttribute
    {
        public GroupCodeAttribute()
            : base("^[a-zA-Z0-9 \\-]+$") { }
        // [a-zA-Z0-9\\-\\.]+  allow alpha, digit, space, dash : of any length
    }

    public sealed class GroupIDAttribute : RegularExpressionAttribute
    {
        public GroupIDAttribute()
            : base("^[0-9]+$") { }
        // [0-9]+  allow digit : of any length
    }

    public sealed class LoyaltyIDAttribute : RegularExpressionAttribute
    {
        public LoyaltyIDAttribute()
            : base("^[0-9]+$") { }
        // [0-9]+  allow digit : of any length
    }

    public sealed class LoyaltyProgramIDAttribute : RegularExpressionAttribute
    {
        public LoyaltyProgramIDAttribute()
            : base("^[0-9]+$") { }
        // [0-9]+  allow digit : of any length
    }

    public sealed class LoyaltyProgramNameAttribute : RegularExpressionAttribute
    {
        public LoyaltyProgramNameAttribute()
            : base("^[a-zA-Z0-9]+$") { }
        // [a-zA-Z0-9]+  allow alpha, digit : of any length
    }

    public sealed class MerchantIDAttribute : ReAttribute
    {
        public MerchantIDAttribute()
            : base("^[a-zA-Z0-9\\-\\.]+$") { }
        // [a-zA-Z0-9\\-\\.]+  allow alpha, digit, dash, period : from 2 to 46 chars long
    }

    public sealed class MultiStoreCodeAttribute : RegularExpressionAttribute
    {
        public MultiStoreCodeAttribute()
            : base("^[a-zA-Z0-9\\-]{2,8}$") { }
        // [a-zA-Z0-9\\-]+  allow alpha, digit, dash : from 2 to 8 chars long
    }

    public sealed class NameAttribute : RegularExpressionAttribute
    {
        public NameAttribute()
            : base("^[a-zA-Z0-9_\\-\\ \\.\\']+$") { }
        // [a-zA-Z0-9_\\-\\ \\.\\']+  allow alpha, digit, underscore, dash, space, period, appostrophe : of any length
    }

    public sealed class NumberAttribute : RegularExpressionAttribute
    {
        public NumberAttribute()
            : base("^[0-9]+$") { }
        // [0-9]+  allow digit : of any length
    }
    public sealed class PasswordContentAttribute : RegularExpressionAttribute
    {
        public PasswordContentAttribute()
            : base("^[a-zA-Z0-9@\\&\\*\\-]+$") { }
        // [a-zA-Z0-9@\\&\\*\\-]+  allow alpha, digit, dash : of any length
    }

    public sealed class PhoneAttribute : RegularExpressionAttribute
    {
        public PhoneAttribute()
            : base("^[\\(0-9][0-9 Xx\\-\\(\\)]+$") { }
        // [\\(0-9Xx]          must start with paranthesis or digit or X
        // [0-9 Xx\\-\\(\\)]+  allow digit, dash, space, X or paranthesis : of any length
    }

    public sealed class PostalCodeAttribute : RegularExpressionAttribute
    {
        public PostalCodeAttribute()
            : base("^[0-9a-zA-Z][0-9a-zA-Z \\-]+$") { }
        // [0-9a-zA-Z]     must start with alpha or digit
        // [0-9a-zA-Z -]+  allow alpha, digit, dash, space : of any length
    }

    public sealed class PrefixAttribute : RegularExpressionAttribute
    {
        public PrefixAttribute()
            : base("^[0-9]+$") { }
        // [0-9]+  allow digit : of any length
    }
    public sealed class RankingAttribute : RegularExpressionAttribute
    {
        public RankingAttribute()
            : base("^[a-zA-Z]+$") { }
        // [a-zA-Z]+  allow alpha : of any length
    }
    public sealed class ReceiptTextAttribute : RegularExpressionAttribute
    {
        public ReceiptTextAttribute()
            : base("^[a-zA-Z0-9_\\-\\ \\.\\,\\'\\(\\)]+$") { }
        // [a-zA-Z0-9_\\-\\ \\.]+  allow alpha, digit, underscore, dash, space, period, comma, apostrophe, paranthesis : of any length
    }

    public sealed class RFMAttribute : RegularExpressionAttribute
    {
        public RFMAttribute()
            : base("^[0-9]+$") { }
        // [0-9]+  allow digits : of any length
    }


    public sealed class RuleDescriptionAttribute : RegularExpressionAttribute
    {
        public RuleDescriptionAttribute()
            : base("^[a-zA-Z0-9 \\-\\$\\.\\/]+$") { }
        // [a-zA-Z0-9 \\-\\$\\.\\/]+  allow alpha, digit, space, dash, dollar sign, period, slash : of any length
    }

    public sealed class SeqNumAttribute : ReAttribute
    {
        public SeqNumAttribute()
            : base("^[0-9]+$") { }
        // [0-9]+  allow digit: of any length
    }

    public sealed class StateAttribute : RegularExpressionAttribute
    {
        public StateAttribute()
            : base("^[a-zA-Z]{2}$") { }
        // [a-zA-Z]{2}         allow alpha : 2 chars
    }


    public sealed class TaxIDAttribute : RegularExpressionAttribute
    {
        public TaxIDAttribute()
            : base("^[0-9][0-9\\- ]+$") { }
        // [0-9]+  allow digit, dash : of any length
    }





//    public sealed class ActiveAttribute : RegularExpressionAttribute
//    {
//        public ActiveAttribute()
//            : base("^[AN]$") { }
//        // [AN]    allow only A or N : of single character length
//    }







    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DisplayOnlyAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "This is a display only field";
        public DisplayOnlyAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
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


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class YesNoAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "Please select one or the other";
        public YesNoAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString != null && ((valueAsString == "Yes") ||(valueAsString == "No")));
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RestaurantRetailAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "Please select one or the other";
        public RestaurantRetailAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString != null && ((valueAsString.ToUpper() == "RESTAURANT") || (valueAsString.ToUpper() == "RETAIL")));
        }
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class AmountAttribute : ValidationAttribute
    {
        //public AmountAttribute()
            //            : base("^[0-9]*\\.([0-9]{2})$") { }
        //    : base("^[0-9\\.]*([0-9]{2})$") { }
        // [1-9]                  allow digit: one digit
        // [0-9]*                 allow digit : zero or any length
        // \\.                    force .
        // ([0-9]{2})             allow digit 2 chars

        private const string _defaultErrorMessage = "Please enter an amount";
        public AmountAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;
            string valueAsString = value as string;

            if (valueAsString.IndexOf('.') > -1)
                return new Regex ("^[0-9\\.]*([0-9]{2})$").IsMatch(valueAsString);
            return new Regex("^[1-9][0-9]*$").IsMatch(valueAsString);
        }
    
    }



    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CardSwipeAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "That was not a valid '{0}'";
        private char FailedChar;

        public CardSwipeAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name);
        }

        public override bool IsValid(object value)
        {
            int CardNumberLength;
            int WhichDigit;
            Char CardChar;
            string ValueAsString = value as string;

            // if the field is empty, allow it to be good
            // if you want the field to be required, add that as a separate attribute

            if (ValueAsString == null)
                return true;

            // card swipe starts off with possible start sentinel, track indicator,
            // or straight card number

            Char FirstChar = ValueAsString[0];
            if (("0123456789B?;%").IndexOf(FirstChar) < 0)
            {
                FailedChar = FirstChar;
                return false;
            }


            // then test the rest of the numbers

            CardNumberLength = 0;
            if (Char.IsDigit (FirstChar))
                CardNumberLength++;

            WhichDigit = 1;
            while (WhichDigit < ValueAsString.Length)
            {
                CardChar = ValueAsString[WhichDigit];
                if (("=^?;").IndexOf(CardChar) > -1)
                {
                    if (CardNumberLength < SystemConstants.CardNumberLength)
                        return true;    // quit after the end of the account number
                    return false;
                }
                if (("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz/.%;?=^ ").IndexOf
                    (CardChar) < 0)
                {
                    FailedChar = CardChar;
                    return false;
                }
                WhichDigit++;
                CardNumberLength++;
            }
            if (CardNumberLength < SystemConstants.CardNumberLength)
                return true;
            return false;
        }
    }






    // the card swipe string assumes that the card number
    // is the first part of the string

    public sealed class CardSwipeString
    {
        public Boolean Valid;

        private String _CardNumber = "";
        private char FailedChar;

        public CardSwipeString(String SwipeData)
        {
            ExtractCardNumber(SwipeData);
        }

        public String CardNumber()
        {
            return _CardNumber;
        }

        private void ExtractCardNumber(String ValueAsString)
        {
            int CardNumberLength;
            int WhichDigit;
            char CardChar;

            _CardNumber = "";

            // card swipe starts off with possible start sentinel, track indicator,
            // or straight card number

            WhichDigit = 0;
            Char FirstChar = ValueAsString[WhichDigit];
            if (("0123456789B?;%").IndexOf(FirstChar) < 0)
            {
                FailedChar = FirstChar;
                Valid = false;
                return;
            }


            

            // check for start sentinel

            CardNumberLength = 0;
            if (Char.IsDigit(FirstChar))
            {
                CardNumberLength++;
                _CardNumber = _CardNumber + FirstChar;
            }
            else
                    // check for track 1 indicator
            {
                WhichDigit++;
                if (! Char.IsDigit(ValueAsString[WhichDigit]))
                    WhichDigit++;
            }

                 // then test the rest of the numbers

            while (WhichDigit < ValueAsString.Length)
            {
                    // get a digit to test

                CardChar = ValueAsString[WhichDigit];

                    // test for end of card number

                if (("=^?;").IndexOf(CardChar) > -1)
                {
                    Valid = true;
                    if (CardNumberLength < SystemConstants.CardNumberLength)
                        return;    // quit after the end of the account number
                    Valid = false;
                    return;
                }
                // skip spaces in the account number
                if (ValueAsString[WhichDigit] != ' ')
                {
                    if (!Char.IsDigit(ValueAsString[WhichDigit]))
                    {
                        FailedChar = CardChar;
                        Valid = false;
                        return;
                    }
                    _CardNumber = _CardNumber + CardChar;
                }
                WhichDigit++;
                CardNumberLength++;
            }

            // after looking at the whole string

            if (CardNumberLength < SystemConstants.CardNumberLength)
            {
                Valid = true; 
                return;
            }
            Valid = false;
        }

    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DateTimeAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "Bad character in Date or Time";
        public DateTimeAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;
            if (value is DateTime) return true;
            DateTime DT;
            if (value is string)
                return DateTime.TryParse((string)value, out DT);
            return false;
        }
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TimeAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "Please enter in 24 hr format";
        public TimeAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;
            if (value is TimeSpan) return true;
            TimeSpan TS;
            if (value is string)
                return TimeSpan.TryParse((string)value, out TS);
            return false;
        }
    }





    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class PropertiesMustMatchAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "'{0}' and '{1}' do not match.";
        private readonly object _typeId = new object();

        public PropertiesMustMatchAttribute(string originalProperty, string confirmProperty)
            : base(_defaultErrorMessage)
        {
            OriginalProperty = originalProperty;
            ConfirmProperty = confirmProperty;
        }

        public string ConfirmProperty { get; private set; }
        public string OriginalProperty { get; private set; }

        public override object TypeId
        {
            get
            {
                return _typeId;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                OriginalProperty, ConfirmProperty);
        }

        public override bool IsValid(object value)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(value);
            object originalValue = properties.Find(OriginalProperty, true /* ignoreCase */).GetValue(value);
            object confirmValue = properties.Find(ConfirmProperty, true /* ignoreCase */).GetValue(value);
            return Object.Equals(originalValue, confirmValue);
        }
    }



    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidatePasswordLengthAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "'{0}' must be at least {1} characters long.";
        private readonly int _minCharacters = Membership.Provider.MinRequiredPasswordLength;

        public ValidatePasswordLengthAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class GuidAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "Bad character in GUID";
        public GuidAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name);
        }

        public override bool IsValid(object value)
        {
            Guid g;
            if (value == null) return true;
            if (value is string)
            {
                if (((string)value).Length == 0) return true;
                return Guid.TryParse((string)value, out g);
            }
            return false;
        }
    }



    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class RejectValueAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "'{0}' not allowed.";
        private readonly object _typeId = new object();

        public RejectValueAttribute(string testProperty)
            : base(_defaultErrorMessage)
        {
            TestProperty = testProperty;
        }

        public string TestProperty { get; private set; }

        public override object TypeId
        {
            get
            {
                return _typeId;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                TestProperty);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;
            return (string)value != TestProperty;
        }
    }


    // this attribute enforces that the value must be one of any char in a string

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class CharValueAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "Improper value - must be one of '{0}'";
        private readonly object _typeId = new object();

        public CharValueAttribute(string testProperty)
            : base(_defaultErrorMessage)
        {
            TestProperty = testProperty;
        }

        public string TestProperty { get; private set; }

        public override object TypeId
        {
            get
            {
                return _typeId;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                TestProperty);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;
            return ((string)value).IndexOfAny(TestProperty.ToCharArray()) != -1;
        }
    }



}