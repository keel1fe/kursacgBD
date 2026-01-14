using kursach.Models;
using System.Data.Entity;
using System.Web.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;

namespace kursach.Controllers
{
    public class PayController : Controller
    {
        private DanceContext db = new DanceContext();

        public ActionResult Index(DateTime? fromDate = null, DateTime? toDate = null,
            string subscriptionFilter = "all", int? daysThreshold = null, int? calculateForId = null)
        {
            try
            {
                // Если передан ID для расчета абонемента
                if (calculateForId.HasValue)
                {
                    var pay = db.Pays.Find(calculateForId.Value);
                    if (pay != null)
                    {
                        // Вызов хранимой процедуры
                        var result = db.Database.SqlQuery<SubscriptionResult>(
                            "EXEC [dbo].[CalculateSubscriptionEndDate] @PaymentID, @Amount, @PaymentDate",
                            new System.Data.SqlClient.SqlParameter("@PaymentID", pay.PayID),
                            new System.Data.SqlClient.SqlParameter("@Amount", pay.Sum),
                            new System.Data.SqlClient.SqlParameter("@PaymentDate", pay.PayDate)).FirstOrDefault();

                        if (result != null)
                        {
                            pay.Payend = result.EndDate;
                            db.SaveChanges();

                            TempData["SuccessMessage"] = $"Абонемент рассчитан: {result.StatusMessage}";
                        }
                    }
                }

                var pays = db.Pays.Include(p => p.Dancer).AsQueryable();

                // Фильтрация по дате платежа
                if (fromDate.HasValue)
                {
                    pays = pays.Where(p => p.PayDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    pays = pays.Where(p => p.PayDate <= toDate.Value);
                }


                // фильтрация по статусу абонемента
                if (!string.IsNullOrEmpty(subscriptionFilter) && subscriptionFilter != "all")
                {
                    var today = DateTime.Today;
                    var threshold = daysThreshold.HasValue ? daysThreshold.Value : 7;

                    switch (subscriptionFilter)
                    {
                        case "active":
                            pays = pays.Where(p => p.Payend.HasValue && p.Payend.Value >= today);
                            break;
                        case "expired":
                            pays = pays.Where(p => p.Payend.HasValue && p.Payend.Value < today);
                            break;
                        case "expiringSoon":
                            var futureDate = today.AddDays(threshold);
                            pays = pays.Where(p => p.Payend.HasValue &&
                                                   p.Payend.Value >= today &&
                                                   p.Payend.Value <= futureDate);
                            break;
                        case "noDate":
                            pays = pays.Where(p => !p.Payend.HasValue || p.Payend.Value == DateTime.MinValue);
                            break;
                    }
                }

                ViewBag.Dancers = db.Dancers.ToList();
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.SubscriptionFilter = subscriptionFilter;
                ViewBag.DaysThreshold = daysThreshold.HasValue ? daysThreshold.Value : 7;
                return View(pays.OrderBy(p => p.PayID).ToList());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при загрузке данных: {ex.Message}";
                // Возвращаем пустой список в случае ошибки
                ViewBag.Dancers = db.Dancers.ToList();
                return View(new List<Pay>());
            }
        }

        // Класс для результата хранимой процедуры
        public class SubscriptionResult
        {
            public int PaymentID { get; set; }
            public decimal Sum { get; set; }
            public DateTime PayDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int MonthsPaid { get; set; }
            public string StatusMessage { get; set; }
        }

        // Пересчет всех абонементов
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CalculateAllSubscriptions()
        {
            try
            {
                db.Database.ExecuteSqlCommand("EXEC [dbo].[UpdateAllSubscription]");
                TempData["SuccessMessage"] = "Все абонементы успешно пересчитаны!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при массовом расчете: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // Подробная информация о записи в таблице
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID платежа" });
            }

            Pay pay = db.Pays.Include(p => p.Dancer).FirstOrDefault(p => p.PayID == id);
            if (pay == null)
            {
                return HttpNotFound();
            }

            // Рассчитываем статус абонемента
            ViewBag.SubscriptionStatus = GetSubscriptionStatus(pay);
            ViewBag.DaysRemaining = GetDaysRemaining(pay);

            return View(pay);
        }

        public ActionResult Create()
        {
            ViewBag.DancerID = new SelectList(db.Dancers, "DancerID", "FullName");
            return View();
        }

        // Создание записи в таблице
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pay pay)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Pays.Add(pay);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Платеж успешно добавлен!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
            }

            ViewBag.DancerID = new SelectList(db.Dancers, "DancerID", "FullName", pay.DancerID);
            return View(pay);
        }

        // Редактирование записи в таблице
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID платежа" });
            }

            Pay pay = db.Pays.Find(id);
            if (pay == null)
            {
                return HttpNotFound();
            }

            ViewBag.DancerID = new SelectList(db.Dancers, "DancerID", "FullName", pay.DancerID);
            return View(pay);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Pay pay)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(pay).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Данные платежа успешно обновлены!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при обновлении: {ex.Message}");
            }

            ViewBag.DancerID = new SelectList(db.Dancers, "DancerID", "FullName", pay.DancerID);
            return View(pay);
        }

        // Удаление записи из таблицы
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home", new { message = "Не указан ID платежа" });
            }

            Pay pay = db.Pays.Include(p => p.Dancer).FirstOrDefault(p => p.PayID == id);
            if (pay == null)
            {
                return HttpNotFound();
            }

            return View(pay);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Pay pay = db.Pays.Find(id);
                db.Pays.Remove(pay);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Платеж успешно удален!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении: {ex.Message}";
                return RedirectToAction("Delete", new { id = id });
            }
        }

        // Вспомогательные методы для определения статуса
        private string GetSubscriptionStatus(Pay pay)
        {
            if (!pay.Payend.HasValue || pay.Payend.Value == DateTime.MinValue)
                return "Нет даты окончания";

            var today = DateTime.Today;
            var endDate = pay.Payend.Value.Date;

            if (endDate < today)
                return "Истек";
            else if (endDate == today)
                return "Истекает сегодня";
            else if (endDate <= today.AddDays(7))
                return "Истекает скоро";
            else
                return "Активен";
        }

        private int? GetDaysRemaining(Pay pay)
        {
            if (!pay.Payend.HasValue || pay.Payend.Value == DateTime.MinValue)
                return null;

            var today = DateTime.Today;
            var endDate = pay.Payend.Value.Date;

            if (endDate < today)
                return -1 * (int)(today - endDate).TotalDays; // отрицательное число для просроченных
            else
                return (int)(endDate - today).TotalDays;
        }
    }
}