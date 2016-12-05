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
using System.Net.Mail;

using Professional_Gift_Card_System;
using Professional_Gift_Card_System.Models;


namespace Professional_Gift_Card_System.Services
{
    #region Services

    public interface IMerchantService
    {
        bool AddMerchant(AddMerchantModel MerchantToAdd, String AgentUserName, bool IfAgent);
        String GetTestRandomMerchantID();
        MerchantPermissions GetPermissions(String MerchantID);
        List<MerchantModel> GetMerchants(String AgentUserName, bool IfAgent);
        List<SelectListItem> GetMerchantsForSelect(String AgentUserName, bool IfAgent);


        MerchantModel GetMerchant(Int32 Which);
        EditMerchDataModel GetMerchant(String MerchantID);
        EditReceiptHeaderModel GetMerchantR(String MerchantID);
        int GetMerchantCardCount(String MerchantID);

        List<MerchantBalancesModel> GetTrialBalances();
        MerchantInvoicingModel GetMerchantInvoice(Int32 WhichMerchant, int TrialOrFinal);


        bool UpdateMerchant(MerchantModel WebMerchant);
        bool UpdateMerchant(String MerchantID, EditMerchDataModel WebMerchant);
        bool UpdateMerchant(String MerchantID, EditReceiptHeaderModel WebMerchant);
        bool UpdateMerchantPaidTo(MerchantPaidUpModel WebMerchant);
        bool ResetMerchantPassword(MerchantResetPasswordModel WebData);

        bool SetMerchantPricing(MerchantPricingModel WebData);

        String ShipCards(String MerchantID, String ClerkID, String CardToShip, Int32 CountToShip, String TransactionText);
        String ShipCards(String MerchantID, String ClerkID, String CardToShip, String LastCardToShip, String TransactionText);

        String SuggestMerchantID(String MerchantName, String Chain, String Group);
        String SuggestUserName(String MerchantName, String Chain, String Group);
        String SuggestMerchantPassword();
        void SuggestMerchantShippingReceipt(SignUpMerchantModel MerchantToAdd);

        bool ValidateMerchant();
        bool ValidateMerchant(String MerchantID, String Password, out String UserName);
        bool ValidateClerk(String MerchantID, String ClerkID, String Password, out String UserName);
    }




    public class MerchantService : IMerchantService
    {

        const String MerchantRole = "Merchant";

        // AddMerchant

        bool IMerchantService.AddMerchant(AddMerchantModel MerchantToAdd, String AgentUserName, bool IfAgent)
        {
            using (GiftEntities GiftData = new GiftEntities())
            {
                IMerchantDAO MerchantData = new MerchantDAO(GiftData);
                String ResultMessage;


                // verify that this merchant is not already in the database

                if (MerchantData.GetMerchant(MerchantToAdd.MerchantID) != null)
                    throw new Exception("Merchant ID already on file");

                if (MerchantData.GetMerchantByUserName(MerchantToAdd.MerchantUserName) != null)
                    throw new Exception("Merchant User Name already on file");

                IUserDAO UserRepository = new UserDAO();

                if (UserRepository.GetUser(MerchantToAdd.MerchantUserName) != null)
                    throw new Exception("Merchant 'user name' already on file");


                Merchant DBMerchant = new Merchant();



                PopulateNewMerchant(DBMerchant, MerchantToAdd);
                MerchantData.CreateMerchant(DBMerchant);


                // we also need to define this "user name" in the system 

                MembershipUser user = AddMerchantToUsers(MerchantToAdd, out ResultMessage);
                if (user == null) throw new Exception(ResultMessage);

                // and give it the merchant role

                AddUserToMerchantRole(user.UserName);
            }
            return true;
        }





        // PopulateNewMerchant - internal routine

        void PopulateNewMerchant(Merchant DBMerchant, AddMerchantModel MerchantToAdd)
        {
            DBMerchant.MerchantGUID = Guid.NewGuid();
            DBMerchant.MerchantName = MerchantToAdd.MerchantName;
            DBMerchant.Address1 = MerchantToAdd.Address1;
            DBMerchant.Address2 = MerchantToAdd.Address2;
            DBMerchant.City = MerchantToAdd.City;
            DBMerchant.State = MerchantToAdd.State;
            DBMerchant.Country = MerchantToAdd.Country;
            DBMerchant.PostalCode = MerchantToAdd.PostalCode;
            DBMerchant.Phone = MerchantToAdd.Phone;
            DBMerchant.email = MerchantToAdd.EMail;
            DBMerchant.EncryptedTaxID = GiftEncryption.Encrypt(MerchantToAdd.TaxID);
            DBMerchant.ContactPerson = MerchantToAdd.ContactPerson;
            DBMerchant.ContactPhone = MerchantToAdd.ContactPhone;
            DBMerchant.TimeZone = MerchantToAdd.TimeZone;
            DBMerchant.LastSeqNumber = 0;
            DBMerchant.SplitTender = "N";
            if (MerchantToAdd.AllowSplitTender == "Yes")
                DBMerchant.SplitTender = "Y";
            DBMerchant.GiftActive = "N";
            if (MerchantToAdd.GiftActive == "Yes")
                DBMerchant.GiftActive = "A";
            DBMerchant.LastBillingDate = DateTime.Now;
            DBMerchant.LastBillingNumber = 0;
            DateTime NextMonth = DateTime.Now.AddMonths(1);
            int DaysToAdd = DateTime.DaysInMonth(NextMonth.Year, NextMonth.Month) - NextMonth.Day;
            DBMerchant.PaidUpTo = NextMonth.AddDays(DaysToAdd);

            DBMerchant.BillingCycle = "M";
            switch (MerchantToAdd.MerchantBillingCycle)
            {
                case "Monthly": DBMerchant.BillingCycle = "M"; break;
                case "Quarterly": DBMerchant.BillingCycle = "Q"; break;
                case "Yearly": DBMerchant.BillingCycle = "Y"; break;
            }

            DBMerchant.MerchantID = MerchantToAdd.MerchantID;
            DBMerchant.Restaurant = "N";
            if (MerchantToAdd.RestaurantRetail != null)
                if (MerchantToAdd.RestaurantRetail.ToUpper() == "RESTAURANT")
                    DBMerchant.Restaurant = "Y";
            DBMerchant.AskForClerkServer = 0;
            if (MerchantToAdd.AskForClerkServer == "Yes")
                DBMerchant.AskForClerkServer = 1;




            DBMerchant.ShippingAddressLine1 = MerchantToAdd.ShippingAddressLine1;
            DBMerchant.ShippingAddressLine2 = MerchantToAdd.ShippingAddressLine2;
            DBMerchant.ShippingAddressLine3 = MerchantToAdd.ShippingAddressLine3;
            DBMerchant.ShippingAddressLine4 = MerchantToAdd.ShippingAddressLine4;
            DBMerchant.ReceiptHeaderLine1 = MerchantToAdd.ReceiptHeaderLine1;
            DBMerchant.ReceiptHeaderLine2 = MerchantToAdd.ReceiptHeaderLine2;
            DBMerchant.ReceiptHeaderLine3 = MerchantToAdd.ReceiptHeaderLine3;
            DBMerchant.ReceiptHeaderLine4 = MerchantToAdd.ReceiptHeaderLine4;
            DBMerchant.ReceiptHeaderLine5 = MerchantToAdd.ReceiptHeaderLine5;
            DBMerchant.ReceiptFooterLine1 = MerchantToAdd.ReceiptFooterLine1;
            DBMerchant.ReceiptFooterLine2 = MerchantToAdd.ReceiptFooterLine2;
        }

