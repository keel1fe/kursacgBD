using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using kursach.Models;
using System.Collections.Generic;

namespace kursach.Controllers
{
    public class AttendanceController : Controller
    {
        private DanceContext db = new DanceContext();

        // GET: Attendance
        public ActionResult Index(string date = null, int? dancerId = null, bool? isPresent = null)
        {
            try
            {
                // Определяем дату по умолчанию - сегодня
                DateTime filterDate;
                if (!string.IsNullOrEmpty(date))
                {
                    if (DateTime.TryParse(date, out DateTime parsedDate))
                    {
                        filterDate = parsedDate.Date;
                    }
                    else
                    {
                        filterDate = DateTime.Today;
                    }
                }
                else
                {
                    filterDate = DateTime.Today;
                }

                var attendances = db.Attendances
                    .Include(a => a.Dancer)
                    .Include(a => a.Timetable)
                    .AsQueryable();

                // Фильтрация по дате
                attendances = attendances.Where(a => DbFunctions.TruncateTime(a.AttendanceDate) == filterDate);

                //// Фильтрация по танцору
                //if (dancerId.HasValue)
                //{
                //    attendances = attendances.Where(a => a.DancerID == dancerId.Value);
                //}

                ViewBag.Dancers = db.Dancers.ToList();
                ViewBag.SelectedDate = filterDate.ToString("yyyy-MM-dd");
                //ViewBag.SelectedDancerId = dancerId;
                ViewBag.SelectedIsPresent = isPresent;

                return View(attendances.ToList());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при загрузке данных: {ex.Message}";
                ViewBag.Dancers = db.Dancers.ToList();
                ViewBag.SelectedDate = DateTime.Today.ToString("yyyy-MM-dd");
                return View(new List<Attendance>());
            }
        }

        // Подробная информация о записи
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID посещаемости" });
            }

            Attendance attendance = db.Attendances
                .Include(a => a.Dancer)
                .Include(a => a.Timetable)
                .FirstOrDefault(a => a.AttendanceID == id);

            if (attendance == null)
            {
                return HttpNotFound();
            }

