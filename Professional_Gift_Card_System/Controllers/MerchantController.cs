// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Text;
using Professional_Gift_Card_System;
using Professional_Gift_Card_System.Models;
using Professional_Gift_Card_System.Services;

namespace Professional_Gift_Card_System.Controllers
{


    public class MerchantController : Controller
    {
        public IMerchantService MerchantServiceInstance { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            //if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MerchantServiceInstance == null) { MerchantServiceInstance = new MerchantService(); }

            base.Initialize(requestContext);
        }



        // does not exist
        // GET: /Merchant/Index

        [AdministrationAuthorize(Roles = "SystemAdministrator,Demo")]
        public ActionResult Index()
        {
            List<MerchantModel> ToShow = null;
            ToShow = MerchantServiceInstance.GetMerchants("", false);
            return View(ToShow);
        }

        //
        // GET: /Merchant/SelectMerchantForEdit

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult SelectMerchantForEdit()
        {
            List<MerchantModel> ToShow = null;
            String AgentUserName = (String)Session["AgentUserName"];
            if (AgentUserName == null) AgentUserName = User.Identity.Name;
            if (AgentUserName.Length == 0) AgentUserName = User.Identity.Name;
            ToShow = MerchantServiceInstance.GetMerchants(AgentUserName, Roles.IsUserInRole(AgentUserName, "Agent"));
            //Session.PushReturnAddress(new ReturnAddress("SelectMerchantForEdit"));
            return View(ToShow);
        }


        //
        // GET: /Merchant/MerchantIndex

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult MerchantIndex(int ID)
        {
            MerchantModel WebMerchant = MerchantServiceInstance.GetMerchant(ID);
            MerchantSelectedModel ToShow = new MerchantSelectedModel();
            ToShow.ID = ID;
            ToShow.MerchantName = WebMerchant.MerchantName;
            ToShow.MerchantAddress1 = WebMerchant.Address1;
            ToShow.MerchantAddress2 = WebMerchant.Address2;
            ToShow.MerchantCityState = WebMerchant.City + ", " + WebMerchant.State + " " + WebMerchant.PostalCode;
            ToShow.MerchantPhoneNumber = WebMerchant.Phone;
            ToShow.PaidUpTo = WebMerchant.PaidToDate.ToShortDateString();
            return View(ToShow);
        }

        //
        // GET: /Merchant/Details

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult Details(int id)
        {
            MerchantModel WebMerchant = MerchantServiceInstance.GetMerchant(id);
            return View(WebMerchant);
        }


        //
        // GET: /Merchant/AddMerchant

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult AddMerchant()
        {
            AddMerchantModel ToAdd = new Professional_Gift_Card_System.Models.AddMerchantModel();
            return View(ToAdd);
        } 

        //
        // POST: /Merchant/AddMerchant

        [HttpPost]
        [HttpWhichButton(ButtonName="AddMerchant")]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult AddMerchant(AddMerchantModel MerchantToAdd)
        {
            IMerchantDAO MerchantData = new MerchantDAO();
            if (MerchantToAdd.MerchantID != null)
            if (MerchantData.GetMerchant(MerchantToAdd.MerchantID) != null)
                ModelState.AddModelError("MerchantID", "Merchant ID already on file");

            if (MerchantToAdd.MerchantUserName != null)
            {
                //if (MerchantData.GetMerchantByUserName(MerchantToAdd.MerchantUserName) != null)
                //    ModelState.AddModelError("UserName", "Merchant User Name already on file");

                IUserDAO UserDAO = new UserDAO();
                if (UserDAO.GetUser(MerchantToAdd.MerchantUserName) != null)
                    ModelState.AddModelError("UserName", "Merchant 'user name' already on file");
            }
            if (ModelState.IsValid)
            {
                // Attempt to add the merchant
                try
                {
                    String AgentUserName = (String)Session["AgentUserName"];
                    if (AgentUserName == null) AgentUserName = User.Identity.Name;
                    if (AgentUserName.Length == 0) AgentUserName = User.Identity.Name;
                    bool createStatus = MerchantServiceInstance.AddMerchant(MerchantToAdd,
                        AgentUserName, Roles.IsUserInRole(AgentUserName, "Agent"));

                    if (createStatus == true)
                    {
                        ModelState.AddModelError("", "Merchant Created");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create the merchant");
                    }
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
                }
            }
            // If we got this far, something failed, redisplay form

            return View(MerchantToAdd);
        }

