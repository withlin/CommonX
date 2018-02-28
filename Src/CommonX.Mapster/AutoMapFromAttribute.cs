using CommonX.Utilities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Mapster
{
    /// <summary>
    ///     From Entity to Dto, Use on Entities.
    /// </summary>
    /// <seealso cref="AutoMapAttributeBase" />
    public class AutoMapFromAttribute : AutoMapAttributeBase
    {
        public AutoMapFromAttribute(params Type[] targetTypes)
            : base(targetTypes)
        {
        }

        public override void CreateMap(TypeAdapterConfig configuration, Type destination)
        {
            if (TargetTypes.IsNullOrEmpty())
            {
                return;
            }

            foreach (Type target in TargetTypes)
            {
                configuration.NewConfig(target, destination);
            }
        }
    }
}
