using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reddit.Models;

namespace Reddit.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<Page> Pages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Community>(entity =>
            {
                entity.HasOne(c => c.Owner)
                      .WithMany(u => u.OwnedCommunities)
                      .HasForeignKey(e => e.OwnerId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(c => c.Subscribers)
                      .WithMany(u => u.SubscribedCommunities);
            });
        }
    }
}
