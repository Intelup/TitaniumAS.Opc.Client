using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Common.Logging;
using TitaniumAS.Opc.Client.Common;
using TitaniumAS.Opc.Client.Da.Browsing.Internal;
using TitaniumAS.Opc.Client.Da.Wrappers;
using TitaniumAS.Opc.Client.Interop.Da;
using TitaniumAS.Opc.Client.Interop.Helpers;

namespace TitaniumAS.Opc.Client.Da.Browsing
{
    /// <summary>
    /// Represents an OPC DA 2.05 browser.
    /// </summary>
    /// <seealso cref="TitaniumAS.Opc.Da.Browsing.IOpcDaBrowser" />
    public class OpcDaBrowser2 : IOpcDaBrowser
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private OpcDaServer _opcDaServer;
        protected OpcBrowseServerAddressSpace OpcBrowseServerAddressSpace { get; set; }
        private OpcItemProperties OpcItemProperties { get; set; }
        private readonly Dictionary<string, string[]> _itemIdToPath = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string[]> _nameToPath = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        private string[] _currentPath = Array.Empty<string>();
        private bool _targetIsLeaf;

        /// <summary>
        /// Gets or sets the OPC DA server for browsing.
        /// </summary>
        /// <value>
        /// The OPC DA server.
        /// </value>
        public OpcDaServer OpcDaServer
        {
            get { return _opcDaServer; }
            set
            {
                _opcDaServer = value;
                if (value == null)
                {
                    OpcBrowseServerAddressSpace = null;
                    OpcItemProperties = null;
                }
                else
                {
                    OpcBrowseServerAddressSpace = _opcDaServer.TryAs<OpcBrowseServerAddressSpace>();
                    OpcItemProperties = _opcDaServer.TryAs<OpcItemProperties>();
                }

                _itemIdToPath.Clear();
                _nameToPath.Clear();
                _currentPath = Array.Empty<string>();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpcDaBrowser2"/> class.
        /// </summary>
        /// <param name="opcDaServer">The OPC DA server for browsing.</param>
        public OpcDaBrowser2(OpcDaServer opcDaServer = null)
        {
            OpcDaServer = opcDaServer;
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
        /// <exception cref="System.InvalidOperationException">Interface IOPCBrowseServerAddressSpace not supported.</exception>
        virtual public OpcDaBrowseElement[] GetElements(string parentItemId, OpcDaElementFilter filter = null,
            OpcDaPropertiesQuery propertiesQuery = null)
        {
            if (OpcBrowseServerAddressSpace == null)
                throw new InvalidOperationException("Interface IOPCBrowseServerAddressSpace not supported.");

            if (parentItemId == null)
                parentItemId = string.Empty;

            if (filter == null)
                filter = new OpcDaElementFilter();

            var elements = GetElementsImpl(parentItemId, filter);

            if (propertiesQuery != null)
            {
                var properties = GetProperties(elements.Select(e => e.ItemId).ToArray(), propertiesQuery);
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].ItemProperties = properties[i];
                }
            }
            else
            {
                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].ItemProperties = OpcDaItemProperties.CreateEmpty();
                }
            }

