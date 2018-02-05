using System;
using System.Diagnostics;
using CommonX.Exceptions;
using CommonX.Logging;
using log4net;

namespace CommonX.Log4Net
{
    /// <summary>Log4Net based logger implementation.
    /// </summary>
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _log;

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="log"></param>
        public Log4NetLogger(ILog log)
        {
            _log = log;
        }

        #region ILogger Members

        public bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled; }
        }

        public bool IsAuditOn => throw new NotImplementedException();

        public bool IsErrorOn => throw new NotImplementedException();

        public bool IsFatalOn => throw new NotImplementedException();

        public bool IsInfoOn => throw new NotImplementedException();

        public bool IsLoggingOn => throw new NotImplementedException();

        public bool IsTimerUsageOn => throw new NotImplementedException();

        public bool IsTraceOn => throw new NotImplementedException();

        public bool IsWarnOn => throw new NotImplementedException();

        public void Debug(object message)
        {
            _log.Debug(message);
        }
        public void DebugFormat(string format, params object[] args)
        {
            _log.DebugFormat(format, args);
        }
        public void Debug(object message, Exception exception)
        {
            _log.Debug(message, exception);
        }
        public void Info(object message)
        {
            _log.Info(message);
        }
        public void InfoFormat(string format, params object[] args)
        {
            _log.InfoFormat(format, args);
        }
        public void Info(object message, Exception exception)
        {
            _log.Info(message, exception);
        }
        public void Error(object message)
        {
            _log.Error(message);
        }
        public void ErrorFormat(string format, params object[] args)
        {
            _log.ErrorFormat(format, args);
        }
        public void Error(object message, Exception exception)
        {
            _log.Error(message, exception);
        }
        public void Warn(object message)
        {
            _log.Warn(message);
        }
        public void WarnFormat(string format, params object[] args)
        {
            _log.WarnFormat(format, args);
        }
        public void Warn(object message, Exception exception)
        {
            _log.Warn(message, exception);
        }
        public void Fatal(object message)
        {
            _log.Fatal(message);
        }
        public void FatalFormat(string format, params object[] args)
        {
            _log.FatalFormat(format, args);
        }
        public void Fatal(object message, Exception exception)
        {
            _log.Fatal(message, exception);
        }

        public Guid WriteLog(string logType, TraceEventType severity, object message, Exception exception, int executionDurationInMilliSeconds, int returnedDurationInMilliSeconds)
        {
            throw new NotImplementedException();
        }

        public Guid Audit(object message)
        {
            throw new NotImplementedException();
        }

        public Guid Audit(BaseApplicationException exception)
        {
            throw new NotImplementedException();
        }

        public Guid Audit(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public Guid Debug(BaseApplicationException exception)
        {
            throw new NotImplementedException();
        }

        public Guid Error(BaseApplicationException exception)
        {
            throw new NotImplementedException();
        }

        public Guid Fatal(BaseApplicationException exception)
        {
            throw new NotImplementedException();
        }

        public Guid Info(BaseApplicationException exception)
        {
            throw new NotImplementedException();
        }

        public Guid Trace(object message)
        {
            throw new NotImplementedException();
        }

        public Guid Trace(BaseApplicationException exception)
        {
            throw new NotImplementedException();
        }

        public Guid Trace(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public Guid Usage()
        {
            throw new NotImplementedException();
        }

        public Guid Usage(string usageName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}