            return View(attendance);
        }

        // Добавление новой записи в таблицу
        public ActionResult Create()
        {
            int nextId = db.Attendances.Any() ? db.Attendances.Max(a => a.AttendanceID) + 1 : 1;

            // Устанавливаем дату по умолчанию - сегодня
            var defaultAttendance = new Attendance
            {
                AttendanceID = nextId,
                AttendanceDate = DateTime.Today,
                IsPresent = true
            };

            ViewBag.DancerID = new SelectList(db.Dancers
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .Select(d => new
                {
                    DancerID = d.DancerID,
                    FullName = d.LastName + " " + d.FirstName
                }), "DancerID", "FullName");

            ViewBag.TimetableID = new SelectList(db.Timetables
                .OrderBy(t => t.Dayofweek)
                .ThenBy(t=> t.Time).ToList()
                .Select(t => new 
                {
                    TimetableID = t.TimetableID,
                    FullNameR = $"{t.Dayofweek} {t.Time:hh\\:mm}"
                }), "TimetableID", "FullNameR");

            return View(defaultAttendance);
        }

        // POST: Attendance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Attendance attendance)
        {
            try
            {
                if (attendance.AttendanceID == 0)
                {
                    int nextId = db.Attendances.Any() ? db.Attendances.Max(a => a.AttendanceID) + 1 : 1;
                    attendance.AttendanceID = nextId;
                }

                if (ModelState.IsValid)
                {
                    db.Attendances.Add(attendance);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Запись о посещаемости успешно добавлена!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                string errorDetails = ex.Message;
                if (ex.InnerException != null)
                {
                    errorDetails += " | " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        errorDetails += " | " + ex.InnerException.InnerException.Message;
                    }
                }
                ModelState.AddModelError("", $"Ошибка при сохранении: {errorDetails}");
            }
            ViewBag.DancerID = new SelectList(db.Dancers
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .Select(d => new
            {
                DancerID = d.DancerID,
                FullName = d.LastName + " " + d.FirstName
            }), "DancerID", "FullName", attendance.DancerID);

            ViewBag.TimetableID = new SelectList(db.Timetables
            .OrderBy(t => t.Dayofweek)
            .ThenBy(t => t.Time).ToList()
            .Select(t => new 
            { 
                TimetableID = t.TimetableID,
                FullNameR = $"{t.Dayofweek} {t.Time:hh\\:mm}"
            }), "TimetableID", "FullNameR", attendance.TimetableID);

            return View(attendance);
        }

        // Редактирование записи
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID посещаемости" });
            }

            Attendance attendance = db.Attendances.Find(id);
            if (attendance == null)
            {
                return HttpNotFound();
            }
            ViewBag.TimetableID = new SelectList(db.Timetables, "TimetableID", "TimetableID", attendance.TimetableID);

            return View(attendance);
        }

        // POST: Attendance/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Attendance attendance)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(attendance).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Запись о посещаемости успешно обновлена!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
            }
            ViewBag.TimetableID = new SelectList(db.Timetables, "TimetableID", "TimetableID", attendance.TimetableID);

            return View(attendance);
        }

        // Удаление записи
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID посещаемости" });
            }

            Attendance attendance = db.Attendances
                .Include(a => a.Dancer)
                .FirstOrDefault(a => a.AttendanceID == id);

            if (attendance == null)
            {
                return HttpNotFound();
            }

            return View(attendance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                Attendance attendance = db.Attendances.Find(id);
                db.Attendances.Remove(attendance);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Запись о посещаемости успешно удалена!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении: {ex.Message}";
                return RedirectToAction("Delete", new { id = id });
            }
        }

        //// Метод для получения танцоров по времени (AJAX)
        //[HttpGet]
        //public JsonResult GetDancersByTime(string dayOfWeek, string time)
        //{
        //    try
        //    {
        //        // Преобразуем время
        //        TimeSpan classTime;
        //        if (!TimeSpan.TryParse(time, out classTime))
        //        {
        //            classTime = new TimeSpan(18, 0, 0); // По умолчанию 18:00
        //        }

        //        // Получаем день недели из даты
        //        var day = DateTime.Today.ToString("dddd"); // Или используем переданный параметр

        //        // Находим группы, которые занимаются в это время
        //        var timetables = db.Timetables
        //      .Where(t => t.Dayofweek == dayOfWeek)
        //      .Where(t => t.Time <= classTime && DbFunctions.AddMinutes(t.Time, t.Duration) >= classTime)
        //      .ToList();

        //        // Получаем ID групп
        //        var groupIds = groups.Select(g => g.GroupID).ToList();

        //        // Получаем танцоров, которые состоят в этих группах
        //        var dancers = db.Dancers
        //            .Where(d => d.DancerGroups.Any(dg => groupIds.Contains(dg.GroupID)))
        //            .Select(d => new
        //            {
        //                DancerID = d.DancerID,
        //                FullName = d.LastName + " " + d.FirstName,
        //                Groups = string.Join(", ", d.DancerGroups
        //                    .Where(dg => groupIds.Contains(dg.GroupID))
        //                    .Select(dg => dg.Group.GroupName))
        //            })
        //            .OrderBy(d => d.FullName)
        //            .ToList();

        //        // Получаем все группы для фильтра
        //        var allGroups = db.Groups
        //            .Where(g => g.DayOfWeek == day)
        //            .Select(g => new
        //            {
        //                GroupID = g.GroupID,
        //                GroupName = g.GroupName,
        //                TimeRange = $"{g.StartTime:hh\\:mm} - {g.EndTime:hh\\:mm}"
        //            })
        //            .ToList();

        //        return Json(new
        //        {
        //            success = true,
        //            dancers = dancers,
        //            groups = allGroups,
        //            filteredGroupIds = groupIds
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = ex.Message
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        // POST: Attendance/MarkAttendance (массовая отметка)
        [HttpPost]
        public ActionResult MarkAttendance(int[] dancerIds, DateTime attendanceDate, bool isPresent)
        {
            try
            {
                if (dancerIds != null && dancerIds.Length > 0)
                {
                    foreach (var dancerId in dancerIds)
                    {
                        var attendance = new Attendance
                        {
                            DancerID = dancerId,
                            AttendanceDate = attendanceDate,
                            IsPresent = isPresent
                        };
                        db.Attendances.Add(attendance);
                    }
                    db.SaveChanges();
                    TempData["SuccessMessage"] = $"Отмечено {dancerIds.Length} танцоров!";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при отметке посещаемости: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}