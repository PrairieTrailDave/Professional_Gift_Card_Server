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
using Professional_Gift_Card_System;
using Professional_Gift_Card_System.Models;

namespace Professional_Gift_Card_System.Services
{
    #region Services

    public interface ICardHolderService
    {
        int MinPasswordLength { get; }

        EditCardHolderModel FindCardHolder(String PhoneNumber, String EmailAddress);
        EditCardHolderModel GetCardHolder(String CardHolderName);
        GiftCardBalance GetCardBalance(String CardHolderName, int ID);
        List<GiftCardBalance> GetCardHolderGiftCardBalances(String CardHolderName);
        List<CardHolderHistoryItem> GetCardHistory(String CardHolderName, int CardID);


        // account signin entry points
        bool ValidateUser(string userName, string password);
        bool ValidateUserByEmail(string eMailAddress, string password, out String userName);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
        MembershipUser CreateCardHolder(CreateCardHolderModel WebData,
                out MembershipCreateStatus status);
        bool VerifySystemInitialized();
        void CreateSuperUser(String UserName, String Password, String SecondPassword);
        void CreateSystemAdmin(String UserName, String Password, String SecondPassword);

        // maintenance
        int GetCardCount(String CardHolderName);
        void RegisterNewCard(String CardHolderName, String CardNumber);
        void UpdateCardHolder(EditCardHolderModel WebData, String CardHolderName);
        void UnregisterCard(String CardHolderName, String WhichCard);
        //bool EncryptZip();
        //bool VerifyEncryptZip();
//        bool EncryptCustomerFile();
//        bool VerifyEncryptCustomerFile();
    }
    public class CardHolderService : ICardHolderService
    {
        const String CardHolderRole = "CardHolder";

        protected MembershipProvider _provider;


        public CardHolderService()
        {
            // the membership provider is populated by the framework
            // we need to pull out the instance and use it
            _provider = Membership.Providers["GiftUserMembershipProvider"];
        }





        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        //
        // MembershipProvider.CreateCardHolder
        //

