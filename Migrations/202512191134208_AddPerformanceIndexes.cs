namespace kursach.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPerformanceIndexes : DbMigration
    {
        public override void Up()
        {
            // 1. Базовые индексы (EF их создаст)
            CreateIndex("dbo.Timetables", "CoachID", name: "IX_Timetables");
            CreateIndex("dbo.Timetables", "Dayofweek", name: "IX_Timetables");

            CreateIndex("dbo.Attendances", "DancerID", name: "IX_Attendances");
            CreateIndex("dbo.Attendances", "TimetableID", name: "IX_Attendances");
            CreateIndex("dbo.Attendances", "AttendanceDate", name: "IX_Attendances");

            CreateIndex("dbo.Groups", "CoachID", name: "IX_Groups");
            CreateIndex("dbo.Groups", "GroupName", name: "IX_Groups_GroupName");

            CreateIndex("dbo.Dancers", new[] { "LastName", "FirstName" }, name: "IX_Dancers_Name");
            CreateIndex("dbo.Dancers", "Email", name: "IX_Dancers_Email", unique: true);

            // 2. Индексы с INCLUDE через SQL (для оптимизации)
            Sql(@"
               
                CREATE NONCLUSTERED INDEX [IX_Timetables1
                ON [dbo].[Timetables] ([CoachID]) 
                INCLUDE ([Dayofweek], [Time]);
        
                CREATE NONCLUSTERED INDEX [IX_Timetables] 
                ON [dbo].[Timetables] ([Dayofweek], [Time]) 
                INCLUDE ([CoachID], [Hall]);
        
                -- Улучшенные индексы для Attendances
                CREATE NONCLUSTERED INDEX [IX_Attendances 
                ON [dbo].[Attendances] ([DancerID]) 
                INCLUDE ([TimetableID], [AttendanceDate], [IsPresent]);
        
                CREATE NONCLUSTERED INDEX [IX_Attendances] 
                ON [dbo].[Attendances] ([TimetableID]) 
                INCLUDE ([DancerID], [IsPresent], [AttendanceDate]);
        
                -- Улучшенные индексы для Groups
                CREATE NONCLUSTERED INDEX [IX_Groups] 
                ON [dbo].[Groups] ([CoachID]) 
                INCLUDE ([GroupName], [Level_gr]);
        
                -- Индекс для Dashboard (сегодняшняя посещаемость)
                CREATE NONCLUSTERED INDEX [IX_Attendances] 
                ON [dbo].[Attendances] ([AttendanceDate], [IsPresent]) 
                INCLUDE ([DancerID])
                WHERE [IsPresent] = 1;
            ");
        }

        public override void Down()
        {
            Sql(@"
                DROP INDEX IF EXISTS [IX_Timetables_CoachID_Optimized] ON [dbo].[Timetables];
                DROP INDEX IF EXISTS [IX_Timetables_DayTime] ON [dbo].[Timetables];
                DROP INDEX IF EXISTS [IX_Attendances_DancerID_Optimized] ON [dbo].[Attendances];
                DROP INDEX IF EXISTS [IX_Attendances_TimetableID_Optimized] ON [dbo].[Attendances];
                DROP INDEX IF EXISTS [IX_Groups_CoachID_Optimized] ON [dbo].[Groups];
                DROP INDEX IF EXISTS [IX_Attendances_Today] ON [dbo].[Attendances];
            ");

            // 2. Удаляем только базовые индексы, созданные в Up()
            DropIndex("dbo.Dancers", "IX_Dancers");
            DropIndex("dbo.Groups", "IX_Groups");
            DropIndex("dbo.Groups", "IX_Groups");
            DropIndex("dbo.Attendances", "IX_Attendances");
            DropIndex("dbo.Attendances", "IX_Attendances");
            DropIndex("dbo.Attendances", "IX_Attendances");
            DropIndex("dbo.Timetables", "IX_Timetables");
            DropIndex("dbo.Timetables", "IX_Timetables");
            DropIndex("dbo.Timetables", "IX_Timetables");
        }
    }
}
