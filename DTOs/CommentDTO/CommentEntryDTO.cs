using System.ComponentModel.DataAnnotations;
namespace ia_back.DTOs.CommentDTO
{
    public class CommentEntryDTO
    {
        [Required(ErrorMessage = "Comment cannot be empty")]
        public string Content { get; set; }
        [Required]
        public int TaskId { get; set; }
        [Required]
        public int UserId { get; set; }
        public int? ParentCommentId { get; set; }
    }
}