        //
        // POST: /Merchant/AddMerchant - Suggest Values

        [HttpPost]
        [HttpWhichButton(ButtonName="SuggestValues")]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult SuggestValues(SuggestMerchantModel MerchantToTest)
        {
            AddMerchantModel MerchantToAdd = new Professional_Gift_Card_System.Models.AddMerchantModel();
            if (ModelState.IsValid)
            {
                MerchantToAdd.MerchantID = MerchantServiceInstance.SuggestMerchantID(MerchantToTest.MerchantName, "", "");
                MerchantToAdd.MerchantUserName = MerchantServiceInstance.SuggestUserName(MerchantToTest.MerchantName, "", "");
                MerchantToAdd.MerchantPassword = MerchantServiceInstance.SuggestMerchantPassword();
            }

            MerchantToAdd.MerchantName = MerchantToTest.MerchantName;

            return View(MerchantToAdd);
        }





        //
        // GET: /Merchant/Edit/

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult Edit(int id)
        {
            MerchantModel WebMerchant = MerchantServiceInstance.GetMerchant(id);

            return View(WebMerchant);
        }

        //
        // POST: /Merchant/Edit/

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MerchantModel WebMerchant)
        {
            if (ModelState.IsValid)
            {
                // Attempt to add the merchant
                try
                {
                    bool createStatus = MerchantServiceInstance.UpdateMerchant(WebMerchant);

                    if (createStatus == true)
                    {
                        return RedirectToAction("SelectMerchantForEdit");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update the merchant");
                    }
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
                }
            }
            // If we got this far, something failed, redisplay form

            return View(WebMerchant);
        }


        //
        // GET: /Merchant/EditMerchData

        [MerchantAuthorize(Roles = "SystemAdministrator,Merchant")]
        public ActionResult EditMerchData()
        {
            String MerchantID = GetFromMerchantIDCookie();
            if (MerchantID == null)
                return RedirectToAction("MerchantLogOn", "Account");
            EditMerchDataModel WebMerchant = MerchantServiceInstance.GetMerchant(MerchantID);
            return View(WebMerchant);
        }

        //
        // POST: /Merchant/EditMerchData

        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,Merchant")]
        [ValidateAntiForgeryToken]
        public ActionResult EditMerchData(EditMerchDataModel WebMerchant)
        {
            String MerchantID = GetFromMerchantIDCookie(); 
            if (ModelState.IsValid)
            {
                // Attempt to update the merchant
                try
                {
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error happened in update :" + ex.Message);
                }
            }

            return View(WebMerchant);
        }

        //
        // GET: /Merchant/EditMerchData

        [MerchantAuthorize(Roles = "SystemAdministrator,Merchant")]
        public ActionResult EditReceiptHeader()
        {
            String MerchantID = GetFromMerchantIDCookie();
            if (MerchantID == null)
                return RedirectToAction("MerchantLogOn", "Account");

            EditReceiptHeaderModel WebMerchant = MerchantServiceInstance.GetMerchantR(MerchantID);
            return View(WebMerchant);
        }

        //
        // POST: /Merchant/Edit/

        [HttpPost]
        [MerchantAuthorize(Roles = "SystemAdministrator,Merchant")]
        [ValidateAntiForgeryToken]
        public ActionResult EditReceiptHeader(EditReceiptHeaderModel WebMerchant)
        {
            String MerchantID = GetFromMerchantIDCookie();
            if (ModelState.IsValid)
            {
                // Attempt to update the merchant
                try
                {
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error happened in update :" + ex.Message);
                }
            }

            return View(WebMerchant);
        }

        //
        // GET: /Merchant/SelectMerchantForPayment

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult SelectMerchantForPayment()
        {
            List<MerchantModel> ToShow = null;
            ToShow = MerchantServiceInstance.GetMerchants("", false);
            return View(ToShow);
        }