        public MembershipUser CreateCardHolder(
            CreateCardHolderModel WebData,
                out MembershipCreateStatus status)
        {
            MembershipUser user = null;

            // generate this user's username

            WebData.UserName = GenerateUserName(WebData.FirstName, WebData.LastName);

            // insure uniqueness by email address

            if (GetCardHolderByEmail(WebData.email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            // verify unique cardholder phone number

            ICardHolderRepository CardHolderRepositoryI = new CardHolderRepository();
            CardHolder CH = CardHolderRepositoryI.GetCardHolderByPhoneNumber(WebData.CellPhoneNumber);
            if (CH != null)
            {
                status = MembershipCreateStatus.DuplicateProviderUserKey;
                return null;
            }

            using (GiftEntities GiftEntity = new GiftEntities())
            {
                ICardHolderRepository CardHolderRepository = new CardHolderRepository(GiftEntity);
                ICardRepository CardService = new CardRepository(GiftEntity);


                // get the information for each card to be registered

                if (WebData.Card1 != null)
                {
                    Card tCard = CardService.GetCard(WebData.Card1);
                    if (tCard != null)
                        WebData.Card1GUID = tCard.CardGUID;
                }
                if (WebData.Card2 != null)
                {
                    Card tCard = CardService.GetCard(WebData.Card2);
                    if (tCard != null)
                        WebData.Card2GUID = tCard.CardGUID;
                }
                if (WebData.Card3 != null)
                {
                    Card tCard = CardService.GetCard(WebData.Card3);
                    if (tCard != null)
                        WebData.Card3GUID = tCard.CardGUID;
                }
                if (WebData.Card4 != null)
                {
                    Card tCard = CardService.GetCard(WebData.Card4);
                    if (tCard != null)
                        WebData.Card4GUID = tCard.CardGUID;
                }
                if (WebData.Card5 != null)
                {
                    Card tCard = CardService.GetCard(WebData.Card5);
                    if (tCard != null)
                        WebData.Card5GUID = tCard.CardGUID;
                }



                // create the cardholder record

                if (!CardHolderRepository.CreateCardHolder(WebData))
                    status = MembershipCreateStatus.UserRejected;
                else
                {
                    status = MembershipCreateStatus.Success;

                    user = _provider.CreateUser(WebData.UserName, WebData.Password, WebData.email, WebData.passwordQuestion, WebData.passwordAnswer, WebData.isApproved, WebData.providerUserKey, out status);
                    AddUserToCardHolderRole(WebData.UserName);

                    if (WebData.Card1 != null)
                        if (WebData.Card1.Length > 0)
                            CardService.RegisterCard(WebData.ID, WebData.CardHolderGUID, WebData.Card1);
                    if (WebData.Card2 != null)
                        if (WebData.Card2.Length > 0)
                            CardService.RegisterCard(WebData.ID, WebData.CardHolderGUID, WebData.Card2);
                    if (WebData.Card3 != null)
                        if (WebData.Card3.Length > 0)
                            CardService.RegisterCard(WebData.ID, WebData.CardHolderGUID, WebData.Card3);
                    if (WebData.Card4 != null)
                        if (WebData.Card4.Length > 0)
                            CardService.RegisterCard(WebData.ID, WebData.CardHolderGUID, WebData.Card4);
                    if (WebData.Card5 != null)
                        if (WebData.Card5.Length > 0)
                            CardService.RegisterCard(WebData.ID, WebData.CardHolderGUID, WebData.Card5);

                }
            }

            return user;
        }



        //
        // MembershipProvider.GetCardHolderByEmail
        //
        // This should be part of the provider, but I can't figure out how to 
        // extend the provider class with more methods

        public string GetCardHolderByEmail(string email)
        {
            String username = "";
            try
            {
                ICardHolderRepository CardHolderRepository = new CardHolderRepository();
                username = CardHolderRepository.GetCardHolderName(email);
            }
            catch (Exception e)
            {
                WriteToEventLog(e, "GetCardHolderByEmail");

                throw e;
            }
            if (username == null)
                username = "";

            return username;
        }







        // AddUserToCardHolderRole


        public void AddUserToCardHolderRole(String username)
        {
            IRolesDAO RoleRepository = new RolesDAO();
            if (!RoleRepository.RoleExists(CardHolderRole, _provider.ApplicationName))
            {
                throw new Exception("Missing Role - " + CardHolderRole);
            }

            IUserInRolesDAO UsersInRoles = new UserInRolesDAO();
            UsersInRoles.AddUserToRole(username, CardHolderRole, _provider.ApplicationName);
        }




        // GenerateUserName - generates a unique username for this CardHolder


        public string GenerateUserName(String FirstName, String LastName)
        {
            String Proposed = FirstName + "_" + LastName;
            int AppendNumber = 1;
            if ((FirstName == null) || (LastName == null))
                Proposed = "Person";

            while (_provider.GetUser(Proposed, false) != null)
            {
                AppendNumber++;
                Proposed = FirstName + "_" + LastName + AppendNumber.ToString();
            }
            return Proposed;
        }


        //
        // WriteToEventLog
        //   A helper function that writes exception detail to the event log. Exceptions
        // are written to the event log as a security measure to avoid private database
        // details from being returned to the browser. If a method does not return a status
        // or boolean indicating the action succeeded or failed, a generic exception is also 
        // thrown by the caller.
        //

        private string eventLog = "Application";
        private string eventSource = "CardHolderProvider";
        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            // disable this as we are not able to see the log on the hosting service
            //log.WriteEntry(message);
        }



        public bool ValidateUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

            return _provider.ValidateUser(userName, password);
        }

        public bool ValidateUserByEmail(string eMailAddress, string password, out String userName)
        {
            if (String.IsNullOrEmpty(eMailAddress)) throw new ArgumentException("Value cannot be null or empty.", "eMailAddress");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

            bool isValid = false;
            try
            {
                IUserDAO UserRepository = new UserDAO();
                isValid = UserRepository.ValidateUserByEmail(eMailAddress, password, _provider.PasswordAttemptWindow, _provider.MaxInvalidPasswordAttempts, out userName);

            }
            catch (Exception e)
            {
                //                if (_provider.WriteExceptionsToEventLog)
                //                {
                //                    WriteToEventLog(e, "ValidateUserByEmail");
                //
                //                    throw e;
                //                }
                //                else
                //                {
                throw e;
                //                }
            }
            return isValid;
        }


