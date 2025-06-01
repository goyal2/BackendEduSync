using System;

namespace EduSyncWebApi.DTO
{
    public class CourseDTO
    {
        public Guid CourseId { get; set; }  
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid InstructorId { get; set; }
        public string? MediaUrl { get; set; }

    }
}
