using System;
using System.Diagnostics;
using CommonX.Components;
using CommonX.Logging;
using CommonX.Messages;

namespace CommonX.Exceptions
{
    [Component]
    public class ExceptionHandler
    {
        private static ILogger _logger;

        static ExceptionHandler()
        {
            using (var scope = ObjectContainer.BeginLifetimeScope())
            {
                _logger = scope.Resolve<ILoggerFactory>().Create(typeof (ExceptionHandler));
            }
        }

        /*
         * 
         * Case 1: FPException, Policy
         *     call ExceptionPolicy.HandleException directly.
         *     
         * Case 2: generic .Net Exception, Policy, expectedType of FPException
         *     wrap original exception as FPException;
         *     then call ExceptionPolicy.HandleException.
         *     
         */

        /// <summary>
        ///     The main entry point into the Exception Handling Application Block.
        ///     Handles the specified <see cref="Exception" /> object according to the given
        ///     <paramref>
        ///         <name>configurationContext</name>
        ///     </paramref>.
        /// </summary>
        /// <param name="exceptionToHandle">An <see cref="Exception" /> object.</param>
        /// <param name="policy">The enum of the ExceptionHandlingPolicy to handle.</param>
        /// <example>
        ///     The following code shows the usage of the exception handling framework.
        ///     <code>
        ///  (1)
        ///  FPValidationException ve = new FPValidationException(MessageKey);
        ///  ExceptionHandler.HandleException(ve, policy);
        ///  
        ///  (2)
        ///  try
        /// 	{
        /// 		Foo();
        /// 	}
        /// 	catch (ApplicationException ae)
        /// 	{
        /// 		ExceptionHandler.HandleException(ae, policy);
        /// 	}
        ///  </code>
        /// </example>
        /// <exception cref="ArgumentNullException">ArgumentNullException</exception>
        //public static void HandleException(Exception exceptionToHandle, ExceptionHandlingPolicy policy = ExceptionHandlingPolicy.LogAndPropagate)
        //{
        //    //var policyName = string.Empty;

        //    #region Validate input parameters.

        //    if (exceptionToHandle == null)
        //    {
        //        throw new ArgumentNullException("The argument exceptionToHandle is null.");
        //    }

        //    #endregion

        //    #region Populate policy name.

        //    //policyName = EnumerationHelper.GetEnumDescription(policy);

        //    if (IsAlreadyLogged(exceptionToHandle) && (policy == ExceptionHandlingPolicy.LogOnly))
        //    {
        //        // do nothing, return directly.
        //        return;
        //    }
        //    if (IsAlreadyLogged(exceptionToHandle) && (policy == ExceptionHandlingPolicy.LogAndPropagate))
        //    {
        //        // only propagate.
        //        // policyName = "Propagate Policy"; 
        //        policy = ExceptionHandlingPolicy.Propagate;
        //    }

        //    #endregion

        //    // Debug exception information into console.
        //    DiagnoseInConsole();

        //    #region Typical call statement of ExceptionPolicy

        //    //removing this and replacing with the Logger
        //    //  bool rethrow = ExceptionPolicy.HandleException(exceptionToHandle, policyName);

        //    if (policy == ExceptionHandlingPolicy.LogAndPropagate ||
        //        policy == ExceptionHandlingPolicy.LogOnly)
        //    {
        //        if (exceptionToHandle is ASOAApplicationException ||
        //            exceptionToHandle is ASOAValidationException)
        //        {
        //            var logEntry = new LogEntry();
        //            logEntry.EventId = ((BaseApplicationException) exceptionToHandle).RichMessage.EventId;
        //            logEntry.Severity = TraceEventType.Warning;
        //            logEntry.AddErrorMessage(exceptionToHandle.ToString());
        //            logEntry.Message = exceptionToHandle.ToString();
        //            _logger.Warn(logEntry);
        //        }
        //        else
        //        {
        //            var logEntry = new LogEntry();
        //            logEntry.EventId = ((exceptionToHandle as BaseApplicationException) != null)? ((BaseApplicationException)exceptionToHandle).RichMessage.EventId : 0;
        //            logEntry.Severity = TraceEventType.Error;
        //            logEntry.AddErrorMessage(exceptionToHandle.ToString());
        //            logEntry.Message = exceptionToHandle.ToString();
        //            _logger.Error(logEntry);
        //        }
        //        SetLoggedStatus(exceptionToHandle);
        //    }
        //    if (policy == ExceptionHandlingPolicy.LogAndPropagate ||
        //        policy == ExceptionHandlingPolicy.Propagate)
        //    {
        //        throw exceptionToHandle;
        //    }

        //    #endregion
        //}

        
        /// <summary>
        /// </summary>
        private static void DiagnoseInConsole()
        {
            Console.WriteLine();
            Console.WriteLine("-------------------------------");
            Console.WriteLine();
            Console.WriteLine("-------------------------------");
            Console.WriteLine();
        }

        /// <summary>
        ///     Check if the exception was already logged by looking for the logged indicator in the data dictionary of the
        ///     exception.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsAlreadyLogged(Exception e)
        {
            if (e.Data.Contains(MessageConstants.LoggedIndicator))
            {
                return (bool) e.Data[MessageConstants.LoggedIndicator];
            }
            return false;
        }

        /// <summary>
        ///     SetLoggedStatus
        /// </summary>
        /// <param name="e"></param>
        public static void SetLoggedStatus(Exception e)
        {
            if (e.Data.Contains(MessageConstants.LoggedIndicator))
            {
                e.Data[MessageConstants.LoggedIndicator] = true;
            }
            else
            {
                e.Data.Add(MessageConstants.LoggedIndicator, true);
            }
        }
    }
}