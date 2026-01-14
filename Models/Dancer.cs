using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kursach.Models
{
    [Table("Dancers")]
    public class Dancer
    {
        [Key]
        [Column("DancerID")]
        public int DancerID { get; set; }

        [Required(ErrorMessage = "Поле 'Фамилия' обязательно для заполнения")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Поле 'Имя' обязательно для заполнения")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Display(Name = "Отчество")]
        public string Surname { get; set; }

        [Display(Name = "Дата рождения")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateofBirthday { get; set; }

        [Display(Name = "Номер телефона")]
        [StringLength(12, MinimumLength = 10, ErrorMessage = "Номер телефона должен содержать от 10 до 12 символов")]
        [RegularExpression(@"^(\+7|8)[0-9]{10}$", ErrorMessage = "Номер телефона должен содержать от 10 до 12 символов")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Эл.почта")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }

        // Связь с Группами (многие ко многим)
        public virtual ICollection<DancerGroup> DancerGroups { get; set; } = new HashSet<DancerGroup>();

        // Связь с Посещаемостью (один ко многим)
        public virtual ICollection<Attendance> Attendances { get; set; } = new HashSet<Attendance>();

        // Связь с Платежами (один ко многим)
        public virtual ICollection<Pay> Pays { get; set; } = new HashSet<Pay>();

        [NotMapped]
        [Display(Name = "ФИО")]
        public string FullName => $"{LastName} {FirstName} {Surname}";
        public Dancer()
        {
            DancerGroups = new List<DancerGroup>();
            Pays = new List<Pay>();
        }
    }
}
