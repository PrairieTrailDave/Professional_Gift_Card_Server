// ************************************************************
//
// Copyright (c) 2016 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Professional_Gift_Card_System.Models
{
    public class SystemConstants
    {

        // declare our singleton instance
        private static readonly SystemConstants _internalInstance = new SystemConstants();

        // declare the public view into this instance

        public static SystemConstants Constants
        {
            get
            {
                return _internalInstance;
            }
        }


        //--------------------------------------------------------


        // define a string to be used in the card number

        public static string Xs = "XXXXXXXXXXX";

        public const int CardNumberLength = 25;
        public const String ApprovedResult = "APP  ";
        // actually it is pulled from the config file
        //public const String ApplicationName = "Gift";
    }
}