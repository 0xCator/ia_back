using ia_back.Data;
using ia_back.Data.Custom_Repositories;
using ia_back.DTOs.CommentDTO;
using ia_back.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ia_back.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : Controller
    {
        private readonly IDataRepository<Comment> _commentRepository;
        private readonly IDataRepository<ProjectTask> _projectTaskRepository;
        private readonly IUserRepository _userRepository;
        
        public CommentController(IDataRepository<Comment> commentRepository,
                                IDataRepository<ProjectTask> projectTaskRepository,
                                IUserRepository userRepository)
        {
            _commentRepository = commentRepository;
            _projectTaskRepository = projectTaskRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment(CommentEntryDTO commentInfo)
        {
            if (commentInfo == null)
            {
                return BadRequest();
            }

            var projectTask = await _projectTaskRepository.GetByIdAsync(commentInfo.TaskId);
            if (projectTask == null)
            {
                return NotFound("Task doesn't exist");
            }

            var user = await _userRepository.GetByIdAsync(commentInfo.UserId);
            if (user == null)
            {
                return NotFound("User doesn't exist");
            }

            if (commentInfo.ParentCommentId != null)
            {
                var parentComment = await _commentRepository.GetByIdAsync((int)commentInfo.ParentCommentId);
                if (parentComment == null)
                {
                    return NotFound("Parent comment doesn't exist");
                }
            }

            Comment comment = new Comment
            {
                TaskId = commentInfo.TaskId,
                UserId = commentInfo.UserId,
                Content = commentInfo.Content,
                ParentCommentId = commentInfo.ParentCommentId
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.Save();

            return Ok(comment);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetComment(int id)
        {
            var comment = await _commentRepository.GetByIdIncludeAsync(id,
                                                                       c=>c.User);
            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }
    }
}
