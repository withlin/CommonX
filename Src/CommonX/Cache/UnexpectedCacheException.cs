using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CommonX.Cache
{
    /// <summary>
    ///     Used when we are forcing an exception condition from within the 
    ///     Cache namespace.
    /// </summary>
    [Serializable()]
    public class UnexpectedCacheException : System.Exception
    {
        #region  Public Constructors
        //%***************************************************************************
        //%*
        //%*                   Public Constructors
        //%*
        //%***************************************************************************

        /// <summary>
        ///     Constructor with no parameters.
        /// </summary>
        public UnexpectedCacheException()
            : base()
        {

        }

        /// <summary>
        ///     Constructor that receives a message as a parameter.
        /// </summary>
        /// <param name="message">
        ///     Textual message of the exception.
        /// </param>
        public UnexpectedCacheException(
            String message)
            : base(message)
        {

        }

        /// <summary>
        ///     Constructor that receives a message and an exception as parameters.
        /// </summary>
        /// <param name="message">
        ///     Textual message of the exception.
        /// </param>
        /// <param name="innerException">
        ///     Exception object to wrap.
        /// </param>
        public UnexpectedCacheException(
            String message,
            System.Exception innerException)
            : base(message, innerException)
        {

        }

        #endregion

        #region  Protected Constructors
        //%***************************************************************************
        //%*
        //%*                   Protected Constructors
        //%*
        //%***************************************************************************

        /// <summary>
        ///     Constructor used when de-serialzing.
        /// </summary>
        /// <param name="info">
        ///     See MSDN documentation.
        /// </param>
        /// <param name="context">
        ///     See MSDN documentation.
        /// </param>
        protected UnexpectedCacheException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {

        }

        #endregion
    }
}