        // convert to AddMerchant format

        void ConvertToAddMerchant(AddMerchantModel MerchantToAdd, SignUpMerchantModel SignUpMerchant)
        {
            MerchantToAdd.ChainID = SignUpMerchant.ChainID;
            MerchantToAdd.MerchantName = SignUpMerchant.MerchantName;
            MerchantToAdd.Address1 = SignUpMerchant.Address1;
            MerchantToAdd.Address2 = SignUpMerchant.Address2;
            MerchantToAdd.City = SignUpMerchant.City;
            MerchantToAdd.State = SignUpMerchant.State;
            MerchantToAdd.Country = SignUpMerchant.Country;
            MerchantToAdd.PostalCode = SignUpMerchant.PostalCode;
            MerchantToAdd.Phone = SignUpMerchant.Phone;
            MerchantToAdd.EMail = SignUpMerchant.EMail;
            MerchantToAdd.TaxID = SignUpMerchant.TaxID;
            MerchantToAdd.ContactPerson = SignUpMerchant.ContactPerson;
            MerchantToAdd.ContactPhone = SignUpMerchant.ContactPhone;
            MerchantToAdd.TimeZone = SignUpMerchant.TimeZone;

            MerchantToAdd.AskForClerkServer = "Yes";
            MerchantToAdd.AllowSplitTender = "No";
            MerchantToAdd.GroupCode = SignUpMerchant.GroupCode;
            MerchantToAdd.ShippingAddressLine1 = SignUpMerchant.ShippingAddressLine1;
            MerchantToAdd.ShippingAddressLine2 = SignUpMerchant.ShippingAddressLine2;
            MerchantToAdd.ShippingAddressLine3 = SignUpMerchant.ShippingAddressLine3;
            MerchantToAdd.ShippingAddressLine4 = SignUpMerchant.ShippingAddressLine4;
            MerchantToAdd.ReceiptHeaderLine1 = SignUpMerchant.ReceiptHeaderLine1;
            MerchantToAdd.ReceiptHeaderLine2 = SignUpMerchant.ReceiptHeaderLine2;
            MerchantToAdd.ReceiptHeaderLine3 = SignUpMerchant.ReceiptHeaderLine3;
            MerchantToAdd.ReceiptHeaderLine4 = SignUpMerchant.ReceiptHeaderLine4;
            MerchantToAdd.ReceiptHeaderLine5 = SignUpMerchant.ReceiptHeaderLine5;
            MerchantToAdd.ReceiptFooterLine1 = SignUpMerchant.ReceiptFooterLine1;
            MerchantToAdd.ReceiptFooterLine2 = SignUpMerchant.ReceiptFooterLine2;
        }



        // AddMerchantToUsers

        MembershipUser AddMerchantToUsers(AddMerchantModel MerchantToAdd, out String ResultMessage)
        {
            MembershipProvider _provider;
            _provider = Membership.Providers["GiftUserMembershipProvider"];
            MembershipUser user;
            MembershipCreateStatus status;
            ResultMessage = "";
            String username = MerchantToAdd.MerchantUserName;
            String password = MerchantToAdd.MerchantPassword;
            String email = MerchantToAdd.EMail;
            String passwordQuestion = "";
            String passwordAnswer = "";
            bool isApproved = true;
            object providerUserKey = null;

            user = _provider.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
            if (user == null)
            {
                ResultMessage = Utility.ErrorCodeToString(status);
            }
            return user;
        }

        // AddUserToMerchantRole

        public void AddUserToMerchantRole(String username)
        {
            MembershipProvider _provider;
            _provider = Membership.Providers["GiftUserMembershipProvider"];

            IRolesDAO RoleRepository = new RolesDAO();
            if (!RoleRepository.RoleExists(MerchantRole, _provider.ApplicationName))
            {
                throw new Exception("Missing Role - " + MerchantRole);
            }

            IUserInRolesDAO UsersInRoles = new UserInRolesDAO();
            UsersInRoles.AddUserToRole(username, MerchantRole, _provider.ApplicationName);
        }









        
        // GetMerchants

        List<MerchantModel> IMerchantService.GetMerchants(String AgentUserName, bool IfAgent)
        {
            IMerchantDAO MerchantData = new MerchantDAO();
            List<Merchant> DBMerchants = new List<Merchant>(); // MerchantData.ListMerchantsForAgent(0);
            List<MerchantModel> WebMerchants = new List<MerchantModel>();
            if (DBMerchants != null)
            {
                foreach (Merchant Merch in DBMerchants)
                {
                    MerchantModel WebMerch = new MerchantModel();
                    MoveToWebPage(WebMerch, Merch);
                    WebMerchants.Add(WebMerch);
                }
            }
            return (WebMerchants);
        }

        // GetMerchantsForSelect

