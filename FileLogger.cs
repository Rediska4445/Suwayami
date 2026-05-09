namespace Suwayami
{
    using System.Text;

    public class FileLogger
    {
        private readonly string _logPath;
        private readonly object _lock = new object();

        public FileLogger(string logDirectory = "logs")
        {
            _logPath = Path.Combine(Directory.GetCurrentDirectory(), logDirectory, $"app_{DateTime.Now:yyyyMMdd}.log");
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
        }

        public async Task LogInfoAsync(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}{Environment.NewLine}";

            lock (_lock)
            {
                Console.WriteLine(logEntry.TrimEnd());
            }

            await File.AppendAllTextAsync(_logPath, logEntry, Encoding.UTF8);
        }

        public async Task LogErrorAsync(string message, Exception? ex = null)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
            if (ex != null) logEntry += $" | {ex.Message}";
            logEntry += Environment.NewLine;

            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(logEntry.TrimEnd());
                Console.ResetColor();
            }

            await File.AppendAllTextAsync(_logPath, logEntry, Encoding.UTF8);
        }
    }
}
