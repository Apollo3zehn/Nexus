﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nexus.Core;
using Nexus.Services;
using System.Text.Json;

namespace Nexus.Controllers
{
    /// <summary>
    /// Provides access to the system.
    /// </summary>
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    internal class SystemController : ControllerBase
    {
        // [authenticated]
        // GET      /api/system/configuration
        // GET      /api/system/file-type 
        // GET      /api/system/help-link

        // [privileged]
        // PUT      /api/system/configuration

        #region Fields

        private readonly AppState _appState;
        private readonly AppStateManager _appStateManager;
        private readonly GeneralOptions _generalOptions;

        #endregion

        #region Constructors

        public SystemController(
            AppState appState,
            AppStateManager appStateManager,
            IOptions<GeneralOptions> generalOptions)
        {
            _generalOptions = generalOptions.Value;
            _appState = appState;
            _appStateManager = appStateManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the default file type.
        /// </summary>
        [HttpGet("file-type")]
        public string? GetDefaultFileType()
        {
            return _generalOptions.DefaultFileType;
        }

        /// <summary>
        /// Gets the configured help link.
        /// </summary>
        [HttpGet("help-link")]
        public string? GetHelpLink()
        {
            return _generalOptions.HelpLink;
        }

        /// <summary>
        /// Gets the system configuration.
        /// </summary>
        [HttpGet("configuration")]
        public IReadOnlyDictionary<string, JsonElement>? GetConfiguration()
        {
            return _appState.Project.SystemConfiguration;
        }

        /// <summary>
        /// Sets the system configuration.
        /// </summary>
        [HttpPut("configuration")]
        [Authorize(Policy = NexusPolicies.RequireAdmin)]
        public Task SetConfigurationAsync(IReadOnlyDictionary<string, JsonElement>? configuration)
        {
            return _appStateManager.PutSystemConfigurationAsync(configuration);
        }

        #endregion
    }
}
