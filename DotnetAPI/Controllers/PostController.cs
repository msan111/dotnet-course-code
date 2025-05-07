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

        [HttpGet("Posts")]
        public IEnumerable<Post> GetPosts()
        {
            string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM TutorialAppSchema.Posts";

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("PostSingle/{postId}")]
        public Post GetPostSingle(int postId)
        {
            string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM TutorialAppSchema.Posts
                WHERE PostId = @postId";

            return _dapper.LoadDataSingle<Post>(sql, new { PostId = postId });
        }

        [HttpGet("PostsByUser/{userId}")]
        public IEnumerable<Post> GetPostsByUser(int userId)
        {
            string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM TutorialAppSchema.Posts
                WHERE UserId = @userId";

            return _dapper.LoadData<Post>(sql, new { UserId = userId });
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

            string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM TutorialAppSchema.Posts
                WHERE UserId = @UserId";

            return _dapper.LoadData<Post>(sql, new { UserId = userId });
        }
        [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAdd)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();

            }
            int userId = int.Parse(userIdClaim);

            string sql = @"
                INSERT INTO TutorialAppSchema.Posts(
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated]
                ) 
                VALUES(
                    @UserId,
                    @PostTitle,
                    @PostContent,
                    @PostCreated,
                    @PostUpdated                    
                )";

            var parameters = new
            {
                UserId = userId,
                PostTitle = postToAdd.PostTitle,
                PostContent = postToAdd.PostContent,
                PostCreated = DateTime.Now,
                PostUpdated = DateTime.Now
            };

            if (_dapper.ExecuteSql(sql, parameters)) return Ok();
            throw new Exception("Failed to add post");

        }

        [HttpPut("Post")]
        public IActionResult EditPost(PostToEditDto postToEdit)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();

            }
            int userId = int.Parse(userIdClaim);

            string sql = @"
                UPDATE TutorialAppSchema.Posts
                    SET [PostTitle] = @PostTitle,
                        [PostContent] = @PostContent,
                        [PostUpdated] = @PostUpdated
                    WHERE PostId = @PostId
                    AND UserId = @userId             
                ";

            var parameters = new
            {
                PostId = postToEdit.PostId,
                UserId = userId,
                PostTitle = postToEdit.PostTitle,
                PostContent = postToEdit.PostContent,
                PostUpdated = DateTime.Now
            };

            if (_dapper.ExecuteSql(sql, parameters)) return Ok();
            return StatusCode(500, "Failed to edit pos.");

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

            string sql = @"
                DELETE FROM TutorialAppSchema.Posts
                WHERE PostId = @postId
                AND UserId = @userId";

            if (_dapper.ExecuteSql(sql, new { PostId = postId, UserId = userId })) return Ok();
            return StatusCode(500, "Failed to delete post.");
        }

        [HttpGet("PostsBySearch/{searchParam}")]
        public IEnumerable<Post> PostsBySearch(string searchParam)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Enumerable.Empty<Post>();
            }
            int userId = int.Parse(userIdClaim);

            string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM TutorialAppSchema.Posts
                    WHERE PostTitle LIKE @SearchPattern
                        OR PostContent LIKE @SearchPattern";


            var parameters = new
            {
                SearchPattern = $"%{searchParam}%"
            };

            return _dapper.LoadData<Post>(sql, parameters);
        }

    }


}
