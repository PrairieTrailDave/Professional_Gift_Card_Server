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
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Professional_Gift_Card_System;
using Professional_Gift_Card_System.Models;

namespace Professional_Gift_Card_System.Services
{
    public interface ITerminalService
    {
        bool ValidateAmount(String Amount, out String Message);
        bool ValidateCardSwipe(String CardSwipe, out String Message);
        bool ValidateClerkID(String ClerkID, bool Required, out String Message);
        bool ValidateMerchantID(String MerchantID, out String Message);
        bool ValidatePhoneNumber(String PhoneNumber, out String Message);
        bool ValidateSequenceNumber(String SeqNum, out String Message);
    }

    public class TerminalService : ITerminalService
    {

        // we are mixing in both data field validation and content validation

        bool ITerminalService.ValidateMerchantID(String MerchantID, out String Message)
        {
            // first do data field validation

            Message = "NO MERCH ID";
            if (MerchantID == null) return false;
            Message = "BAD MERCH ID";
            if (MerchantID.Length > 46) return false;
            MerchantIDAttribute MerchantTest = new MerchantIDAttribute();
            if (!(MerchantTest.IsValid(MerchantID))) return false;

            // then check the database to see if this merchant id is valid
            Message = "MID NOT FOUND";

            using (GiftEntities GiftEntity = new GiftEntities())
            {
                IMerchantDAO MerchantData = new MerchantDAO(GiftEntity);
                Merchant Merch = MerchantData.GetMerchant(MerchantID);
                if (Merch == null) return false;
                Message = "MID NOT ACTIVE";
                if (Merch.GiftActive != "A") return false;

                return true;
            }
        }


        bool ITerminalService.ValidateClerkID(String ClerkID, bool Required, out String Message)
        {
            Message = "NO CLERK ID";
            if (ClerkID == null)
                if (Required) return false;
                else return true;
            Message = "INVALID CLERKID";
            if (ClerkID.Length > 10) return false;
            ClerkIDAttribute ClerkTest = new ClerkIDAttribute();
            if (!(ClerkTest.IsValid(ClerkID))) return false;

            return true;
        }

        bool ITerminalService.ValidateCardSwipe(String CardSwipe, out String Message)
        {
            Message = "NO CARDSWIPE";
            if (CardSwipe == null) return false;
            Message = "INVALID CARDSWPE";
            CardSwipeAttribute CardTest = new CardSwipeAttribute();
            if (!(CardTest.IsValid(CardSwipe))) return false;
            return true;
        }
        bool ITerminalService.ValidateAmount(String Amount, out String Message)
        {
            Message = "NO AMOUNT";
            if (Amount == null) return false;
            Message = "INVALID AMOUNT";
            AmountAttribute AmountTest = new AmountAttribute();
            if (!(AmountTest.IsValid(Amount))) return false;
            return true;
        }

        bool ITerminalService.ValidatePhoneNumber(String PhoneNumber, out String Message)
        {
            Message = "NO PHONENUM";
            if (PhoneNumber == null) return false;
            Message = "INVALID PHONENUM";
            Models.PhoneAttribute PhoneNumTest = new Models.PhoneAttribute();
            if (!(PhoneNumTest.IsValid(PhoneNumber))) return false;
            return true;
        }

        bool ITerminalService.ValidateSequenceNumber(String SeqNum, out String Message)
        {
            Message = "NO SEQNUM";
            if (SeqNum == null) return false;
            Message = "INVALID SEQNUM";
            SeqNumAttribute SeqNumTest = new SeqNumAttribute();
            if (!(SeqNumTest.IsValid(SeqNum))) return false; 
            return true;
        }



    }
}