        List<SelectListItem> IMerchantService.GetMerchantsForSelect(String AgentUserName, bool IfAgent)
        {
            List<SelectListItem> MerchantNameList = new List<SelectListItem>();
            IMerchantDAO MerchantData = new MerchantDAO();
            List<Merchant> DBMerchants = new List<Merchant>(); // MerchantData.ListMerchantsForAgent(0);
            if (DBMerchants != null)
            {
                foreach (Merchant Merch in DBMerchants)
                {
                    SelectListItem ToAdd = new SelectListItem();
                    ToAdd.Text = Merch.MerchantName;
                    ToAdd.Value = Merch.MerchantID;
                    MerchantNameList.Add(ToAdd);
                }
            }
            return (MerchantNameList);
        }

        // GetChainsForSelect
        // We have two drop down menus on the add merchant
        // These show a name, but return an ID.
        // When we display the screen, we have to populate those selections
        // Afterwards, we have to convert the ID's back to the names


        int IMerchantService.GetMerchantCardCount(String MerchantID)
        {
            ICardRepository CardRepositoryInstance = new CardRepository();
            return (CardRepositoryInstance.GetCardCount(MerchantID));
        }

        // GetMerchant 
        // by database ID
        MerchantModel IMerchantService.GetMerchant(Int32 Which)
        {
            IMerchantDAO MerchantData = new MerchantDAO();

            MerchantModel AMerchant = new MerchantModel();

            Merchant DBMerchant = MerchantData.GetMerchant(Which);
            MoveToWebPage(AMerchant, DBMerchant);

            return (AMerchant);
        }





        // get the information so that a merchant can edit it
        EditMerchDataModel IMerchantService.GetMerchant(String MerchantID)
        {
            IMerchantDAO MerchantData = new MerchantDAO();

            EditMerchDataModel WebMerchant = new EditMerchDataModel();

            Merchant DBMerchant = MerchantData.GetMerchant(MerchantID);

            WebMerchant.MerchantName = GetSafeString(DBMerchant.MerchantName);
            WebMerchant.Address1 = GetSafeString(DBMerchant.Address1);
            WebMerchant.Address2 = GetSafeString(DBMerchant.Address2);
            WebMerchant.City = GetSafeString(DBMerchant.City);
            WebMerchant.State = GetSafeString(DBMerchant.State);
            WebMerchant.Country = GetSafeString(DBMerchant.Country);
            WebMerchant.PostalCode = GetSafeString(DBMerchant.PostalCode);
            WebMerchant.Phone = GetSafeString(DBMerchant.Phone);
            WebMerchant.EMail = GetSafeString(DBMerchant.email);
            WebMerchant.TaxID = GiftEncryption.Decrypt(DBMerchant.EncryptedTaxID);
            WebMerchant.ContactPhone = GetSafeString(DBMerchant.ContactPhone);
            WebMerchant.ContactPerson = GetSafeString(DBMerchant.ContactPerson);
            WebMerchant.TimeZone = GetSafeString(DBMerchant.TimeZone);

            WebMerchant.AllowSplitTender = "No";
            if (DBMerchant.SplitTender == "Y")
                WebMerchant.AllowSplitTender = "Yes";
            if (DBMerchant.AskForClerkServer == 0)
                WebMerchant.AskForClerkServer = "No";
            else
                WebMerchant.AskForClerkServer = "Yes";
            WebMerchant.ShippingAddressLine1 = GetSafeString(DBMerchant.ShippingAddressLine1);
            WebMerchant.ShippingAddressLine2 = GetSafeString(DBMerchant.ShippingAddressLine2);
            WebMerchant.ShippingAddressLine3 = GetSafeString(DBMerchant.ShippingAddressLine3);
            WebMerchant.ShippingAddressLine4 = GetSafeString(DBMerchant.ShippingAddressLine4);

            return (WebMerchant);
        }

        EditReceiptHeaderModel IMerchantService.GetMerchantR(String MerchantID)
        {
            IMerchantDAO MerchantData = new MerchantDAO();

            EditReceiptHeaderModel WebMerchant = new EditReceiptHeaderModel();

            Merchant DBMerchant = MerchantData.GetMerchant(MerchantID);

            WebMerchant.ReceiptHeaderLine1 = GetSafeString(DBMerchant.ReceiptHeaderLine1);
            WebMerchant.ReceiptHeaderLine2 = GetSafeString(DBMerchant.ReceiptHeaderLine2);
            WebMerchant.ReceiptHeaderLine3 = GetSafeString(DBMerchant.ReceiptHeaderLine3);
            WebMerchant.ReceiptHeaderLine4 = GetSafeString(DBMerchant.ReceiptHeaderLine4);
            WebMerchant.ReceiptHeaderLine5 = GetSafeString(DBMerchant.ReceiptHeaderLine5);
            WebMerchant.ReceiptFooterLine1 = GetSafeString(DBMerchant.ReceiptFooterLine1);
            WebMerchant.ReceiptFooterLine2 = GetSafeString(DBMerchant.ReceiptFooterLine2);

            return (WebMerchant);
        }











        List<MerchantBalancesModel> IMerchantService.GetTrialBalances()
        {
            List<MerchantBalancesModel> Results = new List<MerchantBalancesModel>();
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                IMerchantDAO MerchantData = new MerchantDAO(GiftEntity);

                List<Merchant> Merchants = MerchantData.GetAllActiveMerchants();

                foreach (Merchant Merch in Merchants)
                {
                    MerchantBalancesModel nBalance = new MerchantBalancesModel();
                    nBalance.MerchantName = Merch.MerchantName;
                    nBalance.MerchantID = Merch.MerchantID;
                    if (Merch.PaidUpTo.HasValue)
                        nBalance.PaidUpToDate = Merch.PaidUpTo.Value.ToShortDateString();
                    else
                        nBalance.PaidUpToDate = DateTime.Now.ToShortDateString();
                    nBalance.GiftTransactionCount = "0";
                    nBalance.LoyaltyTransactionCount = "0";
                    nBalance.Balance = "0.00";


                    gp_InvoiceMerchant_Result InvoiceData = GiftEntity.gp_InvoiceMerchant(Merch.ID, 0).First();
                    if (InvoiceData != null)
                    {
                        if (InvoiceData.ResponseCode[0] == 'A')
                        {
                            nBalance.GiftTransactionCount = InvoiceData.GiftTransactionCount.Value.ToString();
                            nBalance.Balance = InvoiceData.TotalCosts.Value.ToString();
                        }
                        else
                        {
                            if (InvoiceData.ErrorCode == "NOPRI")
                            {
                                nBalance.Balance = "No pricing selected";
                            }
                            else
                                throw new Exception(Utility.ConvertErrorCodes(InvoiceData.ErrorCode));
                        }
                    }

                    Results.Add(nBalance);
                }
            }
            return Results;
        }



