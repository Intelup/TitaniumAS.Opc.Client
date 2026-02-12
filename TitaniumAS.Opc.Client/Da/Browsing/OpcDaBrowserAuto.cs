using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Common.Logging;
using TitaniumAS.Opc.Client.Common;
using TitaniumAS.Opc.Client.Common.Internal;

namespace TitaniumAS.Opc.Client.Da.Browsing
{
    /// <summary>
    /// Represents a browser which automatically browse an OPC DA server using different protocol versions in the following order 3.0, 2.05, 1.0.
    /// </summary>
    /// <seealso cref="TitaniumAS.Opc.Da.Browsing.IOpcDaBrowser" />
    public class OpcDaBrowserAuto : IOpcDaBrowser
    {
        private static readonly ILog Log = LogManager.GetLogger<OpcDaBrowserAuto>();
        private readonly OpcDaBrowser1 _browser1 = new OpcDaBrowser1();
        private readonly OpcDaBrowser2 _browser2 = new OpcDaBrowser2();
        private readonly OpcDaBrowser3 _browser3 = new OpcDaBrowser3();
        private readonly OpcServerCategory[] _capabilities;
        private OpcDaServer _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpcDaBrowserAuto"/> class.
        /// </summary>
        /// <param name="server">The OPC DA server for browsing.</param>
        public OpcDaBrowserAuto(OpcDaServer server)
        {
            OpcDaServer = server;

            _capabilities = ResolveCapabilities(server);

            if (IsSupported(OpcServerCategory.OpcDaServer30))
            {
                Log.Info("OPC DA server advertises support for DA 3.0.");
                _browser3.OpcDaServer = server;
            }
            else if (IsSupported(OpcServerCategory.OpcDaServer20))
            {
                Log.Info("OPC DA server advertises support for DA 2.0.");
                _browser2.OpcDaServer = server;
            }
            else if (IsSupported(OpcServerCategory.OpcDaServer10))
            {
                Log.Info("OPC DA server advertises support for DA 1.0.");
                _browser1.OpcDaServer = server;
            }
             else
            {
                Log.Warn("OPC DA server does not advertise support for DA 1.0, 2.0, or 3.0.");
                throw new InvalidOperationException("OPC DA server does not advertise support for DA 1.0, 2.0, or 3.0.");
            }

        }

        private static OpcServerCategory[] ResolveCapabilities(OpcDaServer server)
        {
            OpcServerCategory[] categories = null;

            try
            {
                string host = server.ServerDescription.Host;

                using (CategoryEnumerator categoryEnumerator = new CategoryEnumerator(host))
                {
                    categories = categoryEnumerator.GetCategories(server.ServerDescription.CLSID);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to load OPC server categories from COM.", ex);
                return Array.Empty<OpcServerCategory>();
            }

            return categories;
        }

        private bool IsSupported(OpcServerCategory category)
        {
            if (_capabilities == null || _capabilities.Length == 0) return false;
            return _capabilities.Any(capability => capability != null && capability.CATID == category.CATID);
        }

        /// <summary>
        /// Gets or sets the OPC DA server for browsing.
        /// </summary>
        /// <value>
        /// The OPC DA server.
        /// </value>
        public OpcDaServer OpcDaServer
        {
            get { return _server; }
            set { _server = value; }
        }

        /// <summary>
        /// Browse the server using specified criteria.
        /// </summary>
        /// <param name="parentItemId">Indicates the item identifier from which browsing will begin. If the root branch is to be browsed then a null should be passed.</param>
        /// <param name="filter">The filtering context.</param>
        /// <param name="propertiesQuery">The properties query.</param>
        /// <returns>
        /// Array of browsed elements.
        /// </returns>
        /// <exception cref="System.AggregateException"></exception>
        public OpcDaBrowseElement[] GetElements(string parentItemId, OpcDaElementFilter filter = null,
            OpcDaPropertiesQuery propertiesQuery = null)
        {
            List<Exception> errors = new List<Exception>();

            if (IsSupported(OpcServerCategory.OpcDaServer30))
            {
                try
                {
                    return _browser3.GetElements(parentItemId, filter, propertiesQuery);
                }
                catch (Exception ex1)
                {
                    errors.Add(ex1);
                    Log.Warn("Failed to browse address space using IOPCBrowse (OPC DA 3.x).", ex1);
                }
            }

            if (IsSupported(OpcServerCategory.OpcDaServer20))
            {
                try
                {
                    return _browser2.GetElements(parentItemId, filter, propertiesQuery);
                }
                catch (Exception ex2)
                {
                    errors.Add(ex2);
                    Log.Warn("Failed to browse address space using IOPCBrowseServerAddressSpace (OPC DA 2.x).", ex2);
                }
            }

            if (IsSupported(OpcServerCategory.OpcDaServer10))
            {
                try
                {
                    return _browser1.GetElements(parentItemId, filter, propertiesQuery);
                }
                catch (Exception ex3)
                {
                    errors.Add(ex3);
                    Log.Warn("Failed to browse address space using IOPCBrowseServerAddressSpace (OPC DA 1.x).", ex3);
                }
            }

            if (errors.Count > 0)
            {
                throw new AggregateException(errors);
            }

            throw new InvalidOperationException("OPC DA server does not advertise support for DA 1.0, 2.0, or 3.0.");
        }

        /// <summary>
        /// Gets properties of items by specified item identifiers.
        /// </summary>
        /// <param name="itemIds">The item identifiers.</param>
        /// <param name="propertiesQuery">The properties query.</param>
        /// <returns>
        /// Array of properties of items.
        /// </returns>
        /// <exception cref="System.AggregateException"></exception>
        public OpcDaItemProperties[] GetProperties(IList<string> itemIds, OpcDaPropertiesQuery propertiesQuery = null)
        {
            List<Exception> errors = new List<Exception>();

            if (IsSupported(OpcServerCategory.OpcDaServer30))
            {
                try
                {
                    return _browser3.GetProperties(itemIds, propertiesQuery);
                }
                catch (Exception ex1)
                {
                    errors.Add(ex1);
                    Log.Warn("Failed to get properties using IOPCBrowse (OPC DA 3.x).", ex1);
                }
            }

            if (IsSupported(OpcServerCategory.OpcDaServer20))
            {
                try
                {
                    return _browser2.GetProperties(itemIds, propertiesQuery);
                }
                catch (Exception ex2)
                {
                    errors.Add(ex2);
                    Log.Warn("Failed to get properties using IOPCItemProperties (OPC DA 2.x).", ex2);
                }
            }

            if (IsSupported(OpcServerCategory.OpcDaServer10))
            {
                try
                {
                    return _browser1.GetProperties(itemIds, propertiesQuery);
                }
                catch (Exception ex3)
                {
                    errors.Add(ex3);
                    Log.Warn("Failed to get properties using IOPCItemProperties (OPC DA 1.x).", ex3);
                }
            }

            if (errors.Count > 0)
            {
                throw new AggregateException(errors);
            }

            throw new InvalidOperationException("OPC DA server does not advertise support for DA 1.0, 2.0, or 3.0.");
        }
    }
}