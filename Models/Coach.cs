using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kursach.Models
{
    [Table("Coaches")]
    public class Coach
    {
        [Key]
        public int CoachID { get; set; }

        [Required(ErrorMessage = "Поле 'Фамилия' обязательно для заполнения")]
        [Display(Name = "Фамилия")]
        public string LastNameC { get; set; }

        [Required(ErrorMessage = "Поле 'Имя' обязательно для заполнения")]
        [Display(Name = "Имя")]
        public string FirstNameC { get; set; }

        [Required(ErrorMessage = "Поле 'Отчество' обязательно для заполнения")]
        [Display(Name = "Отчество")]
        public string SurnameC { get; set; }

        [Display(Name = "Специализация")]
        public string Speciality { get; set; }

        [Display(Name = "Номер телефона")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\+?[0-9]{10,12}$", ErrorMessage = "Номер телефона должен содержать от 10 до 12 символов")]
        public string PhoneNumberC { get; set; }

        [Display(Name = "Профессиональный стаж")]
        public decimal ProfStag { get; set; }

        // Связь с Группами (один ко многим)
        public virtual ICollection<Group> Groups { get; set; } = new HashSet<Group>();

        // Связь с Расписанием (один ко многим)
        public virtual ICollection<Timetable> Timetables { get; set; } = new HashSet<Timetable>();


        // Вычисляемое свойство для отображения ФИО
        [NotMapped]
        [Display(Name = "ФИО тренера")]
        public string FullName => $"{LastNameC} {FirstNameC} {SurnameC}";
    }
}
