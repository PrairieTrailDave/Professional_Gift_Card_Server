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
    interface IPriceDAO
    {
        bool CreatePrice(Price NewPrice);
        void DeletePrice(int ID);
        Price GetPrice(Guid PriceGUID);
        Price GetPrice(string PriceName);
        Price GetPriceByID(int ID);
        List<Price> ListPrices();
        bool UpdatePrice(Price ToUpdate);
    }

    public class PriceDAO : BaseDAO<Price>, IPriceDAO
    {
        public PriceDAO()
        {
        }
        public PriceDAO(GiftEntities nGiftEntity)
        {
            GiftEntity = nGiftEntity;
        }
        bool IPriceDAO.CreatePrice(Price NewPrice)
        {
            InitializeConnection();
            using (GiftEntity)
            {
                GiftEntity.Prices.Add(NewPrice);
                GiftEntity.SaveChanges();
            }
            return true;
        }
        void IPriceDAO.DeletePrice(int ID)
        {
            InitializeConnection();
            var Price = GiftEntity.Prices.FirstOrDefault(d => d.ID == ID);
            if (Price == null) return;
            GiftEntity.Prices.Remove(Price);
            GiftEntity.SaveChanges();
        }

        Price IPriceDAO.GetPrice(Guid PriceGUID)
        {
            InitializeConnection();
            Price DBPrice = (from c in GiftEntity.Prices
                             where c.PriceGUID == PriceGUID
                             select c).FirstOrDefault();
            return DBPrice;
        }

        Price IPriceDAO.GetPrice(string PriceName)
        {
            InitializeConnection();
            Price DBPrice = (from c in GiftEntity.Prices
                             where c.PricingName == PriceName
                             select c).FirstOrDefault();
            return DBPrice;
        }
        Price IPriceDAO.GetPriceByID(int PriceID)
        {
            InitializeConnection();
            Price DBPrice = (from c in GiftEntity.Prices
                             where c.ID == PriceID
                             select c).FirstOrDefault();
            return DBPrice;
        }
        List<Price> IPriceDAO.ListPrices()
        {
            InitializeConnection();
            List<Price> ToReturn = new List<Price>();
            foreach (Price Prc in (from c in GiftEntity.Prices
                                   select c))
            {
                ToReturn.Add(Prc);
            }
            return ToReturn;
        }

        bool IPriceDAO.UpdatePrice(Price ToUpdate)
        {
            InitializeConnection();
            GiftEntity.Prices.Attach(ToUpdate);
            GiftEntity.SaveChanges(); //SaveOptions.AcceptAllChangesAfterSave
            return true;
        }


    }

}