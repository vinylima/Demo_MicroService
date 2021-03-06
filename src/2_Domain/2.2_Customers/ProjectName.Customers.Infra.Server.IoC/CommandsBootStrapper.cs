﻿
using Praticis.Framework.Bus.Abstractions.Commands;

using ProjectName.Customers.Application.Commands._1._0.CustomerCommands;
using ProjectName.Customers.Core.Handlers.CustomerHandlers;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class CommandsBootStrapper
    {
        internal static void AddCustomerCommands(this IServiceCollection services)
        {
            // Customer Command Handlers
            services.AddScoped<ICommandHandler<SaveCustomerCommand>, SaveCustomerCommandHandler>();
        }
    }
}