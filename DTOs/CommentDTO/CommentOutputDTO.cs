namespace ia_back.DTOs.CommentDTO
{
    public class CommentOutputDTO
    {
        public int Id { get; set; }
        public int? ParentCommentId { get; set; }
        public string CommenterUsername { get; set; }
        public string Content { get; set; }
    }
}
