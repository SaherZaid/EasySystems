using EasySystems.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace EasySystems.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<EmailVerificationCode> EmailVerificationCodes => Set<EmailVerificationCode>();

    public DbSet<StoreRequest> StoreRequests => Set<StoreRequest>();

    public DbSet<StoreQuestionAnswer> StoreQuestionAnswers => Set<StoreQuestionAnswer>();

    public DbSet<PackagePlan> PackagePlans => Set<PackagePlan>();
    public DbSet<ContactLead> ContactLeads => Set<ContactLead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PackagePlan>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);
    }
}