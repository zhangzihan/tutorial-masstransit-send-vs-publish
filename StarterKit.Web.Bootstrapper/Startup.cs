﻿namespace StarterKit.Web.Bootstrapper
{
    using Autofac;
    using Autofac.Integration.Mvc;
    using log4net.Config;
    using MassTransit;
    using MassTransit.Log4NetIntegration.Logging;
    using Microsoft.Owin;
    using Owin;
    using System;
    using System.Threading;
    using System.Web.Mvc;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Loads the config from our App.config
            XmlConfigurator.Configure();

            // MassTransit to use Log4Net
            Log4NetLogger.Use();

            var container = IocConfig.RegisterDependencies();

            // Sets the Mvc resolver
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Sets Mvc Owin resolver as well
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();

            // Starts Mass Transit Service bus, and registers stopping of bus on app dispose
            var bus = container.Resolve<IBusControl>();
            var busHandle = bus.StartAsync().Result;

            if (app.Properties.ContainsKey("host.OnAppDisposing"))
            {
                var context = new OwinContext(app.Properties);
                var token = context.Get<CancellationToken>("host.OnAppDisposing");
                if (token != CancellationToken.None)
                {
                    token.Register(() => busHandle.Stop(TimeSpan.FromSeconds(30)));
                }
            }
        }
    }
}