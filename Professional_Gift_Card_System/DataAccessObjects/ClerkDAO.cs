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
    interface IClerkDAO
    {
        bool CreateClerk(String MerchantID, Clerk ToAdd);
        bool UpdateClerk(Clerk ToUpdate);
        Clerk GetClerk(int ID);
        Clerk GetClerk(Guid MerchID, String ClerkID);
        void DeleteClerk(int ID);
        List<Clerk> ListClerks(String MerchantID);
        Int32 GetClerkID(String MerchantID, String ClerkName);
        bool VerifyClerkID(String MerchantID, int ClerkIDToValidate);
    }


    public class ClerkDAO : BaseDAO <Clerk>, IClerkDAO
    {
        #region IClerkDAO Members

        public ClerkDAO()
        {
        }
        public ClerkDAO(GiftEntities nGiftE)
        {
            GiftEntity = nGiftE;
        }

        bool IClerkDAO.CreateClerk(String MerchantID, Clerk ToAdd)
        {
            using (var GiftEntity = new GiftEntities())
            {
                Guid MerchGUID = GiftEntity.Merchants.FirstOrDefault(d => d.MerchantID == MerchantID).MerchantGUID;
                if (MerchGUID == null) throw new Exception("Invalid merchant id " + MerchantID);
                ToAdd.MerchantGUID = MerchGUID;
                GiftEntity.Clerks.Add(ToAdd);
                GiftEntity.SaveChanges();
                return true;
            }
        }

        void IClerkDAO.DeleteClerk(int ID)
        {
            using (var GiftEntity = new GiftEntities())
            {
                var clerk = GiftEntity.Clerks.FirstOrDefault(d => d.ID == ID);
                if (clerk == null) return;
                GiftEntity.Clerks.Remove(clerk);
                GiftEntity.SaveChanges();
            }
        }

        Clerk IClerkDAO.GetClerk(int ID)
        {
            using (var GiftEntity = new GiftEntities())
            {
                Clerk DBClerk = (from c in GiftEntity.Clerks
                                 where c.ID == ID
                                 select c).FirstOrDefault();
                return DBClerk;
            }
        }

        Clerk IClerkDAO.GetClerk(Guid MerchGUID, string ClerkID)
        {
                 Clerk DBClerk = (from c in GiftEntity.Clerks
                                 where c.ClerkID == ClerkID && c.MerchantGUID == MerchGUID
                                 select c).FirstOrDefault();
                return DBClerk;
        }

        int IClerkDAO.GetClerkID(String MerchantID, string ClerkName)
        {
            if (ClerkName == null) return 0;

            using (var GiftEntity = new GiftEntities())
            {
                Guid MerchGUID = GiftEntity.Merchants.FirstOrDefault(d => d.MerchantID == MerchantID).MerchantGUID;
                if (MerchGUID == null) return 0;
                Clerk DBClerk = (from c in GiftEntity.Clerks
                                 where c.Whom == ClerkName && c.MerchantGUID == MerchGUID
                                 select c).FirstOrDefault();
                if (DBClerk == null) return 0;
                return DBClerk.ID;
            }
        }

        List<Clerk> IClerkDAO.ListClerks(string MerchantID)
        {
            using (var GiftEntity = new GiftEntities())
            {
                Guid MerchGUID = GiftEntity.Merchants.FirstOrDefault(d => d.MerchantID == MerchantID).MerchantGUID;


                List<Clerk> ToReturn = new List<Clerk>();
                foreach (Clerk Clk in (from c in GiftEntity.Clerks
                                       where c.MerchantGUID == MerchGUID
                                       select c))
                                           
                {
                    ToReturn.Add(Clk);
                }
                return ToReturn;
            }
        }

        bool IClerkDAO.UpdateClerk(Clerk ToUpdate)
        {
            using (var GiftEntity = new GiftEntities())
            {
                GiftEntity.Clerks.Attach(ToUpdate);
                GiftEntity.SaveChanges(); //SaveOptions.AcceptAllChangesAfterSave
                return true;
            }
        }

        bool IClerkDAO.VerifyClerkID(String MerchantID, int ClerkIDToValidate)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}