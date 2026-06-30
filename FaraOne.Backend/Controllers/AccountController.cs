using FaraOne.Application;
using FaraOne.Application.Context;
using FaraOne.Application.Dto;
using FaraOne.Application.Helpers;
using FaraOne.Domain.Entities;
using FaraOne.Domain.User;
using FaraOne.Infraustructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FaraOne.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserRepository _userRepository;
        private readonly UserTokenRepository _userTokenRepository; 
        private readonly AuthService _authService;

        public AccountController(IConfiguration configuration,
            UserTokenRepository userTokenRepository, UserRepository userRepository, AuthService authService)
        {
            _configuration = configuration;
            _userTokenRepository = userTokenRepository;
            _userRepository = userRepository;
            _authService = authService;
        }


        [Route("GetSmsCode")]
        [HttpGet]
        public IActionResult SmsCode(string PhoneNume)
        {
            var smsCode = _userRepository.GetCode(PhoneNume);
            //Send Code to Phone
            return Ok(smsCode);
        }

        [HttpPost]
        public IActionResult Post([FromBody] RequestLoginDto dto)
        {
            var LoginResult = _userRepository.Login(dto.phone, dto.Code);

            if (LoginResult.IsSuccess == false)
            {
                return Ok(new LoginResultDto()
                {
                    IsSucces = false,
                    Message = LoginResult.Message
                });
            }

            var token = CreateToken(LoginResult.User);

            return Ok(new LoginResultDto()
            {
                IsSucces = true,
                Data = token
            });
        }

        [HttpPost]
        [Route("RefreshToken")]
        public IActionResult RefreshToken(string refreshToken)
        {
            var userToken = _userTokenRepository.FindRefreshToken(refreshToken);
            if (userToken == null)
            {
                return Unauthorized();
            }

            if (userToken.RefreshTokenExp < DateTime.Now)
            {
                return Unauthorized("Token is Expired");
            }

            var token = CreateToken(userToken.User);
            _userTokenRepository.DeleteToken(refreshToken);

            return Ok(token);
        }

        [Authorize]
        [Route("LogOut")]
        [HttpPost]
        public IActionResult LogOut()
        {
            var UserId = User.Claims.First(p => p.Type == "UserId").Value;
            _userRepository.Logout(Guid.Parse(UserId));
            return Ok();
        }
        private LoginDataDto CreateToken(User user)
        {

            var security = new SecurityHelper();
            var expire = DateTime.Now.AddMinutes(int.Parse(_configuration["JwtConfigs:expires"]));
            var claims = new List<Claim>()
                {
                    new Claim(type: "UserId", value: user.Id.ToString()),
                    new Claim(type: "FullName", value: user?.Name?? "")
                };
            var key = _configuration["JwtConfigs:key"];
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credential = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer: _configuration["JwtConfigs:issuer"],
                audience: _configuration["JwtConfigs:audience"],
                expires: expire, notBefore: DateTime.Now,
                claims: claims,
                signingCredentials: credential);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = Guid.NewGuid();
            _userTokenRepository.SaveToken(new UserToken()
            {
                Mobile = "Samsung A8",
                TokenExpire = expire,
                TokenHash = security.Getsha256Hash(jwtToken),
                RefreshToken = security.Getsha256Hash(refreshToken.ToString()),
                RefreshTokenExp = DateTime.Now.AddDays(30),
                User = user
            });


            return new LoginDataDto()
            {
                Token = jwtToken,
                RefreshToken = refreshToken.ToString()
            };
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _authService.Authenticate(request.Username, request.Password);

            if (user == null)
                return Unauthorized(new { message = "Username or password is incorrect" });

            var token = _authService.GenerateJwtToken(user);

            return Ok(new
            {
                Token = token,
                User = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.Name,
                    user.Role,
                    user.Avatar
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.Register(request.Username, request.Email, request.Password, request.Password);
                var token = _authService.GenerateJwtToken(user);

                return Ok(new
                {
                    Token = token,
                    User = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.Name,
                        user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}