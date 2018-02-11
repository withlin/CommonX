using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Attribute
{
    /// <summary>
    /// 关联查询
    /// </summary>
    public class LookupQueryAttribute : System.Attribute
    {
        /// <summary>
        /// 调用服务
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// 调用方法
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 常量值
        /// </summary>
        public string ConstValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceType">接口全称，命名空间,如："CommonX.Shared.Interfaces.ILookupQueryService, CommonX.Shared.Interfaces"</param>
        /// <param name="method">方法名</param>
        /// <param name="propertyName">需要匹配的属性名，多个属性以","分开</param>
        /// <param name="constValue">需要匹配的常量名，多个常量以","分开</param>
        public LookupQueryAttribute(string serviceType, string method, string propertyName = null, string constValue = null)
        {
            this.ServiceType = serviceType;
            this.Method = method;
            this.PropertyName = propertyName;
            this.ConstValue = constValue;
        }
    }
}
