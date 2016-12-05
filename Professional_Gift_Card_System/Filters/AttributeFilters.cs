// ************************************************************
//
// Copyright (c) 2014 Prairie Trail Software, Inc.
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
using System.Reflection;
using System.Text;
using Professional_Gift_Card_System.Models;

// here we define additional attributes that can be used on the models

namespace Professional_Gift_Card_System.Models
{
    // this is used to make sure that the person is authorized to 
    // go to an administration function 
    // If not, then redirect to the admin log on

    public class AdministrationAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary 
                { 
                    { "client", filterContext.RouteData.Values["client"] }, 
                    { "controller", "Account" }, 
                    { "action", "AdministratorLogOn" }, 
                    { "ReturnUrl", filterContext.HttpContext.Request.RawUrl }
                });
            }
        }
    }

    // this is used to make sure that the person is authorized to 
    // go to an merchant function 
    // If not, then redirect to the merchant log on

    public class MerchantAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary 
                { 
                    { "client", filterContext.RouteData.Values["client"] }, 
                    { "controller", "Account" }, 
                    { "action", "MerchantLogOn" }, 
                    { "ReturnUrl", filterContext.HttpContext.Request.RawUrl }
                });
            }
        }
    }

    // this is used to make sure that the person is authorized to 
    // go to a cardholder function 
    // If not, then redirect to the cardholder log on

    public class CardHolderAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary 
                { 
                    { "client", filterContext.RouteData.Values["client"] }, 
                    { "controller", "Account" }, 
                    { "action", "CardHolderLogOn" }, 
                    { "ReturnUrl", filterContext.HttpContext.Request.RawUrl }
                });
            }
        }
    }


    // this turns off the require HTTPs when testing on local machine

    public class RemoteRequireHttpsAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            if (filterContext.HttpContext != null && filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }
            base.OnAuthorization(filterContext);
        }
    }  




    // this attribute allows us to define multiple buttons on a screen
    // and then select which module is executed when that button is pressed
    // This is done by requesting that this module be executed when the buttonname
    // is clicked on.
    //        [HttpWhichButton(ButtonName="AddMerchant")]
    //
    public class HttpWhichButtonAttribute : ActionNameSelectorAttribute
    {
        public String ButtonName { get; set; }
        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            var request = controllerContext.RequestContext.HttpContext.Request;
            var button = request.Form[ButtonName];
            if (button != null) return true;

            return request[methodInfo.Name] != null;
        }
    }




}