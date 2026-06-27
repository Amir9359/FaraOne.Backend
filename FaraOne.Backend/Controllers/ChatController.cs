using FaraOne.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FaraOne.Application.Context;

namespace FaraOneBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IDatabaseContext _dbContext;

    public ChatController(IDatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    private string GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim;
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetUserRooms()
    {
        var userId = GetUserId();

        var rooms = await _dbContext.ChatRooms
            .Where(r => r.UserId == userId  )
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.RoomId,
                r.Status,
                r.CreatedAt,
                r.ClosedAt,
                LastMessage = _dbContext.Messages
                    .Where(m => m.ChatRoomId == r.RoomId)
                    .OrderByDescending(m => m.Timestamp)
                    .Select(m => new { m.Content, m.Timestamp })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(rooms);
    }

    [HttpGet("messages/{roomId}")]
    public async Task<IActionResult> GetMessages(string roomId, [FromQuery] int take = 50)
    {
        var messages = await _dbContext.Messages
            .Where(m => m.ChatRoomId == roomId)
            .OrderByDescending(m => m.Timestamp)
            .Take(take)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return Ok(messages);
    }

    [HttpPost("rooms/create")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        var userId = GetUserId();
        var roomId = Guid.NewGuid().ToString();

        var chatRoom = new ChatRoom
        {
            RoomId = roomId,
            TenantId = request.TenantId,
            UserId = userId,
            AgentId = request.AgentId,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.ChatRooms.AddAsync(chatRoom);
        await _dbContext.SaveChangesAsync();

        return Ok(new { roomId });
    }

    [HttpPost("rooms/{roomId}/close")]
    public async Task<IActionResult> CloseRoom(string roomId)
    {
        var chatRoom = await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.RoomId == roomId);

        if (chatRoom == null)
            return NotFound();

        chatRoom.Status = "closed";
        chatRoom.ClosedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}

public class CreateRoomRequest
{
    public int TenantId { get; set; }
    public int? AgentId { get; set; }
}