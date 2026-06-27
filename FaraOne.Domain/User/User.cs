using System;
using System.Collections.Generic;
using FaraOne.Domain.Entities;
using FaraOne.Domain.Entities.Attribute;
using Microsoft.AspNetCore.Identity;

namespace FaraOne.Domain.User;

[Audtable]
public class User : IdentityUser
{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<UserToken> UserTokens { get; set; }
      
    public string? Avatar { get; set; }
    public string Role { get; set; } = "User"; // User, Admin, Agent
    public int? TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
}
