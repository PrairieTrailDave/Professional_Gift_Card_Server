using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

using Professional_Gift_Card_System.Models;
using Professional_Gift_Card_System.Services;

namespace Professional_Gift_Card_System.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {



        // **************************************
        // URL: /Account/AdministratorLogOn
        // **************************************

        [RemoteRequireHttps]
        [AllowAnonymous]
        public ActionResult AdministratorLogOn()
        {
            // if the system is set up, just display the screen
            // else go to the setup screen

            try
            {
                ICardHolderService CardHolderService = new CardHolderService();
                if (CardHolderService.VerifySystemInitialized())
                    return View();
                return RedirectToAction("Setup", "Account");
            }
            catch (Exception Ex)
            {
                ViewBag.ErrorMessage = Common.StandardExceptionErrorMessage(Ex);
                return RedirectToAction("ViewErrorMessage", "Administration");
            }
            //return RedirectToAction("Index", "Home");
        }

        [RemoteRequireHttps]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult AdministratorLogOn(AdministratorLogOnModel model, string returnUrl)
        {
            ICardHolderService CardHolderService;
            if (ModelState.IsValid)
            {
                try
                {
                    CardHolderService = new CardHolderService();
                    if (CardHolderService.ValidateUser(model.UserName, model.Password))
                    {
                        if (Roles.IsUserInRole(model.UserName, "SystemAdministrator"))
                        {
                            TempData["User"] = model.UserName;
                            Log.LoginAttempts(Request.UserHostAddress, model.UserName, true);
                            return RedirectToAction("Administrator2ndLogOn", "Account");
                        }
                        if (Roles.IsUserInRole(model.UserName, "ClientAdministrator"))
                        {
                            FormsAuthentication.SetAuthCookie(model.UserName, false); //model.RememberMe);
                            Log.LoginAttempts(Request.UserHostAddress, model.UserName, true);
                            return RedirectToAction("ClientAdminIndex", "Administration");
                        }
                        if (Roles.IsUserInRole(model.UserName, "Agent"))
                        {
                            FormsAuthentication.SetAuthCookie(model.UserName, false); //model.RememberMe);
                            Log.LoginAttempts(Request.UserHostAddress, model.UserName, true);
                            return RedirectToAction("AgentIndex", "Administration");
                        }
                        ModelState.AddModelError("", "That user name is not an administrator.");
                        Log.LoginAttempts(Request.UserHostAddress, model.UserName, false);
                    }
                    else
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                        Log.LoginAttempts(Request.UserHostAddress, model.UserName, false);
                    }
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "Admin Log In", Request.Form));
                }
            }
            System.Threading.Thread.Sleep(1000);  // force a one second delay for security purposes
            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [RemoteRequireHttps]
        [AllowAnonymous]
        public ActionResult Administrator2ndLogOn()
        {
            try
            {
                AdministratorLogOnModel ToShow = new AdministratorLogOnModel();
                ToShow.UserName = (string)TempData["User"];
                if (ToShow.UserName == null)
                    ToShow.UserName = "hiGuys!";
                return View(ToShow);
            }
            catch (Exception Ex)
            {
                ViewBag.ErrorMessage = Common.StandardExceptionErrorMessage(Ex);
                return RedirectToAction("ViewErrorMessage", "Administration");
            }
            //return RedirectToAction("Index", "Home");
        }

        [RemoteRequireHttps]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Administrator2ndLogOn(AdministratorLogOnModel model)
        {
            ICardHolderService CardHolderService;
            if (ModelState.IsValid)
            {
                try
                {
                    CardHolderService = new CardHolderService();
                    if (CardHolderService.ValidateUser("secondPassword", model.Password))
                    {
                        if (Roles.IsUserInRole(model.UserName, "SystemAdministrator"))
                        {
                            Log.LoginAttempts(Request.UserHostAddress, model.UserName, true);
                            FormsAuthentication.SetAuthCookie(model.UserName, false); //model.RememberMe);
                            Session["ReturnStack"] = new Stack<String>();
                            return RedirectToAction("Index", "Administration");
                        }
                    }
                    if (CardHolderService.ValidateUser(model.UserName + "SecondPassword", model.Password))
                    {
                        if (Roles.IsUserInRole(model.UserName, "SystemAdministrator"))
                        {
                            Log.LoginAttempts(Request.UserHostAddress, model.UserName, true);
                            FormsAuthentication.SetAuthCookie(model.UserName, false); //model.RememberMe);
                            Session["ReturnStack"] = new Stack<String>();
                            return RedirectToAction("Index", "Administration");
                        }
                    }
                    Log.LoginAttempts(Request.UserHostAddress, model.UserName, false);
                    ModelState.AddModelError("", "Sorry, no go.");
                    System.Threading.Thread.Sleep(1000);  // force a one second delay for security purposes
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionErrorMessage(Ex));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model); // RedirectToAction("Index", "Home")
        }







        // **************************************
        // System Setup
        // **************************************



        [RemoteRequireHttps]
        [AllowAnonymous]
        public ActionResult Setup()
        {
            return View();
        }

        [RemoteRequireHttps]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Setup(AdministratorSetupModel model, string returnUrl)
        {
            ICardHolderService CardHolderService;
            if (ModelState.IsValid)
            {
                if (model.Password == model.RepeatPassword)
                {
                    if (model.SecondPassword == model.RepeatSecondPassword)
                    {
                        try
                        {

                            CardHolderService = new CardHolderService();
                            CardHolderService.CreateSuperUser(model.UserName, model.Password, model.SecondPassword);
                            return RedirectToAction("AdministratorLogOn", "Account");
                        }
                        catch (Exception Ex)
                        {
                            ModelState.AddModelError("", "Error in system initialization:" + Ex.Message);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("RepeatSecondPassword", "The password does not match.");
                    }
                }
                else
                {
                    ModelState.AddModelError("RepeatPassword", "The password does not match.");
                }
            }
            System.Threading.Thread.Sleep(1000);  // force a one second delay for security purposes
            // If we got this far, something failed, redisplay form
            return View(model);
        }




        // **************************************
        // URL: /Account/CardHolderLogOn
        // **************************************
        [RemoteRequireHttps]
        [AllowAnonymous]
        public ActionResult CardHolderLogOn()
        {
            return View();
        }

        [RemoteRequireHttps]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CardHolderLogOn(CardHolderLogOnModel model, string returnUrl)
        {
            ICardHolderService CardHolderService;
            String userName;
            if (ModelState.IsValid)
            {
                try
                {
                    CardHolderService = new CardHolderService();
                    if (CardHolderService.ValidateUserByEmail(model.UserEmailAddress, model.Password, out userName))
                    {
                        Log.LoginAttempts(Request.UserHostAddress, model.UserEmailAddress, true);
                        FormsAuthentication.SetAuthCookie(userName, false);
                        Session["ReturnStack"] = new Stack<String>();
                        return RedirectToAction("Index", "CardHolder");
                    }
                    else
                    {
                        Log.LoginAttempts(Request.UserHostAddress, model.UserEmailAddress, false);
                        ModelState.AddModelError("", "The email address or password provided is incorrect.");
                    }
                }
                catch (Exception Ex)
                {
                    ModelState.AddModelError("", Common.StandardExceptionHandler(Ex, "System Set Up", Request.Form));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }



        //private ApplicationSignInManager _signInManager;
        //private ApplicationUserManager _userManager;

        //public AccountController()
        //{
        //}

        //public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        //{
        //    UserManager = userManager;
        //    SignInManager = signInManager;
        //}

        //public ApplicationSignInManager SignInManager
        //{
        //    get
        //    {
        //        return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        //    }
        //    private set 
        //    { 
        //        _signInManager = value; 
        //    }
        //}

        //public ApplicationUserManager UserManager
        //{
        //    get
        //    {
        //        return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    }
        //    private set
        //    {
        //        _userManager = value;
        //    }
        //}

        ////
        //// GET: /Account/Login
        //[AllowAnonymous]
        //public ActionResult Login(string returnUrl)
        //{
        //    ViewBag.ReturnUrl = returnUrl;
        //    return View();
        //}

        ////
        //// POST: /Account/Login
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // This doesn't count login failures towards account lockout
        //    // To enable password failures to trigger account lockout, change to shouldLockout: true
        //    var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(returnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.RequiresVerification:
        //            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", "Invalid login attempt.");
        //            return View(model);
        //    }
        //}

        ////
        //// GET: /Account/VerifyCode
        //[AllowAnonymous]
        //public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        //{
        //    // Require that the user has already logged in via username/password or external login
        //    if (!await SignInManager.HasBeenVerifiedAsync())
        //    {
        //        return View("Error");
        //    }
        //    return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/VerifyCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // The following code protects for brute force attacks against the two factor codes. 
        //    // If a user enters incorrect codes for a specified amount of time then the user account 
        //    // will be locked out for a specified amount of time. 
        //    // You can configure the account lockout settings in IdentityConfig
        //    var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(model.ReturnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", "Invalid code.");
        //            return View(model);
        //    }
        //}

        ////
        //// GET: /Account/Register
        //[AllowAnonymous]
        //public ActionResult Register()
        //{
        //    return View();
        //}

        ////
        //// POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

        //            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //            // Send an email with this link
        //            // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //            // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //            // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

        //            return RedirectToAction("Index", "Home");
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        ////
        //// GET: /Account/ConfirmEmail
        //[AllowAnonymous]
        //public async Task<ActionResult> ConfirmEmail(string userId, string code)
        //{
        //    if (userId == null || code == null)
        //    {
        //        return View("Error");
        //    }
        //    var result = await UserManager.ConfirmEmailAsync(userId, code);
        //    return View(result.Succeeded ? "ConfirmEmail" : "Error");
        //}

        ////
        //// GET: /Account/ForgotPassword
        //[AllowAnonymous]
        //public ActionResult ForgotPassword()
        //{
        //    return View();
        //}

        ////
        //// POST: /Account/ForgotPassword
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await UserManager.FindByNameAsync(model.Email);
        //        if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
        //        {
        //            // Don't reveal that the user does not exist or is not confirmed
        //            return View("ForgotPasswordConfirmation");
        //        }

        //        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //        // Send an email with this link
        //        // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
        //        // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
        //        // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
        //        // return RedirectToAction("ForgotPasswordConfirmation", "Account");
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        ////
        //// GET: /Account/ForgotPasswordConfirmation
        //[AllowAnonymous]
        //public ActionResult ForgotPasswordConfirmation()
        //{
        //    return View();
        //}

        ////
        //// GET: /Account/ResetPassword
        //[AllowAnonymous]
        //public ActionResult ResetPassword(string code)
        //{
        //    return code == null ? View("Error") : View();
        //}

        ////
        //// POST: /Account/ResetPassword
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    var user = await UserManager.FindByNameAsync(model.Email);
        //    if (user == null)
        //    {
        //        // Don't reveal that the user does not exist
        //        return RedirectToAction("ResetPasswordConfirmation", "Account");
        //    }
        //    var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("ResetPasswordConfirmation", "Account");
        //    }
        //    AddErrors(result);
        //    return View();
        //}

        ////
        //// GET: /Account/ResetPasswordConfirmation
        //[AllowAnonymous]
        //public ActionResult ResetPasswordConfirmation()
        //{
        //    return View();
        //}

        ////
        //// POST: /Account/ExternalLogin
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult ExternalLogin(string provider, string returnUrl)
        //{
        //    // Request a redirect to the external login provider
        //    return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        //}

        ////
        //// GET: /Account/SendCode
        //[AllowAnonymous]
        //public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        //{
        //    var userId = await SignInManager.GetVerifiedUserIdAsync();
        //    if (userId == null)
        //    {
        //        return View("Error");
        //    }
        //    var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
        //    var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
        //    return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/SendCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> SendCode(SendCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }

        //    // Generate the token and send it
        //    if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
        //    {
        //        return View("Error");
        //    }
        //    return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        //}

        ////
        //// GET: /Account/ExternalLoginCallback
        //[AllowAnonymous]
        //public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        //{
        //    var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
        //    if (loginInfo == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    // Sign in the user with this external login provider if the user already has a login
        //    var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(returnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.RequiresVerification:
        //            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
        //        case SignInStatus.Failure:
        //        default:
        //            // If the user does not have an account, then prompt the user to create an account
        //            ViewBag.ReturnUrl = returnUrl;
        //            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
        //            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
        //    }
        //}

        ////
        //// POST: /Account/ExternalLoginConfirmation
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("Index", "Manage");
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        // Get the information about the user from the external login provider
        //        var info = await AuthenticationManager.GetExternalLoginInfoAsync();
        //        if (info == null)
        //        {
        //            return View("ExternalLoginFailure");
        //        }
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //            if (result.Succeeded)
        //            {
        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                return RedirectToLocal(returnUrl);
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    ViewBag.ReturnUrl = returnUrl;
        //    return View(model);
        //}

        ////
        //// POST: /Account/LogOff
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult LogOff()
        //{
        //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        //    return RedirectToAction("Index", "Home");
        //}

        ////
        //// GET: /Account/ExternalLoginFailure
        //[AllowAnonymous]
        //public ActionResult ExternalLoginFailure()
        //{
        //    return View();
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        if (_userManager != null)
        //        {
        //            _userManager.Dispose();
        //            _userManager = null;
        //        }

        //        if (_signInManager != null)
        //        {
        //            _signInManager.Dispose();
        //            _signInManager = null;
        //        }
        //    }

        //    base.Dispose(disposing);
        //}

        //#region Helpers
        //// Used for XSRF protection when adding external logins
        //private const string XsrfKey = "XsrfId";

        //private IAuthenticationManager AuthenticationManager
        //{
        //    get
        //    {
        //        return HttpContext.GetOwinContext().Authentication;
        //    }
        //}

        //private void AddErrors(IdentityResult result)
        //{
        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError("", error);
        //    }
        //}

        //private ActionResult RedirectToLocal(string returnUrl)
        //{
        //    if (Url.IsLocalUrl(returnUrl))
        //    {
        //        return Redirect(returnUrl);
        //    }
        //    return RedirectToAction("Index", "Home");
        //}

        //internal class ChallengeResult : HttpUnauthorizedResult
        //{
        //    public ChallengeResult(string provider, string redirectUri)
        //        : this(provider, redirectUri, null)
        //    {
        //    }

        //    public ChallengeResult(string provider, string redirectUri, string userId)
        //    {
        //        LoginProvider = provider;
        //        RedirectUri = redirectUri;
        //        UserId = userId;
        //    }

        //    public string LoginProvider { get; set; }
        //    public string RedirectUri { get; set; }
        //    public string UserId { get; set; }

        //    public override void ExecuteResult(ControllerContext context)
        //    {
        //        var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
        //        if (UserId != null)
        //        {
        //            properties.Dictionary[XsrfKey] = UserId;
        //        }
        //        context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        //    }
        //}
        //#endregion
    }
}