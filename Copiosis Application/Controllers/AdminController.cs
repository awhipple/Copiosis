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
    public class AdminController : Controller
    {
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
            return View();
        }

        //
        // GET: /Admin/AddClass
        // Add a new item class to Copiosis. Just returns the view.
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AddClass()
        {
            return View();
        }

        //
        // POST: /Admin/AddClass
        // Add a new item class to Copiosis.
        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddClass(AddClassModel m)
        {

            itemClass itemClass = new itemClass();
            using (var db = new CopiosisEntities())
            {
                var isEmpty = db.itemClasses.Where(ic => ic.name == m.name).FirstOrDefault();
                if (isEmpty == null)
                {
                    itemClass.name = m.name;
                    itemClass.suggestedGateway = m.suggestedGateway;
                    itemClass.cPdb = m.cPdb;
                    itemClass.a = m.a;
                    itemClass.aMax = m.aMax;
                    itemClass.d = m.d;
                    itemClass.aPrime = m.aPrime;
                    itemClass.cCb = m.cCb;
                    itemClass.m1 = m.m1;
                    itemClass.pO = m.p0;
                    itemClass.m2 = m.m2;
                    itemClass.cEb = m.cEb;
                    itemClass.s = m.s;
                    itemClass.m3 = m.m3;
                    itemClass.sE = m.sE;
                    itemClass.m4 = m.m4;
                    itemClass.sH = m.sH;
                    itemClass.m5 = m.m5;

                    db.itemClasses.Add(itemClass);
                    db.SaveChanges();
                }
                else
                {
                    m.message = "Name exists";
                }
            }
            return RedirectToAction("Overview");
        }


        // GET: /Admin/EditClass
        // Edit an class. Takes the string representing class name.
        [HttpGet]
        [AllowAnonymous]
        public ActionResult EditClass(string className)
        {
            AddClassModel model = new AddClassModel();

            using (var db = new CopiosisEntities())
            {
                var iClass = db.itemClasses.Where(p => p.name == className).FirstOrDefault();
                if (iClass == null)
                {
                    //ACCOUNTERROR.ErrorSubject = "Error while trying to edit an item";
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

        // GET: /Admin/EditClass
        // Edit an class. Takes the string representing class name.
        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditClass(AddClassModel model, string className)
        {
            //ValidateItemModel(model);
            using (var db = new CopiosisEntities())
            {
                var iClass = db.itemClasses.Where(p => p.name == className).FirstOrDefault();
                if (iClass == null)
                {
                    //ACCOUNTERROR.ErrorSubject = "Error while trying to edit an item";
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
                }
            }
            return RedirectToAction("Overview");
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

    }
}
