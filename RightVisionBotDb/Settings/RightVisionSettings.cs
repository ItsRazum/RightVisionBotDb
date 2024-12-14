namespace RightVisionBotDb.Settings
{
    public class RightVisionSettings
    {
        public string DefaultRightVision { get; set; }

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
