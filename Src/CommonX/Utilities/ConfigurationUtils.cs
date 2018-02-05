using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Utilities
{
    public static class ConfigurationUtils
    {
        /// <summary>
        ///     Determines whether a new instance of this class is needed.  Either
        ///     because it hasn't yet been instantiated, or because enough time
        ///     has expired so a refresh is warranted.
        /// </summary>
        public static bool IsTimeToRefresh(
            DateTime instanceCreationUtcDateTime,
            int refreshMinutes)
        {
            var returnValue = false;

            // Get the amount of time that has expired since the last instance was created.
            var elapsedTimeSinceLastInstance =
                DateTime.UtcNow.Subtract(instanceCreationUtcDateTime);

            // Check to see if we have exceeded the desired number of minutes
            if (elapsedTimeSinceLastInstance.Minutes >= refreshMinutes)
            {
                returnValue = true;
            }

            return returnValue;
        }
    }
}
