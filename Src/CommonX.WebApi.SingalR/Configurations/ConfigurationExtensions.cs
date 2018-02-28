#region
using System.Reflection;
using Autofac;
using CommonX.Autofac;
using CommonX.Components;
using CommonX.Configurations;

#endregion

namespace CommonX.WebApi.SignalR.Configurations
{
    /// <summary>
    ///     Configuration class for framework.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        ///     create SingalR [No Success Use]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="app"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Configuration UseSingalR(this Configuration configuration, Assembly[] ass)
        {
            var objectContainer = ObjectContainer.Current as AutofacObjectContainer;
            if (objectContainer != null)
            {
                ////app.UseCors(CorsOptions.AllowAll);
                //var config = new HubConfiguration()
                //{
                //    EnableDetailedErrors = true
                //};

                var container = objectContainer.Container;
                var builder = new ContainerBuilder();
                //// Register your SignalR hubs.
                //builder.RegisterHubs(ass);
                //config.Resolver = new AutofacDependencyResolver(container);
                //app.MapSignalR("/signalr",config);

                builder.Update(container);
            }
            return configuration;
        }
    }
}