// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.AI.QnA;
using QnABot.Models;
using QnABot.Storages;
using QnABot.Cards;
using QnABot.States;

namespace Microsoft.BotBuilderSamples
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddHttpClient();

            services.AddTransient<IBot, QnABot.Bots.QnABot>();

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton<IStorage, MemoryStorage>();

            services.AddSingleton<BotUserState>();

            services.AddSingleton<BotConversationState>();

            services.AddSingleton<UserStateStorage>();

            services.AddSingleton<ConversationStorage>();

            services.AddTransient<QnAMakerEndpoint>();

            services.AddTransient<IQnAService, QnAService>();

            services.AddTransient<ConversationState>();

            services.AddTransient<UserState>();

            services.AddTransient<ConversationStorage>();

            services.AddTransient<UserStateStorage>();

            services.AddTransient<WelcomeCard>();

            services.AddTransient<SupportTicketCard>();

            services.AddTransient<Incident>();

            services.AddTransient<UserFeedbackCard>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
