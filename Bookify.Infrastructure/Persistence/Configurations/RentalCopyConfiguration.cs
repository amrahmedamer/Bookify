using Bookify.Domain.Enum;

namespace Bookify.Infrastructure.Persistence.Configurations
{
    internal class RentalCopyConfiguration : IEntityTypeConfiguration<RentalCopy>
    {
        public void Configure(EntityTypeBuilder<RentalCopy> builder)
        {
            builder.HasKey(e => new { e.RentalId, e.BookCopyId });
            // global query filter
            builder.HasQueryFilter(r => !r.Rental!.IsDeleted);

            builder.Property(e => e.RentalDate).HasDefaultValueSql("GETDATE()");
            //يفضل انها تكون مسئولية التطبيق بتاعنا مش قاعدة البيانات 
           // builder.Property(e => e.EndDate).HasDefaultValue(DateTime.Today.AddDays((int)RentalsConfigrations.RentalDuration));
            // builder.Property(e => e.EndDate).HasDefaultValueSql($"DATEADD(DAY,{(int)RentalsConfigrations.RentalDuration}, GETDATE())");
        }

    }
}
