using Confluent.Kafka;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonX.Kafka
{
   
    public class KafkaSetting
    {
        public readonly ConcurrentDictionary<string, object> _setting;

        private IEnumerable<KeyValuePair<string, object>> _kafkaSetting;
        public KafkaSetting()
        {
            _setting = new ConcurrentDictionary<string, object>();
        }
        public int ConnectionPoolSize { get; set; } = 10;
        BrokerMetadata metadata = new BrokerMetadata(1, "10.3.87.33", 9092);
        public string Servers { get; set; } = "10.3.87.33:9092,10.3.87.209:9092,10.3.87.211:9092";
        public IEnumerable<KeyValuePair<string, object>> AsKafkaSetting()
        {
            if (_kafkaSetting == null)
            {
                if (string.IsNullOrWhiteSpace(Servers))
                    throw new ArgumentNullException(nameof(Servers));

                _setting["metadata.broker.list"] = Servers;
                _setting["api.version.request"] = "true";
                _setting["queue.buffering.max.ms"] = "10";
                _setting["socket.blocking.max.ms"] = "10";
                _setting["enable.auto.commit"] = "false";
                _setting["log.connection.close"] = "false";
                _setting["group.id"] ="sample.app.NetCore";
                //_setting["group.id"] = new Guid().ToString();

                //ProduceAsync是按照顺序发送的如果发生异常 比如网络不可达，然后kafka client会重试，
                //如果要按照顺序重新发送则设置下面的配置，但是会降低kafka的吞吐量。
                //_setting["queue.buffering.max.ms"] = "1";
                //_setting["socket.blocking.max.ms"] = "1";

                //如果不想批处理则设置如下

                //_setting["default.topic.config"] = new Dictionary<string, object>
                //{
                //    ["acks"] = 1
                //};


                _kafkaSetting = _setting.AsEnumerable();
            }
            return _kafkaSetting;
        }
    }
}
