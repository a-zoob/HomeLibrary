using Microsoft.EntityFrameworkCore;
using HomeLibrary.Domain.Entities;

namespace HomeLibrary.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        // принимает строку подключения из API
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>(entity =>
            {
                // Имя таблицы в БД
                entity.ToTable("Books");

                // Первичный ключ
                entity.HasKey(b => b.Id);

                // Обязательные текстовые поля с ограничением по длине
                entity.Property(b => b.Title)
                      .IsRequired()
                      .HasMaxLength(250);

                entity.Property(b => b.Author)
                      .IsRequired()
                      .HasMaxLength(150);

                // Год издания
                entity.Property(b => b.PublishingYear)
                      .IsRequired();

                entity.Property(b => b.TableOfContentsXml)
                      .HasColumnType("xml")
                      .IsRequired();
            });
        }
    }
}
