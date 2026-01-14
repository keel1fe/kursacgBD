using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Xml;

namespace kursach.Models
{
    public class DanceContext : DbContext
    {
			public DanceContext() : base("name=dance_studio")
			{
				// Отключаем автоматическое создание/миграции
				Database.SetInitializer<DanceContext>(null);
			}
			public DbSet<Dancer> Dancers { get; set; }
            public DbSet<Coach> Coaches { get; set; }
            public DbSet<Group> Groups { get; set; }
			public DbSet<DancerGroup> DancerGroups { get; set; }
			public DbSet<Timetable> Timetables { get; set; }
            public DbSet<Pay> Pays { get; set; }
            public DbSet<Attendance> Attendances { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.HasDefaultSchema("dbo");

            // Настройка связи Тренер - Группы (один-ко-многим)
            modelBuilder.Entity<Coach>()
				.HasMany(c => c.Groups)
				.WithRequired(g => g.Coach)
				.HasForeignKey(g => g.CoachID)
				.WillCascadeOnDelete(false);

            // Настройка составного ключа для DancerGroup
            modelBuilder.Entity<DancerGroup>()
                .HasKey(dg => new { dg.DancerID, dg.GroupID });

            // Настройка связи Dancer -> DancerGroup
            modelBuilder.Entity<Dancer>()
                .HasMany(d => d.DancerGroups)
                .WithRequired(dg => dg.Dancer)
                .HasForeignKey(dg => dg.DancerID)
                .WillCascadeOnDelete(false);

            // Настройка связи Group -> DancerGroup
            modelBuilder.Entity<Group>()
                .HasMany(g => g.DancerGroups)
                .WithRequired(dg => dg.Group)
                .HasForeignKey(dg => dg.GroupID)
                .WillCascadeOnDelete(false);

            // Настройка связи Тренер - Расписание (один-ко-многим)
            modelBuilder.Entity<Coach>()
				.HasMany(c => c.Timetables)
				.WithRequired(t => t.Coach)
				.HasForeignKey(t => t.CoachID)
				.WillCascadeOnDelete(false);

			// Настройка связи Группа - Расписание (один-ко-многим)
			modelBuilder.Entity<Group>()
				.HasMany(g => g.Timetables)
				.WithRequired(t => t.Group)
				.HasForeignKey(t => t.GroupID)
				.WillCascadeOnDelete(false);

			// Настройка связи Танцор - Платежи (один-ко-многим)
			modelBuilder.Entity<Dancer>()
				.HasMany(d => d.Pays)
				.WithRequired(p => p.Dancer)
				.HasForeignKey(p => p.DancerID)
				.WillCascadeOnDelete(false);

			// Настройка связи Танцор - Посещаемость (один-ко-многим)
			modelBuilder.Entity<Dancer>()
				.HasMany(d => d.Attendances)
				.WithRequired(a => a.Dancer)
				.HasForeignKey(a => a.DancerID)
				.WillCascadeOnDelete(false);

			// Настройка связи Расписание - Посещаемость (один-ко-многим)
			modelBuilder.Entity<Timetable>()
				.HasMany(t => t.Attendances)
				.WithRequired(a => a.Timetable)
				.HasForeignKey(a => a.TimetableID)
				.WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
		}
	}
}
