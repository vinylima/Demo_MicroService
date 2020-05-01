﻿
using System;
using System.Dynamic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

using Praticis.Framework.Bus.Abstractions;
using Praticis.Framework.Bus.Abstractions.ValueObjects;
using Praticis.Framework.Environment.Abstractions;

namespace ProjectName.Web.API.Service1.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IServiceBus ServiceBus;
        private readonly IEnvironment _environment;
        //protected readonly IUser CurrentUser;

        public BaseController(IServiceBus serviceBus, IServiceProvider provider)
        {
            //this.CurrentUser = serviceProvider.GetService<IUser>();
            this.ServiceBus = serviceBus;
            this._environment = provider.GetService<IEnvironment>();
        }

        public BaseController(IServiceProvider provider)
        {
            //this.CurrentUser = serviceProvider.GetService<IUser>();
            this.ServiceBus = provider.GetService<IServiceBus>();
            this._environment = provider.GetService<IEnvironment>();
        }

        /// <summary>
        /// Inform if has notifications. By default consider Notifications and SystemErrors.
        /// Use the parameters to chose the options that be analisy.
        /// </summary>
        /// <returns>
        /// Return 'True' if exists or 'False' if don't exists.
        /// </returns>
        protected bool ResultIsValid() => !this.ServiceBus.Notifications.HasNotifications();

        /// <summary>
        /// Inform if has notifications. By default consider Notifications and SystemErrors.
        /// Use the parameters to chose the options that be analisy.
        /// </summary>
        /// <param name="includeNotifications">Use 'True' to consider "Notifications" in the analisy or 'False' to don't.</param>
        /// <param name="includeWarnings">Use 'True' to consider "Warnings" in the analisy or 'False' to don't.</param>
        /// <param name="includeSystemErrors">Use 'True' to consider "System Errors" in the analisy or 'False' to don't.</param>
        /// <returns>
        /// Return 'True' if exists or 'False' if don't exists.
        /// </returns>
        protected bool ResultIsValid(bool includeNotifications = true, bool includeWarnings = false,
            bool includeSystemErrors = true)
        {
            return !this.ServiceBus.Notifications.HasNotifications(includeNotifications, includeWarnings,
                includeSystemErrors);
        }

        /// <summary>
        /// Create a Response to Front-End with serialized data passed by parameters and
        /// informations about possible notifications, warnings and error codes.
        /// </summary>
        /// <param name="response">Response data to serialize.</param>
        /// <returns>
        /// Return an Action Result with Response created.
        /// </returns>
        protected new IActionResult Response(object response = null)
        {
            dynamic responseData = new ExpandoObject();

            responseData.success = this.ResultIsValid();

            responseData.data = response;

            responseData.warnings = this.ServiceBus.Notifications.GetWarnings().Select(e => new
            {
                e.EventId,
                e.Code,
                e.Value,
                e.EventType,
                e.NotificationType,
                e.Time
            });

            responseData.errors = this.ServiceBus.Notifications.GetNotifications().Select(e => new
            {
                e.EventId,
                e.Code,
                e.Value,
                e.EventType,
                e.NotificationType,
                e.Time
            });

            responseData.error_codes = this.ServiceBus.Notifications.GetSystemErrors().Select(e => e.Code);

            if(this._environment.IsDevelopment())
            {
                responseData.system_errors = this.ServiceBus.Notifications.GetSystemErrors().Select(e => new
                {
                    e.EventId,
                    e.Code,
                    e.Value,
                    e.EventType,
                    e.NotificationType,
                    e.Time
                });

                responseData.log = this.ServiceBus.Notifications.GetLogs().Select(e => new
                {
                    e.EventId,
                    e.Code,
                    e.Value,
                    e.EventType,
                    e.NotificationType,
                    e.Time
                });
            }

            if (ResultIsValid())
                return Ok(responseData);

            return BadRequest(responseData);
        }

        /// <summary>
        /// Send Model State Errors to Notification Store.
        /// Use:  if(!ModelState.IsValid)
        ///          NotifyAboutModelStateErrors();
        ///       return Response();
        /// </summary>
        protected void NotifyAboutModelStateErrors()
        {
            string errorMessage = "";
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            foreach (var erro in errors)
            {
                errorMessage = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
                this.ServiceBus.PublishEvent(new Notification(string.Empty, errorMessage));
            }
        }
    }
}