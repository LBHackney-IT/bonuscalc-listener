using Microsoft.EntityFrameworkCore;

namespace BonusCalcListener.Infrastructure
{
    public class BonusCalcContext : DbContext
    {
        public BonusCalcContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<BonusPeriod> BonusPeriods { get; set; }
        public DbSet<Operative> Operatives { get; set; }
        public DbSet<PayBand> PayBands { get; set; }
        public DbSet<PayElement> PayElements { get; set; }
        public DbSet<PayElementType> PayElementTypes { get; set; }
        public DbSet<Scheme> Schemes { get; set; }
        public DbSet<Timesheet> Timesheets { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<Week> Weeks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BonusPeriod>()
                .HasIndex(bp => bp.StartAt)
                .IsUnique();

            modelBuilder.Entity<BonusPeriod>()
                .HasIndex(bp => new { bp.Year, bp.Number })
                .IsUnique();

            modelBuilder.Entity<Operative>()
                .HasOne(o => o.Trade)
                .WithMany(t => t.Operatives)
                .HasForeignKey(o => o.TradeId);

            modelBuilder.Entity<Operative>()
                .HasIndex(o => o.SchemeId);

            modelBuilder.Entity<Operative>()
                .HasOne(o => o.Scheme)
                .WithMany(s => s.Operatives)
                .HasForeignKey(o => o.SchemeId);

            modelBuilder.Entity<PayBand>()
                .Property(pb => pb.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<PayBand>()
                .HasIndex(pb => pb.SchemeId);

            modelBuilder.Entity<PayBand>()
                .HasOne(pb => pb.Scheme)
                .WithMany(s => s.PayBands)
                .HasForeignKey(pb => pb.SchemeId);

            modelBuilder.Entity<PayElement>()
                .HasOne(pe => pe.Timesheet)
                .WithMany(t => t.PayElements)
                .HasForeignKey(t => t.TimesheetId);

            modelBuilder.Entity<PayElement>()
                .HasOne(pe => pe.PayElementType)
                .WithMany(pet => pet.PayElements)
                .HasForeignKey(pe => pe.PayElementTypeId);

            modelBuilder.Entity<PayElement>()
                .Property(pe => pe.Duration)
                .HasPrecision(10, 4);

            modelBuilder.Entity<PayElement>()
                .Property(pe => pe.Value)
                .HasPrecision(10, 4);

            modelBuilder.Entity<PayElement>()
                .Property(pe => pe.Monday)
                .HasPrecision(10, 4)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<PayElement>()
                .Property(pe => pe.Tuesday)
                .HasPrecision(10, 4)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<PayElement>()
                .Property(pe => pe.Wednesday)
                .HasPrecision(10, 4)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<PayElement>()
                .Property(pe => pe.Thursday)
                .HasPrecision(10, 4)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<PayElement>()
                .Property(pe => pe.Friday)
                .HasPrecision(10, 4)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<PayElement>()
                .Property(pe => pe.Saturday)
                .HasPrecision(10, 4)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<PayElement>()
                .Property(pe => pe.Sunday)
                .HasPrecision(10, 4)
                .HasDefaultValue(0.0);

            modelBuilder.Entity<PayElementType>()
                .Property(pet => pet.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<PayElementType>()
                .HasIndex(pet => pet.Description)
                .IsUnique();

            modelBuilder.Entity<PayElementType>()
                .Property(pet => pet.Productive)
                .HasDefaultValue(false);

            modelBuilder.Entity<PayElementType>()
                .Property(pet => pet.Adjustment)
                .HasDefaultValue(false);

            modelBuilder.Entity<Scheme>()
                .Property(s => s.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Scheme>()
                .HasIndex(s => s.Description)
                .IsUnique();

            modelBuilder.Entity<Scheme>()
                .Property(s => s.ConversionFactor)
                .HasPrecision(20, 14)
                .HasDefaultValue(1.0);

            modelBuilder.Entity<Timesheet>()
                .HasIndex(t => new { t.OperativeId, t.WeekId })
                .IsUnique();

            modelBuilder.Entity<Timesheet>()
                .HasOne(t => t.Operative)
                .WithMany(o => o.Timesheets)
                .HasForeignKey(t => t.OperativeId);

            modelBuilder.Entity<Timesheet>()
                .HasOne(t => t.Week)
                .WithMany(w => w.Timesheets)
                .HasForeignKey(t => t.WeekId);

            modelBuilder.Entity<Trade>()
                .HasIndex(t => t.Description)
                .IsUnique();

            modelBuilder.Entity<Week>()
                .HasIndex(w => new { w.BonusPeriodId, w.Number })
                .IsUnique();

            modelBuilder.Entity<Week>()
                .HasOne(w => w.BonusPeriod)
                .WithMany(bp => bp.Weeks)
                .HasForeignKey(w => w.BonusPeriodId);
        }
    }
}
