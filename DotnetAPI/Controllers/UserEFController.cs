using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserEFController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public UserEFController(IConfiguration config, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDto, User>();
            cfg.CreateMap<UserSalary, UserSalary>();
            cfg.CreateMap<UserJobInfo, UserJobInfo>();
        }));
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUserAllUsers()
    {
        IEnumerable<User> users = _userRepository.GetUserAllUsers();
        return users;
    }

    [HttpGet("GetUserById/{userId}")]
    public User GetUserById(int userId)
    {
        return _userRepository.GetUserById(userId);
    }

    [HttpPut("EditUser")]
    public IActionResult UpdateUser(User user)
    {
        User? existingUser =_userRepository.GetUserById(user.UserId);;

        if (existingUser != null)
        {
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.Gender = user.Gender;
            existingUser.Active = user.Active;
        }
        if(_userRepository.SaveChanges())
        {
            return Ok("User updated");
        }
        else
        {
            return NotFound("User not found or no changes made.");
        }
    }

    [HttpPost("CreateUser")]
    public IActionResult CreateUser(UserToAddDto userDto)
    {
        User user = _mapper.Map<User>(userDto);

        _userRepository.AddEntity<User>(user);
        if(_userRepository.SaveChanges())
        {
            return Ok("User created");
        }
        else
        {
            return NotFound("No changes made.");
        }

    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? user = _userRepository.GetUserById(userId);

        if (user != null)
        {
            _userRepository.RemoveEntity<User>(user);
        }
        if(_userRepository.SaveChanges())
        {
            return Ok("User deleted");
        }
        else
        {
            return NotFound("User not found or no changes made.");
        }

    }

    [HttpGet("UserSalary/{userId}")]
    public UserSalary GetSalaryByUserId(int userId)
    {
        return _userRepository.GetSalaryByUserId(userId);
    }


    [HttpPost("CreateUserSalary")]
    public IActionResult CreateUserSalary(UserSalary salary)
    {
        _userRepository.AddEntity<UserSalary>(salary);
        if(_userRepository.SaveChanges())
        {
            return Ok("UserSalary created");
        }
        else
        {
            return NotFound("No changes made.");
        }

    }


    [HttpPut("UpdateUserSalary")]
    public IActionResult UpdateUserSalary(UserSalary salary)
    {
        UserSalary? existingSalary = _userRepository.GetSalaryByUserId(salary.UserId);

        if (existingSalary != null)
        {
            _mapper.Map(salary, existingSalary);
            //userToUpdate.Salary = user.Salary;

        }
        if(_userRepository.SaveChanges())
        {
            return Ok("UserSalary updated");
        }
        else
        {
            return NotFound("UserSalary not found or no changes made.");
        }
    }


    [HttpDelete("DeleteUserSalary/{userId}")]
    public IActionResult DeleteUserSalary(int userId)
    {
        UserSalary? salary = _userRepository.GetSalaryByUserId(userId);

        if (salary != null)
        {
            _userRepository.RemoveEntity<UserSalary>(salary);
        }
       if(_userRepository.SaveChanges())
        {
            return Ok("UserSalary deleted");
        }
        else
        {
            return NotFound("UserSalary not found or no changes made.");
        }


    }


    [HttpGet("UserJobInfo/{userId}")]
    public UserJobInfo GetJobInfoByUserId(int userId)
    {
        return _userRepository.GetJobInfoByUserId(userId);
    }


    [HttpPost("CreateUserJobInfo")]
    public IActionResult CreateUserJobInfoEF(UserJobInfo jobInfo)
    {
        _userRepository.AddEntity<UserJobInfo>(jobInfo);
        if(_userRepository.SaveChanges())
        {
            return Ok("UserJobInfo created");
        }
        else
        {
            return NotFound("No changes made.");
        }


    }


    [HttpPut("UpdateUserJobInfo")]
    public IActionResult UpdateUserJobInfoEF(UserJobInfo jobInfo)
    {
        UserJobInfo? existingJobInfo = _userRepository.GetJobInfoByUserId(jobInfo.UserId);

        if (existingJobInfo != null)
        {
            _mapper.Map(jobInfo, existingJobInfo);
        }
        if(_userRepository.SaveChanges())
        {
            return Ok("UserJobInfo updated");
        }
        else
        {
            return NotFound("UserJobInfo not found or no changes made.");
        }

    }


    [HttpDelete("DeleteUserJobInfo/{userId}")]
    public IActionResult DeleteUserJobInfoEF(int userId)
    {
        UserJobInfo? jobInfo= _userRepository.GetJobInfoByUserId(userId);

        if (jobInfo != null)
        {
            _userRepository.RemoveEntity<UserJobInfo>(jobInfo);
        }
        if(_userRepository.SaveChanges())
        {
            return Ok("UserJobInfo deleted");
        }
        else
        {
            return NotFound("UserJobInfo not found or no changes made.");
        }


    }


}
