# TitaniumAS.Opc.Client

> **Note:** This is a customized fork of the original [TitaniumAS.Opc.Client](https://github.com/titanium-as/TitaniumAS.Opc.Client) library, maintained as an internal component of the CollectorTunnel project. It is not distributed as a standalone package.

.NET client library for OPC DA (Data Access). Provides COM interop wrappers for connecting to and communicating with OPC DA servers.

## Features

- Support for local and network OPC DA servers.
- Support for OPC DA 1.0, 2.05A, and 3.0.
- Browsing of OPC DA server address spaces.
- Async/await support for read and write operations.
- Subscription to data changes via .NET events.
- Server shutdown event handling.

## Build

This library is compiled as part of the CollectorTunnel solution targeting **.NET Framework 4.8**. It is referenced directly as a project dependency — no package restore is required.

## Usage

### Connecting to an OPC DA Server

```csharp
// Make an URL of OPC DA server using builder.
Uri url = UrlBuilder.Build("Matrikon.OPC.Simulation.1");
using (var server = new OpcDaServer(url))
{
    // Connect to the server first.
    server.Connect();
    ...
}
```

### Browsing Elements

```csharp
// Create a browser and browse all elements recursively.
var browser = new OpcDaBrowserAuto(server);
BrowseChildren(browser);
...

void BrowseChildren(IOpcDaBrowser browser, string itemId = null, int indent = 0)
{
    // When itemId is null, root elements will be browsed.
    OpcDaBrowseElement[] elements = browser.GetElements(itemId);

    // Output elements.
    foreach (OpcDaBrowseElement element in elements)
    {
        // Output the element.
        Console.Write(new String(' ', indent));
        Console.WriteLine(element);

        // Skip elements without children.
        if (!element.HasChildren)
            continue;

        // Output children of the element.
        BrowseChildren(browser, element.ItemId, indent + 2);
    }
}
```

### Creating a Group with Items

```csharp
// Create a group with items.
OpcDaGroup group = server.AddGroup("MyGroup");
group.IsActive = true;

var definition1 = new OpcDaItemDefinition
{
    ItemId = "Random.Int2",
    IsActive = true
};
var definition2 = new OpcDaItemDefinition
{
    ItemId = "Bucket Brigade.Int4",
    IsActive = true
};
OpcDaItemDefinition[] definitions = { definition1, definition2 };
OpcDaItemResult[] results = group.AddItems(definitions);

// Handle adding results.
foreach (OpcDaItemResult result in results)
{
    if (result.Error.Failed)
        Console.WriteLine("Error adding items: {0}", result.Error);
}
...
```

### Reading Values

```csharp
// Read all items of the group synchronously.
OpcDaItemValue[] values = group.Read(group.Items, OpcDaDataSource.Device);
...

// Read all items of the group asynchronously.
OpcDaItemValue[] values = await group.ReadAsync(group.Items);
...
```

### Writing Values

```csharp
// Prepare items.
OpcDaItem int2 = group.Items.FirstOrDefault(i => i.ItemId == "Bucket Brigade.Int2");
OpcDaItem int4 = group.Items.FirstOrDefault(i => i.ItemId == "Bucket Brigade.Int4");
OpcDaItem[] items = { int2, int4 };

// Write values to the items synchronously.
object[] values = { 1, 2 };
HRESULT[] results = group.Write(items, values);
...

// Write values to the items asynchronously.
object[] values = { 3, 4 };
HRESULT[] results = await group.WriteAsync(items, values);
...
```

### Subscribing to Value Changes

```csharp
// Configure subscription.
group.ValuesChanged += OnGroupValuesChanged;
group.UpdateRate = TimeSpan.FromMilliseconds(100); // ValuesChanged won't be triggered if zero
...

static void OnGroupValuesChanged(object sender, OpcDaItemValuesChangedEventArgs args)
{
    // Output values.
    foreach (OpcDaItemValue value in args.Values)
    {
        Console.WriteLine("ItemId: {0}; Value: {1}; Quality: {2}; Timestamp: {3}",
            value.Item.ItemId, value.Value, value.Quality, value.Timestamp);
    }
}
```
