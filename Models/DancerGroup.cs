using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kursach.Models
{
    [Table("DancerGroup")] 
    public class DancerGroup
    {
        // Составной первичный ключ
        [Key, Column(Order = 0)]
        [ForeignKey("Dancer")]
        public int DancerID { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("Group")]
        public int GroupID { get; set; }

        // Навигационные свойства
        public virtual Dancer Dancer { get; set; }
        public virtual Group Group { get; set; }
    }
}