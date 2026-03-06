# Core Library Changes

**Date:** 2026-03-05

---

## Files Modified

- `Da/OpcDaGroup.cs`
- `Da/OpcDaServer.cs`
- `Da/Internal/Requests/ReadAsyncRequest.cs`
- `Da/Internal/Requests/ReadMaxAgeAsyncRequest.cs`
- `Da/Internal/Requests/RefreshAsyncRequest.cs`
- `Da/Internal/Requests/RefreshMaxAgeAsyncRequest.cs`
- `Da/Internal/Requests/WriteAsyncRequest.cs`
- `Da/Internal/Requests/WriteVQTAsyncRequest.cs`

---

## Change 1 — `Is<T>(out T value)` overload in `OpcDaGroup`

Added `Is<T>(out T)` overload so callers can check + use the COM wrapper in one call, avoiding a second `As<T>()` (which calls `Activator.CreateInstance` again).

**Added after `Is<T>()` (~line 928):**

```csharp
public bool Is<T>(out T value) where T : ComWrapper
{
    value = As<T>();
    return value != null;
}
```

**Revert:** Delete this method from `OpcDaGroup.cs`.

---

## Change 2 — `KeepAlive` setter in `OpcDaGroup`

Was calling `Is<>()` then `As<>()` separately (two `Activator.CreateInstance` calls). Now uses the `out` overload.

**Original:**

```csharp
set
{
    if (!Is<OpcGroupStateMgt2>())
        return;
    _keepAlive = As<OpcGroupStateMgt2>().SetKeepAlive(value);
}
```

**New:**

```csharp
set
{
    if (!Is<OpcGroupStateMgt2>(out var v))
        return;
    _keepAlive = v.SetKeepAlive(value);
}
```

---

## Change 3 — `RefreshKeepAlive()` in `OpcDaGroup`

Same `Is<>()` + `As<>()` double-call pattern.

**Original:**

```csharp
private TimeSpan RefreshKeepAlive()
{
    if (!Is<OpcGroupStateMgt2>())
        return TimeSpan.Zero;
    return As<OpcGroupStateMgt2>().GetKeepAlive();
}
```

**New:**

```csharp
private TimeSpan RefreshKeepAlive()
{
    if (!Is<OpcGroupStateMgt2>(out var v))
        return TimeSpan.Zero;
    return v.GetKeepAlive();
}
```

---

## Change 4 — `Is<T>(out T value)` overload in `OpcDaServer`

Same overload as Change 1, added to `OpcDaServer` for consistency. Not used yet.

**Added after `Is<T>()` (~line 571):**

```csharp
public bool Is<T>(out T value) where T : ComWrapper
{
    value = As<T>();
    return value != null;
}
```

**Revert:** Delete this method from `OpcDaServer.cs`.

---

## Change 5 — Async request thread leak fix ([#75](https://github.com/titanium-as/TitaniumAS.Opc.Client/issues/75))

`TaskCompletionSource` in all async request classes was missing `TaskCreationOptions.RunContinuationsAsynchronously`. Without it, `TrySetResult()` runs the awaiting continuation synchronously on the OPC COM callback thread, which in a tight loop causes thread starvation in the OPC server.

**Applies to all 6 request files listed above.**

**Original:**

```csharp
private readonly TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>();
```

**New:**

```csharp
private readonly TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
```

**Revert:** Remove `TaskCreationOptions.RunContinuationsAsynchronously` from each constructor.

---

## Full Revert

1. **OpcDaGroup.cs:** Remove `Is<T>(out T)` method, restore `KeepAlive` setter and `RefreshKeepAlive()` to originals above.
2. **OpcDaServer.cs:** Remove `Is<T>(out T)` method.
3. **6 async request files:** Remove `TaskCreationOptions.RunContinuationsAsynchronously` from each `TaskCompletionSource` constructor.
