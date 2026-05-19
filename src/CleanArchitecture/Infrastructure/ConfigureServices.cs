using CleanArchitecture.Application;
using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Repositories;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructuresService(this IServiceCollection services, AppSettings configuration)
    {
        if (configuration.UseInMemoryDatabase)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("CleanArchitecture"));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.ConnectionStrings.DefaultConnection));
        }

        services.AddIdentity<ApplicationUser, RoleIdentity>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

        // register services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IContainerRepository, ContainerRepository>();
        services.AddTransient<IDepotRepository, DepotRepository>();
        services.AddScoped<IBlockRepository, BlockRepository>();
        services.AddTransient<IContainerPositionRepository, ContainerPositionRepository>();
        services.AddTransient<IContainerTransactionRepository, ContainerTransactionRepository>();
        services.AddTransient<ICustomerRepository, CustomerRepository>();
        services.AddTransient<ILineOperatorRepository, LineOperatorRepository>();
        services.AddTransient<IContainerTypeRepository, ContainerTypeRepository>();
        services.AddTransient<IDeliveryOrderRepository, DeliveryOrderRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddTransient<ApplicationDbContextInitializer>();

        return services;
    }
}
