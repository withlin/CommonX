

namespace Shared.MiddleWare.RabbitMQ
{
    using global::RabbitMQ.Client;
    using System.Collections.Generic;
    public static class ExchangeTypeDataDict
    {
        /// <summary>
        /// exchangeType对应数值表
        /// </summary>
        public readonly static IDictionary<ExchangeType, string> ExchangeTypeDict = new Dictionary<ExchangeType, string>()
        {
            
            {ExchangeType.Default, string.Empty},
            //{ExchangeType.Fanout, ExchangeType.Fanout},
            //{ExchangeType.Direct, ExchangeType.Direct},
            //{ExchangeType.Topic,ExchangeType.Topic},
            //{ExchangeType.Headers,ExchangeType.Headers}
        };
    }
}
