using Mapster;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CommonX.Mapster
{
    public class MapsterObjectMapper : IObjectMapper
    {
        private readonly IMapsterConfiguration _mapsterConfiguration;

        public MapsterObjectMapper(IMapsterConfiguration mapsterConfiguration)
        {
            _mapsterConfiguration = mapsterConfiguration;
        }

        public TDestination Map<TDestination>(object source)
        {
            Type sourceType = source.GetType().Namespace == "System.Data.Entity.DynamicProxies"
                ? source.GetType().GetTypeInfo().BaseType
                : source.GetType();

            return (TDestination)source.Adapt(sourceType, typeof(TDestination), _mapsterConfiguration.Configuration);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return source.Adapt(destination, _mapsterConfiguration.Configuration);
        }
    }
}
