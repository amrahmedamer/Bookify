namespace Bookify.Infrastructure.Persistence.Configurations
{
    internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasIndex(e => e.CategoryName).IsUnique();
            builder.Property(e => e.CategoryName).HasMaxLength(50);
            builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
        }

    }
}
