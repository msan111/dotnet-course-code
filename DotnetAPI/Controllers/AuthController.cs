using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;

        private readonly ReusableSql _reusableSql;
        private readonly IMapper _mapper;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
            _reusableSql = new ReusableSql(config);
            _mapper = new Mapper(new MapperConfiguration(cfg => 
            {
                cfg.CreateMap<UserForRegistrationDto, UserComplete>();

            }));
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = @Email";
                IEnumerable<string> existingUsers = _dapper.LoadDataWithParameters<string>(sqlCheckUserExists, new { Email = userForRegistration.Email });
                if (existingUsers.Count() == 0)
                {
                    
                    UserForLoginDto userForSetPassword = new UserForLoginDto()
                    {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password
                    };

                    if (_authHelper.SetPassword(userForSetPassword))
                    {
                        UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistration);
                        userComplete.Active = true;

                        if (_reusableSql.UpsertUser(userComplete))
                        {
                            return Ok("User created");
                        }
                        else
                        {
                            return BadRequest("User creation failed");
                        }

                    }
                    else
                    {
                        return BadRequest("Failed to register user");
                    }

                }
                else
                {
                    return BadRequest("User already exists");
                }
            }
            throw new Exception("Passwords do not match");
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userForSetPassword)
        {
            if (_authHelper.SetPassword(userForSetPassword))
            {
                return Ok("Password reset");
            }
            return BadRequest("Failed to reset password");
            
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"EXEC TutorialAppSchema.spLoginConfirmation_Get
                @Email = @Email";

            DynamicParameters parameter = new DynamicParameters();
            parameter.Add("Email", userForLogin.Email, DbType.String);

            UserForLoginConfirmationDto userForLoginConfirmation = _dapper.
                LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt,parameter);

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForLoginConfirmation.PasswordSalt);

            for (int i = 0; i < passwordHash.Length; i++)
            {
                if (passwordHash[i] != userForLoginConfirmation.PasswordHash[i])
                {
                    return Unauthorized("Invalid password");
                }
            }

            string userIdSql = @"
                SELECT UserId 
                FROM TutorialAppSchema.Users
                WHERE Email = @Email
            ";

            DynamicParameters secondParameter = new DynamicParameters();
            secondParameter.Add("Email", userForLogin.Email, DbType.String);


            int userId = _dapper.LoadDataSingle<int>(userIdSql, secondParameter);

            return Ok(new Dictionary<string, string>
            {
                {"token", _authHelper.CreateToken(userId) }
            });
        }

        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            string userId = User.FindFirst("userId")?.Value + "";

            string userIdSql = @"
                SELECT UserId
                FROM TutorialAppSchema.Users
                WHERE UserId = @userId
            ";
            int userIdFromDB = _dapper.LoadDataSingle<int>(userIdSql, new { userId = userId });

            return Ok(new Dictionary<string, string>
            {
                {"token", _authHelper.CreateToken(userIdFromDB) }
            });
        }

    }
}