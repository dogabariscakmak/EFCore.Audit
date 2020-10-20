using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EFCore.Audit
{
    internal class AuditEntry
    {
        private string _readablePrimaryKey;
        public string ReadablePrimaryKey
        {
            get
            {
                if (string.IsNullOrEmpty(_readablePrimaryKey))
                    _readablePrimaryKey = Entry.ToReadablePrimaryKey();
                return _readablePrimaryKey;
            }
            set
            {
                _readablePrimaryKey = value;
            }
        }
        public Guid HashReferenceId { get; set; }
        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public string DisplayName { get; set; }
        public string SchemaName { get; set; }
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public EntityState EntityState { get; set; }
        public string ByUser { get; set; }
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();
        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditEntry(EntityEntry entry, IAuditUserProvider auditUserProvider)
        {
            Entry = entry;
            ReadablePrimaryKey = Entry.ToReadablePrimaryKey();
            HashReferenceId = ReadablePrimaryKey.ToGuidHash();
            TableName = entry.Metadata.GetTableName();
            DisplayName = entry.Metadata.DisplayName();
            SchemaName = entry.Metadata.GetSchema();
            EntityState = entry.State;
            ByUser = auditUserProvider == default ? "Unknown" : auditUserProvider.GetUser();

            foreach (PropertyEntry property in entry.Properties)
            {
                if (property.IsAuditable())
                {
                    if (property.IsTemporary)
                    {
                        TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                OldValues[propertyName] = property.OriginalValue;
                                NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
        }

        public void Update()
        {
            // Get the final value of the temporary properties
            foreach (var prop in TemporaryProperties)
            {
                NewValues[prop.Metadata.Name] = prop.CurrentValue;
            }

            if (TemporaryProperties != default && TemporaryProperties.Count(x => x.Metadata.IsKey()) > 0)
            {
                ReadablePrimaryKey = Entry.ToReadablePrimaryKey();
                HashReferenceId = ReadablePrimaryKey.ToGuidHash();
            }
        }

        public AuditMetaDataEntity ToAuditMetaDataEntity()
        {
            AuditMetaDataEntity auditMetaData = new AuditMetaDataEntity();
            auditMetaData.DisplayName = DisplayName;
            auditMetaData.Schema = SchemaName;
            auditMetaData.Table = TableName;
            auditMetaData.SchemaTable = $"{(!string.IsNullOrEmpty(SchemaName) ? SchemaName + "." : "")}{TableName}";
            auditMetaData.ReadablePrimaryKey = ReadablePrimaryKey;
            auditMetaData.HashPrimaryKey = HashReferenceId;

            return auditMetaData;
        }

        public AuditEntity ToAuditEntity(AuditMetaDataEntity auditMetaData)
        {
            AuditEntity audit = new AuditEntity();
            audit.OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
            audit.NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues);
            audit.EntityState = EntityState;
            audit.DateTimeOffset = DateTimeOffset.UtcNow;
            audit.ByUser = ByUser;
            audit.AuditMetaData = auditMetaData;

            return audit;
        }

        public AuditEntity ToAuditEntity()
        {
            AuditEntity audit = new AuditEntity();
            audit.OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
            audit.NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues);
            audit.EntityState = EntityState;
            audit.DateTimeOffset = DateTimeOffset.UtcNow;
            audit.ByUser = ByUser;

            return audit;
        }
    }
}