        //  Get the invoicing information on this merchant
        MerchantInvoicingModel IMerchantService.GetMerchantInvoice(Int32 WhichMerchant, int TrialOrFinal)
        {
            MerchantInvoicingModel ToReturn = new MerchantInvoicingModel();

            using (GiftEntities GiftEntity = new GiftEntities())
            {
                IMerchantDAO MerchantData = new MerchantDAO(GiftEntity);
                Merchant tMerchant = MerchantData.GetMerchant(WhichMerchant);
                ToReturn.MerchantName = tMerchant.MerchantName;
                ToReturn.Address1 = tMerchant.Address1;
                ToReturn.Address2 = tMerchant.Address2;
                ToReturn.CityStateZip = tMerchant.City + ", " + tMerchant.State + " " + tMerchant.PostalCode;
                ToReturn.MerchantID = tMerchant.MerchantID;
                if (tMerchant.PaidUpTo.HasValue)
                    ToReturn.PaidUpToDate = tMerchant.PaidUpTo.Value.ToShortDateString();
                else
                    ToReturn.PaidUpToDate = DateTime.Now.ToShortDateString();
                ToReturn.GiftActive = "No";
                if (tMerchant.GiftActive != null)
                    if (tMerchant.GiftActive[0] == 'A')
                        ToReturn.GiftActive = "Yes";
                ToReturn.LoyaltyActive = "No";
                ToReturn.MerchantGiftTransactionCount = "0";
                ToReturn.MerchantGiftMonthlyFee = "";
                ToReturn.MerchantLoyaltyTransactionCount = "0";
                ToReturn.MerchantLoyaltyMonthlyFee = "";
                ToReturn.InvoiceAmount = "0.00";


                gp_InvoiceMerchant_Result InvoiceData = GiftEntity.gp_InvoiceMerchant(WhichMerchant, TrialOrFinal).First();
                if (InvoiceData != null)
                {
                    if (InvoiceData.ResponseCode[0] == 'A')
                    {
                        ToReturn.MerchantGiftTransactionCount = InvoiceData.GiftTransactionCount.Value.ToString();
                        ToReturn.MerchantGiftMonthlyFee = InvoiceData.GiftMonthlyFee.Value.ToString();
                        ToReturn.InvoiceAmount = InvoiceData.TotalCosts.Value.ToString();
                    }
                    else
                        throw new Exception(Utility.ConvertErrorCodes(InvoiceData.ErrorCode));
                }
            }


            return ToReturn;
        }




        MerchantPermissions IMerchantService.GetPermissions(String MerchantID)
        {
            IMerchantDAO MerchantData = new MerchantDAO();
            return MerchantData.GetMerchantPermissions(MerchantID);
        }


        public String GetSafeString(object value)
        {
            if (value == null) return "";
            if (value is string)
                return value.ToString().TrimEnd();
            return (Convert.ToString(value));
        }
        public String DateOnly(String CDate)
        {
            int pos = CDate.IndexOf(' ');
            return CDate.Substring(0, pos);
        }


        // MoveToWebPage
        // move data from the database structure to the web model
        // used in administration functions to create and edit the merchant

        void MoveToWebPage(MerchantModel WebMerchant, Merchant DBMerchant)
        {
            WebMerchant.ID = Convert.ToString(DBMerchant.ID);
            WebMerchant.MerchantID = GetSafeString(DBMerchant.MerchantID);
            WebMerchant.MerchantName = GetSafeString(DBMerchant.MerchantName);
            WebMerchant.Address1 = GetSafeString(DBMerchant.Address1);
            WebMerchant.Address2 = GetSafeString(DBMerchant.Address2);
            WebMerchant.City = GetSafeString(DBMerchant.City);
            WebMerchant.State = GetSafeString(DBMerchant.State);
            WebMerchant.Country = GetSafeString(DBMerchant.Country);
            WebMerchant.PostalCode = GetSafeString(DBMerchant.PostalCode);
            WebMerchant.Phone = GetSafeString(DBMerchant.Phone);
            WebMerchant.EMail = GetSafeString(DBMerchant.email);
            WebMerchant.TaxID = GiftEncryption.Decrypt(DBMerchant.EncryptedTaxID);
            WebMerchant.ContactPhone = GetSafeString(DBMerchant.ContactPhone);
            WebMerchant.ContactPerson = GetSafeString(DBMerchant.ContactPerson);
            WebMerchant.TimeZone = GetSafeString(DBMerchant.TimeZone);
            //WebMerchant.LastSeqNumber = DBMerchant.LastSeqNumber;
            WebMerchant.AllowSplitTender = "No";
            if (DBMerchant.SplitTender == "Y")
                WebMerchant.AllowSplitTender = "Yes";
            if (DBMerchant.GiftActive == "A")
                WebMerchant.GiftActive = "Yes";
            else
                WebMerchant.GiftActive = "No";
                WebMerchant.LoyaltyActive = "No";
            WebMerchant.LastBillingDate = DateOnly(Convert.ToString(DBMerchant.LastBillingDate));
            if (DBMerchant.PaidUpTo == null)
                WebMerchant.PaidToDate = DateTime.Now;
            else
                WebMerchant.PaidToDate = ((DateTime)DBMerchant.PaidUpTo);

            switch (DBMerchant.BillingCycle)
            {
                case "M": WebMerchant.MerchantBillingCycle = "Monthly"; break;
                case "Q": WebMerchant.MerchantBillingCycle = "Quarterly"; break;
                case "Y": WebMerchant.MerchantBillingCycle = "Yearly"; break;
            }
            WebMerchant.Pricing = "";
            if (DBMerchant.PricingCol.HasValue)
                WebMerchant.Pricing = DBMerchant.PricingCol.Value.ToString();

            //WebMerchant.LastBillingNumber = DBMerchant.LastBillingNumber;
            if (DBMerchant.AskForClerkServer == 0)
                WebMerchant.AskForClerkServer = "No";
            else
                WebMerchant.AskForClerkServer = "Yes";
            WebMerchant.RestaurantRetail = "Retail";
            if (DBMerchant.Restaurant != null)
                if (DBMerchant.Restaurant == "Y")
                    WebMerchant.RestaurantRetail = "Restaurant";

            if (DBMerchant.PaidUpTo != null)
                WebMerchant.PaidToDate = ((DateTime)DBMerchant.PaidUpTo);
            WebMerchant.ShippingAddressLine1 = GetSafeString(DBMerchant.ShippingAddressLine1);
            WebMerchant.ShippingAddressLine2 = GetSafeString(DBMerchant.ShippingAddressLine2);
            WebMerchant.ShippingAddressLine3 = GetSafeString(DBMerchant.ShippingAddressLine3);
            WebMerchant.ShippingAddressLine4 = GetSafeString(DBMerchant.ShippingAddressLine4);
            WebMerchant.ReceiptHeaderLine1 = GetSafeString(DBMerchant.ReceiptHeaderLine1);
            WebMerchant.ReceiptHeaderLine2 = GetSafeString(DBMerchant.ReceiptHeaderLine2);
            WebMerchant.ReceiptHeaderLine3 = GetSafeString(DBMerchant.ReceiptHeaderLine3);
            WebMerchant.ReceiptHeaderLine4 = GetSafeString(DBMerchant.ReceiptHeaderLine4);
            WebMerchant.ReceiptHeaderLine5 = GetSafeString(DBMerchant.ReceiptHeaderLine5);
            WebMerchant.ReceiptFooterLine1 = GetSafeString(DBMerchant.ReceiptFooterLine1);
            WebMerchant.ReceiptFooterLine2 = GetSafeString(DBMerchant.ReceiptFooterLine2);
        }

