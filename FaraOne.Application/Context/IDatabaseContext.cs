using FaraOne.Domain.Entities;
using FaraOne.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics.CodeAnalysis;
using FaraOne.Domain;

namespace FaraOne.Application.Context;

public interface IDatabaseContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
    public DbSet<SmsCode> SmsCodes { get; set; }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MiniApp> MiniApps { get; set; }

    int SaveChanges();
    int SaveChanges(bool acceptAllChangesOnSuccess);

    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new CancellationToken());

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
    EntityEntry Entry([NotNull] object entity);
}