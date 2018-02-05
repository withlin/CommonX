using System;

namespace CommonX.Components
{
    /// <summary>An attribute to indicate a class is a component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Interface)]
    public class ComponentAttribute : System.Attribute
    {
        /// <summary>
        /// 模块分级加载
        /// </summary>
        public ModuleLevel ModuleLevel { get; private set; }


        /// <summary>
        /// 拦截器属性
        /// </summary>
        public Interceptor Interceptor { get; private set; }

        public object Key { get; private set; }

        /// <summary>The lifetime of the component.
        /// </summary>
        public LifeStyle LifeStyle { get; private set; }
        /// <summary>Default constructor.
        /// </summary>
        public ComponentAttribute() : this(LifeStyle.Singleton) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="lifeStyle"></param>
        public ComponentAttribute(LifeStyle lifeStyle)
        {
            LifeStyle = lifeStyle;
        }
        public ComponentAttribute(Interceptor interceptor) : this(LifeStyle.Transient, ModuleLevel.Normal, interceptor, null)
        {
        }
        public ComponentAttribute(LifeStyle lifeStyle, ModuleLevel moduleLevel)
        {
            LifeStyle = lifeStyle;
            ModuleLevel = moduleLevel;
        }
        public ComponentAttribute(LifeStyle lifeStyle, Interceptor interceptor) : this(lifeStyle, ModuleLevel.Normal, interceptor, null)
        {
        }
        public ComponentAttribute(LifeStyle lifeStyle, ModuleLevel moduleLevel, Interceptor interceptor) : this(lifeStyle, moduleLevel, interceptor, null)
        {
        }
        public ComponentAttribute(LifeStyle lifeStyle, ModuleLevel moduleLevel, Interceptor interceptor, object key)
        {
            LifeStyle = lifeStyle;
            ModuleLevel = moduleLevel;
            Interceptor = interceptor;
            Key = key;
        }

    }
    /// <summary>An enum to description the lifetime of a component.
    /// </summary>
    public enum LifeStyle
    {
        /// <summary>
        /// Represents a component is a transient component.
        /// </summary>
        Transient,
        /// <summary>
        /// Represents a component is a singleton component.
        ///  不可以用在内部使用了InstancePerRequest的component上面
        /// </summary>
        Singleton,
        /// <summary>
        ///     Represents a component is a lifescope component.
        /// </summary>
        InstancePerRequest,
    }
    /// <summary>
    /// 提供模块分级加载属性
    /// </summary>
    public enum ModuleLevel
    {
        /// <summary>
        ///  核心模块
        /// </summary>
        Core,

        /// <summary>
        /// 普通模块
        /// </summary>
        Normal,

        /// <summary>
        /// 第三方
        /// </summary>
        Third,

        /// <summary>
        /// 所有
        /// </summary>
        All
    }
    /// <summary>
    /// 拦截器enum
    /// </summary>
    [Flags]
    public enum Interceptor
    {
        None = 0,
        /// <summary>
        /// Log 
        /// </summary>
        Log = 1,
        /// <summary>
        /// 性能测量
        /// </summary>
        Measure = 2,

        /// <summary>
        /// 进行缓存，主要用于数据服务上
        /// </summary>
        Caching = 4,
        /// <summary>
        /// 进行异常捕捉并处理
        /// </summary>
        ExceptionHandle = 8,

        ///// <summary>
        ///// 进行lookup数据拦截处理
        ///// </summary>
        //LookupQueryHandle = 16,
    }
}
