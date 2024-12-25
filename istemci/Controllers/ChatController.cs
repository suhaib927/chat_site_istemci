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
    private Chats _chats;

    public ChatController(DatabaseContext databaseContext, SocketService socketService, Chats chats)
    {
        _databaseContext = databaseContext;
        _socketService = socketService;
        _chats  = chats;
    }
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        ChatsListViewModel model = new ChatsListViewModel
        {
            users = await _databaseContext.Users
            .Where(user => user.UserId.ToString() != currentUserId)
            .ToListAsync(),
            groups = await _databaseContext.Groups.ToListAsync()

        };


        return View(model);
    }
    public async Task<IActionResult> LoadChatAsync(string chatId)
    {
        if (Guid.TryParse(chatId, out Guid parsedChatId))
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ChatViewModel model = new ChatViewModel
            {
                user = await _databaseContext.Users.SingleOrDefaultAsync(u => u.UserId == parsedChatId),
            };

            var existingChat = _chats.chats.FirstOrDefault(c => c.ChatKey == chatId);
            if (existingChat != null)
            {
                model.Messages = existingChat.Messages;
            }
            else
            {
                var newChat = new Chat
                {
                    ChatKey = chatId,
                };
                _chats.chats.Add(newChat);
            }
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
    public async Task<IActionResult> SendMessage(string MessageContent, Guid ReceiverId, string Type)
    {


        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            SenderId = userId,
            Sender = await _databaseContext.Users.SingleOrDefaultAsync(u => u.UserId == userId),
            ReceiverId = ReceiverId,
            Receiver = await _databaseContext.Users.SingleOrDefaultAsync(u => u.UserId == ReceiverId),
            MessageContent = MessageContent,
            Type = Type,
            SentAt = DateTime.Now,
            Status = false
        };

        var existingChat = _chats.chats.FirstOrDefault(c => c.ChatKey == ReceiverId.ToString());
        if (existingChat != null)
        {
            existingChat.Messages.Add(message);
        }

        
        // إرسال الرسالة إلى الخادم
        _socketService.SendMessageToServer(message);
        var user = await _databaseContext.Users.SingleOrDefaultAsync(u => u.UserId == ReceiverId);
        return Json(new { message = message, user = user, sentAt = message.SentAt.ToString() });
    }
}