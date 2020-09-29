using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions.Messages;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HotChocoloteSubscriptionTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddLogging(lb =>
            {
                lb.AddSerilog();
            });

            services.AddHttpContextAccessor();

            //services.AddSingleton<NewDataStartMessageHandler>();
            services.AddSingleton<NewDataStopMessageHandler>();
            //services.AddSingleton<DataStartMessageHandler>();

            //services.AddSingleton<IMessageHandler>(sp => sp.GetRequiredService<NewDataStartMessageHandler>());
            services.AddSingleton<IMessageHandler>(sp => sp.GetRequiredService<NewDataStopMessageHandler>());

            services.AddInMemorySubscriptions();

            services.AddGraphQL(sp =>
            SchemaBuilder.New()
                .AddServices(sp)
                .AddQueryType<Query>()
                .AddSubscriptionType<Subscription>()
                .Create(),
                build => 
                build
                .Use(next => async context =>
                {
                    await next.Invoke(context);
                })
                .UseDefaultPipeline());


            services.AddQueryRequestInterceptor((ctx, builder, ct) =>
            {
                if (ctx.WebSockets.IsWebSocketRequest)
                {
                    var cts = new CancellationTokenSource();
                    ctx.Items.Add("DataStopCancellationTokenSource", cts);

                    builder.SetProperty("DataStopCancellationToken", cts.Token);
                }

                return Task.CompletedTask;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseWebSockets();
            app.UseGraphQL("/api/graphql");

            app.UseGraphiQL("/api/graphql", "/graphiql");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
