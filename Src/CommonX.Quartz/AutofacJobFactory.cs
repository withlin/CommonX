using Autofac;
using Autofac.Core.Lifetime;
using CommonX.Autofac;
using CommonX.Components;
using CommonX.Logging;
using Quartz;
using Quartz.Spi;
using Quartz.Util;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace CommonX.Quartz
{
    public class AutofacJobFactory : IJobFactory, IDisposable
    {
        private  readonly ILogger _logger;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly string _scopeName;

        /// <summary> 
        /// Whether the JobInstantiation should fail and throw and exception if
        /// a key (name) and value (type) found in the JobDataMap does not 
        /// correspond to a property setter on the Job class.
        /// </summary>
        public virtual bool ThrowIfPropertyNotFound { get; set; }

        /// <summary> 
        /// Get or set whether a warning should be logged if
        /// a key (name) and value (type) found in the JobDataMap does not 
        /// correspond to a property setter on the Job class.
        /// </summary>
        public virtual bool WarnIfPropertyNotFound { get; set; }


        /// <summary>
        ///     Initializes a new instance of the <see cref="AutofacJobFactory" /> class.
        /// </summary>
        /// <param name="lifetimeScope">The lifetime scope.</param>
        /// <param name="scopeName">Name of the scope.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="lifetimeScope" /> or <paramref name="scopeName" /> is
        ///     <see langword="null" />.
        /// </exception>
        public AutofacJobFactory(ILifetimeScope lifetimeScope, ILoggerFactory loggerFactory, string scopeName)
        {
            if (lifetimeScope == null) throw new ArgumentNullException(nameof(lifetimeScope));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (scopeName == null) throw new ArgumentNullException(nameof(scopeName));
            _lifetimeScope = lifetimeScope;
            _logger = loggerFactory.Create(this.GetType());
            _scopeName = scopeName;
        }

        internal ConcurrentDictionary<object, JobTrackingInfo> RunningJobs { get; } =
            new ConcurrentDictionary<object, JobTrackingInfo>();

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var runningJobs = RunningJobs.ToArray();
            RunningJobs.Clear();

            if (runningJobs.Length > 0)
            {
                _logger.Debug($"Cleaned {runningJobs.Length} scopes for running jobs");
            }
        }

        /// <summary>
        ///     Called by the scheduler at the time of the trigger firing, in order to
        ///     produce a <see cref="T:Quartz.IJob" /> instance on which to call Execute.
        /// </summary>
        /// <remarks>
        ///     It should be extremely rare for this method to throw an exception -
        ///     basically only the the case where there is no way at all to instantiate
        ///     and prepare the Job for execution.  When the exception is thrown, the
        ///     Scheduler will move all triggers associated with the Job into the
        ///     <see cref="F:Quartz.TriggerState.Error" /> state, which will require human
        ///     intervention (e.g. an application restart after fixing whatever
        ///     configuration problem led to the issue wih instantiating the Job.
        /// </remarks>
        /// <param name="bundle">
        ///     The TriggerFiredBundle from which the <see cref="T:Quartz.IJobDetail" />
        ///     and other info relating to the trigger firing can be obtained.
        /// </param>
        /// <param name="scheduler">a handle to the scheduler that is about to execute the job</param>
        /// <throws>SchedulerException if there is a problem instantiating the Job. </throws>
        /// <returns>
        ///     the newly instantiated Job
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="bundle" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="scheduler" /> is <see langword="null" />.</exception>
        /// <exception cref="SchedulerConfigException">
        ///     Error resolving exception. Original exception will be stored in
        ///     <see cref="Exception.InnerException" />.
        /// </exception>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

            var jobType = bundle.JobDetail.JobType;

            //var nestedScope = _lifetimeScope.BeginLifetimeScope(_scopeName);
            var nestedScope = _lifetimeScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            //var nestedScope =
                (ObjectContainer.Current as AutofacObjectContainer)?.Container.BeginLifetimeScope(
                    MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            IJob newJob = null;
            try
            {
                newJob = (IJob)nestedScope.Resolve(jobType);

                JobDataMap jobDataMap = new JobDataMap();
                jobDataMap.PutAll(scheduler.Context);
                jobDataMap.PutAll(bundle.JobDetail.JobDataMap);
                jobDataMap.PutAll(bundle.Trigger.JobDataMap);

                SetObjectProperties(newJob, jobDataMap);

                var jobTrackingInfo = new JobTrackingInfo(nestedScope);
                RunningJobs[newJob] = jobTrackingInfo;

                if (_logger.IsDebugEnabled)
                {
                    _logger.Info($"Scope 0x{jobTrackingInfo.Scope.GetHashCode():x} associated with Job 0x{newJob.GetHashCode():x}");                    
                }

                nestedScope = null;
            }
            catch (Exception ex)
            {
                if (nestedScope != null)
                {
                    DisposeScope(newJob, nestedScope);
                }
                throw new SchedulerConfigException(string.Format(CultureInfo.InvariantCulture,
                    "Failed to instantiate Job '{0}' of type '{1}'",
                    bundle.JobDetail.Key, bundle.JobDetail.JobType), ex);
            }
            return newJob;
        }

        /// <summary>
        ///     Allows the the job factory to destroy/cleanup the job if needed.
        /// </summary>
        public void ReturnJob(IJob job)
        {
            if (job == null)
                return;

            JobTrackingInfo trackingInfo;
            if (!RunningJobs.TryRemove(job, out trackingInfo))
            {
                _logger.Warn($"Tracking info for job 0x{ job.GetHashCode():x} not found");               
                // ReSharper disable once SuspiciousTypeConversion.Global
                var disposableJob = job as IDisposable;
                disposableJob?.Dispose();
            }
            else
            {
                DisposeScope(job, trackingInfo.Scope);
            }
        }

        void DisposeScope(IJob job, ILifetimeScope lifetimeScope)
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Info($"Disposing Scope 0x{ lifetimeScope?.GetHashCode() ?? 0:x} for Job 0x{job?.GetHashCode() ?? 0:x}");               
            }
            lifetimeScope?.Dispose();
        }

        #region Job data

        internal sealed class JobTrackingInfo
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
            /// </summary>
            public JobTrackingInfo(ILifetimeScope scope)
            {
                Scope = scope;
            }

            public ILifetimeScope Scope { get; }
        }

        #endregion Job data

        /// <summary>
        /// Sets the object properties.
        /// </summary>
        /// <param name="obj">The object to set properties to.</param>
        /// <param name="data">The data to set.</param>
		public virtual void SetObjectProperties(object obj, JobDataMap data)
        {
            Type paramType = null;

            foreach (string name in data.Keys)
            {
                string c = name.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture);
                string propName = c + name.Substring(1);

                object o = data[name];
                PropertyInfo prop = obj.GetType().GetProperty(propName);

                try
                {
                    if (prop == null)
                    {
                        HandleError(string.Format(CultureInfo.InvariantCulture, "No property on Job class {0} for property '{1}'", obj.GetType(), name));
                        continue;
                    }

                    paramType = prop.PropertyType;

                    if (o == null && (paramType.IsPrimitive || paramType.IsEnum))
                    {
                        // cannot set null to these
                        HandleError(string.Format(CultureInfo.InvariantCulture, "Cannot set null to property on Job class {0} for property '{1}'", obj.GetType(), name));
                    }
                    if (paramType == typeof(char) && o is string && ((string)o).Length != 1)
                    {
                        // handle special case
                        HandleError(string.Format(CultureInfo.InvariantCulture, "Cannot set empty string to char property on Job class {0} for property '{1}'", obj.GetType(), name));
                    }

                    object goodValue = paramType == typeof(TimeSpan)
                                           ? ObjectUtils.GetTimeSpanValueForProperty(prop, o)
                                           : ObjectUtils.ConvertValueIfNecessary(paramType, o);

                    prop.GetSetMethod().Invoke(obj, new object[] { goodValue });
                }
                catch (FormatException nfe)
                {
                    HandleError(
                            string.Format(CultureInfo.InvariantCulture, "The setter on Job class {0} for property '{1}' expects a {2} but was given {3}", obj.GetType(), name, paramType, o), nfe);
                }
                catch (MethodAccessException)
                {
                    HandleError(string.Format(CultureInfo.InvariantCulture, "The setter on Job class {0} for property '{1}' expects a {2} but was given a {3}", obj.GetType(), name, paramType, o.GetType()));
                }
                catch (ArgumentException e)
                {
                    HandleError(
                            string.Format(CultureInfo.InvariantCulture, "The setter on Job class {0} for property '{1}' expects a {2} but was given {3}", obj.GetType(), name, paramType, o.GetType()), e);
                }
                catch (UnauthorizedAccessException e)
                {
                    HandleError(
                            string.Format(CultureInfo.InvariantCulture, "The setter on Job class {0} for property '{1}' could not be accessed.", obj.GetType(), name), e);
                }
                catch (TargetInvocationException e)
                {
                    HandleError(
                            string.Format(CultureInfo.InvariantCulture, "The setter on Job class {0} for property '{1}' could not be accessed.", obj.GetType(), name), e);
                }
                catch (Exception e)
                {
                    HandleError(
                            string.Format(CultureInfo.InvariantCulture, "The setter on Job class {0} for property '{1}' threw exception when processing.", obj.GetType(), name), e);
                }
            }
        }
        
        private void HandleError(string message)
        {
            HandleError(message, null);
        }

        private void HandleError(string message, Exception e)
        {
            if (ThrowIfPropertyNotFound)
            {
                throw new SchedulerException(message, e);
            }

            if (WarnIfPropertyNotFound)
            {
                if (e == null)
                {
                    _logger.Warn(message);
                }
                else
                {
                    _logger.Warn(message, e);
                }
            }
        }

    }
}
