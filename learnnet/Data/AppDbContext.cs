using learnnet.Entities;
using Microsoft.EntityFrameworkCore;

namespace learnnet.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductDetail> ProductDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure 1-to-1 relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Details)
                .WithOne(d => d.Product)
                .HasForeignKey<ProductDetail>(d => d.ProductId);
        }
    }
}