        // UpdateMerchant
        // This is too much to put into the repository
        bool IMerchantService.UpdateMerchant(MerchantModel MerchantToUpdate)
        {
            Merchant DBMerchant;
            bool ifToAdd = false;
            int screenID = Convert.ToInt32(MerchantToUpdate.ID);

            MerchantToUpdate.MerchantID = MerchantToUpdate.MerchantID.ToUpper();
            using (var GiftEntity = new GiftEntities())
            {
                if (MerchantToUpdate.ID != null)
                {
                    DBMerchant = (from m in GiftEntity.Merchants
                                  where m.ID == screenID
                                  select m).FirstOrDefault<Merchant>();
                }
                else
                    DBMerchant = null;

                // this could be true for database reasons as well as null ID passed
                if (DBMerchant == null)
                {
                    ifToAdd = true;
                    DBMerchant = new Merchant();
                }

                DBMerchant.MerchantName = MerchantToUpdate.MerchantName;
                DBMerchant.Address1 = MerchantToUpdate.Address1;
                DBMerchant.Address2 = MerchantToUpdate.Address2;
                DBMerchant.City = MerchantToUpdate.City;
                DBMerchant.State = MerchantToUpdate.State;
                DBMerchant.Country = MerchantToUpdate.Country;
                DBMerchant.PostalCode = MerchantToUpdate.PostalCode;
                DBMerchant.Phone = MerchantToUpdate.Phone;
                DBMerchant.email = MerchantToUpdate.EMail;
                DBMerchant.EncryptedTaxID = GiftEncryption.Encrypt(MerchantToUpdate.TaxID);
                DBMerchant.ContactPerson = MerchantToUpdate.ContactPerson;
                DBMerchant.ContactPhone = MerchantToUpdate.ContactPhone;
                DBMerchant.TimeZone = MerchantToUpdate.TimeZone;
                DBMerchant.SplitTender = "N";
                if (MerchantToUpdate.AllowSplitTender == "Yes")
                    DBMerchant.SplitTender = "Y";
                DBMerchant.GiftActive = "N";
                if (MerchantToUpdate.GiftActive == "Yes")
                    DBMerchant.GiftActive = "A";

                switch (MerchantToUpdate.MerchantBillingCycle)
                {
                    case "Monthly": DBMerchant.BillingCycle = "M"; break;
                    case "Quarterly": DBMerchant.BillingCycle = "Q"; break;
                    case "Yearly": DBMerchant.BillingCycle = "Y"; break;
                }
                DBMerchant.AskForClerkServer = 0;
                if (MerchantToUpdate.AskForClerkServer == "Yes")
                    DBMerchant.AskForClerkServer = 1;
                DBMerchant.Restaurant = "N";
                if (MerchantToUpdate.RestaurantRetail != null)
                    if (MerchantToUpdate.RestaurantRetail.ToUpper() == "RESTAURANT")
                        DBMerchant.Restaurant = "Y";

                //DBMerchant.PaidUpTo;  - we do not update on the standard screen
                // so that client administrators do not have the right to change this

                DBMerchant.ShippingAddressLine1 = MerchantToUpdate.ShippingAddressLine1;
                DBMerchant.ShippingAddressLine2 = MerchantToUpdate.ShippingAddressLine2;
                DBMerchant.ShippingAddressLine3 = MerchantToUpdate.ShippingAddressLine3;
                DBMerchant.ShippingAddressLine4 = MerchantToUpdate.ShippingAddressLine4;
                DBMerchant.ReceiptHeaderLine1 = MerchantToUpdate.ReceiptHeaderLine1;
                DBMerchant.ReceiptHeaderLine2 = MerchantToUpdate.ReceiptHeaderLine2;
                DBMerchant.ReceiptHeaderLine3 = MerchantToUpdate.ReceiptHeaderLine3;
                DBMerchant.ReceiptHeaderLine4 = MerchantToUpdate.ReceiptHeaderLine4;
                DBMerchant.ReceiptHeaderLine5 = MerchantToUpdate.ReceiptHeaderLine5;
                DBMerchant.ReceiptFooterLine1 = MerchantToUpdate.ReceiptFooterLine1;
                DBMerchant.ReceiptFooterLine2 = MerchantToUpdate.ReceiptFooterLine2;

                if (ifToAdd)
                {
                    DBMerchant.LastSeqNumber = 0;
                    DBMerchant.LastBillingDate = DateTime.Now;
                    DBMerchant.LastBillingNumber = 0;
                    GiftEntity.Merchants.Add(DBMerchant);
                }
                GiftEntity.SaveChanges();
            }
            return true;
        }



