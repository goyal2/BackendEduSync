using System;
using System.Collections.Generic;

namespace EduSyncWebApi.Models;

public partial class AssessmentModel
{
    public Guid AssessmentId { get; set; }

    public Guid CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string Questions { get; set; } = null!;

    public int MaxScore { get; set; }

    public virtual CourseModel Course { get; set; } = null!;

    public virtual ICollection<ResultModel> ResultModels { get; set; } = new List<ResultModel>();
}
