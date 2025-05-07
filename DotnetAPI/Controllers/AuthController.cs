using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
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
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = @Email";
                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists, new { Email = userForRegistration.Email });
                if (existingUsers.Count() == 0)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }
                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

                    string sqlAddAuth = @"
                        INSERT INTO TutorialAppSchema.Auth (
                            Email,
                            PasswordHash,
                            PasswordSalt
                        )
                        VALUES (
                            @Email,
                            @PasswordHash,
                            @PasswordSalt
                        )";

                    List<SqlParameter> parameters = new List<SqlParameter>();

                    parameters.Add(new SqlParameter("@Email", userForRegistration.Email));

                    SqlParameter passwordHashParam = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParam.Value = passwordHash;
                    parameters.Add(passwordHashParam);

                    SqlParameter passwordSaltParam = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParam.Value = passwordSalt;
                    parameters.Add(passwordSaltParam);

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, parameters))
                    {
                        string sqlAddUser = @"
                            INSERT INTO TutorialAppSchema.Users(
                                [FirstName],
                                [LastName],
                                [Email],
                                [Gender],
                                [Active]
                            )
                            VALUES (
                                @FirstName,
                                @LastName,
                                @Email,
                                @Gender,
                                @Active
                            )";
                        var parametersUser = new
                        {
                            userForRegistration.FirstName,
                            userForRegistration.LastName,
                            userForRegistration.Email,
                            userForRegistration.Gender,
                            Active = 1

                        };
                        if (_dapper.ExecuteSql(sqlAddUser, parametersUser))
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
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"
                SELECT PasswordHash, PasswordSalt
                FROM TutorialAppSchema.Auth
                WHERE Email = @Email";


            UserForLoginConfirmationDto userForLoginConfirmation = _dapper.
                LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt, new
                {
                    Email = userForLogin.Email
                });

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

            int userId = _dapper.LoadDataSingle<int>(userIdSql, new
            {
                Email = userForLogin.Email
            });


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