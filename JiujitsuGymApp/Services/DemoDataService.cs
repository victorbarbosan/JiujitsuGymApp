using JiujitsuGymApp.Data;
using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// Fills an empty database with a plausible-looking gym, and takes it back
    /// out again on demand.
    ///
    /// Nothing here relies on an "is demo" column. Demo people are the ones
    /// holding an address on <see cref="DemoEmailDomain"/>, and every schedule,
    /// class and attendance the seed writes hangs off one of those people. That
    /// ownership chain is what makes <see cref="PurgeAsync"/> safe to expose in
    /// the admin UI: the delete is scoped to rows reachable from a demo
    /// account, so a real member's history is out of its reach by construction.
    /// Products are the one exception and carry
    /// <see cref="DemoProductCategory"/> as their marker instead.
    /// </summary>
    public class DemoDataService(
        ApplicationDbContext db,
        UserManager<User> userManager,
        ILogger<DemoDataService> logger)
    {
        /// <summary>
        /// `.invalid` is reserved by RFC 6761 and can never be registered, so a
        /// real signup can never masquerade as demo data and get purged.
        /// </summary>
        public const string DemoEmailDomain = "demo.invalid";

        /// <summary>
        /// Shared by every seeded account so you can sign in as any of them to
        /// see the member-facing side. Meets the Identity rules in Program.cs.
        /// </summary>
        public const string DemoPassword = "Demo123!";

        private const string DemoProductCategory = "demo";

        // Past weeks give the profile totals and attendance history something
        // to count; the future weeks are what the class calendar opens onto.
        private const int WeeksOfHistory = 4;
        private const int WeeksAhead = 4;

        // Fixed seed so a reseed reproduces the same gym. Makes screenshots and
        // manual test scripts stable across databases.
        private const int RandomSeed = 20260805;

        private static readonly (string First, string Last, BeltColor Belt)[] Teachers =
        [
            ("Tainan",  "Dalpra", BeltColor.Black),
            ("Mica",  "Galvao",  BeltColor.Black),
            ("Rafael", "Mendes",   BeltColor.Black),
            ("Kyra",  "Gracie",  BeltColor.Black),
        ];

        private static readonly string[] StudentFirstNames =
        [
            "Alex", "Bruno", "Chloe", "Diego", "Elena", "Farid", "Grace", "Hugo", "Ines", "Jonas",
            "Kira", "Liam", "Marta", "Nuno", "Olga", "Pedro", "Quinn", "Rita", "Samir", "Tessa",
        ];

        private static readonly string[] StudentLastNames =
        [
            "Barros", "Chen", "Duarte", "Eriksson", "Fonseca", "Gomes", "Haddad", "Ivanov", "Jensen", "Kowalski",
            "Lima", "Moreau", "Novak", "Osei", "Pereira", "Quintana", "Rossi", "Silva", "Tanaka", "Ustinov",
        ];

        /// <summary>
        /// Belt spread across the 40 students, weighted like a real academy:
        /// a broad white-belt base thinning out toward brown, plus a handful of
        /// kids' belts so the junior classes are not all white.
        /// </summary>
        private static readonly (BeltColor Belt, int Count)[] StudentBeltMix =
        [
            (BeltColor.White,  16),
            (BeltColor.Blue,    8),
            (BeltColor.Purple,  5),
            (BeltColor.Brown,   3),
            (BeltColor.Grey,    2),
            (BeltColor.Yellow,  2),
            (BeltColor.Orange,  2),
            (BeltColor.Green,   2),
        ];

        private static readonly (string Location, DayOfWeek Day, string Time, int Teacher)[] Schedules =
        [
            ("Kitchener", DayOfWeek.Monday,    "07:00", 0),
            ("Kitchener", DayOfWeek.Monday,    "19:00", 1),
            ("Cambridge",   DayOfWeek.Tuesday,   "19:00", 2),
            ("Cambridge",   DayOfWeek.Tuesday,   "12:00", 2),
            ("Kitchener", DayOfWeek.Wednesday, "07:00", 0),
            ("Kitchener", DayOfWeek.Wednesday, "19:00", 3),
            ("Cambridge",   DayOfWeek.Thursday,  "19:00", 1),
            ("Kitchener", DayOfWeek.Friday,    "18:00", 2),
            ("Kitchener", DayOfWeek.Saturday,  "10:00", 3),
        ];

        private static readonly (string Name, decimal Price, string Description)[] Products =
        [
            ("Academy Gi - A2",        129.00m, "Pearl weave 550gsm competition gi in academy navy."),
            ("Academy Rash Guard",      45.00m, "Long sleeve, IBJJF-legal ranked rash guard."),
            ("Ranked Belt",             28.00m, "Cotton belt with rank bar, sizes A0 to A4."),
            ("Mouthguard",              15.00m, "Boil-and-bite with vented case."),
            ("Finger Tape (10 pack)",   12.00m, "9mm cotton tape for grip and knuckle support."),
            ("Academy Water Bottle",    18.00m, "750ml insulated stainless steel."),
        ];

        /// <summary>
        /// Writes the whole demo gym in one transaction. Refuses to run when
        /// demo rows already exist rather than stacking a second copy on top.
        /// </summary>
        public async Task<(DemoDataResultDto? result, IEnumerable<string> errors)> SeedAsync()
        {
            var existing = await GetStatusAsync();
            if (existing.IsSeeded)
                return (null, ["Demo data is already present. Remove it first if you want to reseed."]);

            // UserManager writes through this same scoped DbContext, so its
            // per-user SaveChanges calls enlist here too and a failure part way
            // through rolls the whole gym back instead of stranding half of it.
            await using var transaction = await db.Database.BeginTransactionAsync();

            var teachers = await CreateUsersAsync(BuildTeachers(), "Teacher");
            if (teachers.errors.Any()) return (null, teachers.errors);

            var students = await CreateUsersAsync(BuildStudents(), "Member");
            if (students.errors.Any()) return (null, students.errors);

            var schedules = CreateSchedules(teachers.users);
            var classes = CreateClasses(schedules);
            var attendances = CreateAttendances(classes, students.users);
            CreateProducts();

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation(
                "Demo data seeded: {Teachers} teachers, {Members} members, {Schedules} schedules, {Classes} classes, {Attendances} attendances",
                teachers.users.Count, students.users.Count, schedules.Count, classes.Count, attendances);

            var status = await GetStatusAsync();
            return (new DemoDataResultDto
            {
                Message = $"Seeded {status.Teachers} instructors, {status.Members} members, "
                        + $"{status.Classes} classes and {status.Attendances} check-ins.",
                Status = status
            }, []);
        }

        /// <summary>
        /// Removes every row the seed owns. Ordered child-first because
        /// Attendance -> User is NoAction, so those rows have to go before the
        /// accounts they point at.
        /// </summary>
        public async Task<DemoDataResultDto> PurgeAsync()
        {
            var before = await GetStatusAsync();

            await using var transaction = await db.Database.BeginTransactionAsync();

            var userIds = await DemoUsers().Select(u => u.Id).ToListAsync();
            var classIds = await db.Classes
                .Where(c => userIds.Contains(c.TeacherId))
                .Select(c => c.Id)
                .ToListAsync();

            // Both directions matter: a demo member may have checked into a real
            // class, and a real member may have checked into a demo one.
            await db.Attendances
                .Where(a => userIds.Contains(a.UserId) || classIds.Contains(a.ClassId))
                .ExecuteDeleteAsync();

            await db.Classes.Where(c => classIds.Contains(c.Id)).ExecuteDeleteAsync();
            await db.ClassSchedules.Where(s => userIds.Contains(s.TeacherId)).ExecuteDeleteAsync();
            await db.Products.Where(p => p.Category == DemoProductCategory).ExecuteDeleteAsync();

            // Role assignments, claims, logins and tokens cascade from Users.
            await db.Users.Where(u => userIds.Contains(u.Id)).ExecuteDeleteAsync();

            await transaction.CommitAsync();

            logger.LogInformation(
                "Demo data purged: {Users} users, {Schedules} schedules, {Classes} classes, {Attendances} attendances",
                before.Teachers + before.Members, before.Schedules, before.Classes, before.Attendances);

            return new DemoDataResultDto
            {
                Message = $"Removed {before.Teachers + before.Members} demo accounts, "
                        + $"{before.Classes} classes and {before.Attendances} check-ins.",
                Status = await GetStatusAsync()
            };
        }

        /// <summary>
        /// Counts exactly what a purge would reach, so a zeroed status is a
        /// reliable "nothing to remove" rather than an estimate.
        /// </summary>
        public async Task<DemoDataStatusDto> GetStatusAsync()
        {
            var userIds = await DemoUsers().Select(u => u.Id).ToListAsync();
            var classIds = await db.Classes
                .Where(c => userIds.Contains(c.TeacherId))
                .Select(c => c.Id)
                .ToListAsync();

            var teacherCount = 0;
            if (userIds.Count > 0)
            {
                var teachers = await userManager.GetUsersInRoleAsync("Teacher");
                teacherCount = teachers.Count(t => userIds.Contains(t.Id));
            }

            var status = new DemoDataStatusDto
            {
                Teachers = teacherCount,
                Members = userIds.Count - teacherCount,
                Schedules = await db.ClassSchedules.CountAsync(s => userIds.Contains(s.TeacherId)),
                Classes = classIds.Count,
                Attendances = await db.Attendances
                    .CountAsync(a => userIds.Contains(a.UserId) || classIds.Contains(a.ClassId)),
                Products = await db.Products.CountAsync(p => p.Category == DemoProductCategory),
                DemoPassword = DemoPassword,
                DemoEmailDomain = DemoEmailDomain
            };

            status.IsSeeded = userIds.Count > 0 || status.Products > 0;
            return status;
        }

        private IQueryable<User> DemoUsers() =>
            db.Users.Where(u => u.Email!.EndsWith("@" + DemoEmailDomain));

        private static List<User> BuildTeachers()
        {
            var joined = DateTime.UtcNow.AddYears(-6);

            return Teachers.Select((t, i) => new User
            {
                FirstName = t.First,
                LastName = t.Last,
                UserName = Email(t.First, t.Last),
                Email = Email(t.First, t.Last),
                PhoneNumber = $"+4477000{i:D5}",
                Belt = t.Belt,
                Address = $"{10 + i} Mat Street, Cambridge",
                CreatedAt = joined.AddMonths(i * 3)
            }).ToList();
        }

        private static List<User> BuildStudents()
        {
            var belts = StudentBeltMix
                .SelectMany(m => Enumerable.Repeat(m.Belt, m.Count))
                .ToList();

            return belts.Select((belt, i) =>
            {
                // Pairing that stays unique across both passes of the name list:
                // the first 20 students take matching indexes, the next 20 shift
                // the surname by one.
                var first = StudentFirstNames[i % StudentFirstNames.Length];
                var last = StudentLastNames[(i + i / StudentLastNames.Length) % StudentLastNames.Length];

                return new User
                {
                    FirstName = first,
                    LastName = last,
                    UserName = Email(first, last),
                    Email = Email(first, last),
                    PhoneNumber = $"+4477100{i:D5}",
                    Belt = belt,
                    Address = $"{i + 1} Guard Lane, Cambridge",
                    // Higher belts have been around longer, so the join dates
                    // line up with the rank instead of contradicting it.
                    CreatedAt = DateTime.UtcNow.AddMonths(-3 - ((int)belt * 9))
                };
            }).ToList();
        }

        private static string Email(string first, string last) =>
            $"{first}.{last}@{DemoEmailDomain}".ToLowerInvariant();

        private async Task<(List<User> users, IEnumerable<string> errors)> CreateUsersAsync(
            List<User> users, string role)
        {
            // Identity hashes at 100k PBKDF2 iterations, which at ~44 accounts is
            // most of the wall clock of a seed run and is slow enough on the Pi to
            // risk a proxy timeout. Hash the shared password once and hand every
            // account the same result: these are throwaway logins whose password
            // is printed in the admin UI, so a shared salt gives away nothing.
            var passwordHash = userManager.PasswordHasher.HashPassword(users[0], DemoPassword);

            foreach (var user in users)
            {
                user.PasswordHash = passwordHash;

                // The password-less overload still runs the user validators and
                // writes the normalised email, username and security stamps that
                // sign-in depends on - it only skips re-hashing what we just set.
                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return ([], result.Errors.Select(e => $"{user.Email}: {e.Description}"));

                await userManager.AddToRoleAsync(user, role);
            }

            return (users, []);
        }

        private List<ClassSchedule> CreateSchedules(List<User> teachers)
        {
            var schedules = Schedules.Select(s => new ClassSchedule
            {
                TeacherId = teachers[s.Teacher].Id,
                Location = s.Location,
                DayOfWeek = s.Day,
                TimeOfDay = TimeSpan.Parse(s.Time),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            }).ToList();

            db.ClassSchedules.AddRange(schedules);
            return schedules;
        }

        private List<Class> CreateClasses(List<ClassSchedule> schedules)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var from = today.AddDays(-WeeksOfHistory * 7);
            var until = today.AddDays(WeeksAhead * 7);

            var classes = new List<Class>();

            foreach (var schedule in schedules)
            {
                for (var date = from; date <= until; date = date.AddDays(1))
                {
                    if (date.DayOfWeek != schedule.DayOfWeek) continue;

                    classes.Add(new Class
                    {
                        TeacherId = schedule.TeacherId,
                        Location = schedule.Location,
                        // Kind must be Utc or Npgsql rejects the timestamptz write.
                        DateTime = date.ToDateTime(TimeOnly.FromTimeSpan(schedule.TimeOfDay), DateTimeKind.Utc),
                        // Navigation rather than the FK: the schedule rows are
                        // still unsaved here and have no identity value yet.
                        Schedule = schedule,
                        CreatedAt = DateTime.UtcNow.AddMonths(-6)
                    });
                }
            }

            db.Classes.AddRange(classes);
            return classes;
        }

        /// <summary>
        /// Checks a varying slice of the roster into each class that has already
        /// happened, so attendance totals differ per member and per session.
        /// </summary>
        private int CreateAttendances(List<Class> classes, List<User> students)
        {
            var random = new Random(RandomSeed);
            var now = DateTime.UtcNow;
            var count = 0;

            foreach (var session in classes.Where(c => c.DateTime < now))
            {
                // Shuffling and taking a prefix keeps each member at most once
                // per class, which the unique (ClassId, UserId) index requires.
                var turnout = random.Next(students.Count / 4, students.Count / 2);
                var roster = students.OrderBy(_ => random.Next()).Take(turnout);

                foreach (var student in roster)
                {
                    db.Attendances.Add(new Attendance
                    {
                        Class = session,
                        UserId = student.Id,
                        CheckedInAt = session.DateTime.AddMinutes(-random.Next(1, 20))
                    });
                    count++;
                }
            }

            return count;
        }

        private void CreateProducts()
        {
            db.Products.AddRange(Products.Select(p => new Product
            {
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                Category = DemoProductCategory,
                CreatedDate = DateTime.UtcNow.AddMonths(-2)
            }));
        }
    }
}