        // update for the merchant self edit process
        bool IMerchantService.UpdateMerchant(String MerchantID, EditReceiptHeaderModel MerchantToUpdate)
        {
            Merchant DBMerchant;

            String MerchantIDToFind = MerchantID.ToUpper();
            using (var GiftEntity = new GiftEntities())
            {
                DBMerchant = (from m in GiftEntity.Merchants
                              where m.MerchantID == MerchantIDToFind
                              select m).FirstOrDefault<Merchant>();

                // this function is only for updating an existing merchant
                // if not found, 
                if (DBMerchant == null)
                {
                    throw new Exception("Unable to find merchant in database");
                }

                DBMerchant.ReceiptHeaderLine1 = MerchantToUpdate.ReceiptHeaderLine1;
                DBMerchant.ReceiptHeaderLine2 = MerchantToUpdate.ReceiptHeaderLine2;
                DBMerchant.ReceiptHeaderLine3 = MerchantToUpdate.ReceiptHeaderLine3;
                DBMerchant.ReceiptHeaderLine4 = MerchantToUpdate.ReceiptHeaderLine4;
                DBMerchant.ReceiptHeaderLine5 = MerchantToUpdate.ReceiptHeaderLine5;
                DBMerchant.ReceiptFooterLine1 = MerchantToUpdate.ReceiptFooterLine1;
                DBMerchant.ReceiptFooterLine2 = MerchantToUpdate.ReceiptFooterLine2;
                GiftEntity.SaveChanges();
            }
            return true;
        }


        bool IMerchantService.UpdateMerchant(String MerchantID, EditMerchDataModel MerchantToUpdate)
        {
            Merchant DBMerchant;

            String MerchantIDToFind = MerchantID.ToUpper();
            using (var GiftEntity = new GiftEntities())
            {
                DBMerchant = (from m in GiftEntity.Merchants
                              where m.MerchantID == MerchantIDToFind
                              select m).FirstOrDefault<Merchant>();

                // this function is only for updating an existing merchant
                // if not found, 
                if (DBMerchant == null)
                {
                    throw new Exception("Unable to find merchant in database");
                }

                DBMerchant.MerchantName = MerchantToUpdate.MerchantName;
                DBMerchant.Address1 = MerchantToUpdate.Address1;
                DBMerchant.Address2 = MerchantToUpdate.Address2;
                DBMerchant.City = MerchantToUpdate.City;
                DBMerchant.State = MerchantToUpdate.State;
                DBMerchant.Country = MerchantToUpdate.Country;
                DBMerchant.PostalCode = MerchantToUpdate.PostalCode;
                DBMerchant.Phone = MerchantToUpdate.Phone;
                DBMerchant.email = MerchantToUpdate.EMail;
                DBMerchant.EncryptedTaxID = GiftEncryption.Encrypt(MerchantToUpdate.TaxID);
                DBMerchant.ContactPerson = MerchantToUpdate.ContactPerson;
                DBMerchant.ContactPhone = MerchantToUpdate.ContactPhone;
                DBMerchant.TimeZone = MerchantToUpdate.TimeZone;

                DBMerchant.SplitTender = "N";
                if (MerchantToUpdate.AllowSplitTender == "Yes")
                    DBMerchant.SplitTender = "Y";
                DBMerchant.AskForClerkServer = 0;
                if (MerchantToUpdate.AskForClerkServer == "Yes")
                    DBMerchant.AskForClerkServer = 1;
                // merchant does not get to update this
                //DBMerchant.Restaurant = "N";
                //if (MerchantToUpdate.RestaurantRetail != null)
                //    if (MerchantToUpdate.RestaurantRetail.ToUpper() == "RESTAURANT")
                //        DBMerchant.Restaurant = "Y";

                DBMerchant.ShippingAddressLine1 = MerchantToUpdate.ShippingAddressLine1;
                DBMerchant.ShippingAddressLine2 = MerchantToUpdate.ShippingAddressLine2;
                DBMerchant.ShippingAddressLine3 = MerchantToUpdate.ShippingAddressLine3;
                DBMerchant.ShippingAddressLine4 = MerchantToUpdate.ShippingAddressLine4;
                GiftEntity.SaveChanges();
            }
            return true;
        }


        // UpdateMerchantPaidTo
        bool IMerchantService.UpdateMerchantPaidTo(MerchantPaidUpModel MerchantToUpdate)
        {
            Merchant DBMerchant;
            int screenID = Convert.ToInt32(MerchantToUpdate.ID);

            MerchantToUpdate.MerchantID = MerchantToUpdate.MerchantID.ToUpper();
            using (var GiftEntity = new GiftEntities())
            {
                if (MerchantToUpdate.ID != null)
                {
                    DBMerchant = (from m in GiftEntity.Merchants
                                  where m.ID == screenID
                                  select m).FirstOrDefault<Merchant>();
                }
                else
                    return false;
                if (DBMerchant == null)
                    return false;

                DBMerchant.PaidUpTo = Convert.ToDateTime(MerchantToUpdate.PaidToDate);
                GiftEntity.SaveChanges();
            }
            return true;
        }



        // yes, this is violating some separation of concerns. 
        // However, the basic problem is that trying to use the membership provider
        // in this situation is an exercise in frustration. 
        // It might help to move the password encryption down to the 
        // user repository and take it out of the membership provider
        bool IMerchantService.ResetMerchantPassword(MerchantResetPasswordModel WebData)
        {
            IMerchantDAO MerchantData = new MerchantDAO();
            Merchant DBMerchant = MerchantData.GetMerchant(Convert.ToInt32(WebData.ID));
            String MerchantUserName = DBMerchant.MerchantUserName;

            IUserDAO UserRepositoryInstance = new UserDAO ();
            String Password = WebData.Password;
            Password = Professional_Gift_Card_System.Models.GiftEncryption.EncryptPassword(Password);
            UserRepositoryInstance.UpdatePassword(MerchantUserName, Password);
            return true;
        }



        bool IMerchantService.SetMerchantPricing(MerchantPricingModel WebData)
        {

            Merchant DBMerchant;

            if (WebData.ID != null)
            {
                using (var GiftEntity = new GiftEntities())
                {
                    IMerchantDAO iMerchantInstance = new MerchantDAO(GiftEntity);
                    Int32 MerchantToFind = Convert.ToInt32(WebData.ID);
                    DBMerchant = iMerchantInstance.GetMerchant(MerchantToFind);
                    if (DBMerchant == null)
                        return false;

                    Price iPrice;
                    IPriceDAO PricingData = new PriceDAO(GiftEntity);
                    iPrice = PricingData.GetPriceByID(Convert.ToInt32(WebData.PricingID));
                    DBMerchant.PricingCol = iPrice.PriceGUID;
                    GiftEntity.SaveChanges();
                }
            }
            return true;
        }




