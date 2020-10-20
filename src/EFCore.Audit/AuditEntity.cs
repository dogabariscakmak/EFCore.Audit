using Microsoft.EntityFrameworkCore;
using System;

namespace EFCore.Audit
{
    public class AuditEntity
    {
        public Guid Id { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public EntityState EntityState { get; set; }
        public string ByUser { get; set; }

        public AuditMetaDataEntity AuditMetaData { get; set; }
    }
}
