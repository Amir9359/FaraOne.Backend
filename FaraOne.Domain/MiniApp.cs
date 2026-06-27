using FaraOne.Domain.Entities.Attribute;
using System.ComponentModel.DataAnnotations;

namespace FaraOne.Domain;

[Audtable]
public class MiniApp
{
    public int Id { get; set; }

    [MaxLength(150)]
    public string AppId { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(800)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Icon { get; set; } = string.Empty;

    [MaxLength(800)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Version { get; set; } = "1.0.0";

    public string Permissions { get; set; } 
    public bool IsActive { get; set; } = true;
    public int? TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}