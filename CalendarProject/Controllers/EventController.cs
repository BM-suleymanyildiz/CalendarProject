using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CalendarProject.Context;
using CalendarProject.Entities;

namespace CalendarProject.Controllers
{
    public class EventController : Controller
    {
        private ProjectContext db = new ProjectContext();

        public ActionResult Index()
        {
            var events = db.Events.Include("Category").ToList();
            return View(events);
        }

        public ActionResult Create()
        {
            try
            {
                var categories = db.Categories.ToList();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Categories = new SelectList(new List<Category>(), "Id", "Name");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,StartDate,EndDate,Description,CategoryId,IsAllDay")] Event eventItem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Events.Add(eventItem);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                var categories = db.Categories.ToList();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", eventItem.CategoryId);
                return View(eventItem);
            }
            catch (Exception ex)
            {
                var categories = db.Categories.ToList();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", eventItem.CategoryId);
                return View(eventItem);
            }
        }

        public ActionResult Edit(int id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
                }
                Event eventItem = db.Events.Find(id);
                if (eventItem == null)
                {
                    return HttpNotFound();
                }
                var categories = db.Categories.ToList();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", eventItem.CategoryId);
                return View(eventItem);
            }
            catch (Exception ex)
            {
                var categories = db.Categories.ToList();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,StartDate,EndDate,Description,CategoryId,IsAllDay")] Event eventItem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(eventItem).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                var categories = db.Categories.ToList();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", eventItem.CategoryId);
                return View(eventItem);
            }
            catch (Exception ex)
            {
                var categories = db.Categories.ToList();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", eventItem.CategoryId);
                return View(eventItem);
            }
        }

        public ActionResult Delete(int id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
                }
                Event eventItem = db.Events.Find(id);
                if (eventItem == null)
                {
                    return HttpNotFound();
                }
                return View(eventItem);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Event eventItem = db.Events.Find(id);
                db.Events.Remove(eventItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}