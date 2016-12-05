// ************************************************************
//
// Copyright (c) 2016 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.Helpers;
using System.Web.UI;
using Professional_Gift_Card_System;
using Professional_Gift_Card_System.Models;
using Professional_Gift_Card_System.Services;

namespace Professional_Gift_Card_System.Controllers
{
    public class MerchantTransController : Controller
    {

        // Note: Demo is allowed to GET, but not POST


        public ITransactionService TransactionServiceInstance { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (TransactionServiceInstance == null) { TransactionServiceInstance = new TransactionService(); }

            base.Initialize(requestContext);
        }

        
        
        //
        // GET: /MerchantTrans/

        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Demo")]
        public ActionResult Index()
        {
            IMerchantService MerchantServiceInstance = new MerchantService();
            String MerchantID = GetFromMerchantIDCookie();
            if (MerchantID == null)
                return RedirectToAction("MerchantLogOn", "Account");
            MerchantPermissions Perms = MerchantServiceInstance.GetPermissions(MerchantID);

            if (Perms.IsRestaurant)
                ViewData["IsRestaurant"] = "1";

            if (Perms.GiftAllowed)
                ViewData["AllowGift"] = "1";

            TempData["ReturnAddress"] = "Index";
            Session.InsureReturnAddress(new ReturnAddress("Index", "MerchantTrans"));
            return View();
        }
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Clerk,Demo")]
        public ActionResult ClerkIndex()
        {
            IMerchantService MerchantServiceInstance = new MerchantService();
            String MerchantID = GetFromMerchantIDCookie();
            MerchantPermissions Perms = MerchantServiceInstance.GetPermissions(MerchantID);
            if (Perms.GiftAllowed)
                ViewData["AllowGift"] = "1";
            TempData["ReturnAddress"] = "ClerkIndex";
            Session.InsureReturnAddress(new ReturnAddress("ClerkIndex"));
            return View();
        }




        //       G I F T   A C T I V A T E
        // 
        // GET:
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Clerk,Demo")]
        public ActionResult GiftCardActivate()
        {
            GiftCardActivateModel WebData = new GiftCardActivateModel();
            WebData.MerchantID = GetFromMerchantIDCookie();
            if (WebData.MerchantID == null)
                return RedirectToAction("MerchantLogOn", "Account");
            WebData.ClerkID = GetFromClerkIDCookie();
            BuildActivateAmounts();
            TempData.Keep("ReturnAddress");
            return View(WebData);
        }

