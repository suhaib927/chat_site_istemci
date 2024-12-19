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
    private readonly SocketService _socketService;

    public ChatController(DatabaseContext databaseContext, SocketService socketService)
    {
        _databaseContext = databaseContext;
        _socketService = socketService;
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
    public async Task<IActionResult> LoadChatAsync(string chatId)
    {
        if (Guid.TryParse(chatId, out Guid parsedChatId))
        {
            ChatViewModel model = new ChatViewModel
            {
                user = await _databaseContext.Users.SingleOrDefaultAsync(u => u.UserId == parsedChatId),
            };
            model.Messages = await _databaseContext.Messages.Where(m => m.SenderId == model.user.UserId && m.Status == true)
            .ToListAsync();
            if (model.user != null)
            {
                return PartialView("ChatDetails", model);
            }
            return NotFound();
        }

        return BadRequest("Invalid chat ID.");
    }

    // إرسال الرسالة للـ Server
    [HttpPost]
    public async Task<IActionResult> SendMessage(Message model)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var message = new Message
        {
            SenderId = userId,
            ReceiverId = model.ReceiverId,
            MessageContent = model.MessageContent,
            Type = model.Type,
            SentAt = DateTime.Now
        };
        // إرسال الرسالة إلى الخادم
        _socketService.SendMessageToServer(message);
        return RedirectToAction("LoadChat", new { chatId = model.ReceiverId.ToString() });

    }
}