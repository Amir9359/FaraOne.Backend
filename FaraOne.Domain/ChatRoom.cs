using FaraOne.Domain.Entities.Attribute;
using System.ComponentModel.DataAnnotations;

namespace FaraOne.Domain;

[Audtable]
public class ChatRoom
{
    public int Id { get; set; }
    [MaxLength(100)]

    public string RoomId { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public string UserId { get; set; }
    public User.User User { get; set; }
    public int? AgentId { get; set; }

    [MaxLength(10)]
    public string Status { get; set; } = "active"; // active, closed, pending
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ClosedAt { get; set; }
}