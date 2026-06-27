using FaraOne.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaraOne.Persistence.ModelConfings;

public class UserConfing: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(300);
        builder.Property(s => s.PhoneNumber).HasMaxLength(20);
        builder.Property(s => s.NormalizedUserName).HasMaxLength(20);
        builder.Property(s => s.UserName).HasMaxLength(20);
        builder.Property(s => s.Email).HasMaxLength(60);

        builder.Property(s => s.Avatar).HasMaxLength(500);
        builder.Property(s => s.Role).HasMaxLength(20);
        builder.Property(s => s.ConcurrencyStamp).HasMaxLength(45);
        builder.Property(s => s.SecurityStamp).HasMaxLength(45);
        builder.Property(s => s.PasswordHash).HasMaxLength(100);

    }
}