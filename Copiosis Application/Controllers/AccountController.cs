using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Copiosis_Application.Filters;
using Copiosis_Application.Models;
using Copiosis_Application.DB_Data;

namespace Copiosis_Application.Controllers
{
    
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        private static string ADMINROLE = "ADMIN";
        private static string USERROLE = "USER";
        
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //var db = new CopiosisEntities();
            //var x = db.locations.FirstOrDefault(l => l.neighborhood == "Kenton");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if(ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                using(var db = new CopiosisEntities())
                {
                    var x = db.users.Where(u => u.username == model.UserName).First();
                    x.lastLogin = DateTime.Now;
                    db.SaveChanges();
                }
                return RedirectToAction("Overview");
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                
                location location;
                // Check if signup code is valid.
                using (var db = new CopiosisEntities())
                {
                    var keyCheck = db.locations.Where(s => s.signupKey.Equals(model.Token));
                    location = keyCheck.FirstOrDefault();
                    if (keyCheck.Any() == false)
                    {
                        ModelState.AddModelError("", "Invalid signup code.");
                        return View(model);
                    }
                }

                // Attempt to register the user
                try
                {
                    //Make sure admin role is created in the roles table, if not create it
                    //Do not ever assign a user to admin role via the application, this should be done via a sql query
                    if(!Roles.RoleExists(ADMINROLE))
                    {
                        Roles.CreateRole(ADMINROLE);
                    }
                    //Make sure user role is created in the roles table, if not create it
                    if (!Roles.RoleExists(USERROLE))
                    {
                        Roles.CreateRole(USERROLE);
                    }

                    // Make calls for .NET to handle authentication.
                    WebSecurity.CreateUserAndAccount(
                        model.UserName, 
                        model.Password,
                        new {
                                firstName   = model.FirstName,
                                lastName    = model.LastName,
                                email       = model.Email,
                                status        = 1,
                                nbr         = 100,
                                lastLogin   = DateTime.Now,
                                locationID  = location.locationID 
                            }
                        );

                    Roles.AddUserToRole(model.UserName, USERROLE);
                    WebSecurity.Login(model.UserName, model.Password);
                    return RedirectToAction("Overview", "Account");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
                
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/Overview
        // Overview of transactions for the current user
        public ActionResult Overview()
        {
            return View();
        }

        // GET: /Account/View
        // View a specific transaction. Probably takes some kind of GUID.
        public ActionResult View(Guid tranId)
        {
            return View();
        }

        // GET: /Account/Create
        // Create a new transaction whether producer or consumer. Just returns the view.
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Account/Create
        // Create a new transaction. Needs to take a model that matchs the form.
        [HttpPost]
        public ActionResult Create(int foo)
        {
            return View();
        }

        // POST: /Account/Confirm 
        // Confirm a transaction. Takes a GUID and the satisfaction rating provided by the consumer.
        [HttpPost]
        public ActionResult Confirm(Guid tranId, int satisfactionRating)
        {
            return View();
        }

        // GET: /Account/Items
        // This will serve as the Item Library to show all the items a user has. Probably takes some kind of GUID.
        [HttpGet]
        public ActionResult Items()
        {
            List<ItemsModel> model = new List<ItemsModel>();
            /* 
             * Down below is essentially your connection to the database. By saying new CopiosisEntities() you are essentially
             * creating a new connection in the database. 
             */
            using(var db = new CopiosisEntities()){
                 /*
                  * This is essentially how you are going to write your database queries. When you enter db. you are given the tables
                  * in the database, in this case locations. From there you can do a Where or something else to select something from 
                  * the database using a lambda expression. So for the rows in locations, give me the first one with the country
                  * equal to USA 
                  */ 
                int userId = WebSecurity.CurrentUserId;
                var items = db.products.Where(a => a.ownerID == userId).ToList();
                foreach (var value in items){
                    ItemsModel item = new ItemsModel();
                    item.ProductName = value.name;
                    item.Description = value.description;
                    item.Gateway = value.gateway;
                    item.ItemClass = value.itemClass;
                    item.ItemGuid = value.guid;
                    model.Add(item);
                }
            }
            /* 
             * Now you need to return your results to the client through some model that you are going to create in the Models folder.
             * What you are going to want in the model is everything that the frontend guys will need to show in the page. So you
             * are probably going to want the name, description, gateway,and item class. When you create this model, make sure you put it 
             * in the Models folder in the solution 
             */
            return View(model);
        }

        // GET: /Account/AddItem
        // Add an item. Just returns the view.
        public ActionResult AddItem()
        {
            AddItemModel model = new AddItemModel();
            Dictionary<string, int> itemClassGateway = new Dictionary<string, int>();
            using (var db = new CopiosisEntities())
            {
                var items = db.itemClasses.ToList();
                foreach (var item in items)
                {
                    itemClassGateway.Add(item.name, (int)item.suggestedGateway);
                }
            }
            model.ItemClassTemplates = itemClassGateway;
            return View(model);
        }
        
        // POST: /Account/AddItem
        // Save a new item to the database. Takes a model of the new item.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddItem(AddItemModel model)
        {
            if (model.Name == null || model.Name == string.Empty)
            {
                throw new ArgumentException("Product name is required");
            }

            if (model.Gateway < 0)
            {
                throw new ArgumentException("Product cannot have a negative gateway");
            }

            if (model.Description == null || model.Description == string.Empty)
            {
                throw new ArgumentException("Product description is required");
            }

            product p = new product();
            using (var db = new CopiosisEntities())
            {
                int? itemClassId = db.itemClasses.Where(ic => ic.name == model.ItemClass).Select(i => i.classID).FirstOrDefault();
                if (itemClassId == null)
                {
                    throw new ArgumentException("Product item class not found");
                }

                p.name = model.Name;
                p.ownerID = WebSecurity.CurrentUserId;
                p.guid = Guid.NewGuid();
                p.gateway = model.Gateway;
                p.description = model.Description;
                p.createdDate = DateTime.Now;
                p.itemClass = (int)itemClassId;

                db.SaveChanges();
            }


            return View();
        }

        // GET: /Account/EditItem
        // Edit an item. Probably takes some kind of GUID.
        public ActionResult EditItem(Guid itemId)
        {
            return View();
        }

        // POST: /Account/EditItem
        // Update an existing item in the database. Takes a model of the new item.
        [HttpPost]
        public ActionResult EditItem(AddItemModel model)
        {
            return View();
        }

        // POST: /Account/DeleteItem
        // Deactivate an item. Take the GUID of the item as a parameter
        [HttpPost]
        public ActionResult DeleteItem(Guid itemId)
        {
            //this will actually probably return some Json result for the client to handle
            return View();
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public ActionResult UsersNBR()
        {
            double? nbr = 0;
            bool result = true;
            using (var db = new CopiosisEntities())
            {
                var user = db.users.Where(u => u.userID == WebSecurity.CurrentUserId).FirstOrDefault();
                if (user == null)
                {
                    result = false;    
                }
                nbr = user.nbr.HasValue ? user.nbr : 0;
            }

            return Json(new {success = result, nbr = result ? nbr : null}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Calculate NBR will calculate the NBR that a producer is to receive based on the 
        /// satisfaction rating the consumer has provided for the transaction.
        /// </summary>
        /// <param name="satisfactionRating">Satisfaction Rating the consumer provided for the transaction</param>
        /// <param name="productId">The ID# of the product involved in the transaction</param>
        /// <param name="providerId">The ID# of the producer in the transaction</param>
        /// <returns>A float for the NBR calculated</returns>
        private float CalculateNBR(int satisfactionRating, int productId, int providerId)
        {
            throw new NotImplementedException();
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }
        
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
