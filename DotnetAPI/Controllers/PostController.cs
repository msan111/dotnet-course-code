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
            var paramList = new Dictionary<string, object>();

            if (postId != 0)
            {
                sql += " @PostId = @PostId";
                paramList["PostId"] = postId;
            }
            if (userId != 0)
            {
                if (paramList.Count > 0) sql += ", ";
                sql += " @UserId = @UserId";
                paramList["UserId"] = userId;
            }
            if (searchParam != "None")
            {
                if (paramList.Count > 0) sql += ", ";
                sql += " @SearchValue = @SearchValue";
                paramList["SearchValue"] = searchParam;
            }
            IEnumerable<Post> posts = _dapper.LoadData<Post>(sql, paramList);
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

            return _dapper.LoadData<Post>(sql, new { UserId = userId });
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

            var parameters = new
            {
                UserId = userId,
                PostTitle = postToUpsert.PostTitle,
                PostContent = postToUpsert.PostContent,
                PostId = postToUpsert.PostId
            };

            if (_dapper.ExecuteSql(sql, parameters)) return Ok();
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

            if (_dapper.ExecuteSql(sql, new { PostId = postId, UserId = userId })) return Ok();
            return StatusCode(500, "Failed to delete post.");
        }

    }


}
