using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Data
{
    public interface IUserRepository
    {
        public bool SaveChanges();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToRemove);
        public IEnumerable<User> GetUserAllUsers();
        public User GetUserById(int userId);
        public UserSalary GetSalaryByUserId(int userId);
        public UserJobInfo GetJobInfoByUserId(int userId);

    }
}