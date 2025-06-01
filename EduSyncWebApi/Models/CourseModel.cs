using System;
using System.Collections.Generic;

namespace EduSyncWebApi.Models;

public partial class CourseModel
{
    public Guid CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid InstructorId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public virtual ICollection<AssessmentModel> AssessmentModels { get; set; } = new List<AssessmentModel>();

    public virtual UserModel Instructor { get; set; } = null!;

    
}
