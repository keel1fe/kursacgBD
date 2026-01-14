using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kursach.Models
{
	[Table("Groups")]
	public class Group
	{
		[Key]
		public int GroupID { get; set; }

		[Required(ErrorMessage = "Поле 'Название группы' обязательно для заполнения")]
		[Display(Name = "Название группы")]
        public string GroupName { get; set; }

		[Display(Name = "Уровень подготовки")]
        public string Level_gr { get; set; }

		[ForeignKey("Coach")]
		[Display(Name = "ТренерID")]
        public int CoachID { get; set; }

		// Навигационное свойство к Тренеру
		public virtual Coach Coach { get; set; }

        // Связь с Танцорами (многие ко многим)
        public virtual ICollection<DancerGroup> DancerGroups { get; set; } = new HashSet<DancerGroup>();
        // Связь с Расписанием (один ко многим)
        public virtual ICollection<Timetable> Timetables { get; set; } = new HashSet<Timetable>();
    }
}