        public MembershipUser CreateUser(string userName, string password, string email, out MembershipCreateStatus status)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");


            MembershipUser user = _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return user;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
            if (String.IsNullOrEmpty(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");

            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }



        // in order to run, the system needs to have been initialized
        bool ICardHolderService.VerifySystemInitialized()
        {
            IUserInRolesDAO UserRoleService = new UserInRolesDAO();
            return UserRoleService.VerifySuperUserExists(_provider.ApplicationName);
        }

        void ICardHolderService.CreateSuperUser(String UserName, String Password, String SecondPassword)
        {
            MembershipCreateStatus status;

            if (String.IsNullOrEmpty(UserName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(Password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(SecondPassword)) throw new ArgumentException("Value cannot be null or empty.", "email");



            // first make sure that super user role exists
            IRolesDAO RoleService = new RolesDAO();
            RoleService.InsureSuperUserRoleExists(_provider.ApplicationName);

            // then add the super user to the list of users

            MembershipUser user = _provider.CreateUser(UserName, Password, "superuser@system",
                null, null, true, null, out status);


            // then add super user to users in roles

            IUserInRolesDAO UserInRoleRepository = new UserInRolesDAO();
            UserInRoleRepository.AddUserToRole(UserName, "SystemAdministrator", _provider.ApplicationName);
   
            // then save the second password by creating a new user with name "secondPassword"

            user = _provider.CreateUser("secondPassword", SecondPassword, "",
                "", //WebData.passwordQuestion, 
                "", //WebData.passwordAnswer, 
                true, //WebData.isApproved, 
                null, //WebData.providerUserKey, 
                out status);


        }

        void ICardHolderService.CreateSystemAdmin(String UserName, String Password, String SecondPassword)
        {
            MembershipCreateStatus status;

            if (String.IsNullOrEmpty(UserName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(Password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(SecondPassword)) throw new ArgumentException("Value cannot be null or empty.", "email");



            // first make sure that super user role exists
            IRolesDAO RoleService = new RolesDAO();
            RoleService.InsureSuperUserRoleExists(_provider.ApplicationName);

            // then add the super user to the list of users

            MembershipUser user = _provider.CreateUser(UserName, Password, UserName + "@system",
                null, null, true, null, out status);
            if (user == null)
            {
                throw new Exception("Problems in creating user");
            }


            // then add super user to users in roles

            IUserInRolesDAO UserInRoleRepository = new UserInRolesDAO();
            UserInRoleRepository.AddUserToRole(UserName, "SystemAdministrator", _provider.ApplicationName);

            // then save the second password by creating a new user with name "secondPassword"

            user = _provider.CreateUser(UserName + "SecondPassword", SecondPassword, UserName + "SecondPassword@systemadmin.system",
                "", //WebData.passwordQuestion, 
                "", //WebData.passwordAnswer, 
                true, //WebData.isApproved, 
                null, //WebData.providerUserKey, 
                out status);


        }






        EditCardHolderModel ICardHolderService.FindCardHolder(String PhoneNumber, String EmailAddress)
        {
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                EditCardHolderModel WebData = new EditCardHolderModel();
                CardHolder CardH = new CardHolder();
                ICardHolderRepository tCardHolderRepository = new CardHolderRepository(GiftEntity);
                if (PhoneNumber != null)
                {
                    CardH = tCardHolderRepository.GetCardHolderByPhoneNumber(PhoneNumber);
                    if (CardH != null)
                        tCardHolderRepository.MoveToWebFormat(WebData, CardH);
                    else
                        return null;
                }
                else
                {
                    CardH = tCardHolderRepository.GetCardholderByEmail(EmailAddress);
                    if (CardH != null)
                        tCardHolderRepository.MoveToWebFormat(WebData, CardH);
                    else
                        return null;
                }

                return WebData;
            }
        }




        /// <summary>
        /// Gets a single cardholder for editing
        /// </summary>
        /// <param name="CardHolderName"></param>
        /// <returns></returns>
        EditCardHolderModel ICardHolderService.GetCardHolder(String CardHolderName)
        {
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                ICardHolderRepository tCardHolderRepository = new CardHolderRepository(GiftEntity);
                EditCardHolderModel WebData = tCardHolderRepository.GetWebCardHolder(CardHolderName);

                return WebData;
            }
        }


        /// <summary>
        /// Returns the balance for a specific card
        /// </summary>
        /// <param name="CardHolderName"></param>
        /// <param name="CardNumber"></param>
        /// <returns></returns>
        GiftCardBalance ICardHolderService.GetCardBalance(String CardHolderName, int ID)
        {
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                ICardHolderRepository tCardHolderRepository = new CardHolderRepository(GiftEntity);
                CardHolder DBCardHolder = tCardHolderRepository.GetCardHolder(CardHolderName);
                Guid CardGUID = WhichCard(DBCardHolder, ID);
                ICardRepository tCardRespository = new CardRepository(GiftEntity);
                List<GiftCardBalance> CBList = tCardRespository.GiftBalance(CardGUID);
                GiftCardBalance CB = new GiftCardBalance();
                CB = CBList[0];
                CB.ID = ID;
                return CB;
            }
        }

        Guid WhichCard(CardHolder DBCardHolder, int ID)
        {
            switch (ID)
            {
                case 1: if (DBCardHolder.Card1 != null) return (Guid)DBCardHolder.Card1;
                    break;
                case 2: if (DBCardHolder.Card2 != null) return (Guid)DBCardHolder.Card2;
                    break;
                case 3: if (DBCardHolder.Card3 != null) return (Guid)DBCardHolder.Card3;
                    break;
                case 4: if (DBCardHolder.Card4 != null) return (Guid)DBCardHolder.Card4;
                    break;
                case 5: if (DBCardHolder.Card5 != null) return (Guid)DBCardHolder.Card5;
                    break;
            }
            return Guid.Empty;
        }
        /// <summary>
        /// returns all the gift card balances for cards registered to this user
        /// </summary>
        /// <param name="CardHolderName"></param>
        /// <returns></returns>
        List<GiftCardBalance> ICardHolderService.GetCardHolderGiftCardBalances(String CardHolderName)
        {

            // was going to put this in a try catch block, 
            // but the exception needs to go back to the screen 

            List<GiftCardBalance> Results = new List<GiftCardBalance>();
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                ICardHolderRepository tCardHolderRepository = new CardHolderRepository(GiftEntity);
                CardHolder DBCardHolder = tCardHolderRepository.GetCardHolder(CardHolderName);
                if (DBCardHolder == null)
                    DBCardHolder = new CardHolder();
                ICardRepository tCardRespository = new CardRepository(GiftEntity);
                List<GiftCardBalance> CBList;

                if (DBCardHolder.Card1 != null)
                {
                    CBList = tCardRespository.GiftBalance((Guid)DBCardHolder.Card1);
                    foreach (GiftCardBalance CB in CBList)
                    {
                        CB.ID = 1;
                        Results.Add(CB);
                    }
                }
                if (DBCardHolder.Card2 != null)
                {
                    CBList = tCardRespository.GiftBalance((Guid)DBCardHolder.Card2);
                    foreach (GiftCardBalance CB in CBList)
                    {
                        CB.ID = 2;
                        Results.Add(CB);
                    }
                }
                if (DBCardHolder.Card3 != null)
                {
                    CBList = tCardRespository.GiftBalance((Guid)DBCardHolder.Card3);
                    foreach (GiftCardBalance CB in CBList)
                    {
                        CB.ID = 3;
                        Results.Add(CB);
                    }
                }
                if (DBCardHolder.Card4 != null)
                {
                    CBList = tCardRespository.GiftBalance((Guid)DBCardHolder.Card4);
                    foreach (GiftCardBalance CB in CBList)
                    {
                        CB.ID = 4;
                        Results.Add(CB);
                    }
                }
                if (DBCardHolder.Card5 != null)
                {
                    CBList = tCardRespository.GiftBalance((Guid)DBCardHolder.Card5);
                    foreach (GiftCardBalance CB in CBList)
                    {
                        CB.ID = 5;
                        Results.Add(CB);
                    }
                }

                return Results;
            }
        }


        List<CardHolderHistoryItem> ICardHolderService.GetCardHistory(String CardHolderName, int CardID)
        {
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                List<CardHolderHistoryItem> Results = new List<CardHolderHistoryItem>();
                ICardHolderRepository tCardHolderRepository = new CardHolderRepository(GiftEntity);
                CardHolder DBCardHolder = tCardHolderRepository.GetCardHolder(CardHolderName);
                IHistoryDAO tHistoryRepository = new HistoryDAO(GiftEntity);

                List<CardHistory> cHistory = tHistoryRepository.GetCardHistory(CardID);
                foreach (CardHistory ch in cHistory)
                {
                    if (ch.TransType != "SHIP")
                    {
                        CardHolderHistoryItem HI = new CardHolderHistoryItem();
                        HI.When = ch.When.ToShortDateString();
                        HI.Where = ch.MerchWhere;
                        HI.WhatHappened = ch.Transaction;
                        HI.Amount = ch.Amount.ToString();
                        if (ch.PointsGranted != null)
                            HI.PointsAwarded = ch.PointsGranted.ToString();
                        HI.Description = ch.Text;
                        Results.Add(HI);
                    }
                }
                return Results;
            }
        }


