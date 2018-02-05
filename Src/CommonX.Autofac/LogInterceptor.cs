using Castle.DynamicProxy;
using CommonX.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonX.Autofac
{
    /// <summary>
    /// Interceptor for log
    /// </summary>
    public class LogInterceptor : IInterceptor
    {
        private readonly ILogger _logger;

        public LogInterceptor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType());
        }

        public void Intercept(IInvocation invocation)
        {
            var msg = string.Format("Log Interceptor -- {0} Calling method {1} with parameters {2}. \nTargetTypeFullName:{3} ",
                invocation.TargetType.Name,
                invocation.Method.Name,
                string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray()),
                invocation.TargetType.FullName);
            _logger.Debug(msg);


            invocation.Proceed();

            //for unit test
            if (invocation.TargetType.FullName.Contains("JZTERP.Frameworks.Common.Tests.Components"))
            {
                invocation.ReturnValue = (int)invocation.ReturnValue + 1;
            }
        }
    }
}
