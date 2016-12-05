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
using Professional_Gift_Card_System.Models;
using Professional_Gift_Card_System.Services;

namespace Professional_Gift_Card_System.Controllers
{
    public class CustomerSupportController : Controller
    {
        //
        // GET: /CustomerSupport/

        public ActionResult Index()
        {
            return View();
        }


        //
        // GET: /ServiceReps/CardholderLookup

        public ActionResult CardholderLookup()
        {
            return View();
        }

        //
        // POST: /ServiceReps/CardholderLookup

        [HttpPost]
        public ActionResult CardholderLookup(CardHolderLookupModel WebData)
        {
            if ((WebData.PhoneNumber == null) && (WebData.Email == null))
                ModelState.AddModelError("PhoneNumber", "Must specify either phone number or email address");

            if (ModelState.IsValid)
            {

                try
                {
                    ICardHolderService CardHolderServiceInstance = new CardHolderService();
                    EditCardHolderModel CHData = CardHolderServiceInstance.FindCardHolder(WebData.PhoneNumber, WebData.Email);
                    if (CHData != null)
                        return View("ShowCardHolder", CHData);
                    else
                        ModelState.AddModelError("", "Nobody found");

                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Ex.Message);
                }
            }
            return View(WebData);
        }

        public ActionResult Back()
        {
            ReturnAddress WhereToGo = Session.PopReturnAddress();
            if (WhereToGo.Controller == null)
                return RedirectToAction(WhereToGo.Module);
            else
                return RedirectToAction(WhereToGo.Module, WhereToGo.Controller);
        }


    }
}
