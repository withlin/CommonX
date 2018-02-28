using CommonX.Configurations;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Mapster
{
    public static class StoveMapsterRegistrationExtensions
    {
        /// <summary>
        ///     Uses the stove mapster.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        [NotNull]
        public static Configuration UseMapster([NotNull] this Configuration configuration)
        {
            configuration.SetDefault<IMapsterConfiguration, MapsterConfiguration>();
            configuration.SetDefault<IObjectMapper, MapsterObjectMapper>();
            return configuration;
        }
    }
}
