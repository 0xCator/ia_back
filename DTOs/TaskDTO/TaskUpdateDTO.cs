namespace ia_back.DTOs.TaskDTO
{
    public class TaskUpdateDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? AssignedDeveloperID { get; set; }
    }
}
