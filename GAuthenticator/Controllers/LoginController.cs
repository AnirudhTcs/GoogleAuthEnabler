using GAuthenticator.Models;
using Google.Authenticator;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace GAuthenticator.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

        //[Authorize]
        public ActionResult UserProfile()
        {
            ViewBag.Message = "Welcome to  " + Session["Username"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult Login(LoginModel login)
        {
            string message = "";
            bool status = false;
            //check UserName and password form our database here
            string GAuthPrivKey = WebConfigurationManager.AppSettings["GAuthPrivateKey"];
            string UserUniqueKey = login.UserName + GAuthPrivKey;
            if (login.UserName == "Admin" && login.Password == "12345") // Admin as user name and 12345 as Password
            {
                status = true;
                Session["UserName"] = login.UserName;

                if (WebConfigurationManager.AppSettings["GAuthEnable"].ToString() == "1")
                {

                    message = "Two Factor Authentication Verification";

                    //Two Factor Authentication Setup
                    TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();

                    Session["UserUniqueKey"] = UserUniqueKey;
                    var setupInfo = TwoFacAuth.GenerateSetupCode("anirudh.s@experionglobal.com", login.UserName, UserUniqueKey, 300, 300);
                    ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                    ViewBag.SetupCode = setupInfo.ManualEntryKey;
                }

            }
            else
            {
                message = "Please Enter the Valid Credential!";
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View();
        }
        public ActionResult TwoFactorAuthenticate()
        {
            var token = Request["CodeDigit"];
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            string UserUniqueKey = Session["UserUniqueKey"].ToString();
            bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, token);
            if (isValid)
            {
                Session["IsValidTwoFactorAuthentication"] = true;
                return RedirectToAction("UserProfile", "Login");
            }
            return RedirectToAction("Login", "Login");
        }
        public ActionResult Logoff()
        {
            Session["UserName"] = null;
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();
            return RedirectToAction("Login");
        }
    }
}