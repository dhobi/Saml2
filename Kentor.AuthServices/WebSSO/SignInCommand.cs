﻿using Kentor.AuthServices.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Net;

namespace Kentor.AuthServices.WebSso
{
    /// <summary>
    /// The sign in command. Use 
    /// CommandFactory.Get(CommandFactory.SignInCommandName) to get an instance.
    /// </summary>
    public class SignInCommand : ICommand
    {
        /// <summary>
        /// Ctor, don't want anyone to create instances.
        /// </summary>
        internal SignInCommand() { }

        /// <summary>
        /// Run the command, initiating the sign in sequence.
        /// </summary>
        /// <param name="request">Request data.</param>
        /// <param name="options">Options</param>
        /// <returns>CommandResult</returns>
        public CommandResult Run(HttpRequestData request, IOptions options)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return Run(
                new EntityId(request.QueryString["idp"].FirstOrDefault()),
                request.QueryString["ReturnUrl"].FirstOrDefault(),
                request,
                options,
                null);
        }

        /// <summary>
        /// Initiate the sign in sequence.
        /// </summary>
        /// <param name="idpEntityId">Entity id of idp to sign in to, or
        /// null to use default (discovery service if configured)</param>
        /// <param name="returnPath">Path to redirect to when the sign in
        /// is complete.</param>
        /// <param name="request">The incoming http request.</param>
        /// <param name="options">Options.</param>
        /// <param name="relayData">Data to store and make available when the
        /// ACS command has processed the response.</param>
        /// <returns>Command Result</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
        public static CommandResult Run(
            EntityId idpEntityId,
            string returnPath,
            HttpRequestData request,
            IOptions options,
            IDictionary<string, string> relayData)
        {
            if(options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var urls = new AuthServicesUrls(request, options.SPOptions);

            IdentityProvider idp;
            if (idpEntityId == null || idpEntityId.Id == null)
            {
                if (options.SPOptions.DiscoveryServiceUrl != null)
                {
                    return RedirectToDiscoveryService(returnPath, options.SPOptions, urls);
                }

                idp = options.IdentityProviders.Default;
            }
            else
            {
                if (!options.IdentityProviders.TryGetValue(idpEntityId, out idp))
                {
                    throw new InvalidOperationException("Unknown idp");
                }
            }

            Uri returnUrl = null;
            if (!string.IsNullOrEmpty(returnPath))
            {
                var appRelativePath = request.Url.AbsolutePath.Substring(
                    request.ApplicationUrl.AbsolutePath.Length).TrimStart('/');

                returnUrl = new Uri(new Uri(urls.ApplicationUrl, appRelativePath), returnPath);
            }

            var authnRequest = idp.CreateAuthenticateRequest(urls);

            options.Notifications.AuthenticationRequestCreated(authnRequest, idp, relayData);

            var commandResult = idp.Bind(authnRequest);

            commandResult.RequestState = new StoredRequestState(
                idp.EntityId, returnUrl, authnRequest.Id, relayData);
            commandResult.SetCookieName = "Kentor." + authnRequest.RelayState;

            return commandResult;
        }

        private static CommandResult RedirectToDiscoveryService(
            string returnPath,
            SPOptions spOptions,
            AuthServicesUrls authServicesUrls)
        {
            string returnUrl = authServicesUrls.SignInUrl.OriginalString;

            if(!string.IsNullOrEmpty(returnPath))
            {
                returnUrl += "?ReturnUrl=" + Uri.EscapeDataString(returnPath);
            }

            var redirectLocation = string.Format(
                CultureInfo.InvariantCulture,
                "{0}?entityID={1}&return={2}&returnIDParam=idp",
                spOptions.DiscoveryServiceUrl,
                Uri.EscapeDataString(spOptions.EntityId.Id),
                Uri.EscapeDataString(returnUrl));

            return new CommandResult()
            {
                HttpStatusCode = HttpStatusCode.SeeOther,
                Location = new Uri(redirectLocation)
            };
        }
    }
}
