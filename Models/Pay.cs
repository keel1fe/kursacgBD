using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kursach.Models
{
    [Table("Pays")]
    public class Pay
	{
		[Key]
        [Display(Name = "PayID")]
        public int PayID { get; set; }

        [Display(Name = "Сумма")]
        public int Sum { get; set; }
        
        [Display(Name = "Оплачено")]
        public bool IsPaid { get; set; }

        [Display(Name = "Дата платежа")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PayDate { get; set; } 

        [Display(Name = "Действует до")]   
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Payend { get; set; }

        // Внешний ключ на Танцора
        [ForeignKey("Dancer")]
        [Display(Name = "Танцор")]
        public int DancerID { get; set; }

        // Навигационное свойство к Танцору
        public virtual Dancer Dancer { get; set; }
    }
}