            return elements;
        }

        /// <summary>
        /// Gets properties of items by specified item identifiers.
        /// </summary>
        /// <param name="itemIds">The item identifiers.</param>
        /// <param name="propertiesQuery">The properties query.</param>
        /// <returns>
        /// Array of properties of items.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Interface IOPCItemProperties not supported.</exception>
        public OpcDaItemProperties[] GetProperties(IList<string> itemIds, OpcDaPropertiesQuery propertiesQuery = null)
        {
            if (OpcItemProperties == null)
                throw new InvalidOperationException("Interface IOPCItemProperties not supported.");

            if (propertiesQuery == null)
                propertiesQuery = new OpcDaPropertiesQuery();

            var result = new OpcDaItemProperties[itemIds.Count];

            for (var i = 0; i < result.Length; i++)
            {
                var itemId = itemIds[i];
                try
                {
                    OpcDaItemProperties itemProperties = OpcItemProperties.QueryAvailableProperties(itemId);
                    if (!propertiesQuery.AllProperties) // filter properties
                    {
                        itemProperties.IntersectProperties(propertiesQuery.PropertyIds);
                    }
                    if (propertiesQuery.ReturnValues) // read property values
                    {
                        OpcItemProperties.GetItemProperties(itemId, itemProperties);
                    }
                    OpcItemProperties.LookupItemIDs(itemId, itemProperties);
                    result[i] = itemProperties;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Cannot get properties for item '{0}'.", ex, itemId);
                    result[i] = OpcDaItemProperties.CreateEmpty();
                }
            }

            return result;
        }

        private OpcDaBrowseElement[] GetElementsImpl(string itemId, OpcDaElementFilter filter)
        {
            IEnumerable<OpcDaBrowseElement> elements;
            var namespaceType = OpcBrowseServerAddressSpace.Organization;
            VarEnum dataTypeFilter = TypeConverter.ToVarEnum(filter.DataType);
            switch (namespaceType)
            {
                case OpcDaNamespaceType.Hierarchial:
                    ChangeBrowsePositionTo(itemId);

                    if (_targetIsLeaf)
                    {
                        return Array.Empty<OpcDaBrowseElement>();
                    }

                    switch (filter.ElementType)
                    {
                        case OpcDaBrowseFilter.All:
                            var branches = OpcBrowseServerAddressSpace.BrowseOpcItemIds(OpcDaBrowseType.Branch,
                                filter.Name, dataTypeFilter,
                                filter.AccessRights)
                                .Select(CreateBranchBrowseElement);
                            var leafs = OpcBrowseServerAddressSpace.BrowseOpcItemIds(OpcDaBrowseType.Leaf, filter.Name,
                                dataTypeFilter,
                                filter.AccessRights)
                                .Select(CreateLeafBrowseElement);
                            elements = branches.Union(leafs);
                            break;
                        case OpcDaBrowseFilter.Branches:
                            elements = OpcBrowseServerAddressSpace.BrowseOpcItemIds(OpcDaBrowseType.Branch, filter.Name,
                                dataTypeFilter,
                                filter.AccessRights)
                                .Select(CreateBranchBrowseElement);
                            break;
                        case OpcDaBrowseFilter.Items:
                            elements = OpcBrowseServerAddressSpace.BrowseOpcItemIds(OpcDaBrowseType.Leaf, filter.Name,
                                dataTypeFilter,
                                filter.AccessRights)
                                .Select(CreateLeafBrowseElement);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case OpcDaNamespaceType.Flat:
                    if (filter.ElementType == OpcDaBrowseFilter.Branches) // no branches in flat namespace
                    {
                        elements = Enumerable.Empty<OpcDaBrowseElement>();
                    }
                    else
                    {
                        elements = OpcBrowseServerAddressSpace.BrowseOpcItemIds(OpcDaBrowseType.Flat, filter.Name,
                            dataTypeFilter,
                            filter.AccessRights)
                            .Select(CreateLeafBrowseElement);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return elements.ToArray();
        }

        protected virtual void ChangeBrowsePositionTo(string itemId)
        {
            _targetIsLeaf = false;
            if (string.IsNullOrEmpty(itemId))
            {
                TryMoveToRoot();
                _currentPath = Array.Empty<string>();
                return;
            }

            if (_itemIdToPath.TryGetValue(itemId, out string[] path))
            {
                MoveToPath(path);
                _currentPath = path;
                return;
            }

            if (_nameToPath.TryGetValue(itemId, out path))
            {
                MoveToPath(path);
                _currentPath = path;
                return;
            }

            if (TryFindPathByItemId(itemId, out path, out bool isLeaf))
            {
                _targetIsLeaf = isLeaf;
                return;
            }

            string derivedName = ExtractBrowseName(itemId);
            if (!string.Equals(derivedName, itemId, StringComparison.Ordinal))
            {
                try
                {
                    OpcBrowseServerAddressSpace.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_TO, derivedName);
                    _currentPath = new[] { derivedName };
                    return;
                }
                catch (ArgumentException)
                {
                }
            }

            OpcBrowseServerAddressSpace.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_TO, itemId);
        }

        private static string ExtractBrowseName(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return itemId;
            }

            int bracketIndex = itemId.LastIndexOf(']');
            if (bracketIndex >= 0 && bracketIndex + 1 < itemId.Length)
            {
                return itemId.Substring(bracketIndex + 1);
            }

            return itemId;
        }

        private static bool TryParseBrowsePath(string itemId, out string[] path)
        {
            path = Array.Empty<string>();
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            if (itemId[0] != '[')
            {
                return false;
            }

            int bracketIndex = itemId.IndexOf(']');
            if (bracketIndex <= 1)
            {
                return false;
            }

            string root = itemId.Substring(1, bracketIndex - 1);
            string remainder = bracketIndex + 1 < itemId.Length
                ? itemId.Substring(bracketIndex + 1)
                : string.Empty;

            remainder = remainder.TrimStart('.', '\\', '/');

            var segments = new List<string>();
            if (!string.IsNullOrWhiteSpace(root))
            {
                segments.Add(root);
            }

            if (!string.IsNullOrWhiteSpace(remainder))
            {
                foreach (string segment in remainder.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    segments.Add(segment);
                }
            }

            if (segments.Count == 0)
            {
                return false;
            }

            path = segments.ToArray();
            return true;
        }

        private void TryMoveToRoot()
        {
            try
            {
                OpcBrowseServerAddressSpace.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_TO, string.Empty);
                return;
            }
            catch (ArgumentException)
            {
            }

            const int maxDepth = 1000;
            for (int i = 0; i < maxDepth; i++)
            {
                try
                {
                    OpcBrowseServerAddressSpace.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_UP);
                }
                catch (COMException ex)
                {
                    if (ex.ErrorCode == HRESULT.E_FAIL)
                    {
                        return;
                    }
                    throw;
                }
            }
        }

        private void MoveToPath(string[] path)
        {
            try
            {
                _targetIsLeaf = false;
                int commonLength = 0;
                int maxCommon = Math.Min(_currentPath.Length, path.Length);
                while (commonLength < maxCommon &&
                       string.Equals(_currentPath[commonLength], path[commonLength], StringComparison.OrdinalIgnoreCase))
                {
                    commonLength++;
                }

                for (int i = _currentPath.Length - 1; i >= commonLength; i--)
                {
                    OpcBrowseServerAddressSpace.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_UP);
                }

                for (int i = commonLength; i < path.Length; i++)
                {
                    OpcBrowseServerAddressSpace.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_DOWN, path[i]);
                }
            }
            catch (Exception)
            {
                TryMoveToRoot();
                foreach (string segment in path)
                {
                    OpcBrowseServerAddressSpace.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_DOWN, segment);
                }
            }
        }

        private void MoveToPathValidated(string originalPath, string[] path)
        {
            TryMoveToRoot();
            _currentPath = Array.Empty<string>();
            _targetIsLeaf = false;

            for (int i = 0; i < path.Length; i++)
            {
                string segment = path[i];
                bool isLast = i == path.Length - 1;
                if (!TryResolveSegment(segment, out bool isLeaf))
                {
                    throw new InvalidOperationException($"Browse path not found: '{originalPath}'.");
                }

                if (isLeaf)
                {
                    if (!isLast)
                    {
                        throw new InvalidOperationException($"Browse path not found: '{originalPath}'.");
                    }

                    _targetIsLeaf = true;
                    return;
                }

                OpcBrowseServerAddressSpace.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_DOWN, segment);
                _currentPath = _currentPath.Concat(new[] { segment }).ToArray();
            }
        }

        private bool TryFindPathByItemId(string itemId, out string[] path, out bool isLeaf)
        {
            path = Array.Empty<string>();
            isLeaf = false;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            const int maxDepth = 64;
            const int maxNodes = 20000;
            int visited = 0;

            TryMoveToRoot();
            _currentPath = Array.Empty<string>();

            if (TryFindPathByItemIdRecursive(itemId, 0, maxDepth, ref visited, maxNodes, out path, out isLeaf))
            {
                _currentPath = path;
                return true;
            }

            TryMoveToRoot();
            _currentPath = Array.Empty<string>();
            return false;
        }

        private bool TryFindPathByItemIdRecursive(string targetItemId, int depth, int maxDepth, ref int visited,
            int maxNodes, out string[] path, out bool isLeaf)
        {
            path = Array.Empty<string>();
            isLeaf = false;
            if (depth > maxDepth || visited >= maxNodes)
            {
                return false;
            }

            string[] branches = OpcBrowseServerAddressSpace.BrowseOpcItemIds(
                OpcDaBrowseType.Branch, string.Empty, VarEnum.VT_EMPTY, OpcDaAccessRights.Ignore);

            foreach (string branch in branches)
            {
                visited++;
                string branchItemId = OpcBrowseServerAddressSpace.TryGetItemId(branch);
                if (!string.IsNullOrEmpty(branchItemId) &&
                    string.Equals(branchItemId, targetItemId, StringComparison.OrdinalIgnoreCase))
                {
                    path = _currentPath.Concat(new[] { branch }).ToArray();
                    isLeaf = false;
                    return true;
                }

                OpcBrowseServerAddressSpace.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_DOWN, branch);
                _currentPath = _currentPath.Concat(new[] { branch }).ToArray();

                if (TryFindPathByItemIdRecursive(targetItemId, depth + 1, maxDepth, ref visited, maxNodes,
                        out path, out isLeaf))
                {
                    return true;
                }

                OpcBrowseServerAddressSpace.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_UP);
                _currentPath = _currentPath.Take(_currentPath.Length - 1).ToArray();
            }

            string[] leaves = OpcBrowseServerAddressSpace.BrowseOpcItemIds(
                OpcDaBrowseType.Leaf, string.Empty, VarEnum.VT_EMPTY, OpcDaAccessRights.Ignore);

            foreach (string leaf in leaves)
            {
                visited++;
                string leafItemId = OpcBrowseServerAddressSpace.TryGetItemId(leaf);
                if (!string.IsNullOrEmpty(leafItemId) &&
                    string.Equals(leafItemId, targetItemId, StringComparison.OrdinalIgnoreCase))
                {
                    path = _currentPath.Concat(new[] { leaf }).ToArray();
                    isLeaf = true;
                    return true;
                }
            }

            return false;
        }

        private bool TryResolveSegment(string segment, out bool isLeaf)
        {
            isLeaf = false;
            if (string.IsNullOrWhiteSpace(segment))
            {
                return false;
            }

            string[] branches = OpcBrowseServerAddressSpace.BrowseOpcItemIds(
                OpcDaBrowseType.Branch, segment, VarEnum.VT_EMPTY, OpcDaAccessRights.Ignore);

            if (branches.Any(b => string.Equals(b, segment, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            string[] leaves = OpcBrowseServerAddressSpace.BrowseOpcItemIds(
                OpcDaBrowseType.Leaf, segment, VarEnum.VT_EMPTY, OpcDaAccessRights.Ignore);

            if (leaves.Any(l => string.Equals(l, segment, StringComparison.OrdinalIgnoreCase)))
            {
                isLeaf = true;
                return true;
            }

            return false;
        }

        private OpcDaBrowseElement CreateLeafBrowseElement(string name)
        {
            string fullItemId = OpcBrowseServerAddressSpace.TryGetItemId(name);
            if (!string.IsNullOrEmpty(fullItemId))
            {
                _itemIdToPath[fullItemId] = _currentPath.Concat(new[] { name }).ToArray();
            }

            _nameToPath[name] = _currentPath.Concat(new[] { name }).ToArray();

            string itemId = fullItemId;

            return new OpcDaBrowseElement
            {
                Name = name,
                HasChildren = false,
                IsItem = true,
                ItemId = itemId
            };
        }

        private OpcDaBrowseElement CreateBranchBrowseElement(string name)
        {
            string itemId = OpcBrowseServerAddressSpace.TryGetItemId(name);
            if (!string.IsNullOrEmpty(itemId))
            {
                _itemIdToPath[itemId] = _currentPath.Concat(new[] { name }).ToArray();
            }

            _nameToPath[name] = _currentPath.Concat(new[] { name }).ToArray();

            return new OpcDaBrowseElement
            {
                Name = name,
                HasChildren = true,
                IsItem = false,
                ItemId = itemId
            };
        }
    }
}