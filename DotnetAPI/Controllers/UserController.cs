using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    DataContextDapper _dapper;
    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        string sql = @" 
            SELECT [UserId],
                [FirstName],
                [LastName],
                [Email],
                [Gender],
            [Active] FROM TutorialAppSchema.Users";
        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        Console.WriteLine(users);
        return users;

    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        string sql = @" 
            SELECT [UserId],
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active] 
            FROM TutorialAppSchema.Users
                WHERE UserId = @userId";
        User user = _dapper.LoadDataSingle<User>(sql, new { UserId = userId });
        Console.WriteLine(user);
        return user;

    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        string sql = @"
            UPDATE TutorialAppSchema.Users
                SET [FirstName] = @FirstName,
                    [LastName] = @LastName,
                    [Email] = @Email,
                    [Gender] = @Gender,
                    [Active] = @Active
                WHERE UserId = @userId";

        var parameters = new
        {
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Gender,
            user.Active
        };
        if (_dapper.ExecuteSql(sql, parameters)) return Ok();
        throw new Exception("Failed to update user");

    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        string sql = @"
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

        var parameters = new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            user.Gender,
            user.Active
        };
        if (_dapper.ExecuteSql(sql, parameters)) return Ok();
        throw new Exception("Failed to Add user");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"
            DELETE FROM TutorialAppSchema.Users
                WHERE UserId = @UserId";

        var parameters = new { UserId = userId};
        if (_dapper.ExecuteSql(sql, parameters)) return Ok();
        throw new Exception("Failed to delete user");
    
    }

}
