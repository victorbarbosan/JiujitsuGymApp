using JiujitsuGymApp.Data;
using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Services
{
    public class ClassService(ApplicationDbContext db)
    {
        public async Task<List<ClassEventDto>> GetClassEventsAsync(DateTime from, DateTime to, string? userId)
        {
            return await db.Classes
                .AsNoTracking()
                .Include(c => c.Teacher)
                .Include(c => c.Attendances)
                .Where(c => c.DeletedAt == null && c.DateTime >= from && c.DateTime < to)
                .OrderBy(c => c.DateTime)
                .Select(c => new ClassEventDto
                {
                    Id = c.Id,
                    Location = c.Location,
                    TeacherName = c.Teacher.FirstName + " " + c.Teacher.LastName,
                    DateTime = c.DateTime.ToString("o"),
                    AttendanceCount = c.Attendances.Count,
                    CheckedIn = userId != null && c.Attendances.Any(a => a.UserId == userId)
                })
                .ToListAsync();
        }

        public async Task<int> GetTotalAttendedAsync(string userId)
        {
            return await db.Attendances.CountAsync(a => a.UserId == userId);
        }

        public async Task<CheckInResult> CheckInAsync(int classId, string userId)
        {
            var classExists = await db.Classes.AnyAsync(c => c.Id == classId && c.DeletedAt == null);
            if (!classExists) return CheckInResult.NotFound;

            var alreadyCheckedIn = await db.Attendances.AnyAsync(a => a.ClassId == classId && a.UserId == userId);
            if (alreadyCheckedIn) return CheckInResult.AlreadyCheckedIn;

            db.Attendances.Add(new Attendance
            {
                ClassId = classId,
                UserId = userId,
                CheckedInAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return CheckInResult.Success;
        }

        public async Task<CheckInResult> UndoCheckInAsync(int classId, string userId)
        {
            var attendance = await db.Attendances
                .FirstOrDefaultAsync(a => a.ClassId == classId && a.UserId == userId);

            if (attendance is null) return CheckInResult.NotFound;

            db.Attendances.Remove(attendance);
            await db.SaveChangesAsync();
            return CheckInResult.Success;
        }
    }

    public enum CheckInResult { Success, NotFound, AlreadyCheckedIn }
}