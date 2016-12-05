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
using Professional_Gift_Card_System.Services;


namespace Professional_Gift_Card_System.Controllers
{
    public class CardHolderController : Controller
    {
        //
        // GET: /CardHolder/

        public ActionResult Index()
        {
            List<GiftCardBalance> GiftCardBalances = new List<GiftCardBalance>();
            try
            {
                String CardHolderName = this.HttpContext.User.Identity.Name;
                ICardHolderService CardHService = new CardHolderService();
                GiftCardBalances = CardHService.GetCardHolderGiftCardBalances(CardHolderName);
                ViewData["User"] = CardHolderName;
            }
            catch (Exception Ex)
            {
                ViewData["Message"] = "Failure while trying to read balances " + Common.StandardExceptionErrorMessage(Ex);
            }
            return View(GiftCardBalances);
        }


        //
        // GET: /CardHolder/CardHolderBalance

        public ActionResult CardHolderBalance()
        {
            List<GiftCardBalance> GiftCardBalances = new List<GiftCardBalance>();
            try
            {
                String CardHolderName = this.HttpContext.User.Identity.Name;
                ICardHolderService CardHService = new CardHolderService();
                GiftCardBalances = CardHService.GetCardHolderGiftCardBalances(CardHolderName);

            }
            catch (Exception Ex)
            {
                ViewData["Message"] = "Failure while trying to read balance " + Common.StandardExceptionErrorMessage(Ex);
            }
            return View(GiftCardBalances);
        }

        //
        // GET: /CardHolder/CardHolderHistory

        [CardHolderAuthorize(Roles = "SystemAdministrator,ClientAdministrator,CardHolder,Demo")]
        public ActionResult CardHolderHistory(int CardID)
        {
            List<CardHolderHistoryItem> CardHistories = new List<CardHolderHistoryItem>();
            try
            {
                String CardHolderName = this.HttpContext.User.Identity.Name;
                ICardHolderService CardHService = new CardHolderService();
                CardHistories = CardHService.GetCardHistory(CardHolderName, CardID);
            }
            catch (Exception Ex)
            {
                ViewData["Message"] = "Failure while trying to read history " + Common.StandardExceptionErrorMessage(Ex);
            }
            return View(CardHistories);
        }

        //
        // GET: /CardHolder/CardHolderTransfer

        [CardHolderAuthorize(Roles = "SystemAdministrator,ClientAdministrator,CardHolder,Demo")]
        public ActionResult CardHolderTransfer()
        {
            return View();
        }

        //
        // GET: /CardHolder/EditCardHolder

        [CardHolderAuthorize(Roles = "SystemAdministrator,ClientAdministrator,CardHolder")]
        public ActionResult EditCardHolder()
        {
            EditCardHolderModel WebData = new EditCardHolderModel();
            try
            {
                String CardHolderName = this.HttpContext.User.Identity.Name;
                ICardHolderService CardHService = new CardHolderService();
                WebData = CardHService.GetCardHolder(CardHolderName);
            }
            catch (Exception Ex)
            {
                ViewData["Message"] = "Failure while trying to get cardholder data " + Common.StandardExceptionErrorMessage(Ex);
            }
            return View(WebData);
        }


        //
        // POST: /CardHolder/EditCardHolder

        [HttpPost]
        [CardHolderAuthorize(Roles = "SystemAdministrator,ClientAdministrator,CardHolder")]
        [ValidateAntiForgeryToken]
        public ActionResult EditCardHolder(EditCardHolderModel WebData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    String CardHolderName = this.HttpContext.User.Identity.Name;
                    ICardHolderService CardHService = new CardHolderService();
                    CardHService.UpdateCardHolder(WebData, CardHolderName);
                    ViewData["Message"] = "Updated!";
                }
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", "Error on Update: " + Common.StandardExceptionErrorMessage(Ex));
            }
            // If we got this far, something failed, redisplay form
            return View(WebData);
        }


        //
        // GET: /CardHolder/RegisterNewCard

        [CardHolderAuthorize(Roles = "SystemAdministrator,ClientAdministrator,CardHolder")]
        public ActionResult RegisterNewCard()
        {
            ViewData["Message"] = " ";
            return View();
        }
        //
        // POST: /CardHolder/RegisterNewCard

        [HttpPost]
        [CardHolderAuthorize(Roles = "SystemAdministrator,ClientAdministrator,CardHolder")]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterNewCard(RegisterNewCardModel WebData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    String CardHolderName = this.HttpContext.User.Identity.Name;
                    ICardHolderService CardHService = new CardHolderService();
                    if (CardHService.GetCardHolder(CardHolderName) == null)
                        ModelState.AddModelError("", "Card holder not found");
                    else
                    {
                        if (CardHService.GetCardCount(CardHolderName) == 5)
                            ModelState.AddModelError("", "Already have 5 cards registered");
                        else
                        {

                            CardHService.RegisterNewCard(CardHolderName, WebData.CardNumber);
                            ViewData["Message"] = "Registered!";
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", "Error on Card Registration: " + Common.StandardExceptionErrorMessage(Ex));
            }
            // If we got this far, something failed, redisplay form
            return View(WebData);
        }




        //
        // GET: /CardHolder/UnregisterCard

        [CardHolderAuthorize(Roles = "SystemAdministrator,ClientAdministrator,CardHolder")]
        public ActionResult UnregisterCard(int ID)
        {
            UnregisterCardModel CardModel = new UnregisterCardModel();
            GiftCardBalance tBalance = new GiftCardBalance();
            try
            {
                String CardHolderName = this.HttpContext.User.Identity.Name;
                ICardHolderService CardHService = new CardHolderService();
                tBalance = CardHService.GetCardBalance(CardHolderName, ID);
                CardModel.ID = tBalance.ID.ToString();
                CardModel.CardNumber = tBalance.CardNumber;
                CardModel.Balance = tBalance.GiftBalance;
                ViewData["Message"] = " ";
            }
            catch (Exception Ex)
            {
                ViewData["Message"] = "Failure while trying to read balance " + Common.StandardExceptionErrorMessage(Ex);
            }
            return View(CardModel);
        }

        //
        // POST: /CardHolder/UnregisterCard

        [HttpPost]
        [CardHolderAuthorize(Roles = "SystemAdministrator,ClientAdministrator,CardHolder")]
        [ValidateAntiForgeryToken]
        public ActionResult UnregisterCard(int ID, UnregisterCardModel WebData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    String CardHolderName = this.HttpContext.User.Identity.Name;
                    ICardHolderService CardHService = new CardHolderService();
                    CardHService.UnregisterCard(CardHolderName, WebData.ID);
                    ViewData["Message"] = "Unregistered!";
                }
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", "Error on Card Unregistration: " + Common.StandardExceptionErrorMessage(Ex));
            }
            // If we got this far, something failed, redisplay form
            return View(WebData);
        }

    }
}
