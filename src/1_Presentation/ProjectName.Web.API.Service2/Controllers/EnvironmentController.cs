﻿
using System;

using Microsoft.AspNetCore.Mvc;

using Praticis.Framework.Environment.Abstractions;

namespace ProjectName.Web.API.Service2.Controllers
{
    [Route("api/[Controller]")]
    public class EnvironmentController : BaseController
    {
        private readonly IEnvironment _environment;

        public EnvironmentController(IServiceProvider provider)
            : base(provider)
        {
            this._environment = provider.GetService<IEnvironment>();
        }

        public IActionResult Index()
            => Json(this._environment);
    }
}