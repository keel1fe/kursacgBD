using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace kursach.Models
{
    [Table("Attendances")]
    public class Attendance
    {
        [Key]
        public int AttendanceID { get; set; }

        [Display(Name = "Дата занятия")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime AttendanceDate { get; set; }

        [Display(Name = "Присутсвие")]
        public bool IsPresent { get; set; }

        // Внешний ключ на Танцора
        [ForeignKey("Dancer")]
        [Display(Name = "Танцор")]
        public int DancerID { get; set; }

        // Внешний ключ на Расписание
        [ForeignKey("Timetable")]
        [Display(Name = "Занятие")]
        public int TimetableID { get; set; }

        public virtual Dancer Dancer { get; set; }
        public virtual Timetable Timetable { get; set; }
    }
}
