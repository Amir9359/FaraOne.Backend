using System;
using System.Linq;
using FaraOne.Application.Context;
using Microsoft.EntityFrameworkCore;
using FaraOne.Application.Helpers;
using FaraOne.Domain.User;

namespace FaraOne.Application
{
    public class UserTokenRepository
    {
        private readonly IDatabaseContext _context;

        public UserTokenRepository(IDatabaseContext context)
        {
            _context = context;
        }


        public void SaveToken(UserToken token)
        {
            _context.UserTokens.Add(token);
            _context.SaveChanges();
        }

        public UserToken FindRefreshToken(string refreshToken)
        {
            var security = new SecurityHelper();
            var RefreshTokenHash = security.Getsha256Hash(refreshToken);
            var userToken = _context.UserTokens.Include(s => s.User)
                .SingleOrDefault(p => p.RefreshToken == RefreshTokenHash);
            return userToken;
        }

        public void DeleteToken(string RefreshToken)
        {
            var userToken = FindRefreshToken(RefreshToken);
            if (userToken != null)
            {
                _context.UserTokens.Remove(userToken);
                _context.SaveChanges();
            }
        }

        public bool CheckExistToken(string Token)
        {
            var security = new SecurityHelper();
            var TokenHash = security.Getsha256Hash(Token);
            // میتوان با ایدی ابتدا پیدا کردن که سربار نباشد پیدا کردن توکن های رشته ای .
            var UserToken = _context.UserTokens.FirstOrDefault(p => p.TokenHash == TokenHash);
            return UserToken == null ? false : true;
        }
    }
}