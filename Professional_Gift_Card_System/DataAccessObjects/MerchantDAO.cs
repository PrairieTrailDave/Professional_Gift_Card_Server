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
using Professional_Gift_Card_System;

namespace Professional_Gift_Card_System.Models
{

    interface IMerchantDAO
    {
        bool CreateMerchant(Merchant ToAdd);
        bool UpdateMerchant(Merchant ToUpdate);
        Merchant GetMerchant(Int32 Which);
        Merchant GetMerchant(String MerchantID);
        Merchant GetMerchant(Guid MerchantGUID);
        List<Merchant> GetAllActiveMerchants();
        Merchant GetMerchantByUserName(String MerchantUserName);
        void DeactivateMerchant();
        bool DeleteMerchant(String MerchantID);
        List<String> ListMerchantIDsLike(String Starting);
        String GetMaxMerchantIDLike(String Starting);
        String GetUniqueUserName(String SuggestedName);
        MerchantPermissions GetMerchantPermissions(String MerchantID);
    }
    public class MerchantDAO :BaseDAO<Merchant>, IMerchantDAO
    {
        public MerchantDAO()
        {
        }
        public MerchantDAO(GiftEntities nGiftEntity)
        {
            GiftEntity = nGiftEntity;
        }
        #region IMerchantRepository Members

        bool IMerchantDAO.CreateMerchant(Merchant ToAdd)
        {
            InitializeConnection();
            ToAdd.MerchantID = ToAdd.MerchantID.ToUpper();
            GiftEntity.Merchants.Add(ToAdd);
            GiftEntity.SaveChanges();
            return true;
        }


        bool IMerchantDAO.DeleteMerchant(String MerchantID)
        {
            throw new Exception("Can not delete a merchant!");
            //return true;
        }

        bool IMerchantDAO.UpdateMerchant(Merchant ToUpdate)
        {
            // update is problematic. 
            return false;
        }

        Merchant IMerchantDAO.GetMerchant(Int32 Which)
        {
            InitializeConnection();
            return (GiftEntity.Merchants.FirstOrDefault(d => d.ID == Which));

        }

        Merchant IMerchantDAO.GetMerchant(String MerchantID)
        {
            MerchantID = MerchantID.ToUpper();
            InitializeConnection();
            return (GiftEntity.Merchants.FirstOrDefault(d => d.MerchantID == MerchantID));
        }

        Merchant IMerchantDAO.GetMerchant(Guid MerchantGUID)
        {
            InitializeConnection();
            return (GiftEntity.Merchants.FirstOrDefault(d => d.MerchantGUID == MerchantGUID));
        }

        List<Merchant> IMerchantDAO.GetAllActiveMerchants()
        {
            InitializeConnection();
            return (GiftEntity.Merchants.Where(m => (m.GiftActive == "A") ).ToList());
        }


        Merchant IMerchantDAO.GetMerchantByUserName(String MerchantUserName)
        {
            InitializeConnection();
            return (GiftEntity.Merchants.FirstOrDefault(d => d.MerchantUserName == MerchantUserName));
        }




        MerchantPermissions IMerchantDAO.GetMerchantPermissions(String MerchantID)
        {
            MerchantPermissions Results = new MerchantPermissions();
            Results.IsRestaurant = false;
            Results.GiftAllowed = false;
            Results.LoyaltyAllowed = false;
            Results.MemberOfChainWithLoyalty = false;
            Results.MemberOfGroupWithLoyalty = false;
            InitializeConnection();
            var SearchResults = (from M in GiftEntity.Merchants
                                 where M.MerchantID == MerchantID
                                 select new
                                  {
                                      M.Restaurant,
                                      M.GiftActive,
                                  }
                                  ).FirstOrDefault();

            if (SearchResults == null)
                return Results;

            if (SearchResults.Restaurant != null)
                if (SearchResults.Restaurant == "Y")
                    Results.IsRestaurant = true;
            if (SearchResults.GiftActive == "A")
                Results.GiftAllowed = true;

            return Results;
        }





        void IMerchantDAO.DeactivateMerchant()
        {
            throw new NotImplementedException();
        }



        List<String> IMerchantDAO.ListMerchantIDsLike(String Starting)
        {
            List<String> ToReturn = new List<String>();

            InitializeConnection();
            var MerchantIDs = (from M in GiftEntity.Merchants
                               where M.MerchantID.StartsWith(Starting)
                               select M.MerchantID);

            foreach (String MerchID in MerchantIDs)
            {
                ToReturn.Add(MerchID);
            }
            return ToReturn;
        }


        String IMerchantDAO.GetMaxMerchantIDLike(String Starting)
        {
            String MerchID = "";

            InitializeConnection();
            var MerchantID = (from M in GiftEntity.Merchants
                              where M.MerchantID.StartsWith(Starting)
                              select M.MerchantID).Max();
            if (MerchantID != null)
                MerchID = MerchantID;
            return MerchID;
        }

        // GetUniqueUserName
        // We could have similarly named merchants,
        // so add a unique number to the end of the name

        // this is a slow algorithm. A better one would do a max starting with.
        // but then, we would need to extract the last number and increment it.

        String IMerchantDAO.GetUniqueUserName(String SuggestedName)
        {
            String MerchUserName = SuggestedName;
            bool NotFound = true;
            int AddedValue = 0;

            InitializeConnection();
            while (NotFound)
            {
                var DBMerchantUserName = (from M in GiftEntity.Merchants
                                          where M.MerchantUserName == MerchUserName
                                          select M.MerchantUserName).FirstOrDefault();
                if (DBMerchantUserName == null)
                    NotFound = false;
                else
                {
                    AddedValue++;
                    MerchUserName = MerchUserName + AddedValue.ToString();
                }
            }
            return MerchUserName;

        }

        #endregion
    }
}