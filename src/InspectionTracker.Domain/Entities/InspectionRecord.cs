using InspectionTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionTracker.Domain.Entities
{
    public class InspectionRecord
    {
        public Guid Id { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public InspectionStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime InspectionDate { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }
    }
}
