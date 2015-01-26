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
using System.Collections;

namespace Copiosis_Application.Controllers
{
    [CustomErrorHandling]
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        private static string ADMINROLE = "ADMIN";
        private static string USERROLE = "USER";
        private static string ERROR_SUBJECT_TEMPDATA_KEY = "errorSubject";
        private static string ERROR_MESSAGE_TEMPDATA_KEY = "errorMessage";
        //Used by CustomErrorHandling attribute to set the error messages -- which does most of the error handling logic:
        public List<string> errorDictionaryKeys = new List<string>{ERROR_SUBJECT_TEMPDATA_KEY, ERROR_MESSAGE_TEMPDATA_KEY};
        public Models.ErrorModel ACCOUNTERROR = new Models.ErrorModel();

        //
        // GET: /Account/

        public ActionResult Index()
        {
            return RedirectToAction("Overview");
        }

        public ActionResult Unauthorized()
        {
            return View();
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if(Request.IsAuthenticated)
            {
                return RedirectToAction("Unauthorized");
            }
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
                List<int> existingVendorCodes = new List<int>();
                // Check if signup code is valid.
                using (var db = new CopiosisEntities())
                {
                    existingVendorCodes = db.users.Where(u => u.vendorCode != -1).Select(u => u.vendorCode).ToList();
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

                    //Generate a random vendor code that is not already assigned to a user
                    Random rand = new Random();
                    int vc = rand.Next(1000, 9999);
                    while(existingVendorCodes.Contains(vc))
                    {
                        vc = rand.Next(1000, 9999);
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
                            nbr = 0,
                            lastLogin = DateTime.Now,
                            locationID = location.locationID,
                            vendorCode = vc
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
                    newSinceLogin = userLastLogin.HasValue ? (userLastLogin.Value.CompareTo(t.dateAdded) < 0) : false,
                    transactionID   = t.transactionID,
                    date            = t.date.ToString(),
                    status          = t.status,
                    dateAdded       = t.dateAdded,
                    dateClosed      = t.dateClosed ?? DateTime.MinValue,
                    nbr             = t.nbr??0.0,
                    otherParty      = t.providerID == userId ? (t.receiver.firstName + " " + t.receiver.lastName) : (t.provider.firstName + " " + t.provider.lastName),
                    productName     = t.product.name,
                    productDesc     = t.productDesc,
                    productGateway  = t.product.gateway
                }).OrderByDescending(t => t.dateAdded).ToList();

                model.pendingOther = db.transactions.Where(
                    a =>
                    (a.providerID == userId || a.receiverID == userId) &&
                    a.dateClosed == null &&
                    userId == a.createdBy 
                ).Select(t => new TransactionModel
                {
                    newSinceLogin       = userLastLogin.HasValue ? (userLastLogin.Value.CompareTo(t.dateAdded) < 0) : false,
                    transactionID       = t.transactionID,
                    date                = t.date.ToString(),
                    status              = t.status,
                    dateAdded           = t.dateAdded,
                    dateClosed          = t.dateClosed ?? DateTime.MinValue,
                    nbr                 = (t.providerID == userId) ? ((t.nbr == null) ? 0.0 : t.nbr) : t.product.gateway,
                    otherParty          = t.providerID == userId ? (t.receiver.lastName + ", " + t.receiver.firstName) : (t.provider.lastName + ", " + t.provider.firstName),
                    productName         = t.product.name,
                    productDesc         = t.productDesc,
                    productGateway      = t.product.gateway
                }).OrderByDescending(t => t.dateAdded).ToList();


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
                    dateClosed          = t.dateClosed ?? DateTime.MinValue,
                    nbr                 = t.nbr ?? 0.0,
                    otherParty          = t.providerID == userId ? (t.receiver.firstName + " " + t.receiver.lastName) : (t.provider.firstName + " " + t.provider.lastName),
                    productName         = t.product.name,
                    productDesc         = t.productDesc,
                    productGateway      = t.product.gateway,
                    isProducer          = t.providerID == userId ? true : false,
                }).OrderByDescending(t => t.dateClosed).ToList();

            }

            return View(model);
        }

        // GET: /Account/View
        // View a specific transaction. Probably takes some kind of GUID.
        public ActionResult View(Guid tranId)
        {
            ACCOUNTERROR.ErrorSubject = "Error while trying to retrieve a transaction";
            if(tranId == null)
            {
                throw new ArgumentNullException("Transaction ID must be specified");
            }

            TransactionModel model = new TransactionModel();

            using(var db = new CopiosisEntities())
            {
                // Get transaction data
                
                
                var transaction = db.transactions.Where(t => t.transactionID == tranId).FirstOrDefault();
                
                // Make sure a transaction was found.
                if(transaction == null)
                {
                    throw new ArgumentNullException(string.Format("Transaction with ID does not exist", tranId));
                }

                // Check permissions to view this transaction.
                if ((WebSecurity.CurrentUserId == transaction.providerID) || 
                    (WebSecurity.CurrentUserId == transaction.receiverID) ||
                    (System.Web.Security.Roles.IsUserInRole(ADMINROLE))
                   )
                {
                    // Various transaction data expected to be displayed
                    model.transactionID = transaction.transactionID;
                    model.date          = transaction.date.HasValue ? transaction.date.Value.ToString() : string.Empty;  // Date the transaction took place on.
                    model.dateAdded     = transaction.dateAdded;        // Date transaction added to system. How long pending??                   
                    model.dateClosed    = transaction.dateClosed ??     // Date transaction was Confirmed or Rejected.
                                          DateTime.MinValue;            // Replaces dateAdded when not null.       
                    model.nbr           = transaction.nbr ?? 0.0;       // NBR earned from this transaction
                    model.status        = transaction.status;           // Pending, Confirmed, or Rejected
                    model.satisfaction  = transaction.satisfaction;

                    // Product info expected to be displayed.
                    model.productGuid = transaction.product.guid;
                    model.productName = transaction.product.name; 
                    model.productDesc = transaction.productDesc;
                    model.productGateway = transaction.product.gateway;

                    // Provider info expected to be displayed.
                    model.providerFirstName = transaction.provider.firstName;
                    model.providerLastName  = transaction.provider.lastName;
                    model.providerUsername  = transaction.provider.username;
                    model.providerNotes     = transaction.providerNotes;

                    // Receiver info expected to be displayed.
                    model.receiverFirstName = transaction.receiver.firstName;
                    model.receiverLastName  = transaction.receiver.lastName;
                    model.receiverUsername  = transaction.receiver.username;
                    model.receiverNotes     = transaction.receiverNotes;

                    // For calculatons
                    model.providerID = transaction.providerID;
                    model.receiverID = transaction.receiverID;
                    model.isPendingUser = (transaction.dateClosed == null &&
                                         transaction.createdBy != WebSecurity.CurrentUserId &&
                                         (transaction.providerID == WebSecurity.CurrentUserId ||
                                          transaction.receiverID == WebSecurity.CurrentUserId)
                                        ) ? true : false;

                }
                else
                {
                    throw new ArgumentException("Current user not authorized to view this transaction");
                }
            }
            return View(model);
        }

        
        // POST: /Account/View
        // Confirm or reject a transaction.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult View(string act, TransactionModel model)
        {
            ACCOUNTERROR.ErrorSubject = "Error while trying to add a transaction";
            if (model.transactionID == null)
            {
                throw new ArgumentNullException("Transaction GUID must be specified");
            }

            if (!(model.result == "Confirmed" || model.result == "Rejected"))
            {
                throw new ArgumentNullException("A transaction must be specified as Confirmed or Rejected");
            }

            using (var db = new CopiosisEntities())
            {
                // Get transaction data
                var transaction = db.transactions.Where(t => t.transactionID == model.transactionID).FirstOrDefault();

                // Make sure a transaction was found.
                if(transaction == null)
                {
                    throw new ArgumentNullException(string.Format("Transaction with ID does not exist", model.transactionID));
                }

                /////////////////////////////////////////////////
                // Check permissions to update this transaction.
                /////////////////////////////////////////////////
                bool update = false;

                // User is the provider and the transaction is waiting on their confirmation.
                if (WebSecurity.CurrentUserId == transaction.providerID && transaction.dateClosed == null)
                {
                    // These are the only things being updated. Anything else sent along in the POST (even if it's in the model)
                    // will be ignored.
                    transaction.providerNotes   = model.providerNotes;
                    transaction.dateClosed      = DateTime.Now;
                    transaction.status          = model.result;   
                 
                    // Make sure the DB gets updated below
                    update = true;
                }

                // User is the receiver and the transaction is waiting on their confirmation.
                else if (WebSecurity.CurrentUserId == transaction.receiverID && transaction.dateClosed == null)
                {
                    // Satisfaction must be specified!
                    if (model.satisfaction == null)
                    {
                        this.ModelState.AddModelError("Satisfaction", "Your satisfaction with this transaction must be specified.");
                        return View(model.transactionID);
                    }
                    
                    transaction.receiverNotes   = model.receiverNotes;
                    transaction.satisfaction    = (short)model.satisfaction;
                    transaction.dateClosed      = DateTime.Now;
                    transaction.status          = model.result;

                    // Make sure DB gets updated below.
                    update = true;
                }

                if (update)
                {
                    // Only modify NBRs if the transaction was actually confirmed, and not rejected.
                    if (model.result == "Confirmed")
                    {
                        // Deduct product cost (NBR) from receiver.
                        transaction.receiver.nbr -= transaction.product.gateway;
                        transaction.receiver.nbr += 2;

                        // Credit provider with NBR. Bind the NBR to the transaction for records purposes.
                        float providerReward  = CalculateNBR((int)transaction.satisfaction, transaction.productID, transaction.providerID) + 2;
                        transaction.provider.nbr += providerReward;
                        transaction.nbr = providerReward;
                    }
                    db.SaveChanges();
                }
            }

            return RedirectToAction("View", new { tranId = model.transactionID });
        }
        

        // GET: /Account/Create
        // Create a new transaction whether producer or consumer. Just returns the view.
        public ActionResult Create(string type)
        {
            if(type == null)
            {
                ACCOUNTERROR.ErrorSubject = "Error while trying retrieve a transaction";
                throw new ArgumentNullException("Type of transaction must be specified");
            }

            string typelower = type.ToLower();
            NewTransactionModel model = new NewTransactionModel();
            PopulateNewTransactionModel(typelower, model);
            return View(model);
        }

        // POST: /Account/Create
        // Create a new transaction. Needs to take a model that matchs the form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string type, NewTransactionModel model)
        {
            ACCOUNTERROR.ErrorSubject = "Error while trying create a transaction";
            if (type == null)
            {
                throw new ArgumentNullException("Type of transaction must be specified");
            }

            string typeLower = type.ToLower();
            if(type == "consumer")
            {
                string[] producerName = model.Producer.Split('|');
                string producerUN = producerName != null ? producerName[1].Trim() : "";
                using(var db = new CopiosisEntities())
                {
                    var producer = db.users.Where(u => u.username == producerUN && u.status == 1).FirstOrDefault();
                    if (producer == null)
                    {
                        throw new ArgumentException(string.Format("Producer {0} not found", producerUN));
                    }

                    var product = db.products.Where(p => p.ownerID == producer.userID && p.name == model.ProductProvided && p.deletedDate == null).FirstOrDefault();
                    if(product == null)
                    {
                        throw new ArgumentException("Product not found");
                    }
                    double? currentUserNBR = db.users.Where(u => u.userID == WebSecurity.CurrentUserId).Select(u => u.nbr).FirstOrDefault();
                    if(!currentUserNBR.HasValue || currentUserNBR.Value < product.gateway)
                    {
                        ModelState.AddModelError("InsufficientNBR", "You do not have enough NBR for this good or service");
                        PopulateNewTransactionModel(type, model);
                        return View(model);
                    }

                    transaction consumerTran = new transaction();
                    consumerTran.transactionID = Guid.NewGuid();
                    consumerTran.createdBy = WebSecurity.CurrentUserId;
                    consumerTran.dateAdded = DateTime.Now;
                    consumerTran.providerID = producer.userID;
                    consumerTran.productID = product.productID;
                    consumerTran.productDesc = product.description;
                    consumerTran.receiverID = WebSecurity.CurrentUserId;
                    consumerTran.status = "PENDING";
                    consumerTran.receiverNotes = model.Notes;
                    consumerTran.satisfaction = (short)model.SatisfactionRating;

                    db.transactions.Add(consumerTran);
                    db.SaveChanges();
                }
            }
            else if(type == "producer")
            {
                string[] consumerName = model.Consumer.Split('|');
                string consumerUN = consumerName[1] != null ? consumerName[1].Trim(): "";
                using(var db = new CopiosisEntities())
                {
                    var consumer = db.users.Where(u => u.username == consumerUN && u.status == 1).FirstOrDefault();
                    if(consumer == null)
                    {
                        throw new ArgumentException(string.Format("Consumer {0} not found", consumerUN));
                    }

                    var product = db.products.Where(p => p.ownerID == WebSecurity.CurrentUserId && p.name == model.ProductProvided && p.deletedDate == null).FirstOrDefault();
                    if(product == null)
                    {
                        throw new ArgumentException("Product not found");
                    }

                    double? consumerNBR = db.users.Where(u => u.userID == consumer.userID).Select(u => u.nbr).FirstOrDefault();
                    if (!consumerNBR.HasValue || consumerNBR.Value < product.gateway)
                    {
                        ModelState.AddModelError("InsufficientNBR", "This consumer does not have enough NBR for this good or service");
                        PopulateNewTransactionModel(type, model);
                        return View(model);
                    }

                    transaction producerTran = new transaction();
                    producerTran.transactionID = Guid.NewGuid();
                    producerTran.createdBy = WebSecurity.CurrentUserId;
                    producerTran.dateAdded = DateTime.Now;
                    producerTran.providerID = WebSecurity.CurrentUserId;
                    producerTran.productID = product.productID;
                    producerTran.productDesc = product.description;
                    producerTran.receiverID = consumer.userID;
                    producerTran.status = "PENDING";
                    producerTran.providerNotes = model.Notes;

                    db.transactions.Add(producerTran);
                    db.SaveChanges();
                }
            }
            else
            {
                throw new ArgumentException("Transaction type not recognized");
            }
            return RedirectToAction("Overview");
        }

        // POST: /Account/AddNotes
        // Add notes to a transaction based on the participant adding the notes
        //I made some changes to this in order to accomodate changes to satisfaction. Additionally, I added a line that saved our changes to the DB when we finish. Finally I just return a
        //Json object
        [HttpPost]
        public ActionResult AddNotes(string participant, string notes, Guid tranId, short? newSatisfaction)
        {
            using (var db = new CopiosisEntities())
            {
                int userId = WebSecurity.CurrentUserId;
                var trans = db.transactions.Where(a => a.transactionID == tranId).FirstOrDefault();
                if(participant == null)
                {
                    return Json(new { success = false });
                }
                
                if(participant.Equals("producer", StringComparison.OrdinalIgnoreCase))
                {
                    if(trans.providerID == userId)
                    {
                        trans.providerNotes = notes;
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                }
                else if (participant.Equals("consumer", StringComparison.OrdinalIgnoreCase))
                {
                    if (trans.receiverID == userId)
                    {
                        if (newSatisfaction != null)
                        {
                            trans.satisfaction = newSatisfaction;
                        }
                        trans.receiverNotes = notes;
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                }
                else
                {
                    return Json(new { success = false });
                }
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        // GET: /Account/Items
        // This will serve as the Item Library to show all the items a user has. Probably takes some kind of GUID.
        [HttpGet]
        public ActionResult Items()
        {
            List<ItemsModel> model = new List<ItemsModel>();
            model = CurrenUserItems();
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
                    ACCOUNTERROR.ErrorSubject = "Error while trying to add an item";
                    throw new ArgumentException("Product item class not found");
                }

                p.name = model.Name;
                p.ownerID = WebSecurity.CurrentUserId;
                p.guid = Guid.NewGuid();
                p.gateway = model.Gateway;
                p.description = model.Description;
                p.createdDate = DateTime.Now;
                p.itemClass = (int)itemClassId;
                p.type = model.ItemType;

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
        public ActionResult FetchProducerItems(string name, int idx, string username)
        {
            List<string> products = new List<string>();
            bool result = true;

            /* Note: this is not a great way to be doing this, relying soley on the first name last name as these may not be unique. But the only solution
             * seems to be to include the username which has been stated that they do not want */
            string[] producerName = name.Split(' ');
            string producerFirstName = producerName[0];
            string producerLastName = producerName[1];
            string currentUserName = username;
            using (var db = new CopiosisEntities())
            {
                int? producerID = db.users.Where(u => u.username == currentUserName).Select(uID => uID.userID).FirstOrDefault();
                if(producerID == null)
                {
                    ACCOUNTERROR.ErrorSubject = "Error while trying to retrieve item(s)";
                    throw new ArgumentNullException(string.Format("No user found with name {0}", name));
                }
                
                products = db.products.Where(po => po.ownerID == producerID && po.deletedDate == null).Select(p => p.name).Distinct().ToList();
                if (products == null)
                {
                    result = true;
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
                    ACCOUNTERROR.ErrorSubject = "Error while trying to edit an item";
                    throw new ArgumentException(string.Format("Product with ID {0} not found", itemId));
                }
                else
                {
                    model.Name = item.name;
                    model.ItemClass = item.itemClass1.name;
                    model.Description = item.description;
                    model.Gateway = item.gateway;
                    model.ItemClassTemplates = FetchItemClassTemplates(db);
                    model.ItemType = item.type;
                }
            }

            return View(model);
        }

        // POST: /Account/EditItem
        // Update an existing item in the database. Takes a model of the new item.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditItem(AddItemModel model, Guid itemId)
        {
            ValidateItemModel(model);
            using (var db = new CopiosisEntities())
            {
                var item = db.products.Where(p => p.guid == itemId && p.ownerID == WebSecurity.CurrentUserId).FirstOrDefault();
                int itemClassId = db.itemClasses.Where(ic => ic.name == model.ItemClass).Select(i => i.classID).First();
                if (item == null)
                {
                    ACCOUNTERROR.ErrorSubject = "Error while trying to edit an item";
                    throw new ArgumentException(string.Format("Product with ID {0} not found", itemId));
                }
                else
                {
                    item.name = model.Name;
                    item.description = model.Description;
                    item.gateway = model.Gateway;
                    item.itemClass = itemClassId;
                    item.type = model.ItemType;
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
                message == ManageMessageId.AccountChangesSaved ? "Your account changes were saved"
                : message == ManageMessageId.PasswordRequired ? "Your password is required"
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (message == ManageMessageId.ChangePasswordSuccess)
            {
                ViewBag.changesSaved = true;
            }
            else
            {
                ViewBag.changesSaved = false;
            }
            try
            {
                bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(WebSecurity.CurrentUserName));
                if (hasLocalAccount == false)
                {
                    return RedirectToAction("Register");
                }
                using (var db = new CopiosisEntities())
                {
                    var dbCurrentUser = db.users.Where(p => p.userID == WebSecurity.CurrentUserId).FirstOrDefault();
                    if (dbCurrentUser == null)
                    {
                        ACCOUNTERROR.ErrorSubject = "Error while trying to retrieve your user account";
                        throw new Exception(string.Format("No match for the current user with user name {0}", WebSecurity.CurrentUserId));
                    }
                    AccountManagerModel model = new AccountManagerModel();
                    model.errorList = new Dictionary<string, string>();
                    user CurrentUser = db.users.Where(p => p.userID == WebSecurity.CurrentUserId).FirstOrDefault();
                    model.currentEmail = CurrentUser.email;
                    model.currentFirstName = CurrentUser.firstName;
                    model.currentLastName = CurrentUser.lastName;
                    ViewBag.isValidatedUser = true;
                    return View(model);
                }
            }
            catch (Exception e)
            {
                ACCOUNTERROR.ErrorSubject = "Error when trying to access your account";
                if (e.InnerException is InvalidOperationException)
                {
                    throw new Exception("You do not have an account. Please register with Copiosis.");
                }
                throw new Exception(e.Message);
            }
            //Not authorized to view to this page. Redirect to register a new account
            return RedirectToAction("Register");
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(AccountManagerModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            //Dictionary<string, ModelState> errors = dict.ToDictionary<string, ModelState>(p => p.Value);
            if (ModelState.IsValid && hasLocalAccount)
            {
                using (var db = new CopiosisEntities())
                {
                    var dbCurrentUser = db.users.Where(p => p.userID == WebSecurity.CurrentUserId).FirstOrDefault();
                    if (dbCurrentUser == null)
                    {
                        ACCOUNTERROR.ErrorSubject = "Error while trying to retrieve your user account";
                        throw new Exception(string.Format("No match for the current user with user name {0}", WebSecurity.CurrentUserId));
                    }
                    ViewBag.isValidatedUser = true;
                    string passwordTemp;
                    bool changePassword;
                    bool noPwProvided;
                    validateManageAccountForm(model, db, dbCurrentUser, out passwordTemp, out changePassword, out noPwProvided);

                    if (ModelState.IsValid == true)
                    {
                        if (changePassword == true)
                        {
                            // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                            bool changePasswordSucceeded = true;
                            try
                            {
                                changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, passwordTemp, model.newPassword);
                            }
                            catch (Exception)
                            {
                                changePasswordSucceeded = false;
                            }

                            if (changePasswordSucceeded == false)
                            {
                                ACCOUNTERROR.ErrorSubject = "Error while trying to update your account";
                                throw new Exception("Could not change your password");
                            }
                            else
                            {
                                try
                                {
                                    WebSecurity.Login(dbCurrentUser.username, passwordTemp);
                                    passwordTemp = model.newPassword;
                                }
                                catch (Exception e)
                                {
                                    ACCOUNTERROR.ErrorSubject = "Error when logging you in";
                                    throw new Exception(e.Message);
                                }
                            }
                        }
                        db.SaveChanges();
                        ViewBag.changesSaved = true;
                        return RedirectToAction("Manage", new { Message = ManageMessageId.AccountChangesSaved });
                    }
                    else
                    {
                        //there was at least one error:
                        ViewBag.changesSaved = false;
                        return View(model);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.changesSaved = false;
            return View(model);
        }


        //
        // GET: /Account/UsersNBR
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
            ACCOUNTERROR.ErrorSubject = "Error while validating an item";
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

            if(!model.ItemType.Equals("Product", StringComparison.OrdinalIgnoreCase) && !model.ItemType.Equals("Service", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Items can only be of type Product or Service");
            }
        }

        /// <summary>
        /// Get the items for the currently logged in user
        /// </summary>
        /// <returns>List of Items</returns>
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
                    item.ItemType = value.type;
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
            using (var db = new CopiosisEntities())
            {
                var product = db.products.Where(a => a.productID == productId && a.ownerID == providerId).FirstOrDefault();
                ACCOUNTERROR.ErrorSubject = "Error while calculating the NBR";
                if(product == null)
                {
                    throw new ArgumentException("Product not found for this provider");
                }

                var item = db.itemClasses.Where(a => a.classID == product.itemClass).FirstOrDefault();
                if(item == null)
                {
                   
                    throw new ArgumentException("Item class not found for this product");
                }

                float Cpdb = (float)item.cPdb;
                float Ccb = (float)item.cCb;
                float Ceb = (float)item.cEb;

                int D = (int)item.d;
                int P0 = (int)item.pO;

                float A = (float)item.a;
                int Aprime = (int)item.aPrime;
                int Amax = (int)item.aMax;

                float M1 = (float)item.m1;
                float M2 = (float)item.m2;
                float M3 = (float)item.m3;
                float M4 = (float)item.m4;
                float M5 = (float)item.m5;

                int S = (int)item.s;
                int Se = (int)item.sE;
                int Sh = (int)item.sH;

                float nbr = Cpdb * (D / Aprime - A / Amax) + (Ccb * (satisfactionRating / M1 + P0 / M2) + Ceb * (S / M3 + Se / M4 + Sh / M5));
                return nbr;
            }
        }
        
        /// <summary>
        /// Generate the initial list of producer items for NewTransaction
        /// </summary>
        /// <param name="currentID">ID of the first producer</param>
        /// <returns>List of Items</returns>
        private List<ItemsModel> FetchInitialProducerItems(int currentID)
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
        //Helper method to validate the Manage Account form for the Account/Manage view
        private void validateManageAccountForm(AccountManagerModel model, CopiosisEntities db, user dbCurrentUser, out string passwordTemp, out bool changePassword, out bool noPwProvided)
        {
            string email = model.emailAddress;
            string firstName = model.firstName;
            string lastName = model.lastName;
            string newPassword = model.newPassword;
            string confirmPassword = model.confirmPassword;
            string currentPassword = model.currentPassword ?? "";
            passwordTemp = new string(currentPassword.ToCharArray());
            changePassword = false;
            noPwProvided = false;
            model.currentEmail = dbCurrentUser.email;
            model.currentFirstName = dbCurrentUser.firstName;
            model.currentLastName = dbCurrentUser.lastName;
            user conflictUser = null;
            if (email != null)
            {
                conflictUser = db.users.Where(m => m.email == email).FirstOrDefault();
                if (conflictUser != null && conflictUser.email.Equals(email))
                {
                    ModelState.AddModelError("emailAddress", "That e-mail address is already being used. Please use a different one");
                }
                else
                {
                    dbCurrentUser.email = email;
                }
            }
            if (firstName != null)
            {
                if (firstName.Equals(dbCurrentUser.firstName))
                {
                    ModelState.AddModelError("firstName", "Enter a different first name");
                }
                else
                {
                    dbCurrentUser.firstName = firstName;
                }
            }
            if (lastName != null)
            {
                if (lastName.Equals(dbCurrentUser.lastName))
                {
                    ModelState.AddModelError("lastName", "Enter a different last name");
                }
                else
                {
                    dbCurrentUser.lastName = lastName;
                }
            }
            if (newPassword != null)
            {
                if (confirmPassword == null)
                {
                    ModelState.AddModelError("confirmPassword", "Confirmation password cannot be empty");
                }
                else if (!newPassword.Equals(confirmPassword))
                {
                    ModelState.AddModelError("confirmPassword", "Confirmation password and new password do not match");
                }
                else if (newPassword.Equals(model.currentPassword))
                {
                    ModelState.AddModelError("newPassword", "Your new password cannot be the same as your current password");
                }
                else
                {
                    changePassword = true;
                }
            }
            if (model.currentPassword == null)
            {
                ModelState.AddModelError("currentPassword", "Please enter your current password to commit to the change(s)");
                noPwProvided = false;
            }
            else if ((Membership.Provider.ValidateUser(db.users.Where(m => m.userID == WebSecurity.CurrentUserId).FirstOrDefault().username, model.currentPassword) == false))
            {
                ModelState.AddModelError("currentPassword", "You entered the wrong current password");
            }
            //build the error list
            if (model.errorList == null)
            {
                model.errorList = new Dictionary<string, string>();
            }
            if (ModelState.IsValid == false)
            {
                int i = 0;
                foreach (ModelState state in ModelState.Values)
                {
                    if (state.Errors.Count >= 1)
                    {
                        model.errorList.Add(ModelState.Keys.ElementAt(i), state.Errors[0].ErrorMessage);
                    }
                    ++i;
                }
            }
        }

        private void PopulateNewTransactionModel(string type, NewTransactionModel model)
        {
            if (type == "consumer")
            {
                model.IsProducer = false;
                List<string> producers = new List<string>();
                List<string> products = new List<string>();
                List<string> usernames = new List<string>();

                using (var db = new CopiosisEntities())
                {
                    var usersWithProducts = db.products.Where(p => p.ownerID != WebSecurity.CurrentUserId && p.user.status == 1 && p.deletedDate == null).Select(u => u.user).Distinct().ToList();

                    if (usersWithProducts.Count > 0)
                    {
                        foreach (var pro in usersWithProducts)
                        {
                            producers.Add(string.Format("{0} {1} | {2}", pro.firstName, pro.lastName, pro.username));
                            usernames.Add(pro.username);
                        }

                        var initialProducer = usersWithProducts.First();
                        var initialItemList = FetchInitialProducerItems(initialProducer.userID);
                        foreach (var item in initialItemList)
                        {
                            products.Add(item.ProductName);
                        }
                    }
                }
                model.Usernames = usernames;
                model.Products = products;
                model.Producers = producers;

            }
            else if (type == "producer")
            {
                model.IsProducer = true;

                var producerItems = CurrenUserItems();
                List<string> products = new List<string>();
                foreach (var item in producerItems)
                {
                    products.Add(item.ProductName);
                }
                model.Products = products;
                List<string> usernames = new List<string>();
                List<string> consumers = new List<string>();
                using (var db = new CopiosisEntities())
                {
                    var c = db.users.Where(u => u.status == 1 && u.userID != WebSecurity.CurrentUserId)
                        .Select(s => new { FirstName = s.firstName, LastName = s.lastName, Username = s.username, Email = s.email, NBR = s.nbr}).ToList();
                    foreach (var con in c)
                    {
                        consumers.Add(string.Format("{0} {1} (NBR: {2}) | {3}", con.FirstName, con.LastName, con.NBR, con.Username));
                        usernames.Add(string.Format("{0}", con.Username));
                    }
                }
                model.Usernames = usernames;
                model.Consumers = consumers;
            }
            else
            {
                ACCOUNTERROR.ErrorSubject = "Error while trying to retrieve a transaction";
                throw new ArgumentException("Transaction type not recognized");
            }

            return;
        }

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
            AccountChangesSaved,
            PasswordRequired,
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
