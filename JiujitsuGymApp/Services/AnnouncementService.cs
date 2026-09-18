using JiujitsuGymApp.Data;
using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Services
{
    public enum ReplyResult
    {
        Success,
        AnnouncementNotFound,
        CannotReplyToReply
    }

    public enum DeleteReplyResult
    {
        Success,
        NotFound,
        Forbidden
    }

    /// <summary>
    /// Encapsulates the Announcements business rules: creating root announcements is
    /// restricted to Admin/Teacher at the controller (via [Authorize(Roles=...)]), while
    /// reply-nesting and delete-ownership checks live here since they depend on data,
    /// not just the caller's role.
    /// </summary>
    public class AnnouncementService(ApplicationDbContext db, UserManager<User> userManager)
    {
        public async Task<List<AnnouncementDto>> GetAnnouncementsAsync(string? currentUserId)
        {
            var isAdmin = currentUserId != null && await IsInRoleAsync(currentUserId, "Admin");

            var announcements = await db.Announcements
                .AsNoTracking()
                .Include(a => a.Author)
                .Include(a => a.Replies.Where(r => r.DeletedAt == null))
                    .ThenInclude(r => r.Author)
                .Where(a => a.ParentId == null && a.DeletedAt == null)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return announcements.Select(a => new AnnouncementDto
            {
                Id = a.Id,
                AuthorName = a.Author.Name,
                Content = a.Content,
                CreatedAt = a.CreatedAt,
                Replies = a.Replies
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => new AnnouncementReplyDto
                    {
                        Id = r.Id,
                        AuthorName = r.Author.Name,
                        Content = r.Content,
                        CreatedAt = r.CreatedAt,
                        CanDelete = isAdmin || (currentUserId != null && currentUserId == a.AuthorId)
                    })
                    .ToList()
            }).ToList();
        }

        /// <summary>Creates a root announcement. Caller must already be Admin/Teacher (enforced by controller authorization).</summary>
        public async Task<int> CreateAnnouncementAsync(string authorId, string content)
        {
            var announcement = new Announcement
            {
                AuthorId = authorId,
                Content = content
            };

            db.Announcements.Add(announcement);
            await db.SaveChangesAsync();
            return announcement.Id;
        }

        /// <summary>Any authenticated user can reply, but only to a root announcement (no nested replies).</summary>
        public async Task<ReplyResult> CreateReplyAsync(int announcementId, string authorId, string content)
        {
            var parent = await db.Announcements
                .FirstOrDefaultAsync(a => a.Id == announcementId && a.DeletedAt == null);

            if (parent is null) return ReplyResult.AnnouncementNotFound;
            if (parent.ParentId != null) return ReplyResult.CannotReplyToReply;

            db.Announcements.Add(new Announcement
            {
                AuthorId = authorId,
                ParentId = parent.Id,
                Content = content
            });

            await db.SaveChangesAsync();
            return ReplyResult.Success;
        }

        /// <summary>Admin can delete any reply; a Teacher can delete replies on announcements they authored.</summary>
        public async Task<DeleteReplyResult> DeleteReplyAsync(int replyId, string currentUserId)
        {
            var reply = await db.Announcements
                .Include(r => r.Parent)
                .FirstOrDefaultAsync(r => r.Id == replyId && r.DeletedAt == null && r.ParentId != null);

            if (reply is null) return DeleteReplyResult.NotFound;

            var isAdmin = await IsInRoleAsync(currentUserId, "Admin");
            var isOwningTeacher = reply.Parent!.AuthorId == currentUserId;

            if (!isAdmin && !isOwningTeacher) return DeleteReplyResult.Forbidden;

            reply.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return DeleteReplyResult.Success;
        }

        private async Task<bool> IsInRoleAsync(string userId, string role)
        {
            var user = await userManager.FindByIdAsync(userId);
            return user != null && await userManager.IsInRoleAsync(user, role);
        }
    }
}
