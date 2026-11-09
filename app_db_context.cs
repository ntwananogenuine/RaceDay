using Microsoft.EntityFrameworkCore;
using raceday_api.Models;

namespace raceday_api.database
{//start of namespace
    //connects the api to the raceday database (code first)
    public class AppDbContext : DbContext
    {//start of class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder b)
        {//start of method
            b.Entity<Role>(e =>
            {
                e.Property(r => r.RoleName).HasMaxLength(20).IsUnicode(false).IsRequired();
                e.HasIndex(r => r.RoleName).IsUnique();
                e.HasData(new Role { RoleId = 1, RoleName = "Organiser" },
                          new Role { RoleId = 2, RoleName = "Participant" });
            });

            b.Entity<User>(e =>
            {
                e.Property(u => u.FullName).HasMaxLength(100).IsUnicode(false).IsRequired();
                e.Property(u => u.Email).HasMaxLength(150).IsUnicode(false).IsRequired();
                e.Property(u => u.PasswordHash).HasMaxLength(255).IsUnicode(false).IsRequired();
                e.Property(u => u.ProfilePicture).HasMaxLength(255).IsUnicode(false);
                e.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.HasIndex(u => u.Email).IsUnique();
                e.HasOne(u => u.Role).WithMany(r => r.Users)
                 .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<Event>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(150).IsUnicode(false).IsRequired();
                e.Property(x => x.Description).HasMaxLength(1000).IsUnicode(false);
                e.Property(x => x.EventDate).HasColumnType("date");
                e.Property(x => x.Location).HasMaxLength(150).IsUnicode(false).IsRequired();
                e.Property(x => x.EventType).HasMaxLength(20).IsUnicode(false).IsRequired();
                e.Property(x => x.BannerImage).HasMaxLength(255).IsUnicode(false);
                e.Property(x => x.DistanceKm).HasColumnType("decimal(5,2)");
                e.HasOne(x => x.Organiser).WithMany()
                 .HasForeignKey(x => x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<Category>(e =>
            {
                e.Property(c => c.Name).HasMaxLength(100).IsUnicode(false).IsRequired();
                e.Property(c => c.DistanceKm).HasColumnType("decimal(5,2)");
                e.Property(c => c.MaxParticipants).HasDefaultValue(100);
                e.HasOne(c => c.Event).WithMany(ev => ev.Categories)
                 .HasForeignKey(c => c.EventId).OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<Enrolment>(e =>
            {
                e.Property(x => x.BibNumber).HasMaxLength(10).IsUnicode(false).IsRequired();
                e.Property(x => x.Status).HasMaxLength(20).IsUnicode(false).IsRequired();
                e.Property(x => x.EnrolmentDate).HasDefaultValueSql("GETDATE()");
                e.HasIndex(x => new { x.ParticipantId, x.CategoryId }).IsUnique();
                e.HasIndex(x => new { x.CategoryId, x.BibNumber }).IsUnique();
                e.HasOne(x => x.Participant).WithMany()
                 .HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Category).WithMany(c => c.Enrolments)
                 .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<Result>(e =>
            {
                e.Property(r => r.FinishTime).HasColumnType("time");
                e.Property(r => r.Status).HasMaxLength(20).IsUnicode(false).IsRequired();
                e.HasOne(r => r.Enrolment).WithOne(en => en.Result)
                 .HasForeignKey<Result>(r => r.EnrolmentId).OnDelete(DeleteBehavior.Cascade);
            });
        }//end of method
    }//end of class
}//end of namespace
