using System;
using System.ComponentModel.DataAnnotations;

namespace FaraOne.Domain.User;

public class SmsCode
{
    public int Id { get; set; }
    [MaxLength(15)]
    public string Phone { get; set; }

    [MaxLength(10)]
    public string Code { get; set; }
    public bool Used { get; set; }
    public int RequestCount { get; set; }
    public DateTime SendDate { get; set; }
}