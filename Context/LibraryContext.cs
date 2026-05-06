using laboratory_4.Entity;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace laboratory_4.Context
{
    public class LibraryContext : DbContext
    {
        public LibraryContext() : base("Server=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Library;Integrated Security=True;TrustServerCertificate=True")
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<LibraryContext>());
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Book> Books { get; set; }

        public List<Book> FindBooksByAuthorQuery(string authorName)
        {
            using (var context = new LibraryContext())
            {
                var books = from book in context.Books
                            where book.Author.FullName.Contains(authorName)
                            orderby book.PublishYear descending, book.Title
                            select book;

                return books.ToList();
            }
        }

        public List<Book> FindBooksByAuthor(string authorName)
        {
            using (var context = new LibraryContext())
            {
                return context.Books
                    .Include("Author")
                    .Include("Genre")
                    .Where(b => b.Author.FullName.Contains(authorName))
                    .OrderByDescending(b => b.PublishYear)
                    .ThenBy(b => b.Title)
                    .ToList();
            }
        }

        public class BookInfo
        {
            public int BookId { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public int PublishYear { get; set; }
        }

        public List<BookInfo> FindBooksByAuthorDto(string authorName)
        {
            using (var context = new LibraryContext())
            {
                return context.Books
                    .Where(b => b.Author.FullName.Contains(authorName))
                    .OrderByDescending(b => b.PublishYear)
                    .Select(b => new BookInfo
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        AuthorName = b.Author.FullName,
                        PublishYear = b.PublishYear
                    })
                    .ToList();
            }
        }

        public List<dynamic> FindBooksByAuthorAnonymous(string authorName)
        {
            using (var context = new LibraryContext())
            {
                return context.Books
                    .Where(b => b.Author.FullName.Contains(authorName))
                    .OrderByDescending(b => b.PublishYear)
                    .Select(b => new
                    {
                        Id = b.BookId,
                        Title = b.Title,
                        Author = b.Author.FullName,
                        Year = b.PublishYear
                    })
                    .ToList<dynamic>();
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>()
                .HasKey(a => a.AuthorId)
                .Property(a => a.FullName)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Genre>()
                .HasKey(g => g.GenreId)
                .Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Book>()
                .HasKey(b => b.BookId)
                .Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(255);
            modelBuilder.Entity<Book>()
                .Property(b => b.PublishYear)
                .IsRequired();

            modelBuilder.Entity<Book>()
                .HasRequired(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId);

            modelBuilder.Entity<Book>()
                .HasRequired(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
