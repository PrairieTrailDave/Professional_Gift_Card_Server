// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

using Professional_Gift_Card_System;
using Professional_Gift_Card_System.Models;

namespace Professional_Gift_Card_System.Services
{
    #region Services
    public interface ICLerkService
    {
        bool AddClerk(String MerchantID, AddClerkModel WebPageData);
        EditClerkModel GetClerk(int ID);
        List<ClerkModel> GetClerks(String MerchantID);
        String SuggestClerkPassword();
        bool DeleteClerk(int databaseID);
    }


    public class ClerkService : ICLerkService
    {
        #region iChainService Members
        String ClerkRole = "Clerk";

        bool ICLerkService.AddClerk(String MerchantID, AddClerkModel WebPageData)
        {
            String ResultMessage = "";
            using (GiftEntities GiftEntity = new GiftEntities())
            {
                IClerkDAO ClerkData = new ClerkDAO(GiftEntity);
                Clerk ClerkToAdd = new Clerk();


                // verify that this is not a duplicate clerk at this merchant location

                IMerchantDAO MerchantData = new MerchantDAO(GiftEntity);
                Merchant Merch = MerchantData.GetMerchant(MerchantID);
                Clerk Duplicate = ClerkData.GetClerk(Merch.MerchantGUID, WebPageData.ClerkID);
                if (Duplicate != null)
                    throw new Exception("Clerk ID already in system");

                // this is something that would make sense to have in a transaction
                // but to do that would require a DAO that can have the 
                // connection defined outside of it. 

                String ClerkUserName = GetUniqueClerkUserName(WebPageData.ClerkName);
                ClerkToAdd.Whom = WebPageData.ClerkName;
                ClerkToAdd.ClerkID = WebPageData.ClerkID;
                ClerkToAdd.UserName = ClerkUserName;
                ClerkData.CreateClerk(MerchantID, ClerkToAdd);

                String ClerkEmail = GetUniqueClerkEmail(MerchantID, ClerkUserName);

                // we also need to define this "user name" in the system 

                MembershipUser user = AddClerkToUsers(ClerkUserName, ClerkEmail, WebPageData.ClerkPassword, out ResultMessage);
                if (user == null) throw new Exception(ResultMessage);

                // and give it the clerk role

                AddUserToClerkRole(ClerkUserName);
            }
            return true;
        }

        // AddClerkToUsers

        MembershipUser AddClerkToUsers(String ClerkUserName, String ClerkEmail, String Password, out String ResultMessage)
        {
            MembershipProvider _provider;
            _provider = Membership.Providers["GiftUserMembershipProvider"];
            MembershipUser user;
            MembershipCreateStatus status;
            ResultMessage = "";
            String username = ClerkUserName;
            String password = Password;
            String email = ClerkEmail;
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


        String GetUniqueClerkUserName(String ClerkName)
        {
            IUserDAO UserData = new UserDAO();
            String results = UserData.GetUniqueUserName(ClerkName);
            return results;
        }

        // if we call get unique user name prior to this call, 
        // then the email is guarenteed to be unique

        String GetUniqueClerkEmail(String MerchantID, String ClerkName)
        {
            String results = ClerkName + "@" + MerchantID;
            return results;
        }
        // AddUserToClerkRole

        public void AddUserToClerkRole(String username)
        {
            MembershipProvider _provider;
            _provider = Membership.Providers["GiftUserMembershipProvider"];

            IRolesDAO RoleDAO = new RolesDAO();
            if (!RoleDAO.RoleExists(ClerkRole, _provider.ApplicationName))
            {
                throw new Exception("Missing Role - " + ClerkRole);
            }

            IUserInRolesDAO UsersInRoles = new UserInRolesDAO();
            UsersInRoles.AddUserToRole(username, ClerkRole, _provider.ApplicationName);
        }




        // G e t C l e r k

        EditClerkModel ICLerkService.GetClerk(int ID)
        {
            IClerkDAO ClerkData = new ClerkDAO();
            Clerk DBClerk = ClerkData.GetClerk(ID);
            EditClerkModel WebClerk = new EditClerkModel();
            WebClerk.DatabaseID = Convert.ToString(ID);
            WebClerk.ClerkID = DBClerk.ClerkID;
            WebClerk.ClerkName = DBClerk.Whom;
            WebClerk.Password = HidePassword("          ");
            return (WebClerk);
        }

        String HidePassword(String ToHide)
        {
            StringBuilder hidden = new StringBuilder();
            int i;
            for (i = 0; i < ToHide.Length; i++)
                hidden.Append('*');
            return hidden.ToString();
        }



        // G e t C l e r k s

        List<ClerkModel> ICLerkService.GetClerks(String MerchantID)
        {
            IClerkDAO ClerkData = new ClerkDAO();
            List<Clerk> DBClerks = ClerkData.ListClerks(MerchantID);
            List<ClerkModel> WebClerks = new List<ClerkModel>();
            foreach (Clerk ch in DBClerks)
            {
                ClerkModel WebCh = new ClerkModel();
                WebCh.DatabaseID = Convert.ToString(ch.ID);
                WebCh.ClerkID = ch.ClerkID;
                WebCh.ClerkName = ch.Whom;
                WebClerks.Add(WebCh);
            }
            return (WebClerks);
        }

        String ICLerkService.SuggestClerkPassword()
        {
            IMerchantService MerchSvcs = new MerchantService();
            return MerchSvcs.SuggestMerchantPassword();
        }


        bool ICLerkService.DeleteClerk(int ClerkID)
        {
            using (var GiftEntity = new GiftEntities())
            {
                IClerkDAO ClerkData = new ClerkDAO();
                Clerk tClerk = ClerkData.GetClerk(ClerkID);

                IUserDAO UserData = new UserDAO();
                UserData.DeleteUser(tClerk.UserName);
                IUserInRolesDAO UserRolesData = new UserInRolesDAO();
                UserRolesData.DeleteUserFromRole(tClerk.UserName);

                ClerkData.DeleteClerk(ClerkID);
            }

            return true;
        }







        public void MoveToWebPage(ClerkModel WebPage, Clerk EntityStore)
        {
            WebPage.ClerkID = EntityStore.ClerkID;
            WebPage.ClerkName = EntityStore.Whom;
        }

        public void MoveFromWebPage(Clerk EntityStorage, ClerkModel WebPage)
        {
            EntityStorage.Whom = WebPage.ClerkName;
            EntityStorage.ClerkID = WebPage.ClerkID;
        }
        #endregion

    }

    #endregion
}