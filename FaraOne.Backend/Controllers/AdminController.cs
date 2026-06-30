using FaraOne.Domain;
using FaraOne.Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FaraOne.Application.Context;
using Microsoft.AspNetCore.SignalR;
using FaraOne.Backend.Hubs;

namespace FaraOne.Backend.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize] // یا [Authorize(Roles = "Admin")] جهت امنیت بیشتر
    public class AdminController : ControllerBase
    {
        private readonly IDatabaseContext _dbContext;
        private readonly IHubContext<ChatHub> _hubContext;

        public AdminController(IDatabaseContext dbContext, IHubContext<ChatHub> hubContext)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
        }

        // دریافت لیست تمامی تیکت‌ها برای ادمین
        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets()
        {
            var rooms = await _dbContext.ChatRooms
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var tickets = new List<object>();

            foreach (var r in rooms)
            {
                var messagesCount = await _dbContext.Messages
                    .CountAsync(m => m.ChatRoomId == r.RoomId);

                var lastMessage = await _dbContext.Messages
                    .Where(m => m.ChatRoomId == r.RoomId)
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefaultAsync();

                tickets.Add(new
                {
                    id = r.RoomId,
                    username = r.User?.Name ?? "کاربر مهمان",
                    userEmail = r.User?.Email ?? "guest@faraone.ir",
                    subject = lastMessage != null
                        ? (lastMessage.Content.Length > 50 ? lastMessage.Content.Substring(0, 50) + "..." : lastMessage.Content)
                        : "شروع گفتگو",
                    createdAt = r.CreatedAt,
                    status = r.Status, // "open" | "in-progress" | "closed"
                    messagesCount = messagesCount
                });
            }

            return Ok(tickets);
        }

        // تغییر وضعیت تیکت (باز، در حال بررسی، بسته شده)
        [HttpPost("tickets/{roomId}/status")]
        public async Task<IActionResult> ChangeStatus(string roomId, [FromBody] ChangeStatusRequest request)
        {
            var room = await _dbContext.ChatRooms
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
                return NotFound(new { error = "تیکت یافت نشد" });

            if (request.Status == "open" || request.Status == "in-progress" || request.Status == "closed")
            {
                if (request.Status == "in-progress")
                    request.Status = "pending";

                room.Status = request.Status;
                if (request.Status == "closed")
                {
                    room.ClosedAt = DateTime.Now;
                }

                await _dbContext.SaveChangesAsync();

                // ارسال پیام سیستمی به چت‌روم جهت اطلاع‌رسانی
                string statusText = request.Status == "open" ? "باز" : request.Status == "pending" ? "در حال بررسی" : "بسته شده";
                var systemMsg = new Message
                {
                    ChatRoomId = roomId,
                    SenderId = "0",
                    SenderName = "System",
                    Content = $"[وضعیت تیکت توسط مدیر به \"{statusText}\" تغییر یافت]",
                    MessageType = "system",
                    Timestamp = DateTime.UtcNow,
                    IsRead = true
                };

                await _dbContext.Messages.AddAsync(systemMsg);
                await _dbContext.SaveChangesAsync();

                // اطلاع‌رسانی زنده به کاربران حاضر در اتاق از طریق سیگنال‌آر
                await _hubContext.Clients.Group(roomId).SendAsync("ReceiveMessage", new
                {
                    id = systemMsg.Id,
                    roomId = systemMsg.ChatRoomId,
                    senderId = 0,
                    senderName = systemMsg.SenderName,
                    content = systemMsg.Content,
                    messageType = systemMsg.MessageType,
                    timestamp = systemMsg.Timestamp,
                    isRead = systemMsg.IsRead
                });

                return Ok(new { success = true });
            }

            return BadRequest(new { error = "وضعیت نامعتبر" });
        }

        // ارسال پاسخ ادمین از طریق API و همگام‌سازی لحظه‌ای با چت‌روم
        [HttpPost("tickets/{roomId}/reply")]
        public async Task<IActionResult> Reply(string roomId, [FromBody] ReplyRequest request)
        {
            var room = await _dbContext.ChatRooms
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
                return NotFound(new { error = "تیکت یافت نشد" });

            var message = new Message
            {
                ChatRoomId = roomId,
                SenderId = request.SenderId.ToString() ?? "1",
                SenderName = request.SenderName ?? "مدیر سیستم",
                Content = request.Content,
                MessageType = "text",
                Timestamp = DateTime.UtcNow,
                IsRead = true
            };

            await _dbContext.Messages.AddAsync(message);

            // باز کردن مجدد تیکت در صورتی که بسته شده بود
            if (room.Status == "closed")
            {
                room.Status = "open";
            }

            await _dbContext.SaveChangesAsync();

            // پخش زنده پیام ادمین به چت‌روم از طریق SignalR
            await _hubContext.Clients.Group(roomId).SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                roomId = message.ChatRoomId,
                senderId = int.Parse(message.SenderId),
                senderName = message.SenderName,
                content = message.Content,
                messageType = message.MessageType,
                timestamp = message.Timestamp,
                isRead = message.IsRead
            });

            return Ok(new { success = true, message });
        }
    }

    public class ChangeStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class ReplyRequest
    {
        public string Content { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public int SenderId { get; set; } = 1;
    }
}