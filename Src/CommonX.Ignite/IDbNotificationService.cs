namespace CommonX.Cache.Ignite
{
    public interface IDbNotificationService: System.IDisposable
    {
        void Start(string moduleName, string[] tableNames, string hostName, string userName, string password);
        void ReConnect();
    }
}
