namespace ia_back.DTOs.TaskDTO
{
    public class TaskEntryDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProjectID { get; set; }
        public int AssignedDevId { get; set; }
    }
}
