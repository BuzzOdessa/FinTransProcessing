// See https://aka.ms/new-console-template for more information
using System.IO;
using System.Text.Json;
using FinTransProcessing;
using FinTransProcessing.EF;
using FinTransProcessing.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


//var file = @"D:\NET\HILEL\HomeWorks\HW-17\transactions_10_thousand.csv";
var file = @"transactions_10_thousand.csv";

var    doLog = false;
var    logDbCommandOnly = false;
bool   fillDatabase = true;

string connectionStr;
var    jsonOptions = new JsonSerializerOptions { WriteIndented = true };



Init();
if (fillDatabase)
  await To_Database(new(),5000 ); // Закинуть в БД. 
CreateJSON_Report();
ShowJSON();


//----------------------------------------------

void ShowJSON()
{
    Console.WriteLine("JSON Report создан");
    Console.WriteLine("Показать файл отчета в консоли? Y/N");
    var k = Console.ReadKey().KeyChar;
    Console.WriteLine("");
    if (k == 'Y' || k == 'y')
    {
        string readText = File.ReadAllText("json_report.json");
        Console.Write(readText);
    }
}

void CreateJSON_Report()
{
    var fileName = "json_report.json";
    if (File.Exists(fileName))
      File.Delete(fileName); 

    using var writer = new StreamWriter(fileName);
    writer.WriteLine('{');    
    WriteSection(UsersSummary() , true);    // Calculate the total income and expenses for each user (UserId).   
    WriteSection(TopCategories(), true);    // Identify the top-3 categories by the number of transactions. 
    WriteSection(HighestSpender(), false);  // Find the user with the highest total expenses (sum of Amount < 0). 
    writer.WriteLine('}');
    void WriteSection(string section, bool comma)
    {
        writer.Write(section);
        if (comma)
        {
            writer.WriteLine(',');            
        }
        writer.WriteLine("");
    }    
}

void Init()
{
    if (!File.Exists("appsettings.json"))
        throw new Exception("Не найден файл appsettings.json");
    if (!File.Exists(file))
        throw new Exception($"Не найден файл {file}");

    var cfgBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false);
    var cfg = cfgBuilder.Build();

    connectionStr = cfg.GetConnectionString("FinTransDb");

    if (connectionStr == null || connectionStr.Length == 0)
        throw new Exception($"Не удалось считать параметры коннекта к Postgress");

    Console.WriteLine("Выполнять заполнение базы данных? Y/N");
    var k = Console.ReadKey().KeyChar;
    Console.WriteLine("");
    fillDatabase = (k == 'Y' || k == 'y');

    Console.WriteLine("Выводить тексты SQL? Y/N");
    k = Console.ReadKey().KeyChar;
    Console.WriteLine("");
    
    logDbCommandOnly = k == 'Y' || k == 'y';
    doLog = doLog || logDbCommandOnly;
}

async Task To_Database(object lockObj, int portion =1000)
{
    Console.WriteLine($"Заполняю таблицу в БД");
    int cnt = 0;
    var semaphoreSlim = new SemaphoreSlim(1); // Сугубо для   ParseFileAsync  
    List<TransactionData> transactionDatas = new();
    using (var db = new FinTransDbContext(connectionStr))
    {
      db.makeLog = doLog;

      db.Transactions.ExecuteDelete(); // Грохнем все. Чисто для удобства тестов
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
                if (cnt % portion == 0)
                    Console.WriteLine($"Обработано {cnt} записей");
            }
            ++cnt;
      }
    // Если какой то остаток остался, например при порции = 33
      if (transactionDatas.Count > 0)
      {
        db.Transactions.AddRange(transactionDatas);
        db.SaveChanges();
        transactionDatas.Clear();
      }
    }
    Console.WriteLine(cnt);
}

