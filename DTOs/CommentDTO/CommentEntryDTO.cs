namespace ia_back.DTOs.CommentDTO
{
    public class CommentEntryDTO
    {
        public string Content { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public int? ParentCommentId { get; set; }
    }
}
