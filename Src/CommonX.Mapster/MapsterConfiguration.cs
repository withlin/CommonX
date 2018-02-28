using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Mapster
{
    public class MapsterConfiguration: IMapsterConfiguration
    {
        public MapsterConfiguration()
        {
            Configurators = new List<Action<TypeAdapterConfig>>();
            Configuration = TypeAdapterConfig.GlobalSettings;
            Adapter = new Adapter(Configuration);
        }

        public TypeAdapterConfig Configuration { get; }

        public List<Action<TypeAdapterConfig>> Configurators { get; }

        public IAdapter Adapter { get; }
    }
}
