﻿using System.Net.Http;
using System.Threading.Tasks;
using HADotNet.Core.Domain;
using HADotNet.Core.Models;

namespace HADotNet.Core.Clients
{
    /// <summary>
    /// Provides access to the info API for retrieving information about Supervisor, Core and Host.
    /// </summary>
    public class InfoClient : BaseClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InfoClient" />.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient" /> preconfigured to communicate with a Home Assistant instance.</param>
        public InfoClient(HttpClient client) : base(client) { }

        /// <summary>
        /// Retrieves Supervisor information.
        /// </summary>
        /// <returns>A <see cref="SupervisorInfoObject"/> representing Supervisor informatio.</returns>
        public async Task<ResponseObject<SupervisorInfoObject>> GetSupervisorInfo()
        {
            try
            {
                return await Get<ResponseObject<SupervisorInfoObject>>("/api/hassio/supervisor/info");
            }
            catch (HttpResponseException hrex) when (hrex.StatusCode == 404)
            {
                throw new SupervisorNotFoundException("This does not appear to be a Home Assistant Supervisor instance. See inner exception for more details.", hrex);
            }
        }

        /// <summary>
        /// Retrieves Host information.
        /// </summary>
        /// <returns>A <see cref="HostInfoObject"/> representing Host informatio.</returns>
        public async Task<ResponseObject<HostInfoObject>> GetHostInfo()
        {
            try
            {
                return await Get<ResponseObject<HostInfoObject>>("/api/hassio/host/info");
            }
            catch (HttpResponseException hrex) when (hrex.StatusCode == 404)
            {
                throw new SupervisorNotFoundException("This does not appear to be a Home Assistant Supervisor instance. See inner exception for more details.", hrex);
            }
        }

        /// <summary>
        /// Retrieves Core information.
        /// </summary>
        /// <returns>A <see cref="CoreInfoObject"/> representing Host informatio.</returns>
        public async Task<ResponseObject<CoreInfoObject>> GetCoreInfo()
        {
            try
            {
                return await Get<ResponseObject<CoreInfoObject>>("/api/hassio/core/info");
            }
            catch (HttpResponseException hrex) when (hrex.StatusCode == 404)
            {
                throw new SupervisorNotFoundException("This does not appear to be a Home Assistant Supervisor instance. See inner exception for more details.", hrex);
            }
        }
    }
}