        // ShipCards

        String IMerchantService.ShipCards(String MerchantID, String ClerkID, String CardToShip, Int32 CountToShip, String TransactionText)
        {
            ICardRepository CardData = new CardRepository();
            return (CardData.ShipCards(MerchantID, ClerkID,
                "", DateTime.Now, CardToShip, CountToShip, TransactionText));
        }

        // ShipCards 

        String IMerchantService.ShipCards(String MerchantID, String ClerkID, String CardToShip, String LastCardToShip, String TransactionText)
        {
            ICardRepository CardData = new CardRepository();
            return (CardData.ShipCards(MerchantID, ClerkID, 
                "", DateTime.Now,
                CardToShip, LastCardToShip, TransactionText));
        }







        //************************************************************
        //      S u g g e s t 

        // SuggestMerchantID
        // Based on a chain affiliation or the unassigned group, 
        // suggest a unique merchant id for this new merchant

        String IMerchantService.SuggestMerchantID(String MerchantName, String Chain, String Group)
        {
            String Suggested = "";
            int NumberOfChainCharacters = 4;


            IMerchantDAO MerchantData = new MerchantDAO();
            if (Chain != null)
            {
                if (Chain.Length > 1)
                {
                    String Leading = Chain.Substring(0, NumberOfChainCharacters);
                    String LastID = MerchantData.GetMaxMerchantIDLike(Leading+ '0');
                    if (LastID != null)
                    {
                        if (LastID.Length > 0)
                        {
                            int LastNumber = PullFromName(LastID, NumberOfChainCharacters);
                            LastNumber++;
                            Suggested = Leading + LastNumber.ToString("D8");
                        }
                        else
                            Suggested = Leading + "00001";
                    }
                    else
                        Suggested = Leading + "00001";
                }
            }


            // if no chain or group fits, 
            // get one from the unassigned group

            if (Suggested.Length < 1)
            {
                String Leading = "100";
                String LastID = MerchantData.GetMaxMerchantIDLike(Leading);
                if (LastID != null)
                {
                    if (LastID.Length > 0)
                    {
                        int LastNumber = Convert.ToInt32(LastID.Substring(NumberOfChainCharacters));
                        LastNumber++;
                        Suggested = Leading + LastNumber.ToString("D8");
                    }
                }
            }

            // if nothing from the unassigned, 
            // suggest the first unassigned

            if (Suggested.Length < 1)
            {
                Suggested = "100001";
            }

            return Suggested;
        }

        // SuggestUserName
        // Build a username out of the merchant name

        String IMerchantService.SuggestUserName(String MerchantName, String Chain, String Group)
        {
            IMerchantDAO MerchantData = new MerchantDAO();
            StringBuilder NewName = new StringBuilder();

            foreach (char c in MerchantName)
            {
                if (c == ' ')
                    NewName.Append('_');
                else
                    NewName.Append(c);
            }

            String SuggestedName = NewName.ToString();
            SuggestedName = MerchantData.GetUniqueUserName(SuggestedName);
            return SuggestedName;
        }

        // SuggestMerchantPassword
        String IMerchantService.SuggestMerchantPassword()
        {
            return "" +
                Randomly.SelectRandomlyFrom("BCDFGHJKLMNPRSTVWXZ") +
                Randomly.SelectRandomlyFrom("aeiouyAEUY") +
                Randomly.SelectRandomlyFrom("bcdfghjkmnprstvwxz") +
                Randomly.SelectRandomlyFrom("0123456789") +
                Randomly.SelectRandomlyFrom("bcdfghjkmnprstvwxz") +
                Randomly.SelectRandomlyFrom("aeiouy") +
                Randomly.SelectRandomlyFrom("bcdfghjkmnprstvwxzBCDFGHJKLMNPRSTVWXZ")
               ;
        }



        // pullFromName

        int PullFromName(String Name, int Start)
        {
            int index;
            int result = 0;

            index = Start;
            while (index < Name.Length)
            {
                if (Char.IsDigit(Name[index]))
                    result = result * 10 + (Name[index] - '0');
                index++;
            }
            return result;
        }



        void IMerchantService.SuggestMerchantShippingReceipt(SignUpMerchantModel MerchantToAdd)
        {
            if ((MerchantToAdd.ShippingAddressLine1 == "") || (MerchantToAdd.ShippingAddressLine1 == null))
                MerchantToAdd.ShippingAddressLine1 = MerchantToAdd.MerchantName;
            if ((MerchantToAdd.ShippingAddressLine2 == "") || (MerchantToAdd.ShippingAddressLine2 == null))
                MerchantToAdd.ShippingAddressLine2 = MerchantToAdd.Address1;
            if ((MerchantToAdd.ShippingAddressLine3 == "")|| (MerchantToAdd.ShippingAddressLine3 == null))
                if ((MerchantToAdd.Address2 != "") && (MerchantToAdd.Address2 != null))
                {
                    MerchantToAdd.ShippingAddressLine3 = MerchantToAdd.Address2;
                    if ((MerchantToAdd.ShippingAddressLine4 == "")|| (MerchantToAdd.ShippingAddressLine4 == null))
                        MerchantToAdd.ShippingAddressLine4 =
                            MerchantToAdd.City + ", " + MerchantToAdd.State + " " + MerchantToAdd.PostalCode;
                }
                else
                {
                    MerchantToAdd.ShippingAddressLine3 =
                        MerchantToAdd.City + ", " + MerchantToAdd.State + " " + MerchantToAdd.PostalCode;
                }

            if ((MerchantToAdd.ReceiptHeaderLine1 == "") || (MerchantToAdd.ReceiptHeaderLine1 == null))
                MerchantToAdd.ReceiptHeaderLine1 = Center(MerchantToAdd.MerchantName, 40);
            if ((MerchantToAdd.ReceiptHeaderLine2 == "") || (MerchantToAdd.ReceiptHeaderLine2 == null))
                MerchantToAdd.ReceiptHeaderLine2 = Center(MerchantToAdd.Address1, 40);
            if ((MerchantToAdd.ReceiptHeaderLine3 == "") || (MerchantToAdd.ReceiptHeaderLine3 == null))
                if ((MerchantToAdd.Address2 != "") && (MerchantToAdd.Address2 != null))
                {
                    MerchantToAdd.ReceiptHeaderLine3 = Center(MerchantToAdd.Address2, 40);
                    if ((MerchantToAdd.ReceiptHeaderLine4 == "") || (MerchantToAdd.ReceiptHeaderLine4 == null))
                    {
                        MerchantToAdd.ReceiptHeaderLine4 = Center(
                            MerchantToAdd.City + ", " + MerchantToAdd.State + " " + MerchantToAdd.PostalCode, 40);
                    }
                    if ((MerchantToAdd.ReceiptHeaderLine5 == "") || (MerchantToAdd.ReceiptHeaderLine5 == null))
                        MerchantToAdd.ReceiptHeaderLine5 = Center(MerchantToAdd.Phone, 40);
                }
                else
                {
                    MerchantToAdd.ReceiptHeaderLine3 = Center(
                        MerchantToAdd.City + ", " + MerchantToAdd.State + " " + MerchantToAdd.PostalCode, 40);
                    if ((MerchantToAdd.ReceiptHeaderLine4 == "") || (MerchantToAdd.ReceiptHeaderLine4 == null))
                    {
                        MerchantToAdd.ReceiptHeaderLine4 = Center(MerchantToAdd.Phone, 40);

                    }
                }
        }

