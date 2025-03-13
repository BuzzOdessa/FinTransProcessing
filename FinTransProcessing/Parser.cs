using System.Globalization;
using FinTransProcessing.Model;

namespace FinTransProcessing
{
    internal class Parser
    {
        public static TransactionData ParseLine(string lineData)
        {
            var splitLine = lineData.Split(',');

            var dateStr = splitLine[2].Trim();

            return new TransactionData()
            {
                TransactionId = splitLine[0],
                UserId = splitLine[1],
                //Date   = DateTime.ParseExact(splitLine[2], "yyyy-MM-dd'T'hh:mm:ss%K",CultureInfo.InvariantCulture),
                Date = DateTime.Parse(dateStr, CultureInfo.InvariantCulture),  // Отработало без явного указания формата
                Amount = decimal.Parse(splitLine[3], CultureInfo.InvariantCulture),
                Category = splitLine[4],

                Description = splitLine[5],
                Merchant = splitLine[6]
            };
        }

        public static async IAsyncEnumerable<TransactionData> ParseFileAsync(string fileName,
            SemaphoreSlim semaphoreSlim,
            CancellationToken cancelToken = default)
        {
            using var reader = new StreamReader(fileName);

            while (!reader.EndOfStream)
            {

                await semaphoreSlim.WaitAsync(cancelToken);
                var line = await reader.ReadLineAsync(cancelToken);
                semaphoreSlim.Release();
                // Тут можно еще всякие проверки втулить
                if (line == null)
                    continue;

                TransactionData parsedData;
                try
                {
                    parsedData = ParseLine(line);
                }
                catch (Exception)
                {
                    continue; // Тут можно залогировать ошибы  если шо
                }
                yield return parsedData; // В трай не лизе
            }
        }
    }
}
