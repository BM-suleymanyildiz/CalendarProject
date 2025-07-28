using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CalendarProject.Context;
using CalendarProject.Entities;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CalendarProject.Controllers
{
    public class CalendarController : Controller
    {
        private ProjectContext db = new ProjectContext();

        public ActionResult Index()
        {
            var events = db.Events.ToList();
            var categories = db.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.CategoryColors = categories.ToDictionary(c => c.Id.ToString(), c => c.Color);
            return View(events);
        }

        public ActionResult Test()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetEvents()
        {
            try
            {
                var totalEvents = db.Events.Count();
                System.Diagnostics.Debug.WriteLine($"Toplam etkinlik sayısı: {totalEvents}");
                
                var events = db.Events.Include("Category")
                    .Where(e => e.StartDate.HasValue && e.EndDate.HasValue)
                    .ToList()
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

        [HttpGet]
        public JsonResult GetExternalEvents()
        {
            try
            {
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

        [HttpPost]
        public JsonResult CreateEventWithoutDate(string Title, string Description, int? CategoryId, bool IsAllDay)
        {
            try
            {
                var newEvent = new Event
                {
                    Title = Title,
                    Description = Description,
                    CategoryId = CategoryId,
                    IsAllDay = IsAllDay,
                    StartDate = null,
                    EndDate = null
                };

                db.Events.Add(newEvent);
                db.SaveChanges();

                return Json(new { success = true, id = newEvent.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateEventDate(int eventId, string startDate, string endDate)
        {
            try
            {
                var eventItem = db.Events.Find(eventId);
                if (eventItem == null)
                {
                    return Json(new { success = false, message = "Etkinlik bulunamadı" });
                }

                DateTime startDateTime, endDateTime;
                if (DateTime.TryParse(startDate, out startDateTime) && DateTime.TryParse(endDate, out endDateTime))
                {
                    eventItem.StartDate = startDateTime;
                    eventItem.EndDate = endDateTime;
                    db.SaveChanges();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Geçersiz tarih formatı" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CreateEvent(Event eventItem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Events.Add(eventItem);
                    db.SaveChanges();
                    return Json(new { success = true, id = eventItem.Id });
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateEvent(Event eventItem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingEvent = db.Events.Find(eventItem.Id);
                    if (existingEvent != null)
                    {
                        existingEvent.Title = eventItem.Title;
                        existingEvent.Description = eventItem.Description;
                        existingEvent.StartDate = eventItem.StartDate;
                        existingEvent.EndDate = eventItem.EndDate;
                        existingEvent.CategoryId = eventItem.CategoryId;
                        existingEvent.IsAllDay = eventItem.IsAllDay;

                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Etkinlik bulunamadı" });
                    }
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

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
                else
                {
                    return Json(new { success = false, message = "Kategori bulunamadı" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DeleteEvent(int id)
        {
            try
            {
                var eventItem = db.Events.Find(id);
                if (eventItem != null)
                {
                    db.Events.Remove(eventItem);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Etkinlik bulunamadı" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ChatWithAI(string message)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var apiKey = System.Configuration.ConfigurationManager.AppSettings["RapidAPIKey"];
                    var apiHost = System.Configuration.ConfigurationManager.AppSettings["RapidAPIHost"];
                    
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://chatgpt-42.p.rapidapi.com/conversationgpt4"),
                        Headers =
                        {
                            { "x-rapidapi-key", apiKey },
                            { "x-rapidapi-host", apiHost },
                        },
                        Content = new StringContent(JsonConvert.SerializeObject(new
                        {
                            messages = new[]
                            {
                                new { role = "user", content = message }
                            },
                            system_prompt = "Sen bir takvim uygulamasının AI asistanısın. Türkçe yanıt ver ve takvim, etkinlik yönetimi konularında yardımcı ol. Kısa ve öz yanıtlar ver.",
                            temperature = 0.9,
                            top_k = 5,
                            top_p = 0.9,
                            max_tokens = 256,
                            web_access = false
                        }), Encoding.UTF8, "application/json")
                    };

                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var responseContent = await response.Content.ReadAsStringAsync();
                        
                        var responseObj = JObject.Parse(responseContent);
                        
                        string aiResponse = "";
                        
                        if (responseObj["result"] != null)
                        {
                            aiResponse = responseObj["result"].ToString();
                        }
                        else if (responseObj["choices"] != null)
                        {
                            var choices = responseObj["choices"];
                            if (choices.Count() > 0)
                            {
                                aiResponse = choices[0]["message"]["content"].ToString();
                            }
                        }
                        else if (responseObj["message"] != null)
                        {
                            aiResponse = responseObj["message"].ToString();
                        }
                        else if (responseObj["content"] != null)
                        {
                            aiResponse = responseObj["content"].ToString();
                        }
                        else
                        {
                            aiResponse = "Yanıt formatı tanınmadı: " + responseContent;
                        }

                        return Json(new { success = true, response = aiResponse });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, response = "Hata: " + ex.Message });
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
