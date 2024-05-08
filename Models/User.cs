using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ia_back.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        //Projects led by the user
        public ICollection<Project> CreatedProjects { get; set; }

        //Projects assigned to the user (as a developer)
        public ICollection<Project> AssignedProjects { get; set; }

        //Project requests
        public ICollection<Project> ProjectRequests { get; set; }


    }

    public enum Role
    {
        TeamLeader,
        Developer
    }
}
