using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CommonX.Cache.Ignite.Store
{
    [Serializable]
    public class OggEntryWithProperty: OggEntry
    {
        public string PropertyJson { get; set; }
    }
        [Serializable]
    public class OggEntry
    {
        /// <summary>
        /// 这个名字等待李永祥给
        /// </summary>
        public string HermesId { get; set; }
        public DbOperation Op { get; set; }
        public string Table { get; internal set; }
        public string Type { get; set; }
        public Dictionary<string, object> Before { get; set; }
        public Dictionary<string, object> After { get; set; }
        /// <summary>
        /// 发生时间
        /// </summary>
        public DateTime TS { get; internal set; }
        public static string GetCacheName(string forCacheName)
        {
            return "Ogg_Db_ChangeNotifications_Cache_" + forCacheName;
        }
        public string JsonMessage { get; set; }
        public override string ToString()
        {
            if (null != JsonMessage) return JsonMessage;
            else return base.ToString();
        }
    }
    [Serializable]
    internal class OggEntryProperty
    {
        internal const string GG_XID = "GG_XID";
        internal const string GG_JMS_TIMESTAMP = "GG_JMS_TIMESTAMP";
        internal const string GG_TX_TIMESTAMP_STR = "GG_TX_TIMESTAMP_STR";
        internal const string GG_SCHEMA = "GG_SCHEMA";
        internal const string GG_JMS_MSG_CREATE_DURATION = "GG_JMS_MSG_CREATE_DURATION";
        internal const string GG_TABLE = "GG_TABLE";
        internal const string GG_TXIND = "GG_TXIND";
        internal const string GG_TX_READ_TIMESTAMP = "GG_TX_READ_TIMESTAMP";
        internal const string GG_OBJNAME = "GG_OBJNAME";
        internal const string GG_ID = "GG_ID";
        internal const string GG_OPTYPE = "GG_OPTYPE";
    }

    [Serializable]
    public class OggEntryColumn
    {
        public string name { get; set; }
        public string index { get; set; }
        [JsonConverter(typeof(BeforeAfterValueConvert))]
        public string after { get; set; }
        [JsonConverter(typeof(BeforeAfterValueConvert))]
        public string before { get; set; }
    }

    public class BeforeAfterValueConvert : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.EndArray)
                    return new Dictionary<string, string>();
                else
                    throw new JsonSerializationException("Non-empty JSON array does not make a valid Dictionary!");
            }
            else if (reader.TokenType == JsonToken.String)
            {
                return reader.Value.ToString();
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                reader.Read();
                while (reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType == JsonToken.EndArray)
                    {
                        reader.Read();
                        continue;
                    }

                    if (reader.TokenType != JsonToken.PropertyName)
                        throw new JsonSerializationException("Unexpected token!");

                    string key = (string)reader.Value;
                    reader.Read();

                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        reader.Read();
                    }

                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        reader.Read();

                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            key = (string)reader.Value;
                            reader.Read();
                        }
                    }

                    //if (reader.TokenType != JsonToken.String && reader.TokenType != JsonToken.Integer)
                    //    throw new JsonSerializationException("Unexpected token!");

                    string value = Convert.ToString(reader.Value);
                    ret.Add(key, value);
                    reader.Read();
                }
                return null;
            }
            else
            {
                throw new JsonSerializationException("Unexpected token!");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class OggEntryOriginal
    {
        public string table { get; set; }
        public string type { get; set; }
        public string ts { get; set; }
        public string pos { get; set; }
        public string numCols { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public List<OggEntryColumn> cols { get; set; }
        public string Hermes_ID { get; set; }
    }

    public static class OggParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static OggEntry Parse(OggEntryOriginal o)
        {
            var ogg = new OggEntry()
            {
                Table = o.table,
                Type = o.type,
                TS = DateTime.Parse(o.ts, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal),
                Op = (DbOperation)Enum.Parse(typeof(DbOperation), o.Properties[OggEntryProperty.GG_OPTYPE]),
                HermesId = o.Hermes_ID
            };
            //如果update 并且before和after都是一样，不更新，返回null
            if (ogg.Op == DbOperation.UPDATE)
            {
                if (o.cols.Where(c => c.name != "VERSION" && c.name != "LASTMODIFYTIME").All(c => c.before == c.after))
                    return null;
            }
            ogg.After = new Dictionary<string, object>();
            ogg.Before = new Dictionary<string, object>();
            foreach (var c in o.cols)
            {
                if (!string.IsNullOrEmpty(c.before))
                {
                    ogg.Before.Add(c.name, c.before);
                }
                if (!string.IsNullOrEmpty(c.after))
                {
                    ogg.After.Add(c.name, c.after);
                }
            }
            return ogg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static OggEntryWithProperty ParseWithProperty(OggEntryOriginal o)
        {
            var ogg = ConvertTo< OggEntry, OggEntryWithProperty>(Parse(o));
            o.Properties = o.Properties ?? new Dictionary<string, string>();
            ogg.PropertyJson = JsonConvert.SerializeObject(o.Properties);
            return ogg;
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <typeparam name="TOrign"></typeparam>
        /// <typeparam name="TGoal"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static TGoal ConvertTo<TOrign, TGoal>(TOrign parent) where TGoal : TOrign, new()
        {
            var child = new TGoal();
            var ParentType = typeof(TOrign);
            var Properties = ParentType.GetProperties();
            foreach (var Propertie in Properties)
            {
                //循环遍历属性
                if (Propertie.CanRead && Propertie.CanWrite)
                {
                    //进行属性拷贝
                    Propertie.SetValue(child, Propertie.GetValue(parent, null), null);
                }
            }
            return child;
        }
    }

    public enum DbOperation
    {
        INSERT,
        UPDATE,
        DELETE
    }
}
