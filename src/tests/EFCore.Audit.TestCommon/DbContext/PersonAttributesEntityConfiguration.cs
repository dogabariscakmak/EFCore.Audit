using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Audit.TestCommon
{
    public class PersonAttributesEntityConfiguration : IEntityTypeConfiguration<PersonAttributesEntity>
    {
        public PersonAttributesEntityConfiguration()
        {
        }

        public void Configure(EntityTypeBuilder<PersonAttributesEntity> builder)
        {
            #region Configuration
            builder.ToTable("PersonAttributes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            #endregion
        }
    }
}
