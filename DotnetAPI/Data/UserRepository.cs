using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContextEF _entityFramework;
        public UserRepository(IConfiguration config)
        {
            _entityFramework = new DataContextEF(config);
        }
        public bool SaveChanges()
        {
            return _entityFramework.SaveChanges() > 0;
        }
        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramework.Add(entityToAdd);
            }
        }
        public void RemoveEntity<T>(T entityToRemove)
        {
            if (entityToRemove != null)
            {
                _entityFramework.Remove(entityToRemove);
            }
        }

        public IEnumerable<User> GetUserAllUsers()
        {
            IEnumerable<User> users = _entityFramework.Users.ToList<User>();
            return users;
        }

        public User GetUserById(int userId)
        {
            User? user = _entityFramework.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) throw new KeyNotFoundException("User not found");
            return user;
        }


        public UserSalary GetSalaryByUserId(int userId)
        {
            UserSalary? userSalary = _entityFramework.UserSalary.FirstOrDefault(u => u.UserId == userId);
            if (userSalary == null) throw new Exception("Failed to get user salary");
            return userSalary;
        }
        public UserJobInfo GetJobInfoByUserId(int userId)
        {
            UserJobInfo? userJobInfo = _entityFramework.UserJobInfo.FirstOrDefault(u => u.UserId == userId);
            if (userJobInfo == null) throw new KeyNotFoundException("User not found");
            return userJobInfo;
        }


    }
}