using CommonX.Components;
using CommonX.Messages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CommonX.Configurations
{
    public class Setting
    {
        public const string CONST_CONN_NAME = "OracleDbContext";
        /// <summary>
        ///     配置文件根目录
        /// </summary>
        public readonly string ConfigBase = "Cfg/";

        /// <summary>
        ///     程序根路径
        /// </summary>
        public readonly string RootPath;

        /// <summary>
        ///     当前id
        /// </summary>
        public Guid Id;

        public Setting()
        {
            Id = Guid.NewGuid();
            RootPath = AppDomain.CurrentDomain.BaseDirectory;//SetupInformation.ApplicationBase
            ModuleLevel = ModuleLevel.All;

            MessageSettings = (MessageSettings)ConfigurationManager.GetSection(MessageSettings.SectionName);
            if (MessageSettings == null)
                MessageSettings = new MessageSettings();

            Branch = ConfigurationManager.AppSettings["Branch"];

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["AccountingMode"])) { AccountingMode = Convert.ToInt32(ConfigurationManager.AppSettings["AccountingMode"]); }
            //启用读取数据库中的缓存配置
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseDbCacheConfiguration"])) { UseDbCacheConfiguration = Convert.ToBoolean(ConfigurationManager.AppSettings["UseDbCacheConfiguration"]); }
            //启用Ignite校验
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseIgniteCompute"])) { UseIgniteCompute = Convert.ToBoolean(ConfigurationManager.AppSettings["UseIgniteCompute"]); }


            ServiceType = ConfigurationManager.AppSettings["ServiceType"];
            ServiceName = ConfigurationManager.AppSettings["ServiceName"];
            AuthUrl = ConfigurationManager.AppSettings["AuthUrl"];
            //取消掉OracleSchema
            //OracleSchema = ConfigurationManager.AppSettings["OracleSchema"];
            OracleSchema = "";

            Transport = ConfigurationManager.AppSettings["NServiceBus/Transport"];
            BusinessRuleConfig = ConfigurationManager.AppSettings["BusinessRuleConfig"];

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseLogInterceptor"])) { UseLogInterceptor = bool.Parse(ConfigurationManager.AppSettings["UseLogInterceptor"]); }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseMeasureInterceptor"])) { UseMeasureInterceptor = bool.Parse(ConfigurationManager.AppSettings["UseMeasureInterceptor"]); }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseExceptionHandlingInterceptor"])) { UseExceptionHandlingInterceptor = bool.Parse(ConfigurationManager.AppSettings["UseExceptionHandlingInterceptor"]); }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseCachingInterceptor"])) { UseCachingInterceptor = bool.Parse(ConfigurationManager.AppSettings["UseCachingInterceptor"]); }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["UseLookupQueryInterceptor"])) { UseLookupQueryInterceptor = bool.Parse(ConfigurationManager.AppSettings["UseLookupQueryInterceptor"]); }
            //RegisterServerHost = ConfigurationManager.AppSettings["RegisterServerHost"];
            //InternetRegisterServerHost = ConfigurationManager.AppSettings["InternetRegisterServerHost"];
            LogKeeperUrl = ConfigurationManager.AppSettings["LogKeeperUrl"];

            if (!string.IsNullOrEmpty(Transport))
            {
                var ar = Transport.Split(';');

                TransportIp = ar[0].Split('=')[1];
                TransportUserName = ar[1].Split('=')[1];
                TransportPassword = ar[2].Split('=')[1];

                if (ar.Length > 3 && !string.IsNullOrEmpty(ar[3]))
                {
                    TransportVirtualHost = ar[3].Split('=')[1];
                }
            }

            if (ConfigurationManager.ConnectionStrings[CONST_CONN_NAME] != null)
            {
                ConnString = ConfigurationManager.ConnectionStrings[CONST_CONN_NAME].ConnectionString;
            }
            ConnPoolNum = new Random().Next(1, 10);
            //if (!string.IsNullOrEmpty(RegisterServerHost))
            //{
            //    RegisterServerHost = RegisterServerHost.EndsWith("/")
            //        ? RegisterServerHost
            //        : string.Format("{0}/", RegisterServerHost);
            //}

            //if (!string.IsNullOrEmpty(InternetRegisterServerHost))
            //{
            //    InternetRegisterServerHost = InternetRegisterServerHost.EndsWith("/")
            //        ? InternetRegisterServerHost
            //        : string.Format("{0}/", InternetRegisterServerHost);
            //}
        }

        /// <summary>
        ///     服务类型
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        ///     服务名称
        /// </summary>
        public string ServiceName { get; set; }

        public string ServerName
        {
            get { return Environment.MachineName; }
        }

        public bool SkipDeserializeError { get; set; }

        /// <summary>
        ///     当前程序级别
        /// </summary>
        public ModuleLevel ModuleLevel { get; set; }

        /// <summary>
        ///     消息设置
        /// </summary>
        public MessageSettings MessageSettings { get; set; }

        /// <summary>
        ///     数据库连接
        /// </summary>
        public string ConnString { get; set; }

        /// <summary>
        /// 使用随机数，使每个字符串不一致，以便使每个iis使用不同的连接池
        /// </summary>
        public int ConnPoolNum { get; set; } = 0;

        /// <summary>
        ///     公司Id,现在用于缓存预热,多公司","分割
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// 单据记账方式
        /// 0-走ERP记账    1-走新版记账
        /// </summary>
        public int AccountingMode { get; set; } = 0;

        /// <summary>
        /// 启用Ignite校验
        /// </summary>
        public bool UseIgniteCompute { get; set; } = false;

        /// <summary>
        /// 启用读取数据库中的缓存配置
        /// </summary>
        public bool UseDbCacheConfiguration { get; set; } = false;

        /// <summary>
        ///     配置服务器 endpoint
        /// </summary>
        public string ConfigServerEndPoint { get; set; }

        /// <summary>
        ///     消息总线 传输层配置
        /// </summary>
        public string Transport { get; set; }
        public string TransportHost { get; set; }

        public string TransportIp { get; set; }

        public string TransportUserName { get; set; }

        public string TransportPassword { get; set; }
        public string TransportVirtualHost { get; set; }
        public  string TransportExchange { get; set; }

        public string LogKeeperUrl { get; set; }

        /// <summary>
        ///     业务规则调用方式
        /// </summary>
        public string BusinessRuleConfig { get; set; }

        ///// <summary>
        ///// 内网注册服务器Host
        ///// </summary>
        //public string RegisterServerHost { get; set; }

        ///// <summary>
        ///// 外地注册服务器Host
        ///// </summary>
        //public string InternetRegisterServerHost { get; set; }


        //public string ValidateConfigPath
        //{
        //    get { return ConfigBase + Branch + "/Validate.config"; }
        //}

        /// <summary>
        ///     当前 endpoint
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        ///     auth url
        /// </summary>
        public string AuthUrl { get; set; }

        /// <summary>
        ///     oracle schema
        /// </summary>
        public string OracleSchema { get; set; }

        /// <summary>
        /// 使用LogInterceptor
        /// </summary>
        public bool UseLogInterceptor { get; set; } = true;

        /// <summary>
        /// 使用MeasureInterceptor
        /// </summary>
        public bool UseMeasureInterceptor { get; set; } = true;

        /// <summary>
        /// 使用ExceptionHandlingInterceptor
        /// </summary>
        public bool UseExceptionHandlingInterceptor { get; set; } = true;

        /// <summary>
        /// 使用UseCachingInterceptor
        /// </summary>
        public bool UseCachingInterceptor { get; set; } = true;

        /// <summary>
        /// 使用LookupQueryInterceptor
        /// </summary>
        public bool UseLookupQueryInterceptor { get; set; }

        /// <summary>
        ///     是否启用数据变更通知
        /// </summary>
        public bool EnableChangeNotify { get; set; } = false;

        /// <summary>
        /// RabbitMQ配置
        /// </summary>
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5607;
        public string VirtualHost { get; set; } = "/";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public bool AutomaticRecoveryEnabled { get; set; } = true;
        public int RequestedConnectionTimeout { get; set; } = 15000;


        #region public method
        public string ConfigServerIP
        {
            get { return Transport.Remove(Transport.IndexOf(';')).Substring(5); }
        }


        public string ValidateConfigPath(string branch, string name, string ruleset = "ERP")
        {
            return ConfigBase + branch + "/validate/" + name + "-" + ruleset.ToUpper() + ".config";
        }
        #endregion
    }
}
