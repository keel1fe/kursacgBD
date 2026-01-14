using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using kursach.Models;

namespace kursach.Controllers
{
    public class DancerController : Controller
    {
        private DanceContext db = new DanceContext();
        
        public ActionResult Index(string search = "")
        {
            //if (Session["UserRole"] == null)
            //{
            //    // если роли нет — отправляем на выбор роли
            //    return RedirectToAction("Index", "Home");
            //}

            //// Проверка роли
            //var userRole = Session["UserRole"]?.ToString() ?? "";
            //ViewBag.UserRole = userRole;

            var dancers = db.Dancers.AsQueryable();

            //поиск по ФИО
            if (!string.IsNullOrEmpty(search))
            {
                dancers = dancers.Where(d =>
                    d.LastName.Contains(search) ||
                    d.FirstName.Contains(search) ||
                    d.Surname.Contains(search));
            }
            return View(dancers.ToList());
        }


        //получение подробной информации о выбранной записи
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID танцора" });
            }

            Dancer dancer = db.Dancers.Find(id);
            if (dancer == null)
            {
                return HttpNotFound();
            }

            return View(dancer);
        }

        //создание записи в таблице
        public ActionResult Create()
        {
            int nextId = db.Dancers.Any() ? db.Dancers.Max(a => a.DancerID) + 1 : 1;
            ViewBag.NextId = nextId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Dancer dancer)
        {
            try
            {
                // ВАЖНО: Ручной парсинг даты
                if (Request.Form["DateofBirthday"] != null)
                {
                    string dateString = Request.Form["DateofBirthday"];

                    // Убираем возможные проблемы с форматом
                    if (!string.IsNullOrEmpty(dateString))
                    {
                        if (DateTime.TryParseExact(dateString, "dd.MM.yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        {
                            dancer.DateofBirthday = parsedDate;
                        }
                        else if (DateTime.TryParse(dateString, out parsedDate))
                        {
                            dancer.DateofBirthday = parsedDate;
                        }
                        else
                        {
                            ModelState.AddModelError("DateofBirthday", "Неверный формат даты. Используйте ДД.ММ.ГГГГ");
                        }
                    }
                }

                // Исправляем DancerID из ViewBag
                if (ViewBag.NextId != null && dancer.DancerID == 0)
                {
                    dancer.DancerID = (int)ViewBag.NextId;
                }

                if (ModelState.IsValid)
                {
                    // Если DancerID все равно 0, генерируем новый
                    if (dancer.DancerID == 0)
                    {
                        int nextId = db.Dancers.Any() ? db.Dancers.Max(a => a.DancerID) + 1 : 1;
                        dancer.DancerID = nextId;
                    }

                    db.Dancers.Add(dancer);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Танцор успешно добавлен!";
                    return RedirectToAction("Index");
                }
                else
                {
                    // Логируем ошибки для отладки
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                    }
                    TempData["ValidationErrors"] = "Пожалуйста, исправьте ошибки в форме";
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
                TempData["ValidationErrors"] = $"Ошибка при сохранении: {errorDetails}";

                // Логи для отладки
                System.Diagnostics.Debug.WriteLine($"Exception: {errorDetails}");
            }

            // Восстанавливаем NextId при возврате на View
            if (!db.Dancers.Any())
            {
                ViewBag.NextId = 1;
            }
            else
            {
                ViewBag.NextId = db.Dancers.Max(a => a.DancerID) + 1;
            }

            return View(dancer);
        }

        //редактирование данных
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID танцора" });
            }
            
            Dancer dancer = db.Dancers.Find(id);
            if (dancer == null)
            {
                return HttpNotFound();
            }

            return View(dancer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Dancer dancer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Если дата пришла в формате dd.MM.yyyy
                    if (Request.Form["DateofBirthday"] != null)
                    {
                        var dateString = Request.Form["DateofBirthday"];
                        if (DateTime.TryParseExact(dateString, "dd.MM.yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        {
                            dancer.DateofBirthday = parsedDate;
                        }
                        db.Entry(dancer).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Данные танцора успешно обновлены!";
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
            }

            return View(dancer);
        }

        //удаление записей из таблицы
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID танцора" });
            }

            Dancer dancer = db.Dancers.Find(id);
            if (dancer == null)
            {
                return HttpNotFound();
            }

            return View(dancer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Dancer dancer = db.Dancers.Find(id);
                db.Dancers.Remove(dancer);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Танцор успешно удален!";
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