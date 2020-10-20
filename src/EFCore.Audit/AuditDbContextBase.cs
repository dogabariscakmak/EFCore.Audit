using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFCore.Audit
{
    public abstract class AuditDbContextBase<TDbContext> : DbContext where TDbContext : DbContext
    {
        private readonly IAuditUserProvider auditUserProvider;

        public DbSet<AuditEntity> Audits { get; set; }
        public DbSet<AuditMetaDataEntity> AuditMetaDatas { get; set; }

        public AuditDbContextBase(DbContextOptions<TDbContext> options) : base(options)
        {
        }

        public AuditDbContextBase(DbContextOptions<TDbContext> options, IAuditUserProvider auditUserProvider) : base(options)
        {
            this.auditUserProvider = auditUserProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditEntity>()
                .HasOne(ae => ae.AuditMetaData)
                .WithMany(amde => amde.AuditChanges);

            modelBuilder.ApplyConfiguration(new AuditEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AuditMetaDataEntityConfiguration());
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            IEnumerable<AuditEntry> entityAudits = OnBeforeSaveChanges();
            int result = base.SaveChanges(acceptAllChangesOnSuccess);
            OnAfterSaveChanges(entityAudits);

            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            IEnumerable<AuditEntry> entityAudits = OnBeforeSaveChanges();
            int result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await OnAfterSaveChangesAsync(entityAudits);

            return result;
        }

        private IEnumerable<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            List<AuditEntry> auditEntries = new List<AuditEntry>();
            foreach (EntityEntry entry in ChangeTracker.Entries())
            {
                if (!entry.ShouldBeAudited())
                {
                    continue;
                }

                auditEntries.Add(new AuditEntry(entry, auditUserProvider));
            }

            BeginTrackingAuditEntries(auditEntries.Where(_ => !_.HasTemporaryProperties));

            // keep a list of entries where the value of some properties are unknown at this step
            return auditEntries.Where(_ => _.HasTemporaryProperties);
        }

        private void OnAfterSaveChanges(IEnumerable<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count() == 0)
                return;

            BeginTrackingAuditEntries(auditEntries);

            base.SaveChanges();
        }

        private async Task OnAfterSaveChangesAsync(IEnumerable<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count() == 0)
                return;

            await BeginTrackingAuditEntriesAsync(auditEntries);

            await base.SaveChangesAsync();
        }

        private void BeginTrackingAuditEntries(IEnumerable<AuditEntry> auditEntries)
        {
            foreach (var auditEntry in auditEntries)
            {
                auditEntry.Update();
                AuditMetaDataEntity auditMetaDataEntity = auditEntry.ToAuditMetaDataEntity();
                AuditMetaDataEntity existedAuditMetaDataEntity = AuditMetaDatas.FirstOrDefault(x => x.HashPrimaryKey == auditMetaDataEntity.HashPrimaryKey && x.SchemaTable == auditMetaDataEntity.SchemaTable);
                if (existedAuditMetaDataEntity == default)
                {
                    Add(auditEntry.ToAuditEntity(auditMetaDataEntity));
                }
                else
                {
                    Add(auditEntry.ToAuditEntity(existedAuditMetaDataEntity));
                }
            }
        }

        private async Task BeginTrackingAuditEntriesAsync(IEnumerable<AuditEntry> auditEntries)
        {
            foreach (var auditEntry in auditEntries)
            {
                auditEntry.Update();
                AuditMetaDataEntity auditMetaDataEntity = auditEntry.ToAuditMetaDataEntity();
                AuditMetaDataEntity existedAuditMetaDataEntity = await AuditMetaDatas.FirstOrDefaultAsync(x => x.HashPrimaryKey == auditMetaDataEntity.HashPrimaryKey && x.SchemaTable == auditMetaDataEntity.SchemaTable);
                if (existedAuditMetaDataEntity == default)
                {
                    await AddAsync(auditEntry.ToAuditEntity(auditMetaDataEntity));
                }
                else
                {
                    await AddAsync(auditEntry.ToAuditEntity(existedAuditMetaDataEntity));
                }
            }
        }
    }
}
