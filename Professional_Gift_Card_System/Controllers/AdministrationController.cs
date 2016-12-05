// ************************************************************
//
// Copyright (c) 2016 Prairie Trail Software, Inc.
// All rights reserved
//
// ************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class AdministrationController : Controller
    {


        public ICardService CardServiceInstance { get; set; }
        public IMerchantService MerchantServiceInstance { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (CardServiceInstance == null) { CardServiceInstance = new CardService(); }
            if (MerchantServiceInstance == null) { MerchantServiceInstance = new MerchantService(); }
          
            base.Initialize(requestContext);
        }


        //
        // GET: /Administration/

        [AdministrationAuthorize(Roles = "SystemAdministrator,Demo")]
        public ActionResult Index()
        {
            Session.InsureReturnAddress(new ReturnAddress("Index", "Administration"));
            return View();
        }






        //
        // GET: /Administration/AddSystemAdmin
        // index page presented to agents

        [RemoteRequireHttps]
        public ActionResult AddSystemAdmin()
        {
            return View();
        }

        [RemoteRequireHttps]
        [HttpPost]
        public ActionResult AddSystemAdmin(AdministratorSetupModel model, string returnUrl)
        {
            ICardHolderService CardHolderService;

            // move tests here to make sure that we handle them better
            if ((model.Password != null) && (model.RepeatPassword != null))
            {
                if (model.Password != model.RepeatPassword)
                {
                    ModelState.AddModelError("RepeatPassword", "The password does not match.");
                }
            }
            if ((model.SecondPassword != null) && (model.RepeatSecondPassword != null))
            {
                if (model.SecondPassword != model.RepeatSecondPassword)
                {
                    ModelState.AddModelError("RepeatSecondPassword", "The password does not match.");
                }
            }
            if (String.IsNullOrEmpty(model.UserName)) ModelState.AddModelError("UserName", "Value cannot be null or empty.");
            if (String.IsNullOrEmpty(model.Password)) ModelState.AddModelError("Password", "Value cannot be null or empty.");
            if (String.IsNullOrEmpty(model.SecondPassword)) ModelState.AddModelError("SecondPassword","Value cannot be null or empty.");

            MembershipProvider _provider = Membership.Providers["GiftUserMembershipProvider"];
            if (model.UserName != null)
                if (_provider.GetUserNameByEmail(model.UserName + "@system") != "")
                {
                    ModelState.AddModelError("UserName", "UserName@system is already on the system");
                }

            if (ModelState.IsValid)
            {
                try
                {

                    CardHolderService = new CardHolderService();
                    CardHolderService.CreateSystemAdmin(model.UserName, model.Password, model.SecondPassword);
                    return RedirectToAction("Index");
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }















        #region CardInventoryManagement

        // ---------------------------------------------------------
        //
        //            C a r d   I n v e n t o r y    M a n a g e m e n t








        //
        // GET: /Administration/ReceiveCards

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ReceiveCards()
        {
            return View();
        }

        //
        // POST: /Administration/ReceiveCards

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")] //")]
        [ValidateAntiForgeryToken]
        public ActionResult ReceiveCards(ReceiveCards WebPageData, string returnUrl)
        {
            String ErrorCode;

            if (ModelState.IsValid)
            {
                // Attempt to add the card

                String CardToAdd = WebPageData.FirstCardNumber;

                if (WebPageData.NumberOfCards != null)
                {
                    Int32 CountToAdd = Convert.ToInt32(WebPageData.NumberOfCards);
                    ErrorCode = CardServiceInstance.AddCards(CardToAdd, CountToAdd, "", "", "");
                }
                else
                {
                    if (WebPageData.LastCardNumber != null)
                    {
                        String LastCardToAdd = WebPageData.LastCardNumber;
                        ErrorCode = CardServiceInstance.AddCards(CardToAdd, LastCardToAdd, "", "", "");
                    }
                    else
                        ErrorCode = CardServiceInstance.AddCard(CardToAdd, "", "", "");
                }

                if (ErrorCode == "APP  ")
                {
                    WebPageData.LastCardNumber = "";
                    ModelState.AddModelError("", "Card(s) Received into Inventory");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create the record: " + Utility.ConvertErrorCodes(ErrorCode));
                }
            }

            return View();
        }


        //
        // GET: /Administration/ShipCards

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ShipCards()
        {
            ShipCards ToShip = new Professional_Gift_Card_System.Models.ShipCards();
            ToShip.MerchantList = MerchantServiceInstance.GetMerchantsForSelect("", false);
            return View(ToShip);
        }

        //
        // POST: /Administration/ShipCards

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")] //")]
        [ValidateAntiForgeryToken]
        public ActionResult ShipCards(ShipCards WebPageData, string returnUrl)
        {
            String ErrorCode;

            if (ModelState.IsValid)
            {
                // Attempt to add the card

                String CardToShip = WebPageData.FirstCardNumber;
                String ClerkID = "";
                String MerchantID = WebPageData.MerchantName;
                String TransactionText = "Shipping Cards";

                if (WebPageData.NumberOfCards != null)
                {
                    Int32 CountToShip = Convert.ToInt32(WebPageData.NumberOfCards);
                    ErrorCode = CardServiceInstance.ShipCards(MerchantID, ClerkID, CardToShip, CountToShip, TransactionText);
                }
                else
                {
                    if (WebPageData.LastCardNumber != null)
                    {
                        String LastCardToShip = WebPageData.LastCardNumber;
                        ErrorCode = CardServiceInstance.ShipCards(MerchantID, ClerkID, CardToShip, LastCardToShip, TransactionText);
                    }
                    else
                        ErrorCode = "Nothing To Ship";
                }

                if (ErrorCode == "APP  ")
                {
                    ModelState.AddModelError("", "Cards shipped to " + MerchantID);
                }
                else
                {
                    ModelState.AddModelError("", "Failed to ship the cards " + Utility.ConvertErrorCodes(ErrorCode));
                }
            }

            // rebuild the merchant list

            WebPageData.MerchantList = MerchantServiceInstance.GetMerchantsForSelect("", false);
            return View(WebPageData);
        }


        
        //
        // GET: /Administration/AddBatchCards

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult AddBatchCards()
        {
            String AgentUserName = (String)Session["AgentUserName"];
            if (AgentUserName == null) AgentUserName = User.Identity.Name;
            if (AgentUserName.Length == 0) AgentUserName = User.Identity.Name;
            ReceiveAndShipCardModel ToShow = new ReceiveAndShipCardModel();
            ToShow.MerchantList = MerchantServiceInstance.GetMerchantsForSelect(
                AgentUserName, Roles.IsUserInRole(AgentUserName, "Agent"));
            return View(ToShow);
        }

        //
        // POST: /Administration/AddBatchCards

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult AddBatchCards(ReceiveAndShipCardModel WebPageData)
        {
            String ErrorCode = "";
            String BadCard = "";
            String AgentUserName = (String)Session["AgentUserName"];
            if (AgentUserName == null) AgentUserName = User.Identity.Name;
            if (AgentUserName.Length == 0) AgentUserName = User.Identity.Name;


            try
            {
                if (!VerifyNotDuplicateCard(WebPageData.BatchCards, out BadCard))
                    ModelState.AddModelError("BatchCards", "Card " + BadCard + " is a duplicate");

                if (ModelState.IsValid)
                {
                    // Attempt to add the card

                    String CardBatch = WebPageData.BatchCards;
                    String MultiStoreCode = WebPageData.MultiStoreCode;

                    if (WebPageData.BatchCards != null)
                    {
                        ErrorCode = CardServiceInstance.AddAndShipBatch(WebPageData.MerchantName, CardBatch);
                    }

                    if (ErrorCode == "APP  ")
                    {
                        ModelState.AddModelError("", "Card(s) Received into Inventory");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to add the cards: " + Utility.ConvertErrorCodes(ErrorCode));
                    }
                }
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }

            // rebuild the merchant list

            WebPageData.MerchantList = MerchantServiceInstance.GetMerchantsForSelect(
                AgentUserName, Roles.IsUserInRole(AgentUserName, "Agent"));
            return View(WebPageData);
        }


        private bool VerifyNotDuplicateCard(String CardList, out String BadCard)
        {
            BadCard = "";
            String[] Cards = CardList.Split('\n');
            foreach (String Crd in Cards)
            {
                BadCard = Crd;
                if (CardServiceInstance.Getcard(Crd) != null)
                    return false;
            }
            return true;

        }






        #endregion CardInventoryManagement







        #region PriceManagement
        // ---------------------------------------------------------
        //
        //            P r i c i n g   M a n a g e m e n t


        //
        // GET: /Administration/ManagePricing

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ManagePricings()
        {
            List<EditPriceModel> ToShow = null;
            try
            {
                IPriceService PriceServiceInstance = new PriceService();
                ToShow = PriceServiceInstance.GetPrices();
                return View(ToShow);
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }
            return View(ToShow);
        }


        //
        // GET: /Administration/AddPricing

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult AddPricing()
        {
            return View();
        }

        //
        // POST: /Administration/AddPricing

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult AddPricing(AddPriceModel WebPageData)
        {
            if (ModelState.IsValid)
            {
                // Attempt to add the chain
                IPriceService PricingService = new PriceService();
                try
                {
                    bool createStatus = PricingService.AddPrice (WebPageData);

                    if (createStatus == true)
                    {
                        return RedirectToAction("ManagePricings", "Administration");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create the record");
                    }
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
                }
            }
            // If we got this far, something failed, redisplay form
            return View(WebPageData); 
        }


        //
        // GET: /Administration/EditPricing

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult EditPricing(int ID)
        {
            IPriceService PricingService = new PriceService();
            EditPriceModel WebPrice = PricingService.GetPrice(ID);
            return View(WebPrice);
        }

        //
        // POST: /Administration/EditPrice

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPricing(EditPriceModel WebPageData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IPriceService PricingService = new PriceService();
                    bool updateStatus = PricingService.UpdatePrice (WebPageData);

                    if (updateStatus == true)
                    {
                        return RedirectToAction("ManagePricings", "Administration");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update the record");
                    }
                }
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }
            // If we got this far, something failed, redisplay form
            return View(WebPageData);
        }


        //
        // GET: /Administration/PricingDetails

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult PricingDetails(int ID)
        {
            IPriceService PricingService = new PriceService();
            EditPriceModel WebPrice = PricingService.GetPrice(ID);
            return View(WebPrice);
        }


        //
        // GET: /Administration/DeletePricing

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult DeletePricing(int ID)
        {
            IPriceService PricingService = new PriceService();
            EditPriceModel WebPrice = PricingService.GetPrice(ID);
            return View(WebPrice);
        }

        //
        // POST: /Administration/DeletePricing

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePricing(int ID, String buttonname)
        {
            try
            {
                    IPriceService PricingService = new PriceService();
                    bool deleteStatus = PricingService.DeletePrice(ID);

                    if (deleteStatus == true)
                    {
                        return RedirectToAction("ManagePricings", "Administration");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to delete the record");
                    }
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }
            // If we got this far, something failed, redisplay form
            return View();
        }

        #endregion MerchantPriceManagement




        //
        // GET: /Administration/MerchantBalances

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult MerchantBalances()
        {
            try
            {
                IMerchantService MerchantServicesInstance = new MerchantService();
                List<MerchantBalancesModel> ToShow = MerchantServiceInstance.GetTrialBalances();
                return View(ToShow);
            }
            catch (Exception Ex)
            {
                List<MerchantBalancesModel> ToShow = new List<MerchantBalancesModel>();
                ToShow.Add(new MerchantBalancesModel { MerchantName = Common.StandardExceptionErrorMessage(Ex) });
                return View(ToShow);
            }
        }


        //
        // GET: /Administration/MerchantTransferSelect

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult MerchantTransferSelect()
        {
            MerchantTransferSelectModel ToShow = new MerchantTransferSelectModel();
            ToShow.StartDate = DateTime.Now.AddDays(-1);
            ToShow.EndDate = DateTime.Now;
            return View(ToShow);
        }


        //
        // GET: /Administration/MerchantTransfers

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult MerchantTransfers()
        {
            return View();
        }


        //
        // GET: /Administration/CardHistory

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult Cardhistory()
        {
            return View();
        }

        //
        // POST: /Administration/CardHistory

        [HttpPost]
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult Cardhistory(WebLookupCard WebCard)
        {

            try
            {
                //CardHistoryModel PCardHistory = new CardHistoryModel();

                List<WebHistory> TCardHistory = new List<WebHistory>();
                IHistoryDAO tHistoryRepository = new HistoryDAO();

                List<CardHistory> cHistory = tHistoryRepository.GetCardHistory(WebCard.CardNumber);
                foreach (CardHistory ch in cHistory)
                {
                    WebHistory HI = new WebHistory();
                    HI.ID = ch.ID.ToString();
                    HI.When = ch.When.ToShortDateString();
                    HI.MerchantName = ch.MerchWhere;
                    HI.TransType = ch.Transaction;
                    HI.Amount = ch.Amount.ToString();
                    HI.PointsGranted = ch.PointsGranted.ToString();
                    HI.Reward = ch.RewardGranted;
                    HI.Text = ch.Text;
                    TCardHistory.Add(HI);
                }
                TempData["ToDisplay"] = TCardHistory;
                return RedirectToAction("LookUpHistory");
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }
            return View();
        }





        //
        // GET: /Administration/LookUpHistory

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult LookUpHistory()
        {
            List<WebHistory> TCardHistory = (List<WebHistory>)TempData["ToDisplay"];
            if (TCardHistory == null)
                return RedirectToAction("Cardhistory", "Administration");
            return View(TCardHistory);
        }






        //
        // GET: /Administration/ListMerchantCards

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ListMerchantCards()
        {
            List<MerchantModel> ToShow = null;
            ToShow = MerchantServiceInstance.GetMerchants("", false);
            return View(ToShow);
        }

        //
        // GET: /Administration/ListThisMerchantCards

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ListThisMerchantCards(String MerchantID)
        {

            List<String> TCards = new List<String>();
            try
            {
                CardRepository tCardRepository = new CardRepository();
                using (GiftEntities GiftEntity = new GiftEntities())
                {
                    var CardListData = (from c in GiftEntity.Cards
                                        where c.MerchantGUID ==
                                        (from m in GiftEntity.Merchants
                                         where m.MerchantID == MerchantID
                                         select m.MerchantGUID).FirstOrDefault()
                                        select c.CardNumber).DefaultIfEmpty();

                    if (CardListData != null)
                        foreach (String CNum in CardListData)
                        {
                            String CardNum = GiftEncryption.Decrypt(CNum);
                            TCards.Add(CardNum);
                        }
                }
            }
            catch (Exception Ex)
            {
                ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
            }
            return View(TCards);
        }


        //
        // GET: /Administration/ShowMerchantCards

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ShowMerchantCards()
        {
            List<WebHistory> TCardHistory = (List<WebHistory>)TempData["ToDisplay"];
            if (TCardHistory == null)
                return RedirectToAction("Cardhistory", "Administration");
            return View(TCardHistory);
        }




        //
        // GET: /Administration/SummaryReport/When

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ReportSummary(DateTime? When, DateTime? StartDate, String Period = "Daily", String Reseller = "", String ChainSelected = "", String MerchantGroup= "", String MerchantSelected = "")
        {
            try
            {
                SummaryReportingData ToShow = new SummaryReportingData();
                using (GiftEntities GiftEntity = new GiftEntities())
                {
                    IHistoryDAO HistoryRepositoryInstance = new HistoryDAO(GiftEntity);
                    HistoryServices HiInstance = new HistoryServices(GiftEntity);

                    DateTime EndDate;
                    if (When == null)
                        EndDate = DateTime.Now.Date;
                    else
                        EndDate = When.Value;
                    DateTime ToStart = DateTime.Now.Date;
                    if (StartDate != null)
                        ToStart = StartDate.Value;
                    ToShow.ReportData = HiInstance.GetSummaryReport(EndDate, ToStart, Period, Reseller, ChainSelected, MerchantGroup, MerchantSelected);

                    ToShow.When = EndDate;
                    ToShow.When = EndDate;
                }
                return View(ToShow);
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Common.StandardExceptionErrorMessage(Ex);
                return RedirectToAction("ViewErrorMessage");
            }
        }


        //
        // GET: /Administration/TransactionCounts/When

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ReportTransactionCounts(DateTime? When, DateTime? StartDate, String ByWhat="", String Period = "Daily")
        {
            DateTime EndDate;
            DateTime ToStart;
            try
            {
                ReportTransactionCountModel ToShow = new ReportTransactionCountModel();

                List<DailyCountModel> ReportData = new List<DailyCountModel>();
                using (GiftEntities GiftEntity = new GiftEntities())
                {
                    HistoryServices HiInstance = new HistoryServices(GiftEntity);

                    if (When == null)
                        EndDate = DateTime.Now.Date;
                    else
                        EndDate = When.Value;
                    ToStart = DateTime.Now.Date;
                    if (StartDate != null)
                        ToStart = StartDate.Value;

                    ReportData = HiInstance.GetTransactionCounts(EndDate, ToStart, Period, ByWhat);
                    Receipt FormattedReport = FormatTransactionCountReport(ReportData, EndDate, Period, ByWhat);
                    TempData["Report"] = FormattedReport;
                }
                ToShow.Period = Period;
                ToShow.When = EndDate;
                ToShow.ByWhat = ByWhat;
                return View(ToShow);
            }
            catch (Exception Ex)
            {
                TempData["Error"] = Common.StandardExceptionErrorMessage(Ex);
                return RedirectToAction("ViewErrorMessage");
            }
        }


        private Receipt FormatTransactionCountReport(List<DailyCountModel> ReportData, DateTime When, String Period, String ByWhat)
        {
            Receipt Rpt = new Receipt();

            Rpt.AddCentered("Transaction Count Report");
            Rpt.AddCentered("For " + Period + " Ending " + When.ToShortDateString());
            Rpt.Add("");
            Rpt.Add(ByWhat);
            foreach (DailyCountModel DC in ReportData)
            {
                Rpt.Add(CountLineFormat(DC.Level, DC.EntityName, DC.TotalCount));
            }
            return Rpt;
        }

        private String CountLineFormat (int Level, String Name, int Count)
        {
            return LevelSpacing(Level) + PadRight(Name, 30) + RightJustify(Count.ToString(), 5); 
        }

        private String LevelSpacing(int level)
        {
            int Spaces = level * 2;
            String Results = "";
            while (Spaces > 0) Results = Results + " ";
            return Results;
        }
        string RightJustify(String Value, int length)
        {
            while (Value.Length < length)
                Value = " " + Value;
            return Value;
        }
        string PadRight (string Value, int length)
        {
            while (Value.Length < length)
                Value = Value + " ";
            return Value;
        }












        #region CustomerServiceRepManagement

        // ---------------------------------------------------------
        //
        //            C u s t o m e r   S e r v i c e   R e p   M a n a g e m e n t

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult ServiceReps()
        {
            return View();
        }



        #endregion CustomerServiceRepManagement




        #region LogFileManagement

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult SelectLog()
        {
            List<LogHistoryModel> ToShow = Log.GetLogFiles();
            return View(ToShow);
        }


        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public String ShowLog(int id)
        {
            List<LogHistoryModel> ToShow = Log.GetLogFiles();
            if (id < ToShow.Count)
            {
                String FileToShow = ToShow[id].FileName;
                return Log.GetLogFile(FileToShow);
            }
            return "No File";
        }

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ActionResult DeleteLog (int id)
        {
            List<LogHistoryModel> ToShow = Log.GetLogFiles();
            if (id < ToShow.Count)
            {
                String FileToShow = ToShow[id].FileName;
                Log.DeleteLogFile(FileToShow);
            }
            return RedirectToAction("SelectLog");
        }
        
        #endregion



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
        //***************************************************************
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Receipt()
        {
            return View();
        }


        public ActionResult ViewErrorMessage()
        {
            ViewBag.ErrorMessage = TempData["Error"];
            return View();
        }





        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ContentResult PhoneList()
        {
            ICardHolderRepository CardHolderServiceInstance = new CardHolderRepository();
            List<String> cDump = CardHolderServiceInstance.GetPhoneList();
            StringBuilder Page = new StringBuilder();
            Page.Append("<table>");
            foreach (String s in cDump)
            {
                Page.Append("<tr>");
                Page.Append(s);
                Page.Append("</tr>");
            }
            Page.Append("</table>");

            return Content(Page.ToString());
        }

