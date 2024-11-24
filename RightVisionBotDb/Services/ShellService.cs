using Newtonsoft.Json;
using RightVisionBotDb.Converters;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Models;
using Serilog;

namespace RightVisionBotDb.Services
{
    public class ShellService
    {
        private ILogger _logger;
        private UserPermissionsConverter UserPermissionsConverter { get; set; }
        private LocationConverter LocationConverter { get; set; }

        public ShellService(
            ILogger logger,
            UserPermissionsConverter userPermissionsConverter,
            LocationConverter locationConverter)
        {
            _logger = logger;
            UserPermissionsConverter = userPermissionsConverter;
            LocationConverter = locationConverter;
        }

        public void Run()
        {
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
                        case "critics":
                            LoadCriticsFromFile(filePath);
                            break;
                        case "participants":
                            var rightvision = commandArgs[3];
                            LoadParticipantsFromFile(filePath, rightvision);
                            break;
                        default:
                            Console.WriteLine("Неизвестная таблица!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Ошибка обработки команды загрузки.");
                }
            }
        }

        private void LoadUsersFromFile(string filePath)
        {
            using var sr = new StreamReader(filePath);

            var settings = new JsonSerializerSettings
            {
                Converters = [UserPermissionsConverter, LocationConverter]
            };

            var data = JsonConvert.DeserializeObject<RvUser[]>(sr.ReadToEnd(), settings) ?? throw new NullReferenceException("Данные не удалось преобразовать.");
            using var db = DatabaseHelper.GetApplicationDbContext();

            foreach (var item in data)
            {
                if (db.RvUsers.Any(u => u.UserId == item.UserId)) continue;

                db.RvUsers.Add(item);
            }

            if (db.ChangeTracker.HasChanges())
                db.SaveChanges();
        }

        private void LoadCriticsFromFile(string filePath)
        {
            using var sr = new StreamReader(filePath);

            var data = JsonConvert.DeserializeObject<CriticForm[]>(sr.ReadToEnd()) ?? throw new NullReferenceException("Данные не удалось преобразовать.");
            using var db = DatabaseHelper.GetApplicationDbContext();

            foreach (var item in data)
            {
                if (db.CriticForms.Any(c => c.UserId == item.UserId)) continue;

                db.CriticForms.Add(item);
            }

            if (db.ChangeTracker.HasChanges())
                db.SaveChanges();
        }

        private void LoadParticipantsFromFile(string filePath, string rightvision)
        {
            using var sr = new StreamReader(filePath);

            var data = JsonConvert.DeserializeObject<ParticipantForm[]>(sr.ReadToEnd()) ?? throw new NullReferenceException("Данные не удалось преобразовать.");
            using var db = DatabaseHelper.GetRightVisionContext(rightvision);

            foreach (var item in data)
            {
                if (db.ParticipantForms.Any(p => p.UserId == item.UserId)) continue;

                db.ParticipantForms.Add(item);
            }

            if (db.ChangeTracker.HasChanges())
                db.SaveChanges();
        }
    }
}
