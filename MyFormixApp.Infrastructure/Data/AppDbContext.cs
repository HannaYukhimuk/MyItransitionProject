using Microsoft.EntityFrameworkCore;
using MyFormixApp.Domain.Entities;

namespace MyFormixApp.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<TemplateTheme> TemplateThemes { get; set; }
        public DbSet<TemplateAccess> TemplateAccesses { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TemplateTag> TemplateTags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Template>()
                .HasOne(t => t.User)
                .WithMany(u => u.Templates)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Template>()
                .HasOne(t => t.Theme)
                .WithMany()
                .HasForeignKey(t => t.ThemeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateAccess>()
                .HasKey(ta => new { ta.TemplateId, ta.UserId });
            modelBuilder.Entity<TemplateAccess>()
                .HasOne(ta => ta.Template)
                .WithMany(t => t.Accesses)
                .HasForeignKey(ta => ta.TemplateId);
            modelBuilder.Entity<TemplateAccess>()
                .HasOne(ta => ta.User)
                .WithMany()
                .HasForeignKey(ta => ta.UserId);

            modelBuilder.Entity<TemplateTag>()
                .HasKey(tt => new { tt.TemplateId, tt.TagId });
            modelBuilder.Entity<TemplateTag>()
                .HasOne(tt => tt.Template)
                .WithMany(t => t.Tags)
                .HasForeignKey(tt => tt.TemplateId);
            modelBuilder.Entity<TemplateTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.Templates)
                .HasForeignKey(tt => tt.TagId);

            modelBuilder.Entity<Question>(entity =>
            {
                entity.Property(q => q.Description).IsRequired(false);
                entity.Property(q => q.Type).HasDefaultValue("text");
                entity.Property(q => q.IsRequired).HasDefaultValue(true);
                entity.Property(q => q.ShowInTable).HasDefaultValue(true);
                entity.Property(q => q.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                entity.HasOne(q => q.Template)
                    .WithMany(t => t.Questions)
                    .HasForeignKey(q => q.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(q => q.Options)
                    .WithOne(o => o.Question)
                    .HasForeignKey(o => o.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<QuestionOption>()
                .Property(o => o.Text)
                .IsRequired();

            modelBuilder.Entity<Form>()
                .HasOne(f => f.Template)
                .WithMany(t => t.Forms)
                .HasForeignKey(f => f.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Form>()
                .HasOne(f => f.User)
                .WithMany(u => u.Forms)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Form)
                .WithMany(f => f.Answers)
                .HasForeignKey(a => a.FormId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Template)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Like
            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.UserId, l.TemplateId });
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Template)
                .WithMany(t => t.Likes)
                .HasForeignKey(l => l.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