        int ICardHolderService.GetCardCount(String CardHolderName)
        {
            ICardHolderRepository tCardHolderRepository = new CardHolderRepository();
            return (tCardHolderRepository.GetCardCount(CardHolderName));
        }



        void ICardHolderService.RegisterNewCard(String CardHolderName, String CardNumber)
        {
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                ICardHolderRepository tCardHolderRepository = new CardHolderRepository(GiftEntity);
                ICardRepository tCardRespository = new CardRepository(GiftEntity);
                Card tCard = tCardRespository.GetCard(CardNumber);
                if (tCard == null)
                    throw new Exception("Card not on file");
                if (tCard.Activated != "Y")
                    throw new Exception("Card not activated yet");
                if (tCard.CardHolderGUID != null)
                    throw new Exception("Card already registered to someone else");

                CardHolder tCardHolder = tCardHolderRepository.RegisterThisCard(CardHolderName, tCard.CardGUID);
                if (tCardHolder != null)
                    tCardRespository.RegisterCard(tCardHolder.ID, tCardHolder.CardHolderGUID, CardNumber);
            }
        }

        void ICardHolderService.UpdateCardHolder(EditCardHolderModel WebData, String CardHolderName)
        {
            using (var GiftEntity = new GiftEntities())
            {
                ICardHolderRepository tCardHolderRepository = new CardHolderRepository(GiftEntity);
                tCardHolderRepository.UpdateCardHolder(WebData, CardHolderName);
            }


        }
        void ICardHolderService.UnregisterCard(String CardHolderName, String IDstr)
        {
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                ICardHolderRepository tCardHolderRepository = new CardHolderRepository(GiftEntity);
                CardHolder DBCardHolder = tCardHolderRepository.GetCardHolder(CardHolderName);
                ICardRepository tCardRespository = new CardRepository(GiftEntity);
                int ID = Convert.ToInt32(IDstr);
                Guid CardGUID = WhichCard(DBCardHolder, ID);
                Card DBCard = tCardRespository.GetCard(CardGUID);
                if (DBCard != null)
                {
                    tCardHolderRepository.UnregisterCard(CardHolderName, CardGUID);
                    tCardRespository.UnregisterCard(CardGUID);
                }
            }
        }

        //bool ICardHolderService.EncryptZip()
        //{
        //    ICardHolderRepository tCardHolderRepository = new CardHolderRepository();
        //    return tCardHolderRepository.EncryptZip();
        //}
        //bool ICardHolderService.VerifyEncryptZip()
        //{
        //    ICardHolderRepository tCardHolderRepository = new CardHolderRepository();
        //    return tCardHolderRepository.VerifyEncryptZip();
        //}

        
//        bool ICardHolderService.EncryptCustomerFile()
//        {
//            ICardHolderRepository tCardHolderRepository = new CardHolderRepository();
//            return tCardHolderRepository.EncryptExistingFile();
//        }
//        bool ICardHolderService.VerifyEncryptCustomerFile()
//        {
//            ICardHolderRepository tCardHolderRepository = new CardHolderRepository();
//            return tCardHolderRepository.VerifyEncryptExistingFile();
//        }

    }


    #endregion
}