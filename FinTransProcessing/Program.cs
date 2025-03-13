// See https://aka.ms/new-console-template for more information
using System;
using System.Globalization;
using System.Numerics;
using FinTransProcessing;
using FinTransProcessing.EF;
using FinTransProcessing.Model;

//var file = @"D:\NET\HILEL\HomeWorks\HW-17\transactions_10_thousand.csv";
var file = @"transactions_10_thousand.csv";
//await To_Console();
//PerformDatabaseOperations();
await To_Database(new(),10000 );

async Task To_Database(object lockObj, int portion =1000)
{
    int cnt = 0;
    var semaphoreSlim = new SemaphoreSlim(1); // Сугубо для   ParseFileAsync  
    List<TransactionData> transactionDatas = new();
    using (var db = new FinTransDbContext())
    {
      db.makeLog = false;
      await foreach (var data in Parser.ParseFileAsync(file, semaphoreSlim))
      {   
            lock (lockObj)
            {                
                transactionDatas.Add(data);
                if (transactionDatas.Count % portion == 0)
                { 
                    db.Transactions.AddRange(transactionDatas);
                    db.SaveChanges();
                    transactionDatas.Clear();
                }
                if (cnt % 1000 == 0)
                    Console.WriteLine($"Обработано {cnt} записей");
            }
            ++cnt;
      }
    // Если какой то остаток остался
      if (transactionDatas.Count > 0)
      {
        db.Transactions.AddRange(transactionDatas);
        db.SaveChanges();
        transactionDatas.Clear();
      }
    }

    Console.WriteLine(cnt);
}


//https://www.codeproject.com/Tips/1046655/Very-Basic-Console-Application-Using-Entity-Framew
TransactionData ParseLine(string lineData)
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


async Task To_Console ()
{
        int cnt = 0;
        var semaphoreSlim = new SemaphoreSlim(1);
      
        await foreach (var data in Parser.ParseFileAsync(file, semaphoreSlim))
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




void FirstBadCode ()
{ 



Console.WriteLine("");
Console.WriteLine("-------------------------------------");
Console.WriteLine("");


Console.WriteLine(" Calculate the total income and expenses for each user (UserId).");
Console.WriteLine("-------------------------------------");

Console.WriteLine("UserId".PadRight(38) + "Income".PadRight(8) +" Expense");

var semaphoreSlim = new SemaphoreSlim(1);
var all = Parser.ParseFileAsync(file, semaphoreSlim).ToBlockingEnumerable();
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
}