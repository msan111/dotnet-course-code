using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }


        [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get";

            DynamicParameters sqlParameters = new DynamicParameters();
            bool hasPreviousParam = false;

            if (postId != 0)
            {

                sql += " @PostId = @PostId";
                sqlParameters.Add("PostId", postId, DbType.Int32);
                hasPreviousParam = true;

            }
            if (userId != 0)
            {
                if (hasPreviousParam) sql += ", ";
                sql += " @UserId = @UserId";
                sqlParameters.Add("UserId", userId, DbType.Int32);
                hasPreviousParam = true;
            }
            if (searchParam != "None")
            {
                if (hasPreviousParam) sql += ", ";
                sql += " @SearchValue = @SearchValue";
                sqlParameters.Add("SearchValue", searchParam, DbType.String);

            }
            IEnumerable<Post> posts = _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
            return posts ?? Enumerable.Empty<Post>();
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Enumerable.Empty<Post>();
            }
            int userId = int.Parse(userIdClaim);

            string sql = @"EXEC TutorialAppSchema.spPosts_Get
                    @UserId = @UserId";

            DynamicParameters parameter = new DynamicParameters();

            parameter.Add("UserId", userId, DbType.Int32);


            return _dapper.LoadDataWithParameters<Post>(sql,parameter);
        }
        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();

            }
            int userId = int.Parse(userIdClaim);

            string sql = @"EXEC TutorialAppSchema.spPosts_Upsert
                @UserId = @UserId
                , @PostTitle = @PostTitle
                , @PostContent = @PostContent
                , @PostId = @PostId";
            
            DynamicParameters sqlParameters = new DynamicParameters();
            
            sqlParameters.Add("UserId", userId, DbType.Int32);
            sqlParameters.Add("PostTitle", postToUpsert.PostTitle, DbType.String);
            sqlParameters.Add("PostContent", postToUpsert.PostContent, DbType.String);
            sqlParameters.Add("PostId", postToUpsert.PostId, DbType.Int32);

            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters)) return Ok();
            throw new Exception("Failed to upsert post");

        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();

            }
            int userId = int.Parse(userIdClaim);

            string sql = @"EXEC TutorialAppSchema.spPost_Delete
                @PostId = @PostId,
                @UserId = @UserId";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("PostId", postId, DbType.Int32);
            sqlParameters.Add("UserId", userId, DbType.Int32);

            if (_dapper.ExecuteSqlWithParameters(sql,sqlParameters)) return Ok();
            return StatusCode(500, "Failed to delete post.");
        }

    }


}
