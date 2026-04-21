using JiujitsuGymApp.Data;
using JiujitsuGymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Services
{
    public class ScheduleService(ApplicationDbContext db)
    {
        private const int WeeksAhead = 4;

        public async Task EnsureSessionsGeneratedAsync()
        {
            var schedules = await db.ClassSchedules
                .Where(s => s.IsActive)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var until = today.AddDays(WeeksAhead * 7);

            foreach (var schedule in schedules)
            {
                for (var date = today; date <= until; date = date.AddDays(1))
                {
                    if (date.DayOfWeek != schedule.DayOfWeek) continue;

                    var sessionTime = date.ToDateTime(TimeOnly.FromTimeSpan(schedule.TimeOfDay), DateTimeKind.Utc);

                    var exists = await db.Classes.AnyAsync(c =>
                        c.ClassScheduleId == schedule.Id &&
                        c.DateTime == sessionTime);

                    if (!exists)
                    {
                        db.Classes.Add(new Class
                        {
                            TeacherId = schedule.TeacherId,
                            Location = schedule.Location,
                            DateTime = sessionTime,
                            ClassScheduleId = schedule.Id
                        });
                    }
                }
            }

            await db.SaveChangesAsync();
        }
    }
}