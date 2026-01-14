using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Caching;
using System.Web.Mvc;
using kursach.Models;

namespace kursach.Controllers
{
    public class CoachController : Controller
    {
        private DanceContext db = new DanceContext();

        // GET: Coach
        public ActionResult Index(string search = "", string specialization = "")
        {
            var Coaches = db.Coaches.AsQueryable();

            // поиск по ФИО
            if (!string.IsNullOrEmpty(search))
            {
                Coaches = Coaches.Where(c =>
                    c.LastNameC.Contains(search) ||
                    c.FirstNameC.Contains(search) ||
                    c.SurnameC.Contains(search));
            }

            // Фильтрация по специализации
            if (!string.IsNullOrEmpty(specialization))
            {
                Coaches = Coaches.Where(c => c.Speciality == specialization);
            }

            // Получаем список специализаций для фильтра
            ViewBag.Specializations = db.Coaches
                .Select(c => c.Speciality)
                .Distinct()
                .ToList();

            ViewBag.SearchTerm = search;
            ViewBag.SelectedSpecialization = specialization;
            return View(Coaches.ToList());
        }

        //подробная информация о записи
        // GET: Coach/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID тренера" });
            }

            Coach Coach = db.Coaches.Find(id);
            if (Coach == null)
            {
                return HttpNotFound();
            }

            return View(Coach);
        }

        //добавление новой записи в таблицу
        // GET: Coach/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Coach/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Coach Coach)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Coaches.Add(Coach);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Тренер успешно добавлен!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
            }

            return View(Coach);
        }


        //редактирование записей таблицы
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID тренера" });
            }

            Coach Coach = db.Coaches.Find(id);
            if (Coach == null)
            {
                return HttpNotFound();
            }

            return View(Coach);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Coach Coach)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(Coach).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Данные тренера успешно обновлены!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
            }

            return View(Coach);
        }

        //удаление записей из таблицы
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID тренера" });
            }

            Coach Coach = db.Coaches.Find(id);
            if (Coach == null)
            {
                return HttpNotFound();
            }

            return View(Coach);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Coach Coach = db.Coaches.Find(id);
                // Проверка, есть ли у тренера группы
                if (Coach.Groups.Any())
                {
                    TempData["ErrorMessage"] = "Невозможно удалить тренера. Сначала удалите или переназначьте его группы.";
                    return RedirectToAction("Delete", new { id = id });
                }

                db.Coaches.Remove(Coach);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Тренер успешно удален!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении: {ex.Message}";
                return RedirectToAction("Delete", new { id = id });
            }
        }
    }
}