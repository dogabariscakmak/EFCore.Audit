using System;
using System.Collections.Generic;

namespace EFCore.Audit
{
    public class AuditMetaDataEntity
    {
        public Guid HashPrimaryKey { get; set; }
        public string SchemaTable { get; set; }
        public string ReadablePrimaryKey { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public string DisplayName { get; set; }
        public ICollection<AuditEntity> AuditChanges { get; set; }
    }
}
