const string GET = "<image-link>";
const string DIR = @"<download-folder>";
const int THREAD_COUNT = 10;
const int EXPECTED_DOWNLOAD_QUANTITY = 100_000;

using var client = new HttpClient();

var threads = Enumerable.Range(0, THREAD_COUNT)
    .Select(n => new Thread(new ThreadStart(Run().GetAwaiter().GetResult)))
    .ToList();

threads.ForEach(t => t.Start());

threads.ForEach(t => t.Join());

async Task Run()
{
    var filecount = Directory.GetFiles(DIR, "*", SearchOption.AllDirectories).Length;
    var total = EXPECTED_DOWNLOAD_QUANTITY;
    while (filecount <= total)
        try
        {
            using var response = await client.GetAsync(GET);

            response.EnsureSuccessStatusCode();

            var base64imageString = (await response.Content.ReadAsStringAsync()).Split(',')[1];

            await File.WriteAllBytesAsync(DIR + $"\\{Guid.NewGuid()}.png", Convert.FromBase64String(base64imageString));

            await Task.Delay(TimeSpan.FromSeconds(0.5));

            filecount = Directory.GetFiles(DIR, "*", SearchOption.AllDirectories).Length;
            Console.WriteLine($"{Math.Round(((double)filecount/(double)total)*100.0, 2)}%");
        }
        catch
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    
}