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
using System.Web.Mvc;

using Professional_Gift_Card_System;
using Professional_Gift_Card_System.Models;

namespace Professional_Gift_Card_System.Services
{
    #region Services

    public interface IPriceService
    {
        bool AddPrice(AddPriceModel WebPageData);
        EditPriceModel GetPrice(int ID);
        List<EditPriceModel> GetPrices();
        List<SelectListItem> GetPricesForSelection();
        bool DeletePrice(int databaseID);
        bool UpdatePrice(EditPriceModel WebPageData);
    }



    public class PriceService : IPriceService
    {

        bool IPriceService.AddPrice(AddPriceModel WebPageData)
        {
            IPriceDAO PriceData = new PriceDAO();
            Price PriceToAdd = new Price();


            // verify that this is not a duplicate Price 

            Price Duplicate = PriceData.GetPrice(WebPageData.PricingName);
            if (Duplicate != null)
                throw new Exception("Price Name already in system");

            PriceToAdd.PricingName = WebPageData.PricingName;
            PriceToAdd.CardPrice = Convert.ToDecimal(WebPageData.CardPrice);
            PriceToAdd.TransactionPrice = Convert.ToDecimal(WebPageData.TransactionPrice);
            PriceToAdd.SupportTransactionPrice = Convert.ToDecimal(WebPageData.SupportTransactionPrice);
            PriceToAdd.GiftMonthlyFee = Convert.ToDecimal(WebPageData.GiftMonthlyFee);
            PriceToAdd.CardholderMonthlyFee = Convert.ToDecimal(WebPageData.CardholderMonthlyFee);
            PriceToAdd.CardHolderPercentageCharge = Convert.ToInt32(WebPageData.CardHolderPercentageCharge);
            PriceToAdd.AfterXMonths = Convert.ToInt32(WebPageData.AfterXMonths);

            PriceData.CreatePrice(PriceToAdd);

            return true;
        }



        // G e t P r i c e

        EditPriceModel IPriceService.GetPrice(int ID)
        {
            IPriceDAO PriceData = new PriceDAO();
            Price DBPrice = PriceData.GetPriceByID(ID);
            EditPriceModel WebPrice = new EditPriceModel();
            WebPrice.DatabaseID = Convert.ToString(DBPrice.ID);
            WebPrice.PricingName = DBPrice.PricingName;
            WebPrice.CardPrice = DBPrice.CardPrice.ToString();
            WebPrice.TransactionPrice = DBPrice.TransactionPrice.ToString();
            WebPrice.SupportTransactionPrice = DBPrice.SupportTransactionPrice.ToString();
            WebPrice.GiftMonthlyFee = DBPrice.GiftMonthlyFee.ToString();
            WebPrice.CardholderMonthlyFee = DBPrice.CardholderMonthlyFee.ToString();
            WebPrice.CardHolderPercentageCharge = DBPrice.CardHolderPercentageCharge.ToString();
            WebPrice.AfterXMonths = DBPrice.AfterXMonths.ToString();
            return (WebPrice);
        }



        // G e t P r i c e s

        List<EditPriceModel> IPriceService.GetPrices()
        {
            IPriceDAO PriceData = new PriceDAO();
            List<Price> DBPrices = PriceData.ListPrices();
            List<EditPriceModel> WebPrices = new List<EditPriceModel>();
            foreach (Price DBPrice in DBPrices)
            {
                EditPriceModel WebPrice = new EditPriceModel();
                WebPrice.DatabaseID = Convert.ToString(DBPrice.ID);
                WebPrice.PricingName = DBPrice.PricingName;
                WebPrice.CardPrice = DBPrice.CardPrice.ToString();
                WebPrice.TransactionPrice = DBPrice.TransactionPrice.ToString();
                WebPrice.SupportTransactionPrice = DBPrice.SupportTransactionPrice.ToString();
                WebPrice.GiftMonthlyFee = DBPrice.GiftMonthlyFee.ToString();
                WebPrice.CardholderMonthlyFee = DBPrice.CardholderMonthlyFee.ToString();
                WebPrice.CardHolderPercentageCharge = DBPrice.CardHolderPercentageCharge.ToString();
                WebPrice.AfterXMonths = DBPrice.AfterXMonths.ToString();
                WebPrices.Add(WebPrice);
            }
            return (WebPrices);
        }


        List<SelectListItem> IPriceService.GetPricesForSelection()
        {
            List<SelectListItem> PriceSelection = new List<SelectListItem>();
            IPriceDAO PriceData = new PriceDAO();
            List<Price> DBPrices = PriceData.ListPrices();
            foreach (Price DBPrice in DBPrices)
            {
                SelectListItem WebPrice = new SelectListItem();
                WebPrice.Value = Convert.ToString(DBPrice.ID);
                WebPrice.Text = DBPrice.PricingName;
                PriceSelection.Add(WebPrice);
            }
            return PriceSelection;
        }




        bool IPriceService.DeletePrice(int PriceID)
        {
            using (var GiftEntity = new GiftEntities())
            {
                IPriceDAO PriceData = new PriceDAO(GiftEntity);
                PriceData.DeletePrice(PriceID);
            }

            return true;
        }




        bool IPriceService.UpdatePrice(EditPriceModel WebPageData)
        {
            using (var GiftEntity = new GiftEntities())
            {
                int IdToSearchFor = Convert.ToInt32(WebPageData.DatabaseID);
                Price PriceToUpdate = (from c in GiftEntity.Prices
                                       where c.ID == IdToSearchFor
                                       select c).FirstOrDefault();
                if (PriceToUpdate == null)
                {
                    throw new Exception("Trying to update a Price that does not exist");
                }
                PriceToUpdate.PricingName = WebPageData.PricingName;
                PriceToUpdate.CardPrice = Convert.ToDecimal(WebPageData.CardPrice);
                PriceToUpdate.TransactionPrice = Convert.ToDecimal(WebPageData.TransactionPrice);
                PriceToUpdate.SupportTransactionPrice = Convert.ToDecimal(WebPageData.SupportTransactionPrice);
                PriceToUpdate.GiftMonthlyFee = Convert.ToDecimal(WebPageData.GiftMonthlyFee);
                PriceToUpdate.CardholderMonthlyFee = Convert.ToDecimal(WebPageData.CardholderMonthlyFee);
                PriceToUpdate.CardHolderPercentageCharge = Convert.ToInt32(WebPageData.CardHolderPercentageCharge);
                PriceToUpdate.AfterXMonths = Convert.ToInt32(WebPageData.AfterXMonths);
                //GiftEntity.Prices.ApplyCurrentValues(PriceToUpdate);
                GiftEntity.SaveChanges(); //SaveOptions.AcceptAllChangesAfterSave
                return true;
            }
        }




    }




    #endregion Services


}