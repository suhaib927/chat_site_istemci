 using Microsoft.AspNetCore.Mvc;
using chat_site_istemci.Services;
using chat_site_istemci.Models;
using Microsoft.AspNetCore.Authorization;
using chat_site_istemci.Entities;
using Microsoft.EntityFrameworkCore;


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
        var model = await _databaseContext.Users.ToListAsync();
        return View(model);
    }
    [Authorize]
    public IActionResult LoadChat(int id)
    {
        //var chat = _chatService.GetChatById(id);
        return PartialView("ChatDetails" /*,chat*/);
    }
}