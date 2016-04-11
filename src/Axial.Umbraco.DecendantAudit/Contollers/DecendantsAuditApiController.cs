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
        public IEnumerable<AuditRecordModel> GetAudit(Guid id, string aliases)
        {
            var parent = Services.ContentService.GetById(id);
            List<AuditRecordModel> records = null;

            if (parent != null)
            {
                var children = Services.ContentService.GetChildren(parent.Id);

                if (children != null)
                {
                    records = GetDecendantAuditRecords(parent.Id, aliases);
                }
            }
            return records;
        }

        private List<AuditRecordModel> GetDecendantAuditRecords(int parentId, string aliases, List<AuditRecordModel> records = null)
        {
            records = records ?? new List<AuditRecordModel>();

            var children = Services.ContentService.GetChildren(parentId);

            foreach (var child in children)
            {
                var db = DatabaseContext.Database;
                //var action = db.Query<string>("SELECT a.[comment] FROM (SELECT Max([Datestamp]) as [date],[logComment] as [comment] FROM [umbracoLog] WHERE NodeId = @0 GROUP BY [logComment]) a", child.Id);

                if (String.IsNullOrEmpty(aliases) || aliases.Contains(child.ContentType.Alias))
                {
                    records.Add(new AuditRecordModel()
                    {
                        Name = child.Name,
                        CreatedBy = Services.UserService.GetProfileById(child.CreatorId).Name,
                        CreatedDate = child.CreateDate.ToString("MM/dd/yyyy HH:mm:ss"),
                        ModifiedBy = Services.UserService.GetProfileById(child.WriterId).Name,
                        ModifiedDate = child.UpdateDate.ToString("MM/dd/yyyy HH:mm:ss"),
                        Action = "",
                        Id = child.Id,
                        ParentId = parentId,
                        ContentTypeAlias = child.ContentType.Alias
                    });

                }
                records = GetDecendantAuditRecords(child.Id, aliases, records);
            }

            return records;

        }
    }
}




