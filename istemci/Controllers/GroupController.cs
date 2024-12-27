using chat_site_istemci.Entities;
using chat_site_istemci.Models;
using chat_site_istemci.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace chat_site_istemci.Controllers
{
    public class GroupController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _configuration;
        private readonly SocketService _socketService;
        private Chats _chats;

        public GroupController(DatabaseContext databaseContext, IConfiguration configuration, SocketService socketService, Chats chats)
        {
            _databaseContext = databaseContext;
            _configuration = configuration;
            _socketService = socketService;
            _chats = chats;
        }



        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateGroupViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _databaseContext.Users.FindAsync(Guid.Parse(userId));
            if (ModelState.IsValid)
            {
                Group group = new Group
                {
                    GroupId = Guid.NewGuid(),
                    GroupName = model.GroupName,
                    GroupImageUrl = string.IsNullOrEmpty(model.GroupImageUrl) ? "/images/default_group.jpg" : model.GroupImageUrl,
                    CreatedAt = DateTime.Now,
                    MaxMembers = 50 
                };

                _databaseContext.Groups.Add(group);
                _databaseContext.SaveChanges();

                var newMember = new GroupMember
                {
                    GroupMemberId = Guid.NewGuid(),
                    GroupId = group.GroupId,
                    UserId = user.UserId
                };

                await _databaseContext.GroupMembers.AddAsync(newMember);
                await _databaseContext.SaveChangesAsync();

                return RedirectToAction("Index", "Chat");

            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GroupId(string groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "GroupId and UserId are required." });
            }

            try
            {
                var group = await _databaseContext.Groups.FindAsync(Guid.Parse(groupId));
                if (group == null)
                {
                    return NotFound(new { message = "Group not found." });
                }

                var user = await _databaseContext.Users.FindAsync(Guid.Parse(userId));
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var existingMember = await _databaseContext.GroupMembers
                    .FirstOrDefaultAsync(gm => gm.GroupId == group.GroupId && gm.UserId == user.UserId);

                if (existingMember != null)
                {
                    return Conflict(new { message = "User is already a member of the group." });
                }

                var newMember = new GroupMember
                {
                    GroupMemberId = Guid.NewGuid(),
                    GroupId = group.GroupId,
                    UserId = user.UserId
                };

                await _databaseContext.GroupMembers.AddAsync(newMember);
                await _databaseContext.SaveChangesAsync();

                return Json(new { GroupId = group.GroupId, GroupImageUrl = group.GroupImageUrl, GroupName = group.GroupName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

    }
}
