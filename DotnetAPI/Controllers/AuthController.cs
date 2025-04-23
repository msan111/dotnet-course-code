using System.Data;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

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
                    byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);

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
                        if(_dapper.ExecuteSql(sqlAddUser, parametersUser))
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

            byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForLoginConfirmation.PasswordSalt);

            for (int i = 0; i < passwordHash.Length; i++)
            {
                if (passwordHash[i] != userForLoginConfirmation.PasswordHash[i])
                {
                    return Unauthorized("Invalid password");
                }
            }

            return Ok();
        }
        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {

            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                Convert.ToBase64String(passwordSalt);

            byte[] passwordHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            );
            return passwordHash;
        }



    }
}