//        [AdministrationAuthorize(Roles = "SystemAdministrator")]
//        public ActionResult EncryptCustomerFile()
//        {
//            ICardHolderService CardHolderServiceInstance = new CardHolderService();
//            CardHolderServiceInstance.EncryptCustomerFile();
//
//            return RedirectToAction("Index", "Administration");
//        }

//        [AdministrationAuthorize(Roles = "SystemAdministrator")]
//        public ActionResult VerifyEncryptCustomerFile()
//        {
//
//            ICardHolderService CardHolderServiceInstance = new CardHolderService();
//            CardHolderServiceInstance.VerifyEncryptCustomerFile();
//
//            return RedirectToAction("Index", "Administration");
//        }



        //
        // GET: /Administration/Dump

        /* 
                  <%: Html.ActionLink("Dump Cards", "DumpCards", "Administration") %>
                  <%: Html.ActionLink("Dump History", "DumpHistory", "Administration") %>

          [AdministrationAuthorize(Roles = "SystemAdministrator")]
                public ContentResult DumpCards()
                {
                    StringBuilder Page = new StringBuilder();
                    ICardRepository CardRespositoryInstance = new CardRepository();
                    List<String> cDump = CardRespositoryInstance.Dump();
                    Page.Append("<p>");
                    foreach (String s in cDump)
                        Page.Append(s + "<br />");
                    Page.Append("</p>");

                    return Content(Page.ToString());
                }
  
  
        <%: Html.ActionLink("Dump History", "DumpHistory", "Administration") %>
*/
        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ContentResult DumpHistory()
        {
            StringBuilder Page = new StringBuilder();

            IHistoryDAO tHistoryRepository = new HistoryDAO();
            List<String> tDump = tHistoryRepository.Dump();
            Page.Append("<p>");
            foreach (String s in tDump)
                Page.Append(s + "<br />");
            Page.Append("</p>");
            //Server.MapPath(".") + "DumpH.txt";
            return Content(Page.ToString());
        }

        [AdministrationAuthorize(Roles = "SystemAdministrator")]
        public ContentResult Update ()
        {
            try
            {
                CardRepository CardRes = new CardRepository();
                //CardRes.ReEncryptCards();
                return Content("Updated");
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message);
            }
        }


    }
}
