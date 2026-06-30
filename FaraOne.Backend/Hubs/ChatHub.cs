using FaraOne.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;
using FaraOne.Application.Context;

namespace FaraOne.Backend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();
        private static readonly ConcurrentDictionary<string, List<string>> _roomUsers = new();
        private readonly IDatabaseContext _dbContext;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger, IDatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private string GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string GetUsername()
        {
            return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            var username = GetUsername();

            if (userId != null)
            {
                _userConnections[userId] = Context.ConnectionId;

                var user = await _dbContext.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = true;
                    await _dbContext.SaveChangesAsync();
                }

                _logger.LogInformation($"User {username} (ID: {userId}) connected.");
                await Clients.All.SendAsync("UserOnline", userId, username);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            var username = GetUsername();

            if (userId != null)
            {
                _userConnections.TryRemove(userId, out _);

                var user = await _dbContext.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                }

                _logger.LogInformation($"User {username} (ID: {userId}) disconnected.");
                await Clients.All.SendAsync("UserOffline", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

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

            // لود کردن هیستوری پیام‌ها و ارسال به کاربرِ وارد شده
            var messages = await _dbContext.Messages
                .Where(m => m.ChatRoomId == roomId)
                .OrderBy(m => m.Timestamp)
                .Take(50)
                .ToListAsync();

            await Clients.Caller.SendAsync("ReceiveMessageHistory", messages);
            await Clients.Group(roomId).SendAsync("UserJoined", userId, username);

            _logger.LogInformation($"User {username} joined room {roomId}");
        }

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

            await _dbContext.Messages.AddAsync(message);
            await _dbContext.SaveChangesAsync();

            // پخش زنده پیام برای کل اعضای گروه
            await Clients.Group(roomId).SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                roomId = message.ChatRoomId,
                senderId =  message.SenderId,
                senderName = message.SenderName,
                content = message.Content,
                messageType = message.MessageType,
                timestamp = message.Timestamp,
                isRead = message.IsRead
            });

            _logger.LogInformation($"Message sent in room {roomId} by {username}");
        }

        public async Task UserTyping(string roomId, bool isTyping)
        {
            var userId = GetUserId();
            var username = GetUsername();

            await Clients.Group(roomId).SendAsync("UserTyping", userId, username, isTyping);
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }
    }
}