using Common.Logging;
using Common.Logging.Factory;
using CommonX.Components;
using CommonX.Logging;
using CommonX.Quartz.Logging.EntLib;

namespace CommonX.Quartz.Logging.EntLib
{
    public class EntLibLoggerFactoryAdapter : AbstractCachingLoggerFactoryAdapter
    {

        private ILoggerFactory factory;

        public EntLibLoggerFactoryAdapter()
        {
            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
            {
                factory = scope.Resolve<ILoggerFactory>();
            }
        }

        protected override ILog CreateLogger(string name)
        {
            return new EntLibLogger(factory.Create(name));
        }
    }
}
