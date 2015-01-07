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
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                using (var db = new CopiosisEntities())
                {
                    var x = db.users.Where(u => u.username == model.UserName).First();
                    x.prevLastLogin = x.lastLogin.HasValue ? x.lastLogin.Value : (DateTime?)null;
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
                    if (!Roles.RoleExists(ADMINROLE))
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
                        new
                        {
                            firstName = model.FirstName,
                            lastName = model.LastName,
                            email = model.Email,
                            status = 1,
                            nbr = 100,
                            lastLogin = DateTime.Now,
                            locationID = location.locationID
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
            TransactionOverviewModel model = new TransactionOverviewModel();

            using (var db = new CopiosisEntities())
            {
                int userId = WebSecurity.CurrentUserId;
                DateTime? userLastLogin = db.users.Where(u => u.userID == userId).Select(u => u.prevLastLogin).FirstOrDefault();

                model.pendingUser = db.transactions.Where(
                    a => 
                    (a.providerID == userId || a.receiverID == userId) &&
                    a.dateClosed == null &&
                    a.createdBy != userId 
                ).Select(t => new TransactionModel {
                    newSinceLogin   = userLastLogin.HasValue ? (userLastLogin.Value < t.dateAdded) : false,
                    transactionID   = t.transactionID,
                    date            = t.date.ToString(),
                    status          = t.status,
                    dateAdded       = t.dateAdded,
                    createdBy       = t.createdBy,
                    dateClosed      = t.dateClosed ?? DateTime.MinValue,
                    nbr             = t.nbr??0.0,
                    satisfaction    = (int)t.satisfaction,

                    providerID          = t.providerID,
                    //providerNotes       = t.providerNotes,
                    providerFirstName   = t.user1.firstName,
                    providerLastName    = t.user1.lastName,
                    providerUsername    = t.user1.username,
                    //providerEmail       = t.user1.email,

                    receiverID          = t.receiverID,
                    //receiverNotes       = t.receiverNotes,
                    receiverFirstName   = t.user2.firstName,
                    receiverLastName    = t.user2.lastName,
                    receiverUsername    = t.user2.username,
                    //receiverEmail       = t.user2.email,

                    productID           = t.productID,
                    productDesc         = t.productDesc,
                    productName         = t.product.name,
                    //productGateway      = t.product.gateway,
                    //productItemClass    = t.product.itemClass,
                    //productCreatedDate  = t.product.createdDate,
                    //productDeletedDate  = t.product.deletedDate??DateTime.MinValue,
                    productGuid         = t.product.guid
                }).ToList();

                model.pendingOther = db.transactions.Where(
                    a =>
                    (a.providerID == userId || a.receiverID == userId) &&
                    a.dateClosed == null &&
                    userId == a.createdBy 
                ).Select(t => new TransactionModel
                {
                    newSinceLogin       = userLastLogin.HasValue ? (userLastLogin.Value < t.dateAdded) : false,
                    transactionID       = t.transactionID,
                    date                = t.date.ToString(),
                    status              = t.status,
                    dateAdded           = t.dateAdded,
                    createdBy           = t.createdBy,
                    dateClosed          = t.dateClosed ?? DateTime.MinValue,
                    nbr                 = t.nbr ?? 0.0,
                    satisfaction        = t.satisfaction,

                    providerID          = t.providerID,
                    providerFirstName   = t.user1.firstName,
                    providerLastName    = t.user1.lastName,
                    providerUsername    = t.user1.username,

                    receiverID          = t.receiverID,
                    receiverFirstName   = t.user2.firstName,
                    receiverLastName    = t.user2.lastName,
                    receiverUsername    = t.user2.username,

                    productID           = t.productID,
                    productDesc         = t.productDesc,
                    productName         = t.product.name,
                    productGuid         = t.product.guid
                }).ToList();


                model.completed = db.transactions.Where(
                    a =>
                    (a.providerID == userId || a.receiverID == userId) &&
                    a.dateClosed != null
                ).Select(t => new TransactionModel
                {
                    transactionID       = t.transactionID,
                    date                = t.date.ToString(),
                    status              = t.status,
                    dateAdded           = t.dateAdded,
                    createdBy           = t.createdBy,
                    dateClosed          = t.dateClosed ?? DateTime.MinValue,
                    nbr                 = t.nbr ?? 0.0,
                    satisfaction        = (int)t.satisfaction,

                    providerID          = t.providerID,
                    providerFirstName   = t.user1.firstName,
                    providerLastName    = t.user1.lastName,
                    providerUsername    = t.user1.username,

                    receiverID          = t.receiverID,
                    receiverFirstName   = t.user2.firstName,
                    receiverLastName    = t.user2.lastName,
                    receiverUsername    = t.user2.username,

                    productID           = t.productID,
                    productDesc         = t.productDesc,
                    productName         = t.product.name,
                    productGuid         = t.product.guid
                }).ToList();

            }

            return View(model);
        }

        // GET: /Account/View
        // View a specific transaction. Probably takes some kind of GUID.
        public ActionResult View(Guid tranId)
        {
            return View();
        }

        // GET: /Account/Create
        // Create a new transaction whether producer or consumer. Just returns the view.
        public ActionResult Create(string type)
        {
            if(type == null)
            {
                throw new ArgumentException("Type of transaction must be specified");
            }

            string typelower = type.ToLower();
            NewTransactionModel model = new NewTransactionModel();
            if(type == "consumer")
            {
                model.Producer = false;
                List<string> producers = new List<string>();
                List<string> products = new List<string>();

                using(var db = new CopiosisEntities())
                {
                    var usersWithProducts = db.products.Where(p => p.ownerID != WebSecurity.CurrentUserId && p.user.status == 1).Select(u => u.user).Distinct().ToList();

                    if (usersWithProducts.Count > 0)
                    {
                        foreach (var pro in usersWithProducts)
                        {
                            producers.Add(string.Format("{0} {1} | {2} | {3}", pro.firstName, pro.lastName, pro.username, pro.email));
                        }

                        var initialProducer = usersWithProducts.First();
                        var initialItemList = InitialProducerItems(initialProducer.userID);
                        foreach (var item in initialItemList)
                        {
                            products.Add(item.ProductName);
                        }
                    }
                }
                model.Products = products;
                model.Producers = producers;
                
            }
            else if(type == "producer")
            {
                model.Producer = true;
                
                var producerItems = CurrenUserItems();
                List<string> products = new List<string>();
                foreach (var item in producerItems)
                {
                    products.Add(item.ProductName);
                }
                model.Products = products;
                
                List<string> consumers = new List<string>();
                using(var db = new CopiosisEntities())
                {
                    var c = db.users.Where(u => u.status == 1 && u.userID != WebSecurity.CurrentUserId)
                        .Select(s => new { FirstName = s.firstName, LastName = s.lastName, Username = s.username, Email = s.email}).ToList();
                    foreach(var con in c)
                    {
                        consumers.Add(string.Format("{0} {1} | {2} | {3}", con.FirstName, con.LastName, con.Username, con.Email));
                    }
                }
                model.Consumers = consumers;
            }
            else
            {
                //throw some error or default to something?
            }
            return View(model);
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
            model = CurrenUserItems();
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
                itemClassGateway = FetchItemClassTemplates(db);
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
            ValidateItemModel(model);
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

                db.products.Add(p);
                db.SaveChanges();
            }

            return RedirectToAction("Items");
        }

        [HttpGet]
        public ActionResult GatewayNBR(string name)
        {
            double? defaultGateway = 0;
            bool result = true;
            using (var db = new CopiosisEntities())
            {
                defaultGateway = db.itemClasses.Where(ic => ic.name == name).Select(i => i.suggestedGateway).FirstOrDefault();
                if (defaultGateway == null)
                {
                    result = false;
                    defaultGateway = 0;
                }
            }

            return Json(new { success = result, defaultGateway = result ? defaultGateway : null }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ProducerItems(string name)
        {
            List<string> products = new List<string>();
            bool result = true;
            using (var db = new CopiosisEntities())
            {
                int? producerID = db.users.Where(u => u.username == name).Select(uID => uID.userID).FirstOrDefault();
                if(producerID == null)
                {
                    result = false;
                }
                
                products = db.products.Where(po => po.ownerID == producerID).Select(p => p.name).ToList();
                if (products == null)
                {
                    result = false;
                }
            }

            return Json(new { success = result, products = result ? products : null }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Account/EditItem
        // Edit an item. Probably takes some kind of GUID.
        [HttpGet]
        public ActionResult EditItem(Guid itemId)
        {
            AddItemModel model = new AddItemModel();

            using (var db = new CopiosisEntities())
            {
                var item = db.products.Where(p => p.guid == itemId && p.ownerID == WebSecurity.CurrentUserId).FirstOrDefault();
                if (item == null)
                {
                    throw new ArgumentException(string.Format("Product with ID {0} not found", itemId));
                }
                else
                {
                    model.Name = item.name;
                    model.ItemClass = item.itemClass1.name;
                    model.Description = item.description;
                    model.Gateway = item.gateway;
                    model.ItemClassTemplates = FetchItemClassTemplates(db);
                }
            }

            return View(model);
        }

        // POST: /Account/EditItem
        // Update an existing item in the database. Takes a model of the new item.
        [HttpPost]
        public ActionResult EditItem(AddItemModel model, Guid itemId)
        {
            ValidateItemModel(model);
            using (var db = new CopiosisEntities())
            {
                var item = db.products.Where(p => p.guid == itemId && p.ownerID == WebSecurity.CurrentUserId).FirstOrDefault();
                int itemClassId = db.itemClasses.Where(ic => ic.name == model.ItemClass).Select(i => i.classID).First();
                if (item == null)
                {
                    throw new ArgumentException(string.Format("Product with ID {0} not found", itemId));
                }
                else
                {
                    item.name = model.Name;
                    item.description = model.Description;
                    item.gateway = model.Gateway;
                    item.itemClass = itemClassId;
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Items");
        }

        // POST: /Account/DeleteItem
        // Deactivate an item. Take the GUID of the item as a parameter
        public ActionResult DeleteItem(Guid itemId)
        {
            bool result = true;
            using(var db = new CopiosisEntities())
            {
                var item = db.products.Where(p => p.guid == itemId && p.ownerID == WebSecurity.CurrentUserId && p.deletedDate == null).FirstOrDefault();
                if(item == null)
                {
                    result = false;
                }
                else
                {
                    item.deletedDate = DateTime.Now;
                    db.SaveChanges();
                }
            }
            
            if(result)
            {
                return RedirectToAction("Items");
            }
            else
            {
                ModelState.AddModelError("DeletionError", "Unable to delete item");
                return View("Items", CurrenUserItems());
            }
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

            return Json(new { success = result, nbr = result ? nbr : null }, JsonRequestBehavior.AllowGet);
        }


        private Dictionary<string, int> FetchItemClassTemplates(CopiosisEntities db)
        {
            Dictionary<string, int> itemClasses = new Dictionary<string, int>();
            var items = db.itemClasses.ToList();
            if (items != null)
            {
                foreach (var item in items)
                {
                    itemClasses.Add(item.name, (int)item.suggestedGateway);
                }
            }
            return itemClasses;
        }

        private void ValidateItemModel(AddItemModel model)
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
        }

        private List<ItemsModel> CurrenUserItems()
        {
            List<ItemsModel> model = new List<ItemsModel>();

            using (var db = new CopiosisEntities())
            {
                int userId = WebSecurity.CurrentUserId;
                var items = db.products.Where(a => a.ownerID == userId && a.deletedDate == null).ToList();
                foreach (var value in items)
                {
                    ItemsModel item = new ItemsModel();
                    item.ProductName = value.name;
                    item.Description = value.description;
                    item.Gateway = value.gateway;
                    item.ItemClass = value.itemClass1.name;
                    item.ItemGuid = value.guid;
                    model.Add(item);
                }
            }
            return model;
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

        //Used to generate the initial list of producer items so that Razor doesn't complain about the lack of objects in NewTransaction.Products.
        private List<ItemsModel> InitialProducerItems(int currentID)
        {
            List<ItemsModel> model = new List<ItemsModel>();

            using (var db = new CopiosisEntities())
            {
                var items = db.products.Where(a => a.ownerID == currentID && a.deletedDate == null).ToList();
                foreach (var value in items)
                {
                    ItemsModel item = new ItemsModel();
                    item.ProductName = value.name;
                    item.Description = value.description;
                    item.Gateway = value.gateway;
                    item.ItemClass = value.itemClass1.name;
                    item.ItemGuid = value.guid;
                    model.Add(item);
                }
            }
            return model;
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
