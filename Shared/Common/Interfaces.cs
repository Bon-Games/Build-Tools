namespace BonGames
{
    public interface IInitializable
    {
        public bool IsInitialized { get; }
        public void Initialize();   
    }

    public interface ILogger
    {
        public void LogI(object obj);

        public void LogW(object obj);

        public void LogE(object obj);
    }
}
