namespace Bookify.Infrastructure.Persistence.Configurations
{
    internal class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            // global query filter
            builder.HasQueryFilter(r => !r.IsDeleted);
            builder.Property(e => e.StartDate).HasDefaultValueSql("GETDATE()");
            builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
        }

    }
}
