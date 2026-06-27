using System;
using System.ComponentModel.DataAnnotations;

namespace FaraOne.Domain.User;

public class UserToken
{
    public int Id { get; set; }

    [MaxLength(150)]
    public string TokenHash { get; set; }
    public DateTime TokenExpire { get; set; }

    [MaxLength(150)]
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExp { get; set; }

    [MaxLength(15)]
    public string Mobile { get; set; }
    public User User { get; set; }
    public Guid UserId { get; set; }
}