        //
        // GET: /Merchant/SelectPaidToDate

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult SelectPaidToDate(int id)
        {
            MerchantModel WebMerchant = MerchantServiceInstance.GetMerchant(id);
            // want to compute a suggested date based on
            // billing frequency
            //ViewData["SuggestedDate"] = WebMerchant.
            return View(WebMerchant);
        }
        //
        // POST: /Merchant/SelectPaidToDate

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult SelectPaidToDate(MerchantPaidUpModel WebMerchant)
        {
            if (ModelState.IsValid)
            {
                // Attempt to update the merchant paid to date
                try
                {
                    bool updateStatus = MerchantServiceInstance.UpdateMerchantPaidTo(WebMerchant);

                    if (updateStatus == true)
                    {
                        return RedirectToAction("SelectMerchantForPayment");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update the merchant");
                    }
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
                }
            }
            // If we got this far, something failed, redisplay form
            int id = Convert.ToInt32(WebMerchant.ID);
            MerchantModel WebMerchantD = MerchantServiceInstance.GetMerchant(id);
            return View(WebMerchantD);
        }


        //
        // GET: /Merchant/SetPricing/id

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult SetPricing(int id)
        {
            MerchantModel WebMerchant = MerchantServiceInstance.GetMerchant(id);
            MerchantPricingModel ToShow = new MerchantPricingModel();
            ToShow.ID = WebMerchant.ID;
            ToShow.MerchantName = WebMerchant.MerchantName;
            ToShow.PricingID = WebMerchant.Pricing;
            IPriceService Prices = new PriceService();
            List<SelectListItem> PriceSelection = Prices.GetPricesForSelection();
            ViewBag.Prices = PriceSelection;
            return View(ToShow);
        }


        // POST: /Merchant/SetPricing

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult SetPricing(MerchantPricingModel WebData)
        {
            try
            {
                MerchantServiceInstance.SetMerchantPricing(WebData);
                ModelState.AddModelError("", "Merchant Updated with that pricing");
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }

            // redisplay the page
            IPriceService Prices = new PriceService();
            List<SelectListItem> PriceSelection = Prices.GetPricesForSelection();
            ViewBag.Prices = PriceSelection;
            return View(WebData);
        }



        //
        // GET: /Merchant/SetAgent/id

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult SetAgent(int id)
        {
            MerchantModel WebMerchant = MerchantServiceInstance.GetMerchant(id);
            MerchantPricingModel ToShow = new MerchantPricingModel();
            ToShow.ID = WebMerchant.ID;
            ToShow.MerchantName = WebMerchant.MerchantName;
            return View(ToShow);
        }


        // POST: /Merchant/SetAgent

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult SetAgent(MerchantPricingModel WebData)
        {
            try
            {
                MerchantServiceInstance.SetMerchantPricing(WebData);
                ModelState.AddModelError("", "Merchant Assigned to that Agent");
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }

            // redisplay the page
            return View(WebData);
        }









        //
        // GET: /Merchant/ShipCards/5

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ShipCards(int id)
        {
            MerchantModel WebMerchant = MerchantServiceInstance.GetMerchant(id);
            MerchantShipModel WebShip = new MerchantShipModel();
            WebShip.ID = WebMerchant.ID;
            WebShip.MerchantID = WebMerchant.MerchantID;
            WebShip.MerchantName = WebMerchant.MerchantName;

            return View(WebShip);
        }

