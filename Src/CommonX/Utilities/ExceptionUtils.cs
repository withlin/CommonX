using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CommonX.Utilities
{
    public static class ExceptionUtils
    {
        /// <summary>
        ///     <para>
        ///         This method is used to add name/value pairs into the <b>Data</b> collection of an
        ///         exception object.  For example an error handler might put the names and values
        ///         of input parameters into the exception to assist with diagnosing the root cause
        ///         of the error.
        ///     </para>
        ///     <para>
        ///         The significance of this method is that it will take the provided <b>name</b>
        ///         and prefix it with the fully qualified name of the method that is adding in the
        ///         name/value pairs of data.  This allows multiple
        ///         methods to potentially catch the same exception and add information into it
        ///         while retaining explicit knowledge of which method added each name/value pair
        ///         of data.
        ///     </para>
        /// </summary>
        /// <param name="name">
        ///     Name of the data being added to the exception.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public static string BuildFullDataName(
            string name)
        {
            try
            {
                // If they don't give us a name for some reason generate a meaningless one.
                if (string.IsNullOrEmpty(name))
                {
                    name = string.Empty;
                }

                var trace = new StackTrace(1, false);
                var frame = trace.GetFrame(0);
                var method = frame.GetMethod();
                var fullMethodName = method.DeclaringType.FullName + "." + method.Name;
                var returnValue = fullMethodName + ": " + name;
                return returnValue;
            }

            catch
            {
                return name;
            }
        }
    }
}
