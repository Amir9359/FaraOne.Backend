using FaraOne.Domain.User;
using System;
using System.Linq;
using FaraOne.Application.Context;

namespace FaraOne.Application
{
    public class UserRepository
    {
        private readonly IDatabaseContext _context;

        public UserRepository(IDatabaseContext context)
        {
            _context = context;
        }


        public User GetUser(string Id)
        {
            var user = _context.Users.SingleOrDefault(s => s.Id == Id);
            return user;
        }

        public bool ValidUser(string userName, string Pass)
        {
            var user = _context.Users.FirstOrDefault();
            return user != null ? true : false;
        }

        public string GetCode(string PhoneNumber)
        {
            var random = new Random();
            var Code =  random.Next(1000, 9999).ToString();
            var smsCode = new SmsCode()
            {
                Code = Code,
                SendDate = DateTime.Now,
                Phone = PhoneNumber,
                RequestCount = 0,
                Used = false
            };
            _context.SmsCodes.Add(smsCode);
            _context.SaveChanges();

            return Code;
        }

        public LoginDto Login(string Phone, string SmsCode)
        {
            var Sms = _context.SmsCodes
                .OrderByDescending(s => s.Id)
                .FirstOrDefault(s => s.Phone == Phone && s.Code == SmsCode);

            if (Sms == null)
            {
                return new LoginDto()
                {
                    IsSuccess = false,
                    Message = "کلمه عبور شما اشتباه است !",
                };
            }
            else
            {
                if (Sms.Used )
                {
                    return new LoginDto()
                    {
                        IsSuccess = false,
                        Message = "کد منقضی شده است !  لطفا کد جدید بگیرید.",
                    };
                }
                //چک کردن زمان ارسال که از یک تایم بیشتر نباشد .
                Sms.RequestCount++;
                Sms.Used = true;
                _context.SaveChanges();

                var user = FindUserByPhone(Phone);

                if (user != null)
                {
                    return new LoginDto()
                    {
                        IsSuccess = true,
                        User = user
                    };
                }
                else
                {
                    user =  RegisterNewUser(Phone);
                    return new LoginDto()
                    {
                        IsSuccess = true,
                        User = user
                    };
                }
            }
        }

        private User RegisterNewUser(string Phone)
        {
            var user = new User()
            {
                PhoneNumber = Phone,
                UserName = Phone, 
                IsActive = true
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public User? FindUserByPhone(string Phone)
        {
            var user = _context.Users.SingleOrDefault(s => s.PhoneNumber == Phone);
            return user ?? null;
        }

        public void Logout(Guid userId)
        {
            var userToken = _context.UserTokens.Where(s => s.UserId == userId).ToList();
            _context.UserTokens.RemoveRange(userToken);
            _context.SaveChanges();
        }
        public class LoginDto
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public User User { get; set; }
        }
    }
}