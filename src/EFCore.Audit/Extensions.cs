using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace EFCore.Audit
{
    internal static class InternalExtensions
    {
        internal static bool ShouldBeAudited(this EntityEntry entry)
        {
            return entry.State != EntityState.Detached && entry.State != EntityState.Unchanged &&
                   !(entry.Entity is AuditEntity) && !(entry.Entity is AuditMetaDataEntity) &&
                   entry.IsAuditable();
        }

        internal static bool IsAuditable(this EntityEntry entityEntry)
        {
            AuditableAttribute enableAuditAttribute = (AuditableAttribute)Attribute.GetCustomAttribute(entityEntry.Entity.GetType(), typeof(AuditableAttribute));

            return enableAuditAttribute != null;
        }

        internal static bool IsAuditable(this PropertyEntry propertyEntry)
        {
            Type entityType = propertyEntry.EntityEntry.Entity.GetType();
            PropertyInfo propertyInfo = entityType.GetProperty(propertyEntry.Metadata.Name);
            bool disableAuditAttribute = Attribute.IsDefined(propertyInfo, typeof(NotAuditableAttribute));

            return IsAuditable(propertyEntry.EntityEntry) && !disableAuditAttribute;
        }
    }

    public static class Extensions
    {
        public static string ToReadablePrimaryKey(this EntityEntry entry)
        {
            IKey primaryKey = entry.Metadata.FindPrimaryKey();
            if (primaryKey == null)
            {
                return null;
            }
            else
            {
                return string.Join(",", (primaryKey.Properties.ToDictionary(x => x.Name, x => x.PropertyInfo.GetValue(entry.Entity))).Select(x => x.Key + "=" + x.Value));
            }
        }

        public static Guid ToGuidHash(this string readablePrimaryKey)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] hashValue = sha512.ComputeHash(Encoding.Default.GetBytes(readablePrimaryKey));
                byte[] reducedHashValue = new byte[16];
                for (int i = 0; i < 16; i++)
                {
                    reducedHashValue[i] = hashValue[i * 4];
                }
                return new Guid(reducedHashValue);
            }
        }
    }
}
