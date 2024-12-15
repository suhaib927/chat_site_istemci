 using Microsoft.AspNetCore.Mvc;
using chat_site_istemci.Services;
using chat_site_istemci.Models;
using Microsoft.AspNetCore.Authorization;
using chat_site_istemci.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;


public class ChatController : Controller

{
    private readonly DatabaseContext _databaseContext;

    public ChatController(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var model = await _databaseContext.Users
            .Where(user => user.UserId.ToString() != currentUserId)
            .ToListAsync();

        return View(model);
    }
    public IActionResult LoadChat(string chatId)
    {
        if (Guid.TryParse(chatId, out Guid parsedChatId))
        {
            ChatViewModel model = new ChatViewModel
            {
                user = _databaseContext.Users.SingleOrDefault(u => u.UserId == parsedChatId),
                Messages = _databaseContext.Messages.ToList()
            };
            if (model.user != null)
            {
                return PartialView("ChatDetails", model);
            }
            return NotFound();
        }

        return BadRequest("Invalid chat ID.");
    }
}