using Microsoft.EntityFrameworkCore;
using story_web.Models;

namespace story_web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Story> Stories => Set<Story>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<ChapterAudio> ChapterAudios => Set<ChapterAudio>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ReadingHistory> ReadingHistories => Set<ReadingHistory>();
    public DbSet<Favourite> Favourites => Set<Favourite>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Author>()
            .HasOne(author => author.User)
            .WithOne(user => user.Author)
            .HasForeignKey<Author>(author => author.id_User)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Story>()
            .HasOne(story => story.Author)
            .WithMany(author => author.Stories)
            .HasForeignKey(story => story.id_Author)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Story>()
            .HasOne(story => story.Category)
            .WithMany(category => category.Stories)
            .HasForeignKey(story => story.id_Category)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Chapter>()
            .HasOne(chapter => chapter.Story)
            .WithMany(story => story.Chapters)
            .HasForeignKey(chapter => chapter.id_Story)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Favourite>()
            .HasOne(favourite => favourite.Story)
            .WithMany()
            .HasForeignKey(favourite => favourite.id_Story)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Favourite>()
            .HasOne(favourite => favourite.User)
            .WithMany()
            .HasForeignKey(favourite => favourite.id_User)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReadingHistory>()
            .HasOne(history => history.Story)
            .WithMany()
            .HasForeignKey(history => history.id_Story)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReadingHistory>()
            .HasOne(history => history.User)
            .WithMany()
            .HasForeignKey(history => history.id_User)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReadingHistory>()
            .HasOne(history => history.Chapter)
            .WithMany()
            .HasForeignKey(history => history.id_Chapter)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.User)
            .WithMany(user => user.Notifications)
            .HasForeignKey(notification => notification.id_User)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
