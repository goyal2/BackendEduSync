using System;
using System.Collections.Generic;

namespace EduSyncWebApi.Models;

public partial class UserModel
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<CourseModel> CourseModels { get; set; } = new List<CourseModel>();

    public virtual ICollection<ResultModel> ResultModels { get; set; } = new List<ResultModel>();
}