async Task TransToConsole ()
{
        int cnt = 0;
        var semaphoreSlim = new SemaphoreSlim(1);
      
        await foreach (var data in Parser.ParseFileAsync(file, semaphoreSlim))
        {            
            lock (data)
            {
                Console.WriteLine(data);
            }
        ++cnt;
        }
    Console.WriteLine(cnt);
}

void TitleToConsole(string title, string dataTitle)
{
    Console.WriteLine("");
    Console.WriteLine("--------------------------------------------------------------------------");
    Console.WriteLine("");
    Console.WriteLine(title);
    Console.WriteLine("--------------------------------------------------------------------------");
    Console.WriteLine(dataTitle);
    Console.WriteLine("-".PadRight(dataTitle.Length, '-'));
}

string UsersSummary(bool to_Console = false)
{
    Console.WriteLine($"Анализирую UsersSummary");
    if (to_Console)
        TitleToConsole(" Calculate the total income and expenses for each user (UserId)."
           , "UserId".PadRight(37) + "| Income".PadRight(8) + " | Expense"
        );
    
    using (var db = new FinTransDbContext(connectionStr))
    {
        db.makeLog = doLog;
        db.logDbCommandOnly = logDbCommandOnly;

        var stats =
            from row in db.Transactions
            group row by new { row.UserId } into result
            select new
            {
                UserId = result.Key.UserId,
                Income = result.Sum(i => i.Amount > 0 ? i.Amount : 0),
                Expense = result.Sum(i => i.Amount < 0 ? i.Amount : 0)
            };

        if (to_Console)
        {
            foreach (var data in stats)         
                Console.WriteLine($"{data.UserId} |{data.Income} |{data.Expense}");

            return "";
        }
        else
        {
            
            string jsonString = "\"users_summary\":" + JsonSerializer.Serialize(stats, jsonOptions);
            return jsonString;
        }
    }
}

string HighestSpender(bool to_Console=false)
{
    Console.WriteLine($"Анализирую HighestSpender");
    if (to_Console)
        TitleToConsole(
              "Find the user with the highest total expenses (sum of Amount < 0)."
            , "UserId".PadRight(37) + "| Expense"
        );
    using (var db = new FinTransDbContext(connectionStr))
    {
        db.makeLog = doLog;
        db.logDbCommandOnly = logDbCommandOnly;

        var stats =
            from row in db.Transactions
            group row by new { row.UserId } into result
            orderby result.Sum(i => i.Amount < 0 ? i.Amount : 0) descending
            select new
            {
                UserId = result.Key.UserId,
                Expense = result.Sum(i => i.Amount < 0 ? i.Amount : 0)
            }
            ;
        stats = stats.Take(1);
        if (to_Console)
        { 
                foreach (var data in stats)                
                    Console.WriteLine($"{data.UserId} |{data.Expense}");
                
                return "";
        }
        else
        {
            string jsonString = JsonSerializer.Serialize(stats, jsonOptions);
            jsonString = jsonString.Substring(1, jsonString.Length-2);
            jsonString = "\"highest_spender\":" + jsonString;                    
            return jsonString;
        } 
    }
}

string TopCategories(bool to_Console = false)
{
    Console.WriteLine($"Анализирую TopCategories");
    if (to_Console)
     TitleToConsole("Identify the top-3 categories by the number of transactions."
      , "Category".PadRight(19) + "|  Count"
     );
    using (var db = new FinTransDbContext(connectionStr))
    {
        db.makeLog = doLog;
        db.logDbCommandOnly = logDbCommandOnly;

        var stats =
            from row in db.Transactions
            group row by new { row.Category } into result
            orderby result.Count() descending
            select new
            {
                Category = result.Key.Category,
                Count = result.Count()
            }
            ;
        stats = stats.Take(3);
        if (to_Console)
        {
            foreach (var data in stats)         
                Console.WriteLine($"{data.Category.PadRight(18)} | {data.Count}");
            return "";
        }
        else
        {
            string jsonString = "\"top_categories\":" + JsonSerializer.Serialize(stats, jsonOptions);
            return jsonString;
        }
    }
}