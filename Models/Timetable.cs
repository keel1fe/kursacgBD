using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kursach.Models
{
	[Table("Timetables")]
	public class Timetable
    {
        [Key]
        [Display(Name = "TimetableID")]
        public int TimetableID {  get; set; }

        [Display(Name = "День недели")]
        public string Dayofweek { get; set; }

        [Display(Name = "Время начала занятий")]
        [DataType(DataType.Time)]
		[DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
		public TimeSpan Time { get; set; }

        [Display(Name = "Продолжительность занятия(мин.)")]
        public decimal Duration { get; set; }

		[Display(Name = "Зал")]
		[Range(1, 10, ErrorMessage = "Номер зала должен быть от 1 до 10")]
		public int Hall { get; set; }

        [Display(Name = "Тип занятия")]
        public string Type { get; set; }

        [ForeignKey("Coach")]   //внешний ключ на тренера
        [Display(Name = "ТренерID")]
        public int? CoachID { get; set; }

        [ForeignKey("Group")]    // Внешний ключ на Группу
        [Display(Name = "Группа")]
        public int? GroupID { get; set; }

        // Навигационные свойства
        public virtual Coach Coach { get; set; }
		public virtual Group Group { get; set; }

		// Связь с Посещаемостью (один ко многим)
		public virtual ICollection<Attendance> Attendances { get; set; } = new HashSet<Attendance>();

		[NotMapped]
		[Display(Name = "Описание занятия")]
		public string Description => $"{Dayofweek} {Time:HH:mm} - {Coach?.FullName ?? "Тренер не назначен"} (Зал {Hall})";
    }
}
