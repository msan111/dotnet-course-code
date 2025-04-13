using System.Data;
using Dapper;
using Helloworld.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Helloworld.Data
{
    public class DataContextDapper
    {
        private readonly string _connectionString;
        public DataContextDapper(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection", "Connection string cannot be null.");
        }
    
        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Query<T>(sql);
        }

         public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.QuerySingle<T>(sql);
        }
        // return whether the sql method successfully executed or not
         public bool ExecuteSql(string sql, object parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return (dbConnection.Execute(sql,parameters) > 0);
        }
        //returns the number of rows affected
        public int ExecuteSqlWithRowCount(string sql, object parameters)
        { 
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Execute(sql,parameters) ;
        }
    }
}