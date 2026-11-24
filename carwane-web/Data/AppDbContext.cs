using Carwane.Web.Models;
using Carwane.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Carwane.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FlightOffer> FlightOffers => Set<FlightOffer>();
    public DbSet<FareCalendarDay> FareCalendarDays => Set<FareCalendarDay>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Enquiry> Enquiries => Set<Enquiry>();
    public DbSet<FareAlert> FareAlerts => Set<FareAlert>();
    public DbSet<FeeSetting> FeeSettings => Set<FeeSetting>();
    public DbSet<FeeSettingAudit> FeeSettingAudits => Set<FeeSettingAudit>();
    public DbSet<ZiyaratPlace> ZiyaratPlaces => Set<ZiyaratPlace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeeSetting>().HasIndex(f => f.IsCurrent);
        modelBuilder.Entity<Booking>().HasIndex(b => b.Reference).IsUnique();
        modelBuilder.Entity<Enquiry>().HasIndex(e => e.Reference).IsUnique();
    }
}

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.FeeSettings.Any())
        {
            // DEMO values only — staging placeholders. Production must start at PKR 0/off.
            db.FeeSettings.Add(new FeeSetting
            {
                Version = 1, IsCurrent = true, IsDemo = true,
                ServiceFeeMode = "flat_per_traveller", ServiceFeeAmountPkr = 3000,
                SupplierFeesEnabled = false, SupplierFeeAmountPkr = 0,
                CardChargeEnabled = false, CardChargeAmountPkr = 0, CardChargePercent = 0,
                DiscountActive = false, ChangedBy = "seed"
            });
            db.FeeSettingAudits.Add(new FeeSettingAudit
            {
                Version = 1, ChangedBy = "seed",
                Summary = "Seeded staging demo fees: service fee PKR 3,000 per traveller, everything else 0/off. DEMO ONLY."
            });
        }
        if (!db.ZiyaratPlaces.Any())
        {
            db.ZiyaratPlaces.AddRange(new ZiyaratService().All());
        }
        db.SaveChanges();
    }
}
