namespace RightVisionBotDb.Exceptions
{
    internal class RvUserNoPunishmentsException : Exception
    {
        public override string Message => "RvUser не имеет наказаний";

        public RvUserNoPunishmentsException()
        {
        }
    }
}
