using JiujitsuGymApp.Data;
using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Services
{
    public class ScheduleService(ApplicationDbContext db)
    {
        private const int WeeksAhead = 4;

        public async Task<List<ClassScheduleDto>> GetSchedulesAsync()
        {
            var schedules = await db.ClassSchedules
                .AsNoTracking()
                .Include(s => s.Teacher)
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.TimeOfDay)
                .ToListAsync();

            return schedules.Select(ToDto).ToList();
        }

        public async Task<(ClassScheduleDto? schedule, IEnumerable<string> errors)> CreateScheduleAsync(CreateClassScheduleDto dto)
        {
            if (!Enum.TryParse<DayOfWeek>(dto.DayOfWeek, out var day))
                return (null, ["Invalid day of week."]);

            if (!TimeSpan.TryParse(dto.TimeOfDay, out var time))
                return (null, ["Invalid time format. Use HH:mm."]);

            var teacher = await db.Users.FirstOrDefaultAsync(u => u.Id == dto.TeacherId);
            if (teacher is null)
                return (null, ["Teacher not found."]);

            var schedule = new ClassSchedule
            {
                TeacherId = dto.TeacherId,
                Location = dto.Location,
                DayOfWeek = day,
                TimeOfDay = time,
                IsActive = true
            };

            db.ClassSchedules.Add(schedule);
            await db.SaveChangesAsync();

            schedule.Teacher = teacher;
            return (ToDto(schedule), []);
        }

        public async Task<bool> DeactivateScheduleAsync(int id)
        {
            var schedule = await db.ClassSchedules.FindAsync(id);
            if (schedule is null) return false;

            schedule.IsActive = false;
            await db.SaveChangesAsync();
            return true;
        }

        public static ClassScheduleDto ToDto(ClassSchedule s) => new()
        {
            Id = s.Id,
            TeacherId = s.TeacherId,
            TeacherName = s.Teacher?.Name ?? string.Empty,
            Location = s.Location,
            DayOfWeek = s.DayOfWeek.ToString(),
            TimeOfDay = s.TimeOfDay.ToString(@"hh\:mm"),
            IsActive = s.IsActive
        };

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