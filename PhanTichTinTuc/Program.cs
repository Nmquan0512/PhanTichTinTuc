using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

namespace PhanTichTinTuc
{
    class Program
    {
        static List<string> sources = new List<string> { "VNExpress", "Tuoi Tre", "Thanh Nien", "BBC", "CNN" };
        static CancellationTokenSource cts = new CancellationTokenSource();
        static ConcurrentDictionary<string, string> fetchedData = new ConcurrentDictionary<string, string>();

        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\n===== MENU =====");
                Console.WriteLine("1. Bat dau tai du lieu");
                Console.WriteLine("2. Huy tai du lieu");
                Console.WriteLine("3. Thoat");
                Console.Write("Chon: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        cts = new CancellationTokenSource();
                        await FetchAllNewsAsync(cts.Token);
                        break;
                    case "2":
                        cts.Cancel();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Lua chon khong hop le.");
                        break;
                }
            }
        }

        static async Task<string> GetNewsAsync(string source, CancellationToken token)
        {
            Console.WriteLine($"[LOG] Dang tai tu {source}...");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                token.ThrowIfCancellationRequested();

                if (source == "CNN")
                    throw new HttpRequestException("Nguon CNN khong phan hoi.");

                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 4)), token);

                stopwatch.Stop();
                string content = $"Tin tuc tu {source} sau {stopwatch.ElapsedMilliseconds} ms";
                return content;
            }
            catch (OperationCanceledException)
            {
                return $"[HUY] Tai tu {source} da bi huy.";
            }
        }

        static async Task FetchAllNewsAsync(CancellationToken token)
        {
            var tasks = sources.Select(async source =>
            {
                try
                {
                    string result = await GetNewsAsync(source, token);
                    if (!result.StartsWith("[HUY]"))
                        fetchedData[source] = result;
                    Console.WriteLine(result);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"[LOI] {source}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LOI CHUNG] {source}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
            Console.WriteLine("\n===== KET QUA =====");
            foreach (var item in fetchedData)
            {
                Console.WriteLine($"- {item.Key}: {item.Value.Length} ky tu");
            }
            Console.WriteLine($"Tong so ky tu: {fetchedData.Values.Sum(s => s.Length)}");
        }

        static void CompareThreadAndTask()
        {
            Console.WriteLine("\nSo sanh Thread vs Task:");
            Stopwatch sw = Stopwatch.StartNew();

            Thread[] threads = new Thread[3];
            for (int i = 0; i < 3; i++)
            {
                threads[i] = new Thread(() => Thread.Sleep(2000));
                threads[i].Start();
            }
            foreach (var t in threads) t.Join();
            sw.Stop();
            Console.WriteLine($"Threads: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            Task[] tasks = new Task[3];
            for (int i = 0; i < 3; i++)
            {
                tasks[i] = Task.Delay(2000);
            }
            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine($"Tasks: {sw.ElapsedMilliseconds} ms");
        }
    }
}
