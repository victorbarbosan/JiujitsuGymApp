using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiujitsuGymApp.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly AnnouncementService _announcementService;
        private readonly ICurrentUserService _currentUserService;

        public AnnouncementsController(AnnouncementService announcementService, ICurrentUserService currentUserService)
        {
            _announcementService = announcementService;
            _currentUserService = currentUserService;
        }

        // GET: Announcements/GetAnnouncements
        [HttpGet]
        public async Task<IActionResult> GetAnnouncements()
        {
            var userId = await _currentUserService.GetUserIdAsync();
            var announcements = await _announcementService.GetAnnouncementsAsync(userId);
            return Json(announcements);
        }

        // POST: Announcements/Create
        // Only Admin/Teacher can start a new announcement thread.
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = await _currentUserService.GetUserIdAsync();
            if (userId is null) return Unauthorized();

            var id = await _announcementService.CreateAnnouncementAsync(userId, dto.Content);
            return Ok(new { id });
        }

        // POST: Announcements/Reply/5
        // Any authenticated user (including students/Members) can reply.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, [FromBody] CreateAnnouncementReplyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = await _currentUserService.GetUserIdAsync();
            if (userId is null) return Unauthorized();

            var result = await _announcementService.CreateReplyAsync(id, userId, dto.Content);

            return result switch
            {
                ReplyResult.AnnouncementNotFound => NotFound(),
                ReplyResult.CannotReplyToReply => BadRequest(new { error = "Cannot reply to a reply." }),
                _ => Ok()
            };
        }

        // DELETE: Announcements/DeleteReply/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReply(int id)
        {
            var userId = await _currentUserService.GetUserIdAsync();
            if (userId is null) return Unauthorized();

            var result = await _announcementService.DeleteReplyAsync(id, userId);

            return result switch
            {
                DeleteReplyResult.NotFound => NotFound(),
                DeleteReplyResult.Forbidden => Forbid(),
                _ => Ok()
            };
        }

        // The Post-suffixed actions below back the server-rendered widget's plain
        // <form> submissions (full-page postback). They share the same service
        // calls as the JSON API above so authorization/business rules stay in one
        // place; only the request/response shape differs.

        // POST: Announcements/CreatePost
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(string content)
        {
            var userId = await _currentUserService.GetUserIdAsync();
            if (userId is null) return Unauthorized();

            if (!string.IsNullOrWhiteSpace(content))
                await _announcementService.CreateAnnouncementAsync(userId, content);

            return RedirectToAction("Index", "Home");
        }

        // POST: Announcements/ReplyPost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyPost(int id, string content)
        {
            var userId = await _currentUserService.GetUserIdAsync();
            if (userId is null) return Unauthorized();

            if (!string.IsNullOrWhiteSpace(content))
                await _announcementService.CreateReplyAsync(id, userId, content);

            return RedirectToAction("Index", "Home");
        }

        // POST: Announcements/DeleteReplyPost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReplyPost(int id)
        {
            var userId = await _currentUserService.GetUserIdAsync();
            if (userId is null) return Unauthorized();

            await _announcementService.DeleteReplyAsync(id, userId);

            return RedirectToAction("Index", "Home");
        }
    }
}
