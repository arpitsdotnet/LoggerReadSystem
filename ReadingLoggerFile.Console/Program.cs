using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReadingLoggerFile.ConsoleUI
{
    public class Program
    {
        static readonly List<Client> _clients = new()
        {
            new Client("PIMR-PG", "https://accsoftpg.pimrindore.ac.in/accsoft_pg/logging/2023/10/{0}.txt", 0, 0, 0, Status.Average),
            new Client("PIMR-UG", "https://accsoftug.pimrindore.ac.in/accsoft2/logging/2023/10/{0}.txt", 0, 0, 0, Status.Average)
        };

        public static void Main(string[] args)
        {
            WriteToConsole("Hello World!");

            foreach (var client in _clients)
            {
                client.Url = string.Format(client.Url, "26");
                ReadFromFileAsync(client);

            }

            ReadFromConsole();
        }
        public static string CalculateStatus(int errorCount)
        {
            if (errorCount > 100)
                return Status.Worst.ToString();
            if (errorCount > 50)
                return Status.Bad.ToString();
            if (errorCount > 25)
                return Status.Average.ToString();
            if (errorCount > 10)
                return Status.Good.ToString();

            return Status.Best.ToString();
        }

        public static void ReadFromFileAsync(Client client)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            WriteToConsole("-------------------------------------------------------------");
            WriteToConsole("Reading logger file for {0}", client.Name);
            var theFile = client.Url;

            var delim = " ,.".ToCharArray();
            var countWords = new[] { "[ERRPR]" };
            var wordPattern = new Regex(@"\b(?:" + String.Join("|", countWords) + @")\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var newFile = "E:/Temp/PIMR/";
            var newFileName = "20231028.txt";

            if (!Directory.Exists(newFile))
                Directory.CreateDirectory(newFile);

            using (WebClient webClient = new())
            {
                webClient.DownloadFile(theFile, newFile + newFileName);
                webClient.Dispose();
            }

            var count = File.ReadLines(newFile + newFileName).Select(l => wordPattern.Matches(l).Count).Sum();

            WriteToConsole("You have {0} error(s) found today.", count);
            WriteToConsole("Your System Status for today is {0}", CalculateStatus(count));
            WriteToConsole("");

            var counterForAll = 0;
            var counterWithLoadReportFailed = 0;
            var counterWithoutLoadReportFailed = 0;
            List<string> linesWithoutLoadReportFailed = new();
            List<string> linesWithLoadReportFailed = new();
            foreach (var lines in File.ReadAllLines(newFile + newFileName))
            {
                counterForAll++;
                if (lines.StartsWith("[ERROR]") && !lines.Contains("Load report failed."))
                {
                    linesWithoutLoadReportFailed.Add("Line: " + counterForAll + " | " + lines);
                    counterWithoutLoadReportFailed++;
                }
                else if (lines.StartsWith("[ERROR]"))
                {
                    linesWithLoadReportFailed.Add("Line: " + counterForAll + " | " + lines);
                    counterWithLoadReportFailed++;
                }
            }

            WriteToConsole("Where these {0} are important: ", counterWithoutLoadReportFailed);

            foreach (var item in linesWithoutLoadReportFailed)
            {
                WriteToConsole(item);
                WriteToConsole("");
            }

            WriteToConsole("");
            WriteToConsole("All other errors: ", counterWithLoadReportFailed);

            foreach (var item in linesWithLoadReportFailed)
            {
                WriteToConsole(item);
                WriteToConsole("");
            }

        }

        public static string ReadFromConsole()
        {
            return Console.ReadLine();
        }
        public static void WriteToConsole(string message)
        {
            Console.WriteLine(message);
        }

        public static void WriteToConsole<T>(string message, T data)
        {
            Console.WriteLine(string.Format(message, data));
        }

        public class Client
        {
            public Client(string Name, string Url, int ErrorCount, int DebugCount, int InfoCount, Status Status)
            {
                this.Name = Name;
                this.Url = Url;
                this.ErrorCount = ErrorCount;
                this.DebugCount = DebugCount;
                this.InfoCount = InfoCount;
                this.Status = Status;
            }

            public string Name { get; set; }
            public string Url { get; set; }
            public int ErrorCount { get; set; }
            public int DebugCount { get; set; }
            public int InfoCount { get; set; }
            public Status Status { get; set; }
        }

        public enum Status
        {
            Worst = 5,
            Bad = 4,
            Average = 3,
            Good = 2,
            Best = 1
        }
    }
}