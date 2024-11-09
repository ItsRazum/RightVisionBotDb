using DryIoc;
using Newtonsoft.Json;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;
using Serilog;

namespace RightVisionBotDb
{
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            #region Initialization

            SQLitePCL.Batteries.Init();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger?.Information("================= RightVisionBot =================");

            Log.Logger?.Information("Регистрация сервисов...");
            App.Container.RegisterInstance(Log.Logger);
            App.Container.Register<LocationManager>(Reuse.Singleton);
            App.Container.Register<RvLogger>(Reuse.Singleton);
            App.Container.Register<LocationsFront>(Reuse.Singleton);
            App.Container.Register<CriticFormService>(Reuse.Singleton);
            App.Container.Register<ParticipantFormService>(Reuse.Singleton);

            App.Container.Register<Bot>(Reuse.Singleton);

            App.Container.Resolve<Bot>().Configure();

            #endregion

            #region Console Shell

            bool shouldContinue = true;
            while (shouldContinue)
            {
                var command = Console.ReadLine();
                if (command != null)
                {
                    var commandArgs = command.Split(' ');
                    var mainCommand = commandArgs.FirstOrDefault();

                    switch (mainCommand)
                    {
                        case "stop":
                            shouldContinue = false;
                            break;

                        case "load":
                            HandleLoadCommand(commandArgs);
                            break;
                    }
                }
            }

            void HandleLoadCommand(string[] commandArgs)
            {
                if (commandArgs.Length != 3)
                {
                    Console.WriteLine(commandArgs.Length < 3
                        ? "Недостаточно аргументов. Укажите целевую таблицу и путь к JSON-файлу."
                        : "Слишком много аргументов!");
                    return;
                }

                var targetTable = commandArgs[1].ToLower();
                var filePath = commandArgs[2];

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Указанный файл не найден!");
                    return;
                }

                try
                {
                    switch (targetTable)
                    {
                        case "users":
                            LoadUsersFromFile(filePath);
                            break;
                        default:
                            Console.WriteLine("Неизвестная таблица!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger?.Error(ex, "Ошибка обработки команды загрузки.");
                }
            }

            void LoadUsersFromFile(string filePath)
            {
                using var sr = new StreamReader(filePath);
                var data = JsonConvert.DeserializeObject<RvUser[]>(sr.ReadToEnd()) ?? throw new NullReferenceException("Данные не удалось преобразовать.");
                using var db = DatabaseHelper.GetApplicationDbContext();

                foreach (var item in data)
                {
                    if (db.RvUsers.Any(u => u.UserId == item.UserId)) continue;

                    var newUser = new RvUser
                    {
                        UserId = item.UserId,
                        Name = item.Name,
                        Telegram = item.Telegram,
                        Lang = item.Lang,
                        Location = item.Location,
                        Role = item.Role,
                        Status = item.Status,
                        Rewards = item.Rewards,
                        Punishments = item.Punishments,
                        UserPermissions = item.UserPermissions
                    };

                    db.RvUsers.Add(newUser);
                }

                if (db.ChangeTracker.HasChanges())
                    db.SaveChanges();
            }

            #endregion
        }
    }
}
