using System.ComponentModel;

namespace CommonX.Exceptions
{
    /// <summary>
    /// Policy                          Log      Propagate
    /// Global Policy                   Y        N
    /// Log Only Policy                 Y        N
    /// Log And Propagate Policy        Y        Y
    /// Propagate Policy                N        Y
    /// </summary>
    public enum ExceptionHandlingPolicy
    {
        
        /// <summary>
        /// <add name="Log Only Policy">
        ///     <exceptionTypes>
        ///         <add name="Exception" type="System.Exception, mscorlib" postHandlingAction="None">
        ///             <exceptionHandlers>
        ///                 <add 
		///						 logCategory="Default Category" 
		///						 eventId="100" 
		///						 severity="Error" 
		///						 title="Exception Handling"
		///						 priority="0"
		///						 formatterType="Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.TextExceptionFormatter, Microsoft.Practices.EnterpriseLibrary.ExceptionHandling" 
		///						 name="Logging Handler"
		///						 type="Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging.LoggingExceptionHandler, Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging"
		///					  />
        ///             </exceptionHandlers>
        ///         </add>
        ///     </exceptionTypes>
        /// </add>
        /// </summary>
        [Description("Log Only Policy")]
        LogOnly = 1,

        /// <summary>
        /// <add name="Log And Propagate Policy">
		///		<exceptionTypes>
        ///			<add name="Exception" type="System.Exception, mscorlib" postHandlingAction="ThrowNewException">
        ///             <exceptionHandlers>
        ///                 <add 
        ///						 logCategory="Default Category" 
        ///						 eventId="100" 
        ///						 severity="Error" 
        ///						 title="Exception Handling"
        ///						 priority="0"
        ///						 formatterType="Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.TextExceptionFormatter, Microsoft.Practices.EnterpriseLibrary.ExceptionHandling" 
        ///						 name="Logging Handler"
        ///						 type="Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging.LoggingExceptionHandler, Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging"
        ///					  />
        ///             </exceptionHandlers>
		///			</add>
		///		</exceptionTypes>
        ///	</add>
        /// </summary>
        [Description("Log And Propagate Policy")]
        LogAndPropagate = 2,

        /// <summary>
        /// <add name="Global Policy">
        ///     <exceptionTypes>
        ///         <add name="Exception" type="System.Exception, mscorlib" postHandlingAction="None">
        ///             <exceptionHandlers>
        ///                 <add 
        ///						 logCategory="Default Category" 
        ///						 eventId="100" 
        ///						 severity="Error" 
        ///						 title="Exception Handling"
        ///						 priority="0"
        ///						 formatterType="Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.TextExceptionFormatter, Microsoft.Practices.EnterpriseLibrary.ExceptionHandling" 
        ///						 name="Logging Handler"
        ///						 type="Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging.LoggingExceptionHandler, Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging"
        ///					  />
        ///             </exceptionHandlers>
        ///         </add>
        ///     </exceptionTypes>
        /// </add>
        /// </summary>
        [Description("Global Policy")]
        Default = 0,

        /// <summary>
        /// <add name="Log And Propagate Policy">
        ///		<exceptionTypes>
        ///			<add name="Exception" type="System.Exception, mscorlib" postHandlingAction="ThrowNewException">
        ///             <exceptionHandlers />
        ///			</add>
        ///		</exceptionTypes>
        ///	</add>
        /// </summary>
        [Description("Propagate Policy")]
        Propagate = 3,
        
        
        //#region Unused

        //[Description("Log, Wrap as ValidationException And Propagate Policy")]
        //LogAndWrapAndPropagate_ValidationException = 3,

        //[Description("Log, Wrap as ApplicationException And Propagate Policy")]
        //LogAndWrapAndPropagate_ApplicationException = 4,

        //[Description("Log, Wrap as SystemException And Propagate Policy")]
        //LogAndWrapAndPropagate_SystemException = 5
        //#endregion

    }
}
