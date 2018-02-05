using Autofac;
using CommonX.Logging;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Specialized;

namespace CommonX.Quartz
{

    /// <summary>
    ///     Registers <see cref="ISchedulerFactory" /> and default <see cref="IScheduler" />.
    /// </summary>
    public class QuartzAutofacFactoryModule : Module
    {
        /// <summary>
        ///     Default name for nested lifetime scope.
        ///     替换成MatchingScopeLifetimeTags.RequestLifetimeScopeTag
        /// </summary>
        public const string LifetimeScopeName = "quartz.job";

        readonly string _lifetimeScopeName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QuartzAutofacFactoryModule" /> class with a default lifetime scope
        ///     name.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">lifetimeScopeName</exception>
        public QuartzAutofacFactoryModule()
            : this(LifetimeScopeName)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QuartzAutofacFactoryModule" /> class.
        /// </summary>
        /// <param name="lifetimeScopeName">Name of the lifetime scope to wrap job resolution and execution.</param>
        /// <exception cref="System.ArgumentNullException">lifetimeScopeName</exception>
        public QuartzAutofacFactoryModule(string lifetimeScopeName)
        {
            if (lifetimeScopeName == null) throw new ArgumentNullException(nameof(lifetimeScopeName));
            _lifetimeScopeName = lifetimeScopeName;
        }

        /// <summary>
        ///     Provides custom configuration for Scheduler.
        ///     Returns <see cref="NameValueCollection" /> with custom Quartz settings.
        ///     <para>See http://quartz-scheduler.org/documentation/quartz-2.x/configuration/ for settings description.</para>
        ///     <seealso cref="StdSchedulerFactory" /> for some configuration property names.
        /// </summary>
        public Func<IComponentContext, NameValueCollection> ConfigurationProvider { get; set; }

        /// <summary>
        ///     Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        ///     Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">
        ///     The builder through which components can be
        ///     registered.
        /// </param>
        protected override void Load( ContainerBuilder builder)
        {
            builder.Register(c => new AutofacJobFactory(c.Resolve<ILifetimeScope>(),c.Resolve<ILoggerFactory>(), _lifetimeScopeName))
                .AsSelf()
                .As<IJobFactory>()
                .SingleInstance();

            builder.Register<ISchedulerFactory>(c =>
            {
                var cfgProvider = ConfigurationProvider;

                var autofacSchedulerFactory = cfgProvider != null
                    ? new AutofacSchedulerFactory(cfgProvider(c), c.Resolve<AutofacJobFactory>())
                    : new AutofacSchedulerFactory(c.Resolve<AutofacJobFactory>());
                return autofacSchedulerFactory;
            }).SingleInstance();

            builder.Register(c => c.Resolve<ISchedulerFactory>().GetScheduler()).SingleInstance();
        }
    }
}
