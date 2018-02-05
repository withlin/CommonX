using Quartz;
using Quartz.Core;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonX.Quartz
{
    public class AutofacSchedulerFactory : StdSchedulerFactory
    {
        readonly AutofacJobFactory _jobFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Quartz.Impl.StdSchedulerFactory" /> class.
        /// </summary>
        /// <param name="jobFactory">Job factory.</param>
        /// <exception cref="ArgumentNullException"><paramref name="jobFactory" /> is <see langword="null" />.</exception>
        public AutofacSchedulerFactory(AutofacJobFactory jobFactory)
        {
            if (jobFactory == null) throw new ArgumentNullException(nameof(jobFactory));
            _jobFactory = jobFactory;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Quartz.Impl.StdSchedulerFactory" /> class.
        /// </summary>
        /// <param name="props">The properties.</param>
        /// <param name="jobFactory">Job factory</param>
        /// <exception cref="ArgumentNullException"><paramref name="jobFactory" /> is <see langword="null" />.</exception>
        public AutofacSchedulerFactory(NameValueCollection props,  AutofacJobFactory jobFactory)
            : base(props)
        {
            if (jobFactory == null) throw new ArgumentNullException(nameof(jobFactory));
            _jobFactory = jobFactory;
        }

        /// <summary>
        ///     Instantiates the scheduler.
        /// </summary>
        /// <param name="rsrcs">The resources.</param>
        /// <param name="qs">The scheduler.</param>
        /// <returns>Scheduler.</returns>
        protected override IScheduler Instantiate(QuartzSchedulerResources rsrcs, QuartzScheduler qs)
        {
            var scheduler = base.Instantiate(rsrcs, qs);
            scheduler.JobFactory = _jobFactory;
            return scheduler;
        }
    }
}
