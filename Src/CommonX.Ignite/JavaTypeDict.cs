using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonX.Cache.Ignite
{
    public class JavaTypeDict
    {
        public static Dictionary<Type, string> m_Dict = new Dictionary<Type, string>();
        static JavaTypeDict()
        {
            m_Dict.Add(typeof(int), "java.lang.Integer");
            m_Dict.Add(typeof(string), "java.lang.String");
            m_Dict.Add(typeof(decimal), "java.math.BigDecimal");
            m_Dict.Add(typeof(DateTime), "java.util.Calendar");
            m_Dict.Add(typeof(int?),"java.lang.Integer");
            m_Dict.Add(typeof(decimal?), "java.math.BigDecimal");
            m_Dict.Add(typeof(DateTime?), "java.util.Calendar");
            m_Dict.Add(typeof(long), "java.lang.Long");
            m_Dict.Add(typeof(long?), "java.lang.Long");
        }
        public static bool IsExist(Type type)
        {
            return m_Dict.ContainsKey(type);
        }
        public static string GetJavaType(Type type)
        {
            return m_Dict[type];
        }
    }
}
