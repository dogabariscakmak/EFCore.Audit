using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Audit.TestCommon
{
    public class AddressEntityConfiguration : IEntityTypeConfiguration<AddressEntity>
    {
        public AddressEntityConfiguration()
        {
        }

        public void Configure(EntityTypeBuilder<AddressEntity> builder)
        {
            #region Configuration
            builder.ToTable("Addresses");
            builder.HasKey(x => new { x.PersonId, x.Type });
            #endregion
        }
    }
}
