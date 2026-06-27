using FaraOne.Domain;  
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;
using FaraOne.Application.Context;

namespace FaraOne.Backend.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> _userConnections = new();
    private static readonly ConcurrentDictionary<string, List<string>> _roomUsers = new();
    private readonly IDatabaseContext _dbContext;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(  ILogger<ChatHub> logger, IDatabaseContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    // دریافت userId از Claim
    private string GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim;
    }

    private string GetUsername()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    // اتصال کاربر به هاب
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var username = GetUsername();

        if (userId != null)
        {
            _userConnections[userId.ToString()] = Context.ConnectionId;

            // به‌روزرسانی وضعیت آنلاین کاربر
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = true;
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation($"User {username} (ID: {userId}) connected with ConnectionId: {Context.ConnectionId}");

            // اطلاع رسانی به سایر کاربران
            await Clients.All.SendAsync("UserOnline", userId, username);
        }

        await base.OnConnectedAsync();
    }

    // قطع اتصال کاربر
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var username = GetUsername();

        if (userId != null)
        {
            _userConnections.TryRemove(userId.ToString(), out _);

            // به‌روزرسانی وضعیت آفلاین کاربر
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = false;
                user.LastSeen = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation($"User {username} (ID: {userId}) disconnected");

            // اطلاع رسانی به سایر کاربران
            await Clients.All.SendAsync("UserOffline", userId, username);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // پیوستن به اتاق چت
    public async Task JoinRoom(string roomId)
    {
        var userId = GetUserId();
        var username = GetUsername();

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        if (!_roomUsers.ContainsKey(roomId))
        {
            _roomUsers[roomId] = new List<string>();
        }

        if (!_roomUsers[roomId].Contains(Context.ConnectionId))
        {
            _roomUsers[roomId].Add(Context.ConnectionId);
        }

        // دریافت تاریخچه پیام‌ها
        var messages = await _dbContext.Messages
            .Where(m => m.ChatRoomId == roomId)
            .OrderBy(m => m.Timestamp)
            .Take(50)
            .ToListAsync();

        // ارسال تاریخچه به کاربر جدید
        await Clients.Caller.SendAsync("ReceiveMessageHistory", messages);

        // اطلاع رسانی به دیگران
        await Clients.Group(roomId).SendAsync("UserJoined", userId, username);

        _logger.LogInformation($"User {username} joined room {roomId}");
    }

    // خروج از اتاق چت
    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        if (_roomUsers.ContainsKey(roomId))
        {
            _roomUsers[roomId].Remove(Context.ConnectionId);
        }

        var userId = GetUserId();
        var username = GetUsername();

        await Clients.Group(roomId).SendAsync("UserLeft", userId, username);

        _logger.LogInformation($"User {username} left room {roomId}");
    }

    // ارسال پیام
    public async Task SendMessage(string roomId, string content, string messageType = "text")
    {
        var userId = GetUserId();
        var username = GetUsername();
 
        var message = new Message
        {
            ChatRoomId = roomId,
            SenderId = userId,
            SenderName = username,
            Content = content,
            MessageType = messageType,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        // ذخیره در دیتابیس
        await _dbContext.Messages.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        // ارسال پیام به همه اعضای اتاق
        await Clients.Group(roomId).SendAsync("ReceiveMessage", new
        {
            Id = message.Id,
            RoomId = message.ChatRoomId,
            SenderId = message.SenderId,
            SenderName = message.SenderName,
            Content = message.Content,
            MessageType = message.MessageType,
            Timestamp = message.Timestamp,
            IsRead = message.IsRead
        });

        _logger.LogInformation($"Message sent in room {roomId} by {username}");
    }

    // تایید خوانده شدن پیام
    public async Task MarkAsRead(int messageId)
    {
        var message = await _dbContext.Messages.FindAsync(messageId);
        if (message != null && !message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            await Clients.Group(message.ChatRoomId).SendAsync("MessageRead", messageId, GetUserId());
        }
    }

    // نشان دادن تایپ کردن
    public async Task UserTyping(string roomId, bool isTyping)
    {
        var userId = GetUserId();
        var username = GetUsername();

        await Clients.Group(roomId).SendAsync("UserTyping", userId, username, isTyping);
    }

    // ایجاد اتاق چت جدید
    public async Task<string> CreateRoom(int tenantId, int? agentId = null)
    {
        var userId = GetUserId();
        var roomId = Guid.NewGuid().ToString();

        var chatRoom = new ChatRoom
        {
            RoomId = roomId,
            TenantId = tenantId,
            UserId = userId,
            AgentId = agentId,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.ChatRooms.AddAsync(chatRoom);
        await _dbContext.SaveChangesAsync();

        await JoinRoom(roomId);

        // ارسال پیام خوش‌آمدگویی سیستمی
        await SendMessage(roomId, "به پشتیبانی FaraOne خوش آمدید! چگونه می‌توانیم به شما کمک کنیم؟", "system");

        return roomId;
    }

    // بستن اتاق چت
    public async Task CloseRoom(string roomId)
    {
        var chatRoom = await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.RoomId == roomId);
        if (chatRoom != null)
        {
            chatRoom.Status = "closed";
            chatRoom.ClosedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            await Clients.Group(roomId).SendAsync("RoomClosed", roomId);
            await LeaveRoom(roomId);
        }
    }

    // دریافت لیست کاربران آنلاین
    public async Task<List<object>> GetOnlineUsers()
    {
        var onlineUsers = new List<object>();

        foreach (var connection in _userConnections)
        {
            var userId = int.Parse(connection.Key);
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null && user.IsOnline)
            {
                onlineUsers.Add(new
                {
                    Id = user.Id,
                    Username = user.UserName,
                    FullName = user.Name,
                    Avatar = user.Avatar
                });
            }
        }

        return onlineUsers;
    }

    // پینگ برای بررسی اتصال
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
    }
}