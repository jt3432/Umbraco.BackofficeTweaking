using Axial.Umbraco.DecendantsAudit.Models;
using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Axial.Umbraco.DecendantsAudit.Controllers
{
    [PluginController("DecendantsAudit")]
    [IsBackOffice]
    public class DecendantsAuditApiController : UmbracoAuthorizedJsonController
    {

        [System.Web.Http.HttpGet]
        public IEnumerable<AuditRecordModel> GetAudit(Guid id)
        {
            var parent = Services.ContentService.GetById(id);
            var children = Services.ContentService.GetChildren(parent.Id);

            List<AuditRecordModel> records = new List<AuditRecordModel>();
            records = GetDecendantAuditRecords(parent.Id);

            return records;
        }

        private List<AuditRecordModel> GetDecendantAuditRecords(int parentId, List<AuditRecordModel> records = null)
        {
            records = records ?? new List<AuditRecordModel>();

            var children = Services.ContentService.GetChildren(parentId);

            foreach (var child in children)
            {
                var db = DatabaseContext.Database;
                //var action = db.Query<string>("SELECT a.[comment] FROM (SELECT Max([Datestamp]) as [date],[logComment] as [comment] FROM [umbracoLog] WHERE NodeId = @0 GROUP BY [logComment]) a", child.Id);

                records.Add(new AuditRecordModel()
                {
                    Name = child.Name,
                    CreatedBy = Services.UserService.GetProfileById(child.CreatorId).Name,
                    CreatedDate = child.CreateDate.ToString("MM/dd/yyyy HH:mm:ss"),
                    ModifiedBy = Services.UserService.GetProfileById(child.WriterId).Name,
                    ModifiedDate = child.UpdateDate.ToString("MM/dd/yyyy HH:mm:ss"),
                    Action = "",
                    Id = child.Id,
                    ParentId = parentId
                });

                records = GetDecendantAuditRecords(child.Id, records);
            }

            return records;

        }
    }
}




