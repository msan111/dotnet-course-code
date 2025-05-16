using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    DataContextDapper _dapper;
    public UserCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("GetUsers/{userId}/{isActive}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        var paramList = new Dictionary<string, object>();

        if (userId != 0)
        {
            sql += " @UserId = @userId";
            paramList["UserId"] = userId;
        }

        if (isActive)
        {
            if (paramList.Count > 0) sql += ", ";
            sql += " @Active = @Active";
            paramList["Active"] = true;
        }

        IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql, paramList);
        return users;

    }

    [HttpPut("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user)
    {

        string sql = @"
        EXEC TutorialAppSchema.spUser_Upsert
                @FirstName = @FirstName,
                @LastName = @LastName,
                @Email = @Email,
                @Gender = @Gender,
                @JobTitle = @JobTitle,
                @Department = @Department,
                @Salary = @Salary,
                @Active = @Active,
                @UserId = @UserId";

        var parameters = new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            user.Gender,
            user.JobTitle,
            user.Department,
            user.Salary,
            user.Active,
            user.UserId
        };
        if (_dapper.ExecuteSql(sql, parameters)) return Ok();
        throw new Exception("Failed to update user");

    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"
            EXEC TutorialAppSchema.spUser_Delete
            @UserId = @UserId";

        var parameters = new { UserId = userId };
        if (_dapper.ExecuteSql(sql, parameters)) return Ok();
        throw new Exception("Failed to delete user");

    }

}
