using CommonX.Components;
using CommonX.Configurations;

namespace CommonX.DataAccess.InMemory
{
    /// <summary>configuration class Redis extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>Use InMemory to implement the Repository.
        /// </summary>
        /// <returns></returns>
        public static Configuration UseInMemoryRepository(this Configuration configuration)
        {
            ObjectContainer.Current.RegisterGeneric(typeof(IRepository<,>), typeof(InMemoryRepository<>));
            ObjectContainer.Current.RegisterGeneric(typeof(IReadRepository<,>), typeof(InMemoryRepository<>));
            //ObjectContainer.Current.Register<IUnitOfWork,EmptyUnitOfWork>();
            return configuration;
        }
    }
}