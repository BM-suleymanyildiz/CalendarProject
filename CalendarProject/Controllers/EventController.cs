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

        // GET: Event
        public ActionResult Index()
        {
            var events = db.Events.Include("Category").ToList();
            return View(events);
        }

        // GET: Event/Create
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
                // Hata durumunda boş liste döndür
                ViewBag.Categories = new SelectList(new List<Category>(), "Id", "Name");
                return View();
            }
        }

        // POST: Event/Create
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

        // GET: Event/Edit/5
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

        // POST: Event/Edit/5
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

        // GET: Event/Delete/5
        public ActionResult Delete(int id)
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

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Event eventItem = db.Events.Find(id);
            db.Events.Remove(eventItem);
            db.SaveChanges();
            return RedirectToAction("Index");
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