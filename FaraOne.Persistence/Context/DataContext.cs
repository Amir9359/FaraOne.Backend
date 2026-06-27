using FaraOne.Domain.Entities;
using FaraOne.Domain.Entities.Attribute;
using FaraOne.Domain.User;
using FaraOne.Persistence.ModelConfings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection.Emit;
using FaraOne.Application.Context;
using FaraOne.Domain;

namespace FaraOne.Persistence.Context
{
    public class DataContext : DbContext , IDatabaseContext
    {     // فقط همین سازنده باقی بماند
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }    
        public DbSet<SmsCode> SmsCodes { get; set; }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MiniApp> MiniApps { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("dbo");

            builder.Entity<Message>()
                .HasIndex(m => m.ChatRoomId)
                .HasDatabaseName("IX_Messages_RoomId");

             
            builder.Entity<ChatRoom>()
                .HasIndex(c => c.RoomId)
                .IsUnique();

            builder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (entityType.ClrType.GetCustomAttributes(typeof(AudtableAttribute), true).Length > 0)
                {
                    builder.Entity(entityType.Name).Property<DateTime>("InsertTime").HasDefaultValue(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                    builder.Entity(entityType.Name).Property<DateTime?>("EditTime");
                    builder.Entity(entityType.Name).Property<DateTime?>("RemoveTime");
                    builder.Entity(entityType.Name).Property<bool>("IsRemoved").HasDefaultValue(false);

                }
            }

            builder.Entity<User>().HasQueryFilter(s => EF.Property<bool>(s, "IsRemoved") == false);

            builder.ApplyConfiguration(new UserConfing());

            //seed 
            //DatabaseContextSeed.CatalogSeed(builder);

            // do not create Address in Order tbl
            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            var modifiedEntities = ChangeTracker.Entries()
                .Where(s => s.State == EntityState.Added ||
                            s.State == EntityState.Modified ||
                            s.State == EntityState.Deleted
                );
            foreach (var entity in modifiedEntities)
            {
                var entityType = entity.Context.Model.FindEntityType(entity.Entity.GetType());
                if (entityType != null)
                {

                    var insertTime = entityType.FindProperty("InsertTime");
                    var updateTime = entityType.FindProperty("EditTime");
                    var removeTime = entityType.FindProperty("RemoveTime");
                    var isRemoved = entityType.FindProperty("IsRemoved");

                    if (entity.State == EntityState.Added && insertTime != null)
                    {
                        entity.Property("InsertTime").CurrentValue = DateTime.Now;
                        entity.Property("IsRemoved").CurrentValue = false;
                        var ss = entity.Property("IsRemoved").CurrentValue;
                    }
                    if (entity.State == EntityState.Modified && updateTime != null)
                    {
                        entity.Property("EditTime").CurrentValue = DateTime.Now;
                    }
                    if (entity.State == EntityState.Deleted && removeTime != null && isRemoved != null)
                    {
                        entity.Property("RemoveTime").CurrentValue = DateTime.Now;
                        entity.Property("IsRemoved").CurrentValue = true;
                        entity.State = EntityState.Modified;
                    }

                }
            }
            return base.SaveChanges();
        }
    }
     
    }