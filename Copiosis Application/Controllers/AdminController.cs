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
                    (a.deletedDate == null || a.deletedDate == DateTime.MinValue) && a.itemClass != 1
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
                    (a.deletedDate == null || a.deletedDate == DateTime.MinValue) && a.itemClass == 1
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
        // GET: /Admin/ViewClasses
        // View a list of the classes. Clicking one opens EditClass.
        [HttpGet]
        public ActionResult ViewClasses(string message = null)
        {
            ViewClassesModel model = new ViewClassesModel();
            if (message != null)
            {
                ViewBag.savedChanges = true;
                ViewBag.className = message;
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
                        newItemClass.pO = m.p0;
                        newItemClass.m2 = m.m2;
                        newItemClass.cEb = m.cEb;
                        newItemClass.s = m.s;
                        newItemClass.m3 = m.m3;
                        newItemClass.sE = m.sE;
                        newItemClass.m4 = m.m4;
                        newItemClass.sH = m.sH;
                        newItemClass.m5 = m.m5;

                        db.itemClasses.Add(newItemClass);
                        db.SaveChanges();
                        return RedirectToAction("ViewClasses", new { message = m.name });
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
                var iClass = db.itemClasses.Where(p => p.name == className).FirstOrDefault();
                if (iClass == null)
                {
                    ADMINERROR.ErrorSubject = "Error while trying to edit an item";
                    throw new ArgumentException(string.Format("ItemClass with Name {0} not found", className));
                }
                else
                {
                    model.name = iClass.name;
                    model.suggestedGateway = (int)iClass.suggestedGateway;
                    model.cPdb = (float)iClass.cPdb;
                    model.a = (float)iClass.a;
                    model.aMax = (int)iClass.aMax;
                    model.d = (int)iClass.d;
                    model.aPrime = (int)iClass.aPrime;
                    model.cCb = (float)iClass.cCb;
                    model.m1 = (float)iClass.m1;
                    model.p0 = (int)iClass.pO;
                    model.m2 = (float)iClass.m2;
                    model.cEb = (float)iClass.cEb;
                    model.s = (int)iClass.s;
                    model.m3 = (float)iClass.m3;
                    model.sE = (short)iClass.sE;
                    model.m4 = (float)iClass.m4;
                    model.sH = (short)iClass.sH;
                    model.m5 = (float)iClass.m5;
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
                    var iClass = db.itemClasses.Where(p => p.name == className).FirstOrDefault();
                    if (iClass == null)
                    {
                        ADMINERROR.ErrorSubject = "Error while trying to edit an item";
                        throw new ArgumentException(string.Format("ItemClass with Name {0} not found", className));
                    }
                    else
                    {
                        iClass.name = model.name;
                        iClass.suggestedGateway = model.suggestedGateway;
                        iClass.cPdb = model.cPdb;
                        iClass.a = model.a;
                        iClass.aMax = model.aMax;
                        iClass.d = model.d;
                        iClass.aPrime = model.aPrime;
                        iClass.cCb = model.cCb;
                        iClass.m1 = model.m1;
                        iClass.pO = model.p0;
                        iClass.m2 = model.m2;
                        iClass.cEb = model.cEb;
                        iClass.s = model.s;
                        iClass.m3 = model.m3;
                        iClass.sE = model.sE;
                        iClass.m4 = model.m4;
                        iClass.sH = model.sH;
                        iClass.m5 = model.m5;
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("ViewClasses");
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
