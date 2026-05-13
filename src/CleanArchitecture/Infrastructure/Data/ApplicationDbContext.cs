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
    public DbSet<Customer> Customers { get; set; }
    public DbSet<LineOperator> LineOperators { get; set; }
    public DbSet<ContainerType> ContainerTypes { get; set; }
    public DbSet<DeliveryOrder> DeliveryOrders { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Media> Media { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Entity<Container>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ContainerNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.ContainerOwner)
                .HasMaxLength(255);

            entity.Property(x => x.ContainerCondition)
                .HasMaxLength(255);

            entity.Property(x => x.ContainerClassification)
                .HasMaxLength(50);

            entity.Property(x => x.CurrentStatus)
                .HasMaxLength(50);

            entity.HasOne(x => x.ContainerTypeNavigation)
                .WithMany(x => x.Containers)
                .HasForeignKey(x => x.ContainerTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LineOperator)
                .WithMany(x => x.Containers)
                .HasForeignKey(x => x.LineOperatorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
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

            entity.HasOne(x => x.DeliveryOrder)
                .WithMany(x => x.ContainerTransactions)
                .HasForeignKey(x => x.DeliveryOrderId)
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
        builder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.CustomerTaxCode)
                .HasMaxLength(50);

            entity.Property(x => x.Address)
                .HasMaxLength(500);
        });

        builder.Entity<LineOperator>(entity =>
        {
            entity.HasKey(x => x.Id);
        
            entity.Property(x => x.LineOperatorCode)
                .IsRequired()
                .HasMaxLength(50);
        
            entity.Property(x => x.LineOperatorName)
                .IsRequired()
                .HasMaxLength(255);
        });
        
        builder.Entity<ContainerType>(entity =>
        {
            entity.HasKey(x => x.Id);
        
            entity.Property(x => x.ContainerTypeCode)
                .IsRequired()
                .HasMaxLength(50);
        
            entity.Property(x => x.ContainerTypeName)
                .IsRequired()
                .HasMaxLength(255);
        
            entity.Property(x => x.ISOCode)
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.Entity<DeliveryOrder>(entity =>
        {
            entity.HasKey(x => x.Id);
        
            entity.Property(x => x.DONumber)
                .IsRequired()
                .HasMaxLength(100);
        
            entity.Property(x => x.VesselVoyage)
                .HasMaxLength(255);
        
            entity.Property(x => x.OrderStatus)
                .HasMaxLength(50);
        
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.DeliveryOrders)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        
            entity.HasOne(x => x.LineOperator)
                .WithMany(x => x.DeliveryOrders)
                .HasForeignKey(x => x.LineOperatorId)
                .OnDelete(DeleteBehavior.Restrict);
        
            entity.HasOne(x => x.ContainerType)
                .WithMany(x => x.DeliveryOrders)
                .HasForeignKey(x => x.ContainerTypeId)
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
