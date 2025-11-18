using CalculatorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CalculatorApp.Data
{
    public class CalculatorContext : DbContext
    {
        public CalculatorContext(DbContextOptions<CalculatorContext> options) : base(options)
        {
        }

        public DbSet<CalculationHistory> CalculationHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalculationHistory>(entity =>
            {
                entity.ToTable("CalculationHistory");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Operation)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Operand1)
                    .IsRequired()
                    .HasColumnType("double");

                entity.Property(e => e.Operand2)
                    .HasColumnType("double");

                entity.Property(e => e.Result)
                    .IsRequired()
                    .HasColumnType("double");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime");
            });
        }
    }
}