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
    [Authorize(Roles="ADMIN")]
    [CustomErrorHandling]
    public class AdminController : Controller
    {
        private Models.ErrorModel ADMINERROR = new Models.ErrorModel();
        //
        // GET: /Admin/
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Overview");
        }

        //
        // GET: /Admin/Overview
        // See which items in the system are still using the default item class.
        [HttpGet]
        public ActionResult Overview()
        {
            ClassOverviewModel model = new ClassOverviewModel();

            using (var db = new CopiosisEntities())
            {
                int userId = WebSecurity.CurrentUserId;
                model.ItemClassTemplates = FetchItemClassTemplates(db);

                model.products = db.products.Where(
                    a =>
                    (a.deletedDate == null || a.deletedDate == DateTime.MinValue) && a.itemClass1.name != "Default"
                ).Select(t => new ClassModel
                {
                    classID = t.itemClass,
                    className = t.itemClass1.name,
                    productName = t.name,
                    productDesc = t.description,
                    productOwner = (t.user.firstName + " " + t.user.lastName),
                    productGuid = t.guid
                }).OrderByDescending(t => t.productName).ToList();

                model.productsDefault = db.products.Where(
                    a =>
                    (a.deletedDate == null || a.deletedDate == DateTime.MinValue) && a.itemClass1.name == "Default"
                ).Select(t => new ClassModel
                {
                    classID = t.itemClass,
                    className = t.itemClass1.name,
                    productName = t.name,
                    productDesc = t.description,
                    productOwner = (t.user.firstName + " " + t.user.lastName),
                    productGuid = t.guid
                }).OrderByDescending(t => t.productName).ToList();

            }


            return View(model);
        }



        //
        // GET: /Admin/Users
        // View a list of the users in Copiosis. Each user will have a ListBox which controls whether
        // they are active or not, and what their rank is (Admin or not).
        // Changes to the ListBoxes will call backend actions via Ajax.
        [HttpGet]
        public ActionResult ViewUsers()
        {
            ViewUsersModel model = new ViewUsersModel();

            using (var db = new CopiosisEntities())
            {

                model.users = db.users.Select(t => new UserModel
                {
                    userId = t.userID,
                    userName = t.username,
                    firstName = t.firstName,
                    lastName = t.lastName

                }).OrderByDescending(t => t.userName).ToList();

            }
            return View(model);
        }


        //
        // POST: /Admin/ChangeUserIsAdmin
        // Change whether a user is an Admin in Copiosis.
        [HttpPost]
        public ActionResult ChangeUserIsAdmin(string role, int userId)
        {
            using (var db = new CopiosisEntities())
            {
                var user = db.users.Where(p => p.userID == userId).FirstOrDefault();

                ADMINERROR.ErrorSubject = "Error while trying to change an item's class";
                if (user == null)
                {
                    throw new ArgumentException(string.Format("No user found with that ID: {0}", userId));
                }

                // user.itemClass = classID;
                db.SaveChanges();
            }
            return Json(new { success = true });
        }


        //
        // GET: /Admin/ViewClasses
        // View a list of the classes. Clicking one opens EditClass.
        [HttpGet]
        public ActionResult ViewClasses()
        {
            ViewBag.savedChanges = false;
            ViewBag.noEdit = false;
            ViewClassesModel model = new ViewClassesModel();
            //Handle cases for success banner in the Admin/ViewClasses view
            if (TempData["AddClass"] != null || TempData["EditClass"] != null)
            {
                //Case 1: The admin adds a new class and it is successful
                if (TempData["AddClass"] != null)
                {
                    ViewBag.newClass = true;
                    ViewBag.className = TempData["AddClass"];
                }
                //Case 2: The admin edits a class and it is successful
                else if (TempData["EditClass"] != null)
                {
                    ViewBag.newClass = false;
                    ViewBag.className = TempData["EditClass"];
                }
                ViewBag.savedChanges = true;
            }
            //Case 3: The admin presses the submit button from the EditClass page but changes nothing
            else if (TempData["NoEdit"] != null)
            {
                ViewBag.noEdit = true;
                ViewBag.className = TempData["NoEdit"];
            }
            using (var db = new CopiosisEntities())
            {

                model.ItemClassTemplates = db.itemClasses.Select(t => new ViewClassModel
                {
                    classID = t.classID,
                    className = t.name,
                    numUsing = db.products.Where(p => p.itemClass == t.classID).Count()
                
                }).OrderByDescending(t => t.className).ToList();

            }
            return View(model);
        }

        //
        // GET: /Admin/AddClass
        // Add a new item class to Copiosis. Just returns the view.
        [HttpGet]
        public ActionResult AddClass()
        {
            AddClassModel model = new AddClassModel();
             return View(model);
        }

        //
        // POST: /Admin/AddClass
        // Add a new item class to Copiosis.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddClass(AddClassModel m)
        {
            if (ModelState.IsValid)
            {
                itemClass newItemClass = new itemClass();
                using (var db = new CopiosisEntities())
                {
                    itemClass conflictingItemClass = db.itemClasses.Where(ic => ic.name == m.name).FirstOrDefault();
                    if (conflictingItemClass != null)
                    {
                        ModelState.AddModelError("name", "There is already a class of this name");
                        return View(m);
                    }
                    else
                    {
                        newItemClass.name = m.name;
                        newItemClass.suggestedGateway = m.suggestedGateway;
                        newItemClass.cPdb = m.cPdb;
                        newItemClass.a = m.a;
                        newItemClass.aMax = m.aMax;
                        newItemClass.d = m.d;
                        newItemClass.aPrime = m.aPrime;
                        newItemClass.cCb = m.cCb;
                        newItemClass.m1 = m.m1;
                        newItemClass.pO = m.pO;
                        newItemClass.m2 = m.m2;
                        newItemClass.cEb = m.cEb;
                        newItemClass.s = m.s;
                        newItemClass.m3 = m.m3;
                        newItemClass.sE = m.sE;
                        newItemClass.m4 = m.m4;
                        newItemClass.sH = m.sH;
                        newItemClass.m5 = m.m5;
                        //save changes
                        db.itemClasses.Add(newItemClass);
                        db.SaveChanges();
                        TempData["AddClass"] = newItemClass.name;
                        return RedirectToAction("ViewClasses");
                    }
                }
            }
            else
            {
                return View(m);
            }
        }


        // GET: /Admin/EditClass
        // Edit an class. Takes the string representing class name.
        [HttpGet]
        public ActionResult EditClass(string className)
        {
            AddClassModel model = new AddClassModel();

            using (var db = new CopiosisEntities())
            {
                var currentItemClass = db.itemClasses.Where(p => p.name == className).FirstOrDefault();
                if (currentItemClass == null)
                {
                    ADMINERROR.ErrorSubject = "Error while trying to edit an item";
                    throw new ArgumentException(string.Format("ItemClass with Name {0} not found", className));
                }
                else
                {
                    model.name = currentItemClass.name;
                    model.suggestedGateway = (int)currentItemClass.suggestedGateway;
                    model.cPdb = (float)currentItemClass.cPdb;
                    model.a = (float)currentItemClass.a;
                    model.aMax = (int)currentItemClass.aMax;
                    model.d = (int)currentItemClass.d;
                    model.aPrime = (int)currentItemClass.aPrime;
                    model.cCb = (float)currentItemClass.cCb;
                    model.m1 = (float)currentItemClass.m1;
                    model.pO = (int)currentItemClass.pO;
                    model.m2 = (float)currentItemClass.m2;
                    model.cEb = (float)currentItemClass.cEb;
                    model.s = (int)currentItemClass.s;
                    model.m3 = (float)currentItemClass.m3;
                    model.sE = (short)currentItemClass.sE;
                    model.m4 = (float)currentItemClass.m4;
                    model.sH = (short)currentItemClass.sH;
                    model.m5 = (float)currentItemClass.m5;
                }
            }
            return View(model);
        }

        // POST: /Admin/EditClass
        // Edit an class. Takes the string representing class name.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditClass(AddClassModel model, string className)
        {
            if (ModelState.IsValid)
            {
                using (var db = new CopiosisEntities())
                {
                    var currentItemClass = db.itemClasses.Where(p => p.name == className).FirstOrDefault();
                    if (currentItemClass == null)
                    {
                        ADMINERROR.ErrorSubject = "Error while trying to edit an item";
                        throw new ArgumentException(string.Format("ItemClass with Name {0} not found", className));
                    }
                    else
                    {
                        if (model.name.Equals(currentItemClass.name) == false)
                        {
                            itemClass conflictingItemClass = db.itemClasses.Where(ic => ic.name == model.name).FirstOrDefault();
                            if (conflictingItemClass != null)
                            {
                                ModelState.AddModelError("name", "There is already a class of this name");
                                return View(model);
                            }
                        }
                        //Case when the are no changes to the current class
                        else if (model.Equals(currentItemClass) == true)
                        {
                            TempData["NoEdit"] = currentItemClass.name;
                            return RedirectToAction("ViewClasses");
                        }
                        currentItemClass.name = model.name;
                        currentItemClass.suggestedGateway = model.suggestedGateway;
                        currentItemClass.cPdb = model.cPdb;
                        currentItemClass.a = model.a;
                        currentItemClass.aMax = model.aMax;
                        currentItemClass.d = model.d;
                        currentItemClass.aPrime = model.aPrime;
                        currentItemClass.cCb = model.cCb;
                        currentItemClass.m1 = model.m1;
                        currentItemClass.pO = model.pO;
                        currentItemClass.m2 = model.m2;
                        currentItemClass.cEb = model.cEb;
                        currentItemClass.s = model.s;
                        currentItemClass.m3 = model.m3;
                        currentItemClass.sE = model.sE;
                        currentItemClass.m4 = model.m4;
                        currentItemClass.sH = model.sH;
                        currentItemClass.m5 = model.m5;
                        db.SaveChanges();
                        TempData["EditClass"] = currentItemClass.name;
                        return RedirectToAction("ViewClasses");
                        
                    }
                }
            }
            else
            {
                return View(model);
            }
        }

        //
        // POST: /Admin/ChangeClass
        // Change the class of an item already in Copiosis.
        [HttpPost]
        public ActionResult ChangeClass(string newClass, Guid itemGuid)
        {
            using (var db = new CopiosisEntities())
            {
                var item = db.products.Where(p => p.guid == itemGuid).FirstOrDefault();
                var classID = db.itemClasses.Where(ic => ic.name == newClass).Select(ic => ic.classID).FirstOrDefault();
                ADMINERROR.ErrorSubject = "Error while trying to change an item's class";
                if (item == null)
                {
                    throw new ArgumentException(string.Format("No product found with GUID: {0}", itemGuid));
                }
                if (classID == null)
                {
                    throw new ArgumentException(string.Format("No matching item class with name: {0}", itemGuid));
                }

                item.itemClass = classID;
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        //
        // GET: /Admin/Rejected
        // View all transactions that have been rejected in Copiosis.
        [HttpGet]
        public ActionResult Rejected()
        {
            RejectedModel model = new RejectedModel();

            using (var db = new CopiosisEntities())
            {

                model.rejected = db.transactions.Where(a => (a.status == "Rejected")).Select(t => new RejectedTransactionModel
                {
                    transactionID = t.transactionID,
                    dateRejected = t.dateClosed ?? DateTime.MinValue,
                    producer = db.users.Where(u => u.userID == t.providerID).Select(u => u.username).FirstOrDefault(),
                    consumer = db.users.Where(u => u.userID == t.receiverID).Select(u => u.username).FirstOrDefault(),
                    name = t.product.name,
                    gateway = t.product.gateway
                }).OrderByDescending(t => t.dateRejected).ToList();

            }
            return View(model);
        }

        #region Helpers
        private List<SelectListItem> FetchItemClassTemplates(CopiosisEntities db)
        {
            List<SelectListItem> itemClasses = new List<SelectListItem>();
            var items = db.itemClasses.ToList();
            if (items != null)
            {
                foreach (var item in items)
                {
                    itemClasses.Add(
                        new SelectListItem { Text = item.name, Value = item.name }
                    );
                }
            }
            return itemClasses;
        }

        public Models.ErrorModel getError()
        {
            return this.ADMINERROR;
        }
        #endregion
    }
}
