namespace RightVisionBotDb.Settings
{
    public class DataSettings
    {
        private string _rightVisionDatabasesPath;

        public string RightVisionDatabasesPath 
        { 
            get => _rightVisionDatabasesPath;
            set => _rightVisionDatabasesPath = value
                .Replace('/', '\\')
                .Replace('\\', Path.DirectorySeparatorChar); 
        }
    }
}
