using Common.Logging;
using Common.Logging.Factory;
using CommonX.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonX.Quartz.Logging.EntLib
{
    public class EntLibLogger : AbstractLogger
    {

        private readonly ILogger _logger;

        #region constructor

        public EntLibLogger(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region AbstractLogger成员

        public override bool IsDebugEnabled
        {
            get
            {
                return _logger.IsDebugEnabled;
            }
        }

        public override bool IsTraceEnabled => throw new NotImplementedException();

        public override bool IsInfoEnabled => throw new NotImplementedException();

        public override bool IsWarnEnabled => throw new NotImplementedException();

        public override bool IsErrorEnabled => throw new NotImplementedException();

        public override bool IsFatalEnabled => throw new NotImplementedException();

        //public override bool IsErrorEnabled
        //{
        //    get
        //    {
        //        return _logger.IsErrorOn;
        //    }
        //}

        //public override bool IsFatalEnabled
        //{
        //    get
        //    {
        //        return _logger.IsFatalOn;
        //    }
        //}

        //public override bool IsInfoEnabled
        //{
        //    get
        //    {
        //        return _logger.IsInfoOn;
        //    }
        //}

        //public override bool IsTraceEnabled
        //{
        //    get
        //    {
        //        return _logger.IsTraceOn;
        //    }
        //}

        //public override bool IsWarnEnabled
        //{
        //    get
        //    {
        //        return _logger.IsWarnOn;
        //    }
        //}

        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {           
            switch (level)
            {
                case LogLevel.Debug:
                    _logger.Debug(message, exception);
                    break;
                case LogLevel.Error:
                    _logger.Error(message, exception);
                    break;
                case LogLevel.Fatal:
                    _logger.Fatal(message, exception);
                    break;
                case LogLevel.Info:
                    _logger.Info(message, exception);
                    break;
                case LogLevel.Trace:
                    _logger.Info(message, exception);
                    break;
                case LogLevel.Warn:
                    _logger.Info(message, exception);
                    break;
                default:
                    _logger.Info(message, exception);
                    break;
            }
        }

        #endregion
    }
}
