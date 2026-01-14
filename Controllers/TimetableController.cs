using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using kursach.Models;

namespace kursach.Controllers
{
    public class TimetableController : Controller
    {
        private DanceContext db = new DanceContext();

        public ActionResult Index(string day = "", int? coachId = null, int? hall = null)
        {
            var timetables = db.Timetables.Include(t => t.Coach).Include(t => t.Group).AsQueryable();

            // Фильтрация по дню недели
            if (!string.IsNullOrEmpty(day))
            {
                timetables = timetables.Where(t => t.Dayofweek == day);
            }

            // Фильтрация по тренеру
            if (coachId.HasValue)
            {
                timetables = timetables.Where(t => t.CoachID == coachId.Value);
            }

            ViewBag.Coaches = db.Coaches.OrderBy(c => c.LastNameC).ToList();
            ViewBag.DaysOfWeek = new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };
            ViewBag.SelectedDay = day;
            ViewBag.SelectedCoachId = coachId;
            ViewBag.SelectedHall = hall;

            return View(timetables.AsEnumerable().OrderBy(t => GetDayOfWeekOrder(t.Dayofweek)).ThenBy(t => t.Time).ToList());
        }

        //подробная информацию о записи в таблице
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID расписания" });
            }

            Timetable timetable = db.Timetables.Include(t => t.Coach).Include(t => t.Group).FirstOrDefault(t => t.TimetableID == id);
            if (timetable == null)
            {
                return HttpNotFound();
            }

            return View(timetable);
        }

        //создание записи в таблице
        public ActionResult Create()
        {
            ViewBag.Coaches = new SelectList(db.Coaches, "CoachID", "FullName");
            ViewBag.DaysOfWeek = new SelectList(new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" });
            ViewBag.Halls = Enumerable.Range(1, 5).Select(i => new SelectListItem { Value = i.ToString(), Text = $"Зал {i}" });
            ViewBag.LessonTypes = new SelectList(new[] { "Индивидуальное", "Групповое", "Мастер-класс"});

            return View();
        }

        // POST: Timetable/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Timetable timetable)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Timetables.Add(timetable);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Расписание успешно добавлено!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
            }

            ViewBag.Coaches = new SelectList(db.Coaches, "CoachID", "FullName", timetable.CoachID);
            ViewBag.DaysOfWeek = new SelectList(new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" }, timetable.Dayofweek);
            ViewBag.Halls = Enumerable.Range(1, 5).Select(i => new SelectListItem { Value = i.ToString(), Text = $"Зал {i}", Selected = i == timetable.Hall });
            ViewBag.LessonTypes = new SelectList(new[] { "Индивидуальное", "Групповое", "Мастер-класс"}, timetable.Type);

            return View(timetable);
        }

        //редактирование записи в таблице
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID расписания" });
            }

            Timetable timetable = db.Timetables.Find(id);
            if (timetable == null)
            {
                return HttpNotFound();
            }

            ViewBag.Coaches = new SelectList(db.Coaches, "CoachID", "FullName", timetable.CoachID);
            ViewBag.DaysOfWeek = new SelectList(new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" }, timetable.Dayofweek);
            ViewBag.Halls = Enumerable.Range(1, 5).Select(i => new SelectListItem { Value = i.ToString(), Text = $"Зал {i}", Selected = i == timetable.Hall });
            ViewBag.LessonTypes = new SelectList(new[] { "Индивидуальное", "Групповое", "Мастер-класс" }, timetable.Type);

            return View(timetable);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Timetable timetable)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(timetable).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Расписание успешно обновлено!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
            }

            ViewBag.Coaches = new SelectList(db.Coaches, "CoachID", "FullName", timetable.CoachID);
            ViewBag.DaysOfWeek = new SelectList(new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" }, timetable.Dayofweek);
            ViewBag.Halls = Enumerable.Range(1, 5).Select(i => new SelectListItem { Value = i.ToString(), Text = $"Зал {i}", Selected = i == timetable.Hall });
            ViewBag.LessonTypes = new SelectList(new[] { "Индивидуальное", "Групповое", "Мастер-класс" }, timetable.Type);

            return View(timetable);
        }

        //удаление записи из таблицы
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID расписания" });
            }

            Timetable timetable = db.Timetables.Include(t => t.Coach).Include(t => t.Group).FirstOrDefault(t => t.TimetableID == id);
            if (timetable == null)
            {
                return HttpNotFound();
            }

            return View(timetable);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Timetable timetable = db.Timetables.Find(id);
                db.Timetables.Remove(timetable);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Расписание успешно удалено!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении: {ex.Message}";
                return RedirectToAction("Delete", new { id = id });
            }
        }

        // Вспомогательный метод для порядка дней недели
        private int GetDayOfWeekOrder(string dayName)
        {
            if (string.IsNullOrEmpty(dayName))
                return 8;

            switch (dayName.ToLower())
            {
                case "понедельник": return 1;
                case "вторник": return 2;
                case "среда": return 3;
                case "четверг": return 4;
                case "пятница": return 5;
                case "суббота": return 6;
                case "воскресенье": return 7;
                default: return 8;
            }
        }
    }
}