using Microsoft.AspNetCore.Mvc;
using chat_site_istemci.Services;
using chat_site_istemci.Models;


public class ChatController : Controller
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    public IActionResult Index()
    {
        var chats = _chatService.GetAllChats();
        var model = new ChatViewModel { Chats = chats };
        return View(model);
    }

    public IActionResult LoadChat(int id)
    {
        var chat = _chatService.GetChatById(id);
        return PartialView("ChatDetails", chat);
    }
}