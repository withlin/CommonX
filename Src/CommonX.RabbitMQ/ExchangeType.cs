using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonX.RabbitMQ
{
    /// <summary>
    /// 路由枚举
    /// </summary>
    public enum ExchangeType
    {
        /// <summary>
        /// 空值""|0
        /// </summary>
        Default = 0,

        /// <summary>
        /// 不进行路由，将消息分发到所有绑定的队列，此时routingKey无用|1
        /// </summary>
        Fanout = 1,

        /// <summary>
        /// 对exchange进行路由，通过匹配routingKey来将消息分发到绑定的队列中|2
        /// </summary>
        Direct = 2,

        /// <summary>
        /// 对exchange进行路由，通过模糊匹配routingKey来将消息分发到绑定的队列中，性能比direct低|3
        /// </summary>
        Topic = 3,

        /// <summary>
        /// 对exchange进行路由，通过对header中参数类型为（键=值）的值进行匹配
        /// </summary>
        Headers = 4
    }
}
