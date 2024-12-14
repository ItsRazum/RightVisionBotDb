namespace RightVisionBotDb.Settings
{
    public class AcademySettings
    {
        public string DefaultAcademy { get; set; }
        private string _academyDatabasesPath;
        public string AcademyDatabasesPath
        {
            get => _academyDatabasesPath;
            set => _academyDatabasesPath = value
                .Replace('/', '\\')
                .Replace('\\', Path.DirectorySeparatorChar);
        }
    }
}
