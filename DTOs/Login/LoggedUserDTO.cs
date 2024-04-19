using ia_back.Models;

namespace ia_back.DTOs.Login
{
    public class LoggedUserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        //Projects led by the user
        public ICollection<Project> CreatedProjects { get; set; }

        //Projects assigned to the user (as a developer)
        public ICollection<Project> AssignedProjects { get; set; }

        //Project requests
        public ICollection<Project> ProjectRequests { get; set; }
    }
}
