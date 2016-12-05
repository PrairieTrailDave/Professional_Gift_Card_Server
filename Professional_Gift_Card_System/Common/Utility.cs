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
using System.Web.Security;
using System.Security.Cryptography;


namespace Professional_Gift_Card_System.Models
{
    public class Utility
    {

        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
                case MembershipCreateStatus.InvalidProviderUserKey:
                    return "Invalid Provider User Key. Please contact system administrator.";
                case MembershipCreateStatus.DuplicateProviderUserKey:
                    return "Duplicate phone number. Please contact system administrator.";
                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        public static String ConvertErrorCodes(String ErrorCode)
        {
            switch (ErrorCode)
            {
                case "HSTER": return "An internal error has occured, please call your support number";
                case "INVIN": return "A field has invalid characters in it.";

                case "CHNAL": return "The chain has already been added";

                case "MERID": return "The merchant number entered is not a working merchant number";
                case "MEREX": return "The merchant number has expired. Contact your sales rep for assistance.";
                case "MERFD": return "Unable to find that merchant number";
                case "MERAL": return "The merchant has already been added";
                case "MERCH": return "There was an error in the chain for this merchant";
                case "MERLY": return "No loyalty system found for this merchant";
                case "CHMER": return "There is a merchant linked to this chain";
                case "GRMER": return "Error with the Merchant Group";


                case "BDCRD": return "Unable to read card";
                case "CRDER": return "Card not found";
                case "CRDIN": return "Card already in inventory";
                case "CRDAC": return "Card not active";
                case "CRDAL": return "Card already active";
                case "CRDAS": return "Card already shipped to a merchant";
                case "CRDNS": return "Card hasnt been shipped to a merchant";
                case "NOTVL": return "Card is not valid at this location";
                case "CRDIS": return "Card unavailable to activate";
                case "CRDPR": return "Card already registered to a card holder";

                case "CHINA": return "Person already in cardholder database";

                case "NVOID": return "Can not void the selected transaction";
                case "AVOID": return "The selected transaction is already voided";
                case "NTRAN": return "No transaction could be found";
                case "NLAST": return "Can not void - not the last transaction on that card";
                case "NSF  ": return "Insufficient Funds";

                case "DUPCK": return "The clerk is already in the system";
                case "NSCLK": return "Password or clerk not on system";

                case "NOPRI": return "Price not found";
                case "PRIAL": return "The price has already been added";
                case "NOIMP": return "This chain does not allow for individual merchant pricing";
                case "DUP  ": return "Duplicate Transaction";
                case "DUPCD": return "Duplicate card";
                case "DUPPH": return "Duplicate Phone Number";
                case "DUPEM": return "Duplicate email address";
                case "USRJT": return "Problem with adding user";
                case "PRVER": return "Provider error";

                case "APP  ": return "Transaction Approved";

                case "NOLOY": return "No loyalty system is defined for this merchant";
                case "PHNER": return "Phone Number not found";
                case "NOPHN": return "Phone Number not found";
                case "NORWD": return "No such reward found";
                case "RWDNV": return "Reward is not available for this merchant";
                case "NSP  ": return "Not enough Points available to purchase this reward";
                case "RWDEX": return "Reward has already expired";
                case "RWDER": return "Reward not found";
                case "NOPTS": return "No points selected";
                case "USEDA": return "Reward already used";

                case "NORIG": return "Tip must occur on same day as original transaction";
                case "BADDT": return "Bad date in local time";
            }
            return ErrorCode;
        }

    }

    public class Randomly
    {
        // SelectRandomlyFrom - pull one character out of a sample set

        public static char SelectRandomlyFrom(String SelectSet)
        {
            byte NumberOfChars = Convert.ToByte(SelectSet.Length);
            if (NumberOfChars == 0) return ' ';

            // Create a new instance of the RNGCryptoServiceProvider.
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            // Create a byte array to hold the random value.
            byte[] randomNumber = new byte[1];
            do
            {
                // Fill the array with a random value.
                rngCsp.GetBytes(randomNumber);
            }
            while (!IsFairSelection(randomNumber[0], NumberOfChars));
            // Return the randomly selected character
            return SelectSet[(randomNumber[0] % NumberOfChars)];

        }


        // SelectRandomlyFrom - pull one character out of a sample set

        public static long SelectRandomlyFrom(long LowerBound, long UpperBound)
        {
            int NumberOfValues = (int)(UpperBound - LowerBound);
            if (NumberOfValues == 0) return LowerBound;

            Random Randomizer = new Random();

            return Randomizer.Next(NumberOfValues) + LowerBound;
        }

        
        
        
        // IsFairSelection - internal used in random selection
        private static bool IsFairSelection(byte roll, byte numChars)
        {
            // There are MaxValue / numChars full sets of numbers that can come up
            // in a single byte.  
            int fullSetsOfValues = Byte.MaxValue / numChars;

            // If the selection is within this range of fair values, then we let it continue.
            return roll < numChars * fullSetsOfValues;
        }


    }

}