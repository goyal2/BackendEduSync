using System;

namespace EduSyncWebApi.DTO
{
    public class ResultDTO
    {
        public Guid ResultId { get; set; }
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }

        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }

        //public AssessmentDTO? Assessment { get; set; }
        //public UserDTO? User { get; set; }
    }
}
