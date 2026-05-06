using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using laboratory_4.Context;
using laboratory_4.Entity;
using Microsoft.AspNetCore.Hosting;

namespace laboratory_4
{
    class Program
    {
        static List<MenuOption> menuOptions;
        static LibraryContext context;

        static void Main()
        {
            context = new LibraryContext();
            InitializeGenres();
            InitializeMenu();

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "БИБЛИОТЕКА v4.0";

            int selectedIndex = 0;
            WriteMenu(selectedIndex);

            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.DownArrow when selectedIndex < menuOptions.Count - 1:
                        selectedIndex++;
                        WriteMenu(selectedIndex);
                        break;
                    case ConsoleKey.UpArrow when selectedIndex > 0:
                        selectedIndex--;
                        WriteMenu(selectedIndex);
                        break;
                    case ConsoleKey.Enter:
                        menuOptions[selectedIndex].Action();
                        Thread.Sleep(300);
                        Console.Clear();
                        WriteMenu(0);
                        break;
                }
            } while (keyInfo.Key != ConsoleKey.Escape);

            context.Dispose();
        }

        static void InitializeGenres()
        {
            if (!context.Genres.Any())
            {
                context.Genres.AddRange(new[]
                {
                    new Genre { Name = "Фантастика" },
                    new Genre { Name = "Фэнтези" },
                    new Genre { Name = "Детектив" },
                    new Genre { Name = "Роман" },
                    new Genre { Name = "Классика" },
                    new Genre { Name = "Научная литература" },
                    new Genre { Name = "Поэзия" },
                    new Genre { Name = "История" },
                    new Genre { Name = "Биография" },
                    new Genre { Name = "Юмор" }
                });
                context.SaveChanges();
            }
        }

        static void AddAuthor()
        {
            Console.Clear();
            PrintHeader("➕ Добавление автора");

            Console.Write("👤 ФИО автора: ");
            string fullName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowMessage("❌ Имя не может быть пустым!", ConsoleColor.Red);
                return;
            }

            try
            {
                var author = new Author { FullName = fullName };
                context.Authors.Add(author);
                context.SaveChanges();
                ShowMessage($"✅ Автор '{fullName}' добавлен! ID: {author.AuthorId}", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Ошибка: {ex.Message}", ConsoleColor.Red);
            }
        }

        static void AddBook()
        {
            Console.Clear();
            PrintHeader("Добавление книги");

            var authors = context.Authors.ToList();
            if (!authors.Any())
            {
                ShowMessage("Сначала добавьте авторов!", ConsoleColor.Yellow);
                return;
            }

            PrintList("Авторы:", authors.Select(a => $"{a.AuthorId,3}. {a.FullName}").ToList());

            Console.Write("Выберите автора (ID): ");
            if (!int.TryParse(Console.ReadLine(), out int authorId) ||
                !authors.Any(a => a.AuthorId == authorId))
            {
                ShowMessage("Неверный ID автора!", ConsoleColor.Red);
                return;
            }

            var genres = context.Genres.ToList();
            PrintList("Жанры:", genres.Select(g => $"{g.GenreId,3}. {g.Name}").ToList());

            Console.Write("Выберите жанр (ID): ");
            if (!int.TryParse(Console.ReadLine(), out int genreId) ||
                !genres.Any(g => g.GenreId == genreId))
            {
                ShowMessage("Неверный ID жанра!", ConsoleColor.Red);
                return;
            }

            Console.Write("Название книги: ");
            string title = Console.ReadLine()?.Trim();

            Console.Write("Год издания: ");
            if (!int.TryParse(Console.ReadLine(), out int year) || year < 1500 || year > 2026)
            {
                ShowMessage("Год должен быть 1500-2026!", ConsoleColor.Red);
                return;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                ShowMessage("Название не может быть пустым!", ConsoleColor.Red);
                return;
            }

            try
            {
                var book = new Book
                {
                    Title = title,
                    AuthorId = authorId,
                    GenreId = genreId,
                    PublishYear = year
                };
                context.Books.Add(book);
                context.SaveChanges();
                ShowMessage($"Книга '{title}' добавлена! ID: {book.BookId}", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Ошибка: {ex.Message}", ConsoleColor.Red);
                ShowMessage($"Полная ошибка: {ex}", ConsoleColor.Red);
                ShowMessage($"Inner: {ex.InnerException?.Message}", ConsoleColor.Red);

                Console.WriteLine("=== ПОЛНЫЙ СТЕК ТРЕЙС ===");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("==========================");

                Console.ReadKey();
            }
        }

        static void SearchBooksByAuthor()
        {
            Console.Clear();
            PrintHeader("🔍 Поиск книг по автору");

            Console.Write("👤 Имя автора: ");
            string authorName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(authorName))
            {
                ShowMessage("❌ Введите имя автора!", ConsoleColor.Red);
                return;
            }

            var books = context.FindBooksByAuthor(authorName);

            Console.WriteLine($"\n📚 Найдено книг: {books.Count}");
            if (books.Any())
            {
                Console.WriteLine("\n" + "Название".PadRight(35) + "| Год  | Жанр".PadRight(15) + "| Автор");
                Console.WriteLine(new string('─', 75));
                foreach (var book in books)
                {
                    Console.WriteLine($"{book.Title.PadRight(35)}| {book.PublishYear,4} | {book.Genre.Name.PadRight(13)}| {book.Author.FullName}");
                }
            }
            else
            {
                Console.WriteLine("  Нет книг по заданному автору.");
            }
            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        static void ShowBooksByGenre()
        {
            Console.Clear();
            PrintHeader("📊 Книги по жанрам");

            var genreStats = context.Books
                .GroupBy(b => b.Genre.Name)
                .Select(g => new { Genre = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            Console.WriteLine("Жанр".PadRight(25) + " | Кол-во книг");
            Console.WriteLine(new string('═', 40));

            foreach (var item in genreStats)
            {
                Console.WriteLine($"{item.Genre.PadRight(25)} | {item.Count,9}");
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        static void BooksByYearRange()
        {
            Console.Clear();
            PrintHeader("📅 Книги за период");

            Console.Write("С какого года: ");
            if (!int.TryParse(Console.ReadLine(), out int fromYear))
            {
                ShowMessage("❌ Неверный год!", ConsoleColor.Red);
                return;
            }

            Console.Write("По какой год: ");
            if (!int.TryParse(Console.ReadLine(), out int toYear) || toYear < fromYear)
            {
                ShowMessage("❌ Неверный диапазон!", ConsoleColor.Red);
                return;
            }

            var books = context.Books
                .Include("Author")
                .Where(b => b.PublishYear >= fromYear && b.PublishYear <= toYear)
                .OrderBy(b => b.PublishYear)
                .Take(20)
                .ToList();

            Console.WriteLine($"\n📚 Книг за период {fromYear}-{toYear}: {books.Count}");
            Console.WriteLine("\n" + "Название".PadRight(40) + "| Год | Автор");
            Console.WriteLine(new string('─', 60));
            foreach (var book in books)
            {
                Console.WriteLine($"{book.Title.PadRight(40)}| {book.PublishYear,4} | {book.Author.FullName}");
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        static void ShowTopAuthors()
        {
            Console.Clear();
            PrintHeader("🏆 Топ авторов");

            var topAuthors = context.Books
                .GroupBy(b => b.Author.FullName)
                .Select(g => new { Author = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            Console.WriteLine("Автор".PadRight(40) + " | Книг");
            Console.WriteLine(new string('═', 50));

            for (int i = 0; i < topAuthors.Count; i++)
            {
                var author = topAuthors[i];
                Console.WriteLine($"  {i + 1}. {author.Author.PadRight(39)} | {author.Count,4}");
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        static void InitializeMenu()
        {
            menuOptions = new List<MenuOption>
            {
                new MenuOption("➕ Добавить автора", AddAuthor),
                new MenuOption("📖 Добавить книгу", AddBook),
                new MenuOption("🔍 Поиск по автору", SearchBooksByAuthor),
                new MenuOption("📊 Книги по жанрам", ShowBooksByGenre),
                new MenuOption("📅 Книги за период", BooksByYearRange),
                new MenuOption("🏆 Топ авторов", ShowTopAuthors),
                new MenuOption("❌ Выход", () => Environment.Exit(0))
            };
        }

        static void WriteMenu(int selectedIndex)
        {
            Console.Clear();
            PrintHeader("БИБЛИОТЕКА v4.0");

            for (int i = 0; i < menuOptions.Count; i++)
            {
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine($"  {menuOptions[i].Name}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {menuOptions[i].Name}");
                }
            }

            Console.WriteLine("\n" + new string('═', 50));
            Console.WriteLine("↑↓ - выбор | Enter - выполнить | Esc - выход");
        }

        static void PrintHeader(string title)
        {
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine($"                    {title,-25}                       ");
            Console.WriteLine("══════════════════════════════════════════════════════════\n");
        }

        static void PrintList(string header, List<string> items)
        {
            Console.WriteLine($"\n{header}:");
            foreach (var item in items)
            {
                Console.WriteLine($"  {item}");
            }
            Console.WriteLine();
        }

        static void ShowMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"\n{message}");
            Console.ResetColor();
            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey();
        }
    }

    public class MenuOption
    {
        public string Name { get; }
        public Action Action { get; }

        public MenuOption(string name, Action action)
        {
            Name = name;
            Action = action;
        }
    }
}