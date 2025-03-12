// See https://aka.ms/new-console-template for more information
using System.Globalization;
using FinTransProcessing;

//var file = @"D:\NET\HILEL\HomeWorks\HW-17\transactions_10_thousand.csv";
var file = @"transactions_10_thousand.csv";
//await To_Console();

ParsedData ParseLine(string lineData)
{ 
    var splitLine = lineData.Split(',');

    var dateStr = splitLine[2].Trim();

    return new ParsedData()
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


async IAsyncEnumerable<ParsedData> ParseFileAsync(string fileName, 
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

        ParsedData parsedData ;
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


async Task To_Console ()
{
        int cnt = 0;
        var semaphoreSlim = new SemaphoreSlim(1);
      
        await foreach (var data in ParseFileAsync(file, semaphoreSlim))
        {
            //await semaphoreSlim1.WaitAsync();
            lock (data)
            {
                Console.WriteLine(data);
            }
        ++cnt;
        }

    Console.WriteLine(cnt);
}





Console.WriteLine("");
Console.WriteLine("-------------------------------------");
Console.WriteLine("");


Console.WriteLine(" Calculate the total income and expenses for each user (UserId).");
Console.WriteLine("-------------------------------------");

Console.WriteLine("UserId".PadRight(38) + "Income".PadRight(8) +" Expense");

var semaphoreSlim = new SemaphoreSlim(1);
var all = ParseFileAsync(file, semaphoreSlim).ToBlockingEnumerable();
var userStats = 
            from data in all
            group data by new { data.UserId} into result
            select new
            {
                UserId = result.Key.UserId,                
                Income = result.Sum(i => i.Amount>0? i.Amount:0),
                Expense = result.Sum(i => i.Amount < 0 ? i.Amount : 0)
            };

foreach (var data in userStats)
{
    Console.WriteLine($"{data.UserId} |{data.Income} |{data.Expense}");
}

Console.WriteLine("");
Console.WriteLine("-------------------------------------");
Console.WriteLine("");

Console.WriteLine("Find the user with the highest total expenses (sum of Amount < 0).");
Console.WriteLine("-------------------------------------");
var  userExp = userStats.OrderBy(x => x.Expense).FirstOrDefault();

Console.WriteLine(userExp.UserId + " " + userExp.Expense);


var categories = all.GroupBy(x => x.Category)
                    .Select(group => new {
                            Category = group.Key,
                            Count = group.Count()
                     })
                  .OrderByDescending(g => g.Count)
                  ;
Console.WriteLine("");
Console.WriteLine("-------------------------------------");
Console.WriteLine("");
Console.WriteLine("Identify the top-3 categories by the number of transactions.");
Console.WriteLine("-------------------------------------");

Console.WriteLine("Category".PadRight(18) + "  Count");
foreach (var data in categories)
{
    Console.WriteLine(data.Category.PadRight(18) + "|  " + data.Count.ToString());
}