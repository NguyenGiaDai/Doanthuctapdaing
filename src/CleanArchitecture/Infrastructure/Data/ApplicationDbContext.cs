using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace CleanArchitecture.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
            IdentityDbContext<ApplicationUser, RoleIdentity, Guid,
            IdentityUserClaim<Guid>, UserRoles, IdentityUserLogin<Guid>,
            IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>(options)
{
    public DbSet<Depot> Depots { get; set; }
    public DbSet<Container> Containers { get; set; }
    public DbSet<Block> Blocks { get; set; }
    public DbSet<ContainerPosition> ContainerPositions { get; set; }
    public DbSet<ContainerTransaction> ContainerTransactions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Media> Media { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Entity<ContainerTransaction>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TransactionType)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.VehicleNumber)
                .HasMaxLength(50);

            entity.Property(x => x.Note)
                .HasMaxLength(500);

            entity.HasOne(x => x.Container)
                .WithMany(x => x.ContainerTransactions)
                .HasForeignKey(x => x.ContainerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FromBlock)
                .WithMany(x => x.FromContainerTransactions)
                .HasForeignKey(x => x.FromBlockId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ToBlock)
                .WithMany(x => x.ToContainerTransactions)
                .HasForeignKey(x => x.ToBlockId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin").HasKey(l => new
        {
            l.LoginProvider,
            l.ProviderKey,
            l.UserId
        });
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens").HasKey(x => x.UserId);

        builder.Seed();
    }
}
