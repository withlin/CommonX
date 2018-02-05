using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Config.Entities
{
    /// <summary>
    /// 执行SQL参数
    /// </summary>
    [Serializable]
    public class ParamInfo
    {
        public ParamInfo()
        {

        }

        public ParamInfo(string paramName, string paramType, object paramValue)
        {
            this.ParamName = paramName;
            this.ParamType = paramType;
            this.ParamValue = paramValue;
        }

        /// <summary>
        /// 参数名
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public string ParamType { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public object ParamValue { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }
    }
}