        String Center(String toCenter, int width)
        {
            int toAdd;

            while (toCenter[0] == ' ')
                toCenter = toCenter.Substring(1);
            if (toCenter.Length < width)
            {
                toAdd = (width - toCenter.Length) / 2;
                string AddedSpaces = "";
                while (toAdd > 0)
                {
                    AddedSpaces = AddedSpaces + " ";
                    toAdd--;
                }
                toCenter = AddedSpaces + toCenter;
            }
            return toCenter;
        }



        char GetNextConsonant(String Name, ref int index)
        {
            String Consonants = "bcdfghjklmnpqrstvwxz";
            while (index < Name.Length)
            {
                if (Consonants.IndexOf(Name[index]) > -1)
                    return Name[index++];
                index++;
            }
            return '0';
        }









        // ValidateMerchant

        bool IMerchantService.ValidateMerchant()
        {
            throw new NotImplementedException();
        }

        // ValidateMerchant
        // used in merchant log on
        bool IMerchantService.ValidateMerchant(String MerchantID, String Password, out String UserName)
        {
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                MembershipProvider _provider;
                IMerchantDAO MerchantData = new MerchantDAO(GiftEntity);
                UserName = "";

                if (String.IsNullOrEmpty(MerchantID)) throw new ArgumentException("Value cannot be null or empty.", "MerchantID");
                if (String.IsNullOrEmpty(Password)) throw new ArgumentException("Value cannot be null or empty.", "Password");

                Merchant DBMerchant = MerchantData.GetMerchant(MerchantID);

                // verify that this merchant is active
                if (DBMerchant != null)
                {
                    if ((DBMerchant.GiftActive == "A"))
                    {
                        _provider = Membership.Providers["GiftUserMembershipProvider"];
                        UserName = DBMerchant.MerchantUserName;
                        return _provider.ValidateUser(DBMerchant.MerchantUserName, Password);
                    }
                }
                //else
                //    throw new Exception("Merchant ID not found");
                return false;
            }
        }

        // Validateclerk
        // used in clerk log on
        // the clerks generally sign on to a system with just their clerk id and password
        // Once they log on, the system will populate the transaction screens with the merchant id & clerk
        
        bool IMerchantService.ValidateClerk(string MerchantID, string ClerkID, string Password, out String UserName)
        {
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                MembershipProvider _provider;
                UserName = "";
                IMerchantDAO MerchantData = new MerchantDAO(GiftEntity);

                if (String.IsNullOrEmpty(MerchantID)) throw new ArgumentException("Value cannot be null or empty.", "MerchantID");
                if (String.IsNullOrEmpty(ClerkID)) throw new ArgumentException("Value cannot be null or empty.", "ClerkID");
                if (String.IsNullOrEmpty(Password)) throw new ArgumentException("Value cannot be null or empty.", "Password");

                Merchant DBMerchant = MerchantData.GetMerchant(MerchantID);
                if (DBMerchant == null) return false;

                // verify that this merchant is active

                if ((DBMerchant.GiftActive == "A"))
                {
                    // then check to make sure this clerk is correct

                    IClerkDAO ClerkData = new ClerkDAO(GiftEntity);
                    Clerk tClerk = ClerkData.GetClerk(DBMerchant.MerchantGUID, ClerkID);
                    if (tClerk == null) return false;

                    _provider = Membership.Providers["GiftUserMembershipProvider"];
                    UserName = tClerk.UserName;
                    return _provider.ValidateUser(UserName, Password);
                }
            }
            return false;
        }

        String IMerchantService.GetTestRandomMerchantID()
        {
            String RandomID;

            using (GiftEntities GiftEntity = new GiftEntities())
            {
                IMerchantDAO MerchantData = new MerchantDAO(GiftEntity);

                do
                {
                    RandomID = "" +
                    Randomly.SelectRandomlyFrom("ABCDEFGHIJKLMNOQPRSTUVWXYZ0123456789") +
                    Randomly.SelectRandomlyFrom("ABCDEFGHIJKLMNOQPRSTUVWXYZ0123456789") +
                    Randomly.SelectRandomlyFrom("ABCDEFGHIJKLMNOQPRSTUVWXYZ0123456789") +
                    Randomly.SelectRandomlyFrom("ABCDEFGHIJKLMNOQPRSTUVWXYZ0123456789") +
                    Randomly.SelectRandomlyFrom("ABCDEFGHIJKLMNOQPRSTUVWXYZ0123456789") +
                    Randomly.SelectRandomlyFrom("ABCDEFGHIJKLMNOQPRSTUVWXYZ0123456789") +
                    Randomly.SelectRandomlyFrom("ABCDEFGHIJKLMNOQPRSTUVWXYZ0123456789");
                } while (MerchantData.GetMerchant(RandomID) != null);
            }
            return RandomID;
        }
    }

    #endregion
}