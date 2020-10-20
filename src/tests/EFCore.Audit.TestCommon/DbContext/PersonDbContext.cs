using Microsoft.EntityFrameworkCore;

namespace EFCore.Audit.TestCommon
{
    public class PersonDbContext : AuditDbContextBase<PersonDbContext>
    {
        public DbSet<PersonEntity> Persons { get; set; }
        public DbSet<AddressEntity> Addresses { get; set; }
        public DbSet<PersonAttributesEntity> PersonAttributes { get; set; }

        public PersonDbContext(DbContextOptions<PersonDbContext> options) : base(options)
        {
        }

        public PersonDbContext(DbContextOptions<PersonDbContext> options, IAuditUserProvider auditUserProvider) : base(options, auditUserProvider)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonEntity>()
               .HasMany(p => p.Addresses)
               .WithOne(a => a.Person);

            modelBuilder.Entity<PersonEntity>()
               .HasMany(p => p.Attributes)
               .WithOne(a => a.Person);

            modelBuilder.ApplyConfiguration(new PersonEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AddressEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PersonAttributesEntityConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
