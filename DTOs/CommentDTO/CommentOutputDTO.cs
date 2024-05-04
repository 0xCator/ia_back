using ia_back.DTOs.ProjectDTO;

namespace ia_back.DTOs.CommentDTO
{
    public class CommentOutputDTO
    {
        public int Id { get; set; }
        public int? ParentCommentId { get; set; }
        public DeveloperInfo CommenterInfo { get; set; }
        public string Content { get; set; }
    }
}
