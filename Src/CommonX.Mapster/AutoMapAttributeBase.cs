using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Mapster
{
    public abstract class AutoMapAttributeBase : Attribute
    {
        protected AutoMapAttributeBase([NotNull] params Type[] targetTypes)
        {
            TargetTypes = targetTypes;
        }

        [NotNull]
        public Type[] TargetTypes { get; private set; }

        public abstract void CreateMap([NotNull] TypeAdapterConfig configuration, [NotNull] Type needstoMap);
    }
}
