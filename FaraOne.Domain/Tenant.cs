using FaraOne.Domain.Entities.Attribute;
using System.ComponentModel.DataAnnotations;

namespace FaraOne.Domain;

[Audtable]
public class Tenant
{
    public int Id { get; set; }
     
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Subdomain { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Logo { get; set; }
    public string PrimaryColor { get; set; } = "#0055ff"; 
}