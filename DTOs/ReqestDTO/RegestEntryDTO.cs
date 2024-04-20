using ia_back.Models;

namespace ia_back.DTOs.ReqestDTO
{
    public class ReqestEntryDTO
    {
        public int ProjectId{ get; set; }
        public Project Project { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

    }
}

