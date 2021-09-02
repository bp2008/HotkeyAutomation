﻿using System.Net.Http;
using System.Threading.Tasks;
using HADotNet.Core.Models;

namespace HADotNet.Core.Clients
{
    /// <summary>
    /// Provides access to the automations API for working with automations.
    /// </summary>
    public class AutomationClient : BaseClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationClient" />.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient" /> preconfigured to communicate with a Home Assistant instance.</param>
        public AutomationClient(HttpClient client) : base(client) { }

        /// <summary>
        /// Create the <see cref="AutomationObject"/>.
        /// </summary>
        /// <param name="automation">The <see cref="AutomationObject"/>.</param>
        /// <returns>The <see cref="AutomationResultObject"/>.</returns>
        public async Task<AutomationResultObject> Create(AutomationObject automation) => await Post<AutomationResultObject>($"/api/config/automation/config/{automation.Id}", automation);

        /// <summary>
        /// Read the <see cref="AutomationObject"/>.
        /// </summary>
        /// <param name="id">The automation id.</param>
        /// <returns>The <see cref="AutomationObject"/>.</returns>
        public async Task<AutomationObject> Get(string id) => await Get<AutomationObject>($"/api/config/automation/config/{id}");

        /// <summary>
        /// Update the <see cref="AutomationObject"/>.
        /// </summary>
        /// <param name="automation">The <see cref="AutomationObject"/>.</param>
        /// <returns>The <see cref="AutomationResultObject"/>.</returns>
        public async Task<AutomationResultObject> Update(AutomationObject automation) => await Create(automation);

        /// <summary>
        /// Delete the <see cref="AutomationObject"/>.
        /// </summary>
        /// <param name="id">The automation id.</param>
        /// <returns>The <see cref="AutomationResultObject"/>.</returns>
        public new async Task<AutomationResultObject> Delete(string id) => await Delete<AutomationResultObject>($"/api/config/automation/config/{id}");
    }
}
