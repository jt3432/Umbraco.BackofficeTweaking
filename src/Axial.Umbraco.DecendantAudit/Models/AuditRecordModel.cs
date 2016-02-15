using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axial.Umbraco.DecendantsAudit.Models
{
    public class AuditRecordModel
    {
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string Action { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }
    }
}
