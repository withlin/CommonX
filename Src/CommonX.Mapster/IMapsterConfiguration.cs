using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Mapster
{
    public interface IMapsterConfiguration
    {
        TypeAdapterConfig Configuration { get; }

        List<Action<TypeAdapterConfig>> Configurators { get; }

        IAdapter Adapter { get; }
    }
}
