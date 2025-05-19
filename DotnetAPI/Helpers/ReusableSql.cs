using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;

namespace DotnetAPI.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;
        public ReusableSql(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        public bool UpsertUser(UserComplete user)
        {

            string sql = @"EXEC TutorialAppSchema.spUser_Upsert
                @FirstName = @FirstName,
                @LastName = @LastName,
                @Email = @Email,
                @Gender = @Gender,
                @JobTitle = @JobTitle,
                @Department = @Department,
                @Salary = @Salary,
                @Active = @Active,
                @UserId = @UserId";

            var parameters = new DynamicParameters();

            parameters.Add("FirstName", user.FirstName, DbType.String);
            parameters.Add("LastName", user.LastName, DbType.String);
            parameters.Add("Email", user.Email, DbType.String);
            parameters.Add("Gender", user.Gender, DbType.String);
            parameters.Add("JobTitle", user.JobTitle, DbType.String);
            parameters.Add("Department", user.Department, DbType.String);
            parameters.Add("Salary", user.Salary, DbType.Decimal);
            parameters.Add("Active", user.Active, DbType.Boolean);
            parameters.Add("UserId", user.UserId, DbType.Int32);

            return _dapper.ExecuteSqlWithParameters(sql, parameters);
           
        }
    }
}