        //
        // POST: 

        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Clerk")]
        [ValidateAntiForgeryToken]
        public ActionResult GiftCardActivate(GiftCardActivateModel WebData)
        {
            TempData.Keep("ReturnAddress");

            try
            {
                if (WebData.Amount != null)
                    if (WebData.Amount.IndexOf('.') < 0)
                        WebData.Amount = WebData.Amount + ".00";

                if (ModelState.IsValid)
                {

                    ReceiptInformation RecInfo = TransactionServiceInstance.ActivateGiftCard(WebData.MerchantID,
                        WebData.ClerkID, 'W', "", WebData.TerminalID, WebData.LocalTime, WebData.CardSwipe, Convert.ToDecimal(WebData.Amount), "");

                    if (RecInfo.ResponseCode == 'A')
                    {
                        Receipt Recpt = TransactionServiceInstance.FormatGiftReceipt(RecInfo);
                        // set return address for the receipt page
                        //Session.PushReturnAddress(new ReturnAddress("GiftCardActivate"));
                        TempData["Receipt"] = Recpt;
                        return RedirectToAction("Receipt");

                    }
                    else
                    {  // need locale based lookup of error codes
                        ModelState.AddModelError("", "Error on Activate: " + Utility.ConvertErrorCodes(RecInfo.ErrorCode));
                        Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                    }
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Activate", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            // If we got this far, something failed, redisplay form
            BuildActivateAmounts();
            return View(WebData);
        }

        private void BuildActivateAmounts()
        {
            ViewBag.ActivateAmounts = new List<string>();
            ViewBag.ActivateAmounts.Add("$5");
            ViewBag.ActivateAmounts.Add("$10");
            ViewBag.ActivateAmounts.Add("$15");
            ViewBag.ActivateAmounts.Add("$20");
            ViewBag.ActivateAmounts.Add("$25");
            ViewBag.ActivateAmounts.Add("$50");
            ViewBag.ActivateAmounts.Add("$75");
            ViewBag.ActivateAmounts.Add("$100");
        }






        //       G I F T   S A L E
        // 
        // GET:
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Clerk,Demo")]
        public ActionResult GiftCardSale()
        {
            GiftCardSaleModel WebData = new GiftCardSaleModel();
            WebData.MerchantID = GetFromMerchantIDCookie();
            if (WebData.MerchantID == null)
                return RedirectToAction("MerchantLogOn", "Account");
            WebData.ClerkID = GetFromClerkIDCookie();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now);
            Response.Cache.SetNoStore();
            TempData.Keep("ReturnAddress");
            return View(WebData);
        }

        //
        // POST: 

        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Clerk")]
        [ValidateAntiForgeryToken]
        public ActionResult GiftCardSale(GiftCardSaleModel WebData)
        {
            TempData.Keep("ReturnAddress");
            try
            {
                if (ModelState.IsValid)
                {

                    ReceiptInformation RecInfo = TransactionServiceInstance.SellGiftCard(
                        WebData.MerchantID, WebData.ClerkID, 'W',
                        "", WebData.TerminalID, WebData.LocalTime,
                        WebData.CardSwipe, Convert.ToDecimal(WebData.Amount),
                        "", WebData.SalesDescription);

                    if (RecInfo.ResponseCode == 'A')
                    {
                        Receipt Recpt = TransactionServiceInstance.FormatGiftReceipt(RecInfo);
                        // set return address for the receipt page
                        //Session.PushReturnAddress(new ReturnAddress("GiftCardSale"));
                        TempData["Receipt"] = Recpt;
                        return RedirectToAction("Receipt");

                    }
                    else
                    {  // need locale based lookup of error codes
                        ModelState.AddModelError("", "Error on Sale: " + Utility.ConvertErrorCodes(RecInfo.ErrorCode));
                        Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                    }
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Sale", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            // If we got this far, something failed, redisplay form
            return View(WebData);
        }





        //       G I F T   R e t u r n
        // 
        // GET:
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Clerk,Demo")]
        public ActionResult GiftCardReturn()
        {
            GiftCardReturnModel WebData = new GiftCardReturnModel();
            WebData.MerchantID = GetFromMerchantIDCookie();
            if (WebData.MerchantID == null)
                return RedirectToAction("MerchantLogOn", "Account");
            WebData.ClerkID = GetFromClerkIDCookie();
            TempData.Keep("ReturnAddress");
            return View(WebData);
        }

        //
        // POST: 
        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Clerk")]
        [ValidateAntiForgeryToken]
        public ActionResult GiftCardReturn(GiftCardReturnModel WebData)
        {
            TempData.Keep("ReturnAddress");
            try
            {
                if (ModelState.IsValid)
                {

                    ReceiptInformation RecInfo = TransactionServiceInstance.GiftCardReturn(
                        WebData.MerchantID, WebData.ClerkID, 'W',
                        "", WebData.TerminalID, WebData.LocalTime,
                        WebData.CardSwipe, Convert.ToDecimal(WebData.AmountOfReturn),
                        "", WebData.ReturnReason);

                    if (RecInfo.ResponseCode == 'A')
                    {
                        Receipt Recpt = TransactionServiceInstance.FormatGiftReceipt(RecInfo);
                        // set return address for the receipt page
                        //Session.PushReturnAddress(new ReturnAddress("GiftCardReturn"));
                        TempData["Receipt"] = Recpt;
                        return RedirectToAction("Receipt");

                    }
                    else
                    {  // need locale based lookup of error codes
                        ModelState.AddModelError("", "Error on Return: " + Utility.ConvertErrorCodes(RecInfo.ErrorCode));
                        Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                    }
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Return", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            // If we got this far, something failed, redisplay form
            return View(WebData);
        }




        //       G I F T   B a l a n c e
        // 
        // GET:
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Clerk,Demo")]
        public ActionResult GiftCardBalance()
        {
            GiftInquiryModel WebData = new GiftInquiryModel();
            WebData.MerchantID = GetFromMerchantIDCookie();
            if (WebData.MerchantID == null)
                return RedirectToAction("MerchantLogOn", "Account");
            WebData.ClerkID = GetFromClerkIDCookie();
            TempData.Keep("ReturnAddress");
            return View(WebData);
        }

        //
        // POST: 
        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Clerk")]
        [ValidateAntiForgeryToken]
        public ActionResult GiftCardBalance(GiftInquiryModel WebData)
        {
            TempData.Keep("ReturnAddress");
            try
            {
                if (ModelState.IsValid)
                {

                    ReceiptInformation RecInfo = TransactionServiceInstance.GiftCardInquiry(
                        WebData.MerchantID, WebData.ClerkID, 'W',
                        "", WebData.TerminalID, WebData.LocalTime,
                        WebData.CardSwipe);

                    if (RecInfo.ResponseCode == 'A')
                    {
                        Receipt Recpt = TransactionServiceInstance.FormatGiftReceipt(RecInfo);
                        // set return address for the receipt page
                        //Session.PushReturnAddress(new ReturnAddress("GiftCardBalance"));
                        TempData["Receipt"] = Recpt;
                        return RedirectToAction("Receipt");

                    }
                    else
                    {  // need locale based lookup of error codes
                        ModelState.AddModelError("", "Error on Balance Inquiry: " + Utility.ConvertErrorCodes(RecInfo.ErrorCode));
                        Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                    }
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Balance Inquiry", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            // If we got this far, something failed, redisplay form
            return View(WebData);
        }















        #region Reports
        // ***************************************************************
        //
        //       R e p o r t s
        //

        //   D a i l y  S a l e s  R e p o r t

        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Demo")]
        public ActionResult DailySales(String TerminalID, DateTime LocalTime)
        {
            if (ModelState.IsValid)
            try
            {
                String MerchantID = GetFromMerchantIDCookie();

                DailySalesInformation RecInfo = TransactionServiceInstance.DailyReport(
                                        MerchantID, "", 'W', "", TerminalID, LocalTime);

                if (RecInfo.ResponseCode == 'A')
                {
                    Receipt Recpt = TransactionServiceInstance.FormatDailyReport(RecInfo);
                    // set return address for the receipt page
                    TempData["Receipt"] = Recpt;
                    return RedirectToAction("Receipt");

                }
                else
                {  // need locale based lookup of error codes
                    ModelState.AddModelError("", "Error on Daily Sales Report: " + Utility.ConvertErrorCodes(RecInfo.ErrorCode));
                    Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Daily Sales Report", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            return View("ReportError");
        }



        //   P r i o r  S u m m a r y  R e p o r t

        // GET : MerchantTrans/PriorSummary

        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Demo")]
        public ActionResult PriorSummaries()
        {
            PriorSummariesModel ToShow = new PriorSummariesModel();
            String MerchantID = GetFromMerchantIDCookie();
            ToShow.PriorDaysCloseTimes = TransactionServiceInstance.GetPriorCloses(MerchantID);
            return View(ToShow);
        }

        // GET : MerchantTrans/PriorSummary/Date
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Demo")]
        public ActionResult PriorSummary(String TerminalID, DateTime LocalTime, String ID)
        {
            String MerchantID = GetFromMerchantIDCookie();
            if (ID.Length < 4)
                return RedirectToAction("PriorSummaries");
            string[] Parts = (ID + ".").Split('.');
            string[] DateParts = (Parts[0] + "-").Split('-');
            string ProperDate = DateParts[1] + "/" + DateParts[0] + "/" + DateParts[2] + " " + Parts[1].Substring(0,2) + ":" + Parts[1].Substring(2);

            //if (ModelState.IsValid)
            DateTime Result;
            if (DateTime.TryParse(ProperDate, out Result))
            {
                try
                {
                    DailySalesInformation RecInfo = TransactionServiceInstance.PriorDailyReport(
                                            MerchantID, "", 'W', TerminalID, LocalTime, ProperDate);

                    if (RecInfo.ResponseCode == 'A')
                    {
                        Receipt Recpt = TransactionServiceInstance.FormatDailyReport(RecInfo);
                        // set return address for the receipt page
                        TempData["Receipt"] = Recpt;
                        return RedirectToAction("Receipt");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to retrieve that report");
                        Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                    }
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Prior Daily Report", Request.Form));
                    Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid Date");
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }

            return RedirectToAction("PriorSummaries");
        }

        // we have a special format for the date and time passed back in the prior day's 


        //   D e t a i l   R e p o r t

        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant,Demo")]
        public ActionResult DetailReport(String TerminalID, DateTime LocalTime)
        {
            if (ModelState.IsValid)
            try
            {
                String MerchantID = GetFromMerchantIDCookie();

                DetailReportInformation RecInfo = TransactionServiceInstance.DetailReport(
                                        MerchantID, "", 'W', "", TerminalID, LocalTime);

                if (RecInfo.ResponseCode == 'A')
                {
                    Receipt Recpt = TransactionServiceInstance.FormatDetailReport(RecInfo);
                    // set return address for the receipt page
                    TempData["Receipt"] = Recpt;
                    return RedirectToAction("Receipt");

                }
                else
                {  // need locale based lookup of error codes
                    ModelState.AddModelError("", "Error on Detail Report: " + Utility.ConvertErrorCodes(RecInfo.ErrorCode));
                    Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Detail Report", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            return View("ReportError");
        }


        //   C l o s e   B a t c h

        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        public ActionResult CloseBatch()
        {
            return View();
        }

            
        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        [ValidateAntiForgeryToken]
        public ActionResult CloseBatch(String TerminalID, DateTime LocalTime)
        {
            
            if (ModelState.IsValid)
            try
            {
                String MerchantID = GetFromMerchantIDCookie();

                bool CloseBatchResp = TransactionServiceInstance.CloseBatch(
                                        MerchantID, "", 'W', "", TerminalID, LocalTime);

                if (CloseBatchResp)
                {
                    Receipt Recpt = TransactionServiceInstance.FormatCloseBatch(LocalTime);
                    // set return address for the receipt page
                    TempData["Receipt"] = Recpt;
                    return RedirectToAction("Receipt");

                }
                else
                {  // need locale based lookup of error codes
                    ModelState.AddModelError("", "Error on Close Batch: ");
                    Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Close Batch", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            return View("ReportError");
        }









        #endregion Reports

        //***************************************************************
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Receipt()
        {
            return View();
        }









        #region ClerkManagement

        // ***************************************************************
        //
        // M a n a g e   C l e r k s
        //
        // GET:
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        public ActionResult ClerkManagement()
        {
            ICLerkService ClerkServiceInstance = new ClerkService();
            String MerchantID = GetFromMerchantIDCookie();
            List<ClerkModel> ClerkList = new List<ClerkModel>();
            ClerkList = ClerkServiceInstance.GetClerks(MerchantID);
            Session.PushReturnAddress(new ReturnAddress("ClerkManagement"));
            return View(ClerkList);
        }


        // C l e r k A d d

        // GET: /MerchantTrans/ClerkAdd

        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        public ActionResult ClerkAdd()
        {
            return View();
        }




        // POST: /MerchantTrans/AddClerk

        // suggest clerk user name and password
        [HttpPost]
        [HttpWhichButton(ButtonName = "SuggestValues")]
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        public ActionResult SuggestValues(SuggestClerkModel SuggestData)
        {
            AddClerkModel WebData = new AddClerkModel();
            WebData.ClerkID = SuggestData.ClerkID;
            WebData.ClerkName = SuggestData.ClerkName;
            try
            {
                if (ModelState.IsValid)
                {
                    ICLerkService ClerkServiceInstance = new ClerkService();
                    WebData.ClerkPassword = ClerkServiceInstance.SuggestClerkPassword();
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Suggest Clerk password", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }

            return View(WebData);
        }



        [HttpPost]
        [HttpWhichButton(ButtonName = "ClerkAdd")]
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        public ActionResult ClerkAdd(AddClerkModel WebData)
        {
            try
            {
                String MerchantID = GetFromMerchantIDCookie();
                if (ModelState.IsValid)
                {
                    ICLerkService ClerkServiceInstance = new ClerkService();
                    if (ClerkServiceInstance.AddClerk(MerchantID, WebData))
                        return RedirectToAction("ClerkManagement");
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Add Clerk", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            // If we got this far, something failed, redisplay form
            return View(WebData);
        }






        // C l e r k E d i t
        // GET: /MerchantTrans/ClerkEdit
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        public ActionResult ClerkEdit(int id)
        {
            ICLerkService ClerkServiceInstance = new ClerkService();
            String MerchantID = GetFromMerchantIDCookie();
            EditClerkModel ToEdit = ClerkServiceInstance.GetClerk(id);

            return View(ToEdit);
        }


        // POST: /MerchantTrans/ClerkEdit
        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        [HttpWhichButton(ButtonName = "UpdateClerk")]
        public ActionResult ClerkEdit(EditClerkModel WebData)
        {

            try
            {
                String MerchantID = GetFromMerchantIDCookie();
                if (ModelState.IsValid)
                {
                    ICLerkService ClerkServiceInstance = new ClerkService();

                    // I tried to put this into an update method, 
                    // but the compiler couldn't find the methods

                    ClerkServiceInstance.DeleteClerk(Convert.ToInt32(WebData.DatabaseID));

                    AddClerkModel ToAdd = new AddClerkModel();
                    ToAdd.ClerkID = WebData.ClerkID;
                    ToAdd.ClerkName = WebData.ClerkName;
                    ToAdd.ClerkPassword = WebData.Password;
                    ClerkServiceInstance.AddClerk(MerchantID, ToAdd);
                    ViewData["Message"] = "Update Successful";
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Clerk Edit", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            return View(WebData);
        }

        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        [HttpWhichButton(ButtonName = "SuggestClerkPassword")]
        public ActionResult ChangeClerkPassword(SuggestClerkModel WebData)
        {
            EditClerkModel ToEdit = new EditClerkModel();
            ToEdit.ClerkID = WebData.ClerkID;
            ToEdit.ClerkName = WebData.ClerkName;
            ToEdit.DatabaseID = WebData.DatabaseID;
            try
            {
                if (ModelState.IsValid)
                {
                    ICLerkService ClerkServiceInstance = new ClerkService();
                    ToEdit.Password = ClerkServiceInstance.SuggestClerkPassword();
                }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Change clerk password", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            ActionResult NewView = View(ToEdit);
            return View(ToEdit);
        }





        // GET: C l e r k D e l e t e 
        //
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        public ActionResult ClerkDelete(int id)
        {

            ICLerkService ClerkServiceInstance = new ClerkService();
            String MerchantID = GetFromMerchantIDCookie();
            EditClerkModel ToDelete = ClerkServiceInstance.GetClerk(id);

            return View(ToDelete);
        }


        // POST: C l e r k D e l e t e 
        //
        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,ClientAdministrator,Merchant")]
        public ActionResult ClerkDelete(int ID, String whichButton)
        {
            try
            {
                    ICLerkService ClerkServiceInstance = new ClerkService();
                    if (ClerkServiceInstance.DeleteClerk(ID))
                    {
                        ViewData["Message"] = "Clerk deleted";
                        return RedirectToAction("ClerkManagement");
                    }

            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Clerk Delete", Request.Form));
                Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
            }
            return View();
        }

        #endregion ClerkManagement













        #region MerchantDataManagement
        //
        // GET: /Merchant/EditMerchData

        [MerchantAuthorize(Roles = "SystemAdministrator,Agent,ClientAdministrator,Merchant")]
        public ActionResult EditMerchData()
        {
            String MerchantID = GetFromMerchantIDCookie();
            if (MerchantID == null)
                return RedirectToAction("MerchantLogOn", "Account");
            IMerchantService MerchantServiceInstance = new MerchantService(); 
            EditMerchDataModel WebMerchant = MerchantServiceInstance.GetMerchant(MerchantID);
            return View(WebMerchant);
        }

        //
        // POST: /Merchant/EditMerchData

        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,Agent,ClientAdministrator,Merchant")]
        [ValidateAntiForgeryToken]
        public ActionResult EditMerchData(EditMerchDataModel WebMerchant)
        {
            String MerchantID = GetFromMerchantIDCookie();
            if (ModelState.IsValid)
            {
                // Attempt to update the merchant
                try
                {
                    IMerchantService MerchantServiceInstance = new MerchantService();
                    bool updateStatus = MerchantServiceInstance.UpdateMerchant(MerchantID, WebMerchant);

                    if (updateStatus == true)
                    {
                        ModelState.AddModelError("", "Updated!");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update the merchant");
                    }
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Edit Merchant Data", Request.Form));
                    Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                }
            }

            return View(WebMerchant);
        }

        //
        // GET: /Merchant/EditMerchData

        [MerchantAuthorize(Roles = "SystemAdministrator,Agent,ClientAdministrator,Merchant")]
        public ActionResult EditReceiptHeader()
        {
            String MerchantID = GetFromMerchantIDCookie();
            if (MerchantID == null)
                return RedirectToAction("MerchantLogOn", "Account");

            IMerchantService MerchantServiceInstance = new MerchantService();
            EditReceiptHeaderModel WebMerchant = MerchantServiceInstance.GetMerchantR(MerchantID);
            return View(WebMerchant);
        }

        //
        // POST: /Merchant/Edit/

        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,Agent,ClientAdministrator,Merchant")]
        [ValidateAntiForgeryToken]
        public ActionResult EditReceiptHeader(EditReceiptHeaderModel WebMerchant)
        {
            String MerchantID = GetFromMerchantIDCookie();
            if (ModelState.IsValid)
            {
                // Attempt to update the merchant
                try
                {
                    IMerchantService MerchantServiceInstance = new MerchantService();
                    bool updateStatus = MerchantServiceInstance.UpdateMerchant(MerchantID, WebMerchant);

                    if (updateStatus == true)
                    {
                        ModelState.AddModelError("", "Updated!");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update the merchant");
                    }
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Update Merchant Header", Request.Form));
                    Log.BadData(Request.UserHostAddress, Request.Url.ToString(), Request.Form);
                }
            }

            return View(WebMerchant);
        }
        #endregion







        // support routines



        public ActionResult _MerchantIDPartial()
        {
            return View();
        }
        public ActionResult Back()
        {
            ReturnAddress WhereToGo = Session.PopReturnAddress();
            if (WhereToGo.Controller == null)
                return RedirectToAction(WhereToGo.Module);
            else
                return RedirectToAction(WhereToGo.Module, WhereToGo.Controller);
        }

        public ActionResult Back2()
        {
            Session.PopReturnAddress();
            return Back();
        }


        String GetFromMerchantIDCookie()
        {
            String MerchID = "";
            HttpCookie cookie = Request.Cookies.Get("MerchantID");
            if (cookie == null)
                MerchID = "Demo";
            else
                MerchID = cookie.Value;
            MerchantIDAttribute Tester = new MerchantIDAttribute();
            if ((!Tester.IsValid(MerchID)) || (MerchID.Length > 46))
            {
                ModelState.AddModelError("", "Invalid Merchant ID");
                MerchID = "";
            }
            return MerchID;
        }
        String GetFromClerkIDCookie()
        {
            String ClerkID = "";
            HttpCookie cookie = Request.Cookies.Get("ClerkID");
            if (cookie != null)
            {
                ClerkID = cookie.Value;
            }
            ClerkIDAttribute Tester = new ClerkIDAttribute();
            if (!Tester.IsValid(ClerkID))
            {
                ClerkID = "";
            }
            return ClerkID;
        }
    }
}
