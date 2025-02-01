namespace Bookify.Infrastructure.Persistence.Configurations
{
    internal class AreaConfiguration : IEntityTypeConfiguration<Area>
    {
        public void Configure(EntityTypeBuilder<Area> builder)
        {
            builder.HasIndex(e => new {e.GovernorateId ,e.Name}).IsUnique();
            builder.Property(e => e.Name).HasMaxLength(100);
            builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
        }

    }
}
