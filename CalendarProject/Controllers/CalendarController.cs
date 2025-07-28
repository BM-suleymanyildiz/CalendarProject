using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CalendarProject.Context;
using CalendarProject.Entities;

namespace CalendarProject.Controllers
{
    public class CalendarController : Controller
    {
        private ProjectContext db = new ProjectContext();

        // GET: Calendar
        public ActionResult Index()
        {
            var events = db.Events.ToList();
            var categories = db.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.CategoryColors = categories.ToDictionary(c => c.Id.ToString(), c => c.Color);
            return View(events);
        }

        // GET: Calendar/Test
        public ActionResult Test()
        {
            return View();
        }

        // GET: Calendar/GetEvents
        [HttpGet]
        public JsonResult GetEvents()
        {
            try
            {
                // Debug: Veritabanındaki etkinlik sayısını kontrol et
                var totalEvents = db.Events.Count();
                System.Diagnostics.Debug.WriteLine($"Toplam etkinlik sayısı: {totalEvents}");
                
                // Sadece geçerli tarihi olan etkinlikleri getir (null olmayan)
                var events = db.Events.Include("Category")
                    .Where(e => e.StartDate.HasValue && e.EndDate.HasValue)
                    .ToList() // Önce veritabanından çek
                    .Select(e => new
                    {
                        id = e.Id,
                        title = e.Title,
                        start = e.StartDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                        end = e.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
                        backgroundColor = e.Category != null ? e.Category.Color : "#3c8dbc",
                        borderColor = e.Category != null ? e.Category.Color : "#3c8dbc",
                        allDay = e.IsAllDay,
                        description = e.Description,
                        categoryName = e.Category != null ? e.Category.Name : ""
                    }).ToList();

                System.Diagnostics.Debug.WriteLine($"Döndürülen etkinlik sayısı: {events.Count}");
                
                // Her etkinliği debug et
                foreach (var evt in events)
                {
                    System.Diagnostics.Debug.WriteLine($"Etkinlik: {evt.title}, Başlangıç: {evt.start}, Bitiş: {evt.end}, Renk: {evt.backgroundColor}");
                }
                
                return Json(events, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetEvents hatası: {ex.Message}");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Calendar/GetExternalEvents - Tarihi olmayan etkinlikleri getir
        [HttpGet]
        public JsonResult GetExternalEvents()
        {
            try
            {
                // Sadece tarihi olmayan etkinlikleri getir (null olan)
                var events = db.Events.Include("Category")
                    .Where(e => !e.StartDate.HasValue && !e.EndDate.HasValue)
                    .Select(e => new
                    {
                        id = e.Id,
                        title = e.Title,
                        description = e.Description,
                        categoryId = e.CategoryId,
                        categoryName = e.Category != null ? e.Category.Name : "",
                        backgroundColor = e.Category != null ? e.Category.Color : "#3c8dbc",
                        borderColor = e.Category != null ? e.Category.Color : "#3c8dbc",
                        textColor = "#fff",
                        isAllDay = e.IsAllDay
                    }).ToList();

                return Json(events, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Calendar/CreateEventWithoutDate - Tarih olmadan etkinlik oluştur
        [HttpPost]
        public JsonResult CreateEventWithoutDate(string Title, string Description, int? CategoryId, bool IsAllDay)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("CreateEventWithoutDate çağrıldı");
                System.Diagnostics.Debug.WriteLine($"Title: {Title}");
                System.Diagnostics.Debug.WriteLine($"Description: {Description}");
                System.Diagnostics.Debug.WriteLine($"CategoryId: {CategoryId}");
                System.Diagnostics.Debug.WriteLine($"IsAllDay: {IsAllDay}");
                
                if (string.IsNullOrEmpty(Title))
                {
                    System.Diagnostics.Debug.WriteLine("Title boş");
                    return Json(new { success = false, message = "Etkinlik başlığı boş olamaz" });
                }

                // Tarih olmadan etkinlik oluştur
                var newEvent = new Event
                {
                    Title = Title,
                    Description = Description ?? "",
                    CategoryId = CategoryId,
                    IsAllDay = IsAllDay,
                    StartDate = null, // Tarih yok
                    EndDate = null    // Tarih yok
                };

                db.Events.Add(newEvent);
                db.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"Etkinlik oluşturuldu. ID: {newEvent.Id}");

                return Json(new { 
                    success = true, 
                    id = newEvent.Id,
                    title = newEvent.Title,
                    description = newEvent.Description,
                    categoryId = newEvent.CategoryId,
                    isAllDay = newEvent.IsAllDay
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateEventWithoutDate hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Calendar/UpdateEventDate - Etkinlik tarihini güncelle
        [HttpPost]
        public JsonResult UpdateEventDate(int eventId, string startDate, string endDate)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"UpdateEventDate çağrıldı - EventId: {eventId}, StartDate: {startDate}, EndDate: {endDate}");
                
                if (eventId <= 0)
                {
                    return Json(new { success = false, message = "Geçersiz etkinlik ID'si" });
                }

                var existingEvent = db.Events.Find(eventId);
                if (existingEvent == null)
                {
                    return Json(new { success = false, message = "Etkinlik bulunamadı" });
                }

                System.Diagnostics.Debug.WriteLine($"Mevcut etkinlik bulundu: {existingEvent.Title}");

                // Tarihleri parse et ve güncelle
                DateTime parsedStartDate, parsedEndDate;
                if (DateTime.TryParse(startDate, out parsedStartDate) && DateTime.TryParse(endDate, out parsedEndDate))
                {
                    existingEvent.StartDate = parsedStartDate;
                    existingEvent.EndDate = parsedEndDate;
                    
                    System.Diagnostics.Debug.WriteLine($"Tarihler güncellendi: {parsedStartDate} - {parsedEndDate}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Tarih parse hatası: {startDate}, {endDate}");
                    return Json(new { success = false, message = "Geçersiz tarih formatı" });
                }

                db.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Veritabanı güncellendi");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateEventDate hatası: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Calendar/CreateEvent
        [HttpPost]
        public JsonResult CreateEvent(Event eventItem)
        {
            try
            {
                if (eventItem == null)
                {
                    return Json(new { success = false, message = "Event verisi boş olamaz" });
                }

                if (string.IsNullOrEmpty(eventItem.Title))
                {
                    return Json(new { success = false, message = "Etkinlik başlığı boş olamaz" });
                }

                if (!eventItem.CategoryId.HasValue || eventItem.CategoryId.Value <= 0)
                {
                    return Json(new { success = false, message = "Geçerli bir kategori seçilmedi" });
                }

                if (ModelState.IsValid)
                {
                    db.Events.Add(eventItem);
                    db.SaveChanges();

                    return Json(new { success = true, id = eventItem.Id });
                }

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Model validation hatası", errors = errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Calendar/UpdateEvent
        [HttpPost]
        public JsonResult UpdateEvent(Event eventItem)
        {
            try
            {
                if (eventItem == null)
                {
                    return Json(new { success = false, message = "Event verisi boş olamaz" });
                }

                if (eventItem.Id <= 0)
                {
                    return Json(new { success = false, message = "Geçersiz Event ID" });
                }

                var existingEvent = db.Events.Find(eventItem.Id);
                if (existingEvent == null)
                {
                    return Json(new { success = false, message = "Etkinlik bulunamadı" });
                }

                // Etkinliği güncelle
                existingEvent.Title = eventItem.Title;
                existingEvent.StartDate = eventItem.StartDate;
                existingEvent.EndDate = eventItem.EndDate;
                existingEvent.Description = eventItem.Description;
                existingEvent.IsAllDay = eventItem.IsAllDay;

                // Kategori ID'si varsa güncelle
                if (eventItem.CategoryId.HasValue && eventItem.CategoryId.Value > 0)
                {
                    existingEvent.CategoryId = eventItem.CategoryId.Value;
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Calendar/GetCategoryColor
        [HttpGet]
        public JsonResult GetCategoryColor(int categoryId)
        {
            try
            {
                var category = db.Categories.Find(categoryId);
                if (category != null)
                {
                    return Json(new { success = true, color = category.Color }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Kategori bulunamadı" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Calendar/DeleteEvent
        [HttpPost]
        public JsonResult DeleteEvent(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "Geçersiz etkinlik ID'si" });
                }

                var eventItem = db.Events.Find(id);
                if (eventItem == null)
                {
                    return Json(new { success = false, message = "Etkinlik bulunamadı" });
                }

                db.Events.Remove(eventItem);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