        //
        // POST: /Merchant/ShipCards/5

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ShipCards(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add ship card logic here

                return RedirectToAction("SelectMerchantForEdit");
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
                return View();
            }
        }



        //
        // GET: /Merchant/InvoiceMerchants

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult InvoiceMerchants()
        {
            List<MerchantModel> ToShow = null;
            try
            {
                ToShow = MerchantServiceInstance.GetMerchants("", false);
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }
            return View(ToShow);
        }




        //
        // GET: /Merchant/Invoice/5

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult Invoice(int id)
        {
            MerchantInvoicingModel ToShow = new MerchantInvoicingModel();
            try
            {
                ToShow = MerchantServiceInstance.GetMerchantInvoice(id, 0);
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }
            return View(ToShow);
        }

        //
        // POST: /Merchant/Invoice/5

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult Invoice(int id, FormCollection collection)
        {
            try
            {
                // get the final invoice data
                MerchantInvoicingModel ToShow = MerchantServiceInstance.GetMerchantInvoice(id, 1);

                // format it

                return RedirectToAction("SelectMerchantForEdit");
            }
            catch(Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
                return View();
            }
        }




        //
        // GET: /Merchant/History/

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult History(int id)
        {
                HttpContext.Response.Cookies.Add(
                new HttpCookie("ID", id.ToString())
                );
                return RedirectToAction("ShowHistory");
        }


        public ActionResult ShowHistory(int page = 0)
        {
            MerchantHistoryModel ToShow = new MerchantHistoryModel();
            ToShow.MHistory = new List<WebHistory>();
            String ID = GetFromIDCookie();
            if (ID == null)
                return RedirectToAction("SelectMerchantForEdit");
            if (ID.Length < 1)
                return RedirectToAction("SelectMerchantForEdit");
            int mID = Convert.ToInt32(ID);
            int hcount = 0;
            ToShow.PageIndex = page;
            try
            {
                IHistoryDAO tHistoryDAO = new HistoryDAO();

                List<CardHistory> cHistory = tHistoryDAO.GetMerchantHistory(mID, page, 15);
                foreach (CardHistory ch in cHistory)
                {
                    WebHistory HI = new WebHistory();
                    HI.ID = ch.ID.ToString();
                    HI.When = ch.When.ToShortDateString();
                    HI.CardNumber = ch.CardNumber;
                    HI.TransType = ch.Transaction;
                    HI.Amount = ch.Amount.ToString();
                    if (ch.PointsGranted != null)
                        HI.PointsGranted = ch.PointsGranted.ToString();
                    ToShow.MHistory.Add(HI);
                    hcount++;
                }
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }
            ToShow.HasPreviousPage = (page > 0);
            ToShow.HasNextPage = (hcount == 15);
            return View(ToShow);
        }



        //
        // GET: /Merchant/ResetPassword

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ResetPassword(int id)
        {
            MerchantResetPasswordModel ToShow = new MerchantResetPasswordModel();
            ToShow.Password = MerchantServiceInstance.SuggestMerchantPassword();
            ToShow.SecondPassword = ToShow.Password;
            ToShow.ID = Convert.ToString(id);
            return View(ToShow);
        }

        // POST: /Merchant/ResetPassword

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(MerchantResetPasswordModel WebData)
        {
            if (WebData.Password != WebData.SecondPassword)
            {
                ModelState.AddModelError("SecondPassword", "The verify is not matching the Password");
            }
            if (ModelState.IsValid)
            {
                // Attempt to modify the password
                try
                {
                    bool createStatus = MerchantServiceInstance.ResetMerchantPassword(WebData);

                    if (createStatus == true)
                    {
                        return RedirectToAction("SelectMerchantForEdit");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to reset the merchant password");
                    }
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
                }
            }
            // If we got this far, something failed, redisplay form

            return View(WebData);
        }




        public ActionResult RunAsMerchant (int id)
        {
            // get the merchant id and set the cookie

            MerchantModel WebMerchant = MerchantServiceInstance.GetMerchant(id);
            HttpContext.Response.Cookies.Add(
                        new HttpCookie("MerchantID", WebMerchant.MerchantID)
                        );
            HttpContext.Response.Cookies["MerchantID"].HttpOnly = true;
            Session["Who"] = "Merchant";
            Session["ReturnStack"] = new Stack<String>();
            return RedirectToAction("Index", "MerchantTrans");

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

        String GetFromIDCookie()
        {
            String ID = "";
            HttpCookie cookie = Request.Cookies.Get("ID");
            if (cookie != null)
            {
                ID = cookie.Value;
            }
            return ID;
        }
        String GetFromMerchantIDCookie()
        {
            String MerchID = "";
            HttpCookie cookie = Request.Cookies.Get("MerchantID");
            if (cookie != null)
            {
                MerchID = cookie.Value;
            }
            MerchantIDAttribute Tester = new MerchantIDAttribute();
            if ((!Tester.IsValid(MerchID)) || (MerchID.Length > 46))
            {
                ModelState.AddModelError("", "Invalid Merchant ID");
                MerchID = "";
            }
            return MerchID;
        }

    }
}
