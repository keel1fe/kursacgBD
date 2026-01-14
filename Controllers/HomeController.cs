using System;
using System.Web.Mvc;

namespace kursach.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Если уже есть роль в Session, сразу переходим на главную
            if (Session["UserRole"] != null)
            {
                return RedirectToAction("Index1");
            }

            ViewBag.PageTitle = "Выбор роли";
            return View();
        }

        public ActionResult Index1(string role = "")
        {
            // Если роль передана в параметре, сохраняем ее
            if (!string.IsNullOrEmpty(role))
            {
                Session["UserRole"] = role.ToLower();
                Session["UserName"] = GetUserNameByRole(role);
            }
            // Если роль не передана, но есть в Session - используем ее
            else if (Session["UserRole"] != null)
            {
                role = Session["UserRole"].ToString();
            }
            // Если вообще нет роли - идем на выбор
            else
            {
                return RedirectToAction("Index");
            }

            ViewBag.UserRole = role;
            ViewBag.UserName = GetUserNameByRole(role);
            ViewBag.PageTitle = "Главная страница";

            return View();
        }

        // Метод для смены роли
        public ActionResult ChangeRole()
        {
            Session.Clear(); // Очищаем Session
            return RedirectToAction("Index");
        }

        // Вспомогательный метод для получения имени по роли
        private string GetUserNameByRole(string role)
        {
            switch (role.ToLower())
            {
                case "admin": return "Администратор";
                case "coach": return "Тренер";
                case "dancer": return "Танцор";
                default: return "Пользователь";
            }
        }
    }
}