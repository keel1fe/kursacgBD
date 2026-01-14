using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using kursach.Models;

namespace kursach.Controllers
{
    public class GroupController : Controller
    {
        private DanceContext db = new DanceContext();

        // GET: Group
        public ActionResult Index(string level = "", int? CoachId = null)
        {
            var groups = db.Groups.Include(g => g.Coach).AsQueryable();

            // Фильтрация по уровню
            if (!string.IsNullOrEmpty(level))
            {
                groups = groups.Where(g => g.Level_gr == level);
            }

            // Фильтрация по тренеру
            if (CoachId.HasValue)
            {
                groups = groups.Where(g => g.CoachID == CoachId.Value);
            }

            ViewBag.Coaches = db.Coaches.ToList();
            ViewBag.SelectedLevel = level;
            ViewBag.SelectedCoachId = CoachId;

            return View(groups.ToList());
        }


        //подробная информация о записи
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID группы" });
            }

            Group group = db.Groups.Include(g => g.Coach).FirstOrDefault(g => g.GroupID == id);
            if (group == null)
            {
                return HttpNotFound();
            }

            return View(group);
        }


        //создание записи в таблице
        public ActionResult Create()
        {
            ViewBag.Coaches = new SelectList(db.Coaches, "CoachID", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Group group)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Groups.Add(group);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Группа успешно создана!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
            }

            ViewBag.Coaches = new SelectList(db.Coaches, "CoachID", "FullName", group.CoachID);
            return View(group);
        }


        //редактирование данных в таблице
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID группы" });
            }

            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }

            ViewBag.Coaches = new SelectList(db.Coaches, "CoachID", "FullName", group.CoachID);
            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Group group)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(group).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Данные группы успешно обновлены!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
            }

            ViewBag.Coaches = new SelectList(db.Coaches, "CoachID", "FullName", group.CoachID);
            return View(group);
        }

        //удаление записей из таблицы
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID группы" });
            }

            Group group = db.Groups.Include(g => g.Coach).FirstOrDefault(g => g.GroupID == id);
            if (group == null)
            {
                return HttpNotFound();
            }

            return View(group);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Group group = db.Groups.Find(id);
                db.Groups.Remove(group);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Группа успешно удалена!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении: {ex.Message}";
                return RedirectToAction("Delete", new { id = id });
            }
        }

        [HttpGet]
        public ActionResult ManageDancers(int id)
        {
            var userRole = Session["UserRole"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Index", "Home");
            }

            var group = db.Groups
                .Include(g => g.DancerGroups.Select(dg => dg.Dancer))
                .Include(g => g.Coach)
                .FirstOrDefault(g => g.GroupID == id);

            if (group == null)
            {
                return HttpNotFound();
            }

            // Получаем ID танцоров, которые уже в группе
            var dancerIdsInGroup = group.DancerGroups
                .Select(dg => dg.DancerID)
                .ToList();


            // Получаем всех танцоров, которые еще не в группе
            var allDancers = db.Dancers
                .Where(d => !dancerIdsInGroup.Contains(d.DancerID))
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .Select(d => new
                {
                    DancerID = d.DancerID,
                    FullName = d.LastName + " " + d.FirstName
                })
                .ToList();

            ViewBag.AllDancers = allDancers;
            ViewBag.UserRole = userRole;
            return View(group);
        }

        //добавление танцора в группу
        [HttpPost]
        public ActionResult AddDancerToGroup(int groupId, int dancerId)
        {
            // Проверяем права
            var userRole = Session["UserRole"]?.ToString() ?? "";
            if (userRole != "admin" && userRole != "coach")
            {
                TempData["ErrorMessage"] = "У вас нет прав для добавления танцоров в группу";
                return RedirectToAction("ManageDancers", new { id = groupId });
            }

            try
            {
                var group = db.Groups.Find(groupId);
                var dancer = db.Dancers.Find(dancerId);

                if (group != null && dancer != null)
                {
                    var dancerGroup = new DancerGroup
                    {
                        GroupID = groupId,
                        DancerID = dancerId,
                    };

                    db.DancerGroups.Add(dancerGroup);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = $"Танцор {dancer.FullName} добавлен в группу {group.GroupName}";
                }
                else
                {
                    TempData["ErrorMessage"] = "Группа или танцор не найдены";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToAction("ManageDancers", new { id = groupId });
        }

        //удаление танцора из группы
        [HttpPost]
        public ActionResult DeleteDancerFromGroup(int groupId, int dancerId)
        {

            try
            {
                var dancerGroup = db.DancerGroups
                            .Include(dg => dg.Dancer)
                            .Include(dg => dg.Group)
                            .FirstOrDefault(dg => dg.GroupID == groupId && dg.DancerID == dancerId);
                if (dancerGroup != null)
                {
                    var dancerName = $"{dancerGroup.Dancer.LastName} {dancerGroup.Dancer.FirstName}";
                    var groupName = dancerGroup.Group.GroupName;

                    db.DancerGroups.Remove(dancerGroup);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = $"Танцор {dancerName} удален из группы {groupName}";
                }
                else
                {
                    TempData["ErrorMessage"] = "Танцор не найден в группе";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToAction("ManageDancers", new { id = groupId });
        }
    }
}