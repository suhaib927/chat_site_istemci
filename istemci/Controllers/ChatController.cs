using Microsoft.AspNetCore.Mvc;
using chat_site_istemci.Services;
using chat_site_istemci.Models;
using Microsoft.AspNetCore.Authorization;
using chat_site_istemci.Entities;
using Microsoft.EntityFrameworkCore;
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
        _chats = chats;
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
            groups = await _databaseContext.Groups
            .Where(group => _databaseContext.GroupMembers
            .Any(member => member.GroupId == group.GroupId && member.UserId.ToString() == currentUserId))
            .ToListAsync()

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
                group = await _databaseContext.Groups.SingleOrDefaultAsync(u => u.GroupId == parsedChatId),
                myId = Guid.Parse(currentUserId),
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
            if (model.user != null || model.group != null)
            {
                return PartialView("ChatDetails", model);
            }
            return NotFound();
        }

        return BadRequest("Invalid chat ID.");
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(string MessageContent, Guid ReceiverId, string Type, Guid GroupId, Guid MyId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Guid receiverId;
        if (GroupId != Guid.Parse("00000000-0000-0000-0000-000000000000"))
        {
            receiverId = GroupId;

        }
        else
        {
            receiverId = ReceiverId;
        }

        var sender = await _databaseContext.Users.SingleOrDefaultAsync(u => u.UserId == Guid.Parse(userId));
        var receiver = await _databaseContext.Users.SingleOrDefaultAsync(u => u.UserId == ReceiverId);
        var group = await _databaseContext.Groups.SingleOrDefaultAsync(u => u.GroupId == GroupId);

        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            SenderId = userId,
            Sender = sender,
            ReceiverId = ReceiverId.ToString(),
            GroupId = GroupId.ToString(),
            MessageContent = MessageContent,
            Type = Type,
            SentAt = DateTime.Now,
            Status = false
        };

        var existingChat = _chats.chats.FirstOrDefault(c => c.ChatKey == receiverId.ToString());
        if (existingChat != null)
        {
            existingChat.Messages.Add(message);
        }
        else
        {
            var newChat = new Chat
            {
                ChatKey = receiverId.ToString(),
                Messages = new List<Message> { message }
            };
            _chats.chats.Add(newChat);
        }

        try
        {
            _socketService.SendMessageToServer(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to server: {ex.Message}");
            return StatusCode(500, "Failed to send message to server.");
        }

        return Json(new { message = message, user = receiver, group = group, sentAt = message.SentAt.ToString(), myId = MyId });
    }

    
}