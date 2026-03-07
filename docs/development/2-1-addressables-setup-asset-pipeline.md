# Story 2.1: Addressables Setup & Asset Pipeline

Status: ready-for-dev

## Story

As a **developer**,
I want **Unity Addressables configured with Core persistent group and per-zone groups, plus IAssetLoader service implementation**,
so that **assets can be loaded/unloaded efficiently per zone without memory leaks**.

## Acceptance Criteria

1. **Addressables package installed and configured**
   - Unity Addressables package added via Package Manager
   - Default Addressables settings configured
   - Build path set for local builds

2. **Addressables groups created**
   - `Core` group — persistent assets (player, UI, core systems)
   - `Zone_Template` group — template for per-zone groups
   - Labels defined: `core`, `zone_village_001`, `zone_forest_001` (examples)

3. **IAssetLoader service implemented**
   - `AssetLoader` class with `[Service(typeof(IAssetLoader))]`
   - `LoadAssetAsync<T>()` — load single asset by address
   - `InstantiateAsync()` — instantiate prefab
   - `ReleaseAsset()` — release single asset handle
   - `ReleaseAllZoneAssets()` — release all assets for a zone
   - `PreloadZoneAssetsAsync()` — preload zone assets by label

4. **No memory leaks**
   - Handles tracked per zone
   - Release called on zone unload
   - Proper cleanup in service disposal

## USER Manual Steps (Unity Editor)

**⚠️ Ces étapes DOIVENT être faites manuellement par l'utilisateur dans Unity :**

### Step 1: Install Addressables Package
1. Ouvrir `Window` → `Package Manager`
2. Cliquer `+` → `Add package by name...`
3. Entrer `com.unity.addressables`
4. Cliquer `Add`

### Step 2: Initialize Addressables
1. Ouvrir `Window` → `Asset Management` → `Addressables` → `Groups`
2. Si demandé, cliquer `Create Addressables Settings`
3. Vérifier que le dossier `Assets/AddressableAssetsData/` est créé

### Step 3: Create Addressables Groups
1. Dans la fenêtre Addressables Groups, clic droit → `Create New Group` → `Packed Assets`
2. Renommer en `Core`
3. Répéter pour créer `Zone_Village_001`, `Zone_Forest_001` (exemples)

### Step 4: Configure Labels
1. Dans Addressables Groups, ouvrir le menu `Tools` → `Labels`
2. Ajouter les labels: `core`, `zone_village_001`, `zone_forest_001`

### Step 5: Verify Compilation (After LLM Implementation)
1. Retourner dans Unity après l'implémentation LLM
2. Attendre la recompilation automatique
3. Vérifier qu'il n'y a **aucune erreur** dans la Console

---

## LLM Automated Tasks

**✅ Ces tâches seront automatiquement effectuées par le LLM dev-story :**

- [ ] **Task 1: Create AssetLoader service**
  - [ ] Create `AssetLoader.cs` in `Scripts/World/Zones/`
  - [ ] Implement `IAssetLoader` interface
  - [ ] Apply `[Service(typeof(IAssetLoader))]` attribute
  - [ ] Track handles per zone for cleanup

- [ ] **Task 2: Implement LoadAssetAsync<T>**
  - [ ] Use `Addressables.LoadAssetAsync<T>()`
  - [ ] Return via UniTask
  - [ ] Track handle for release

- [ ] **Task 3: Implement InstantiateAsync**
  - [ ] Use `Addressables.InstantiateAsync()`
  - [ ] Support optional parent transform
  - [ ] Track handle for release

- [ ] **Task 4: Implement Release methods**
  - [ ] `ReleaseAsset()` — release single handle
  - [ ] `ReleaseAllZoneAssets()` — release all handles for a zone label

- [ ] **Task 5: Implement PreloadZoneAssetsAsync**
  - [ ] Load all assets with zone label
  - [ ] Track handles for later release

## Dev Notes

### Architecture Compliance

**Asset Pipeline Rules (from Architecture):**
- NO `Resources/` folder — all runtime assets via Addressables
- Zone assets in per-zone Addressables groups — load on enter, release on exit
- Shared assets (Player, UI, core) in persistent `Core` group
- Async loading via UniTask integration
- Labels for group loading: `zone_village_001`, `zone_forest_001`

### Dependencies

- **UniTask** — for async/await pattern
- **Unity Addressables** — `com.unity.addressables`
- **Story 1.3** — `IAssetLoader` interface already exists

### File Structure

```
Assets/Scripts/World/Zones/
└── AssetLoader.cs
```

### Code Template

```csharp
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sisus.Init;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ValenthiaChronicles.Core;

[Service(typeof(IAssetLoader))]
public class AssetLoader : IAssetLoader
{
    private readonly Dictionary<string, AsyncOperationHandle> _handles = new();
    private readonly Dictionary<string, List<AsyncOperationHandle>> _zoneHandles = new();

    public async UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
    {
        var handle = Addressables.LoadAssetAsync<T>(address);
        await handle.ToUniTask();
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _handles[address] = handle;
            return handle.Result;
        }
        
        GameLogger.Error(LogTag.Zone, $"Failed to load asset: {address}");
        return null;
    }

    public async UniTask<GameObject> InstantiateAsync(string address, Transform parent = null)
    {
        var handle = Addressables.InstantiateAsync(address, parent);
        await handle.ToUniTask();
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _handles[address] = handle;
            return handle.Result;
        }
        
        GameLogger.Error(LogTag.Zone, $"Failed to instantiate: {address}");
        return null;
    }

    public void ReleaseAsset(string address)
    {
        if (_handles.TryGetValue(address, out var handle))
        {
            Addressables.Release(handle);
            _handles.Remove(address);
        }
    }

    public void ReleaseAllZoneAssets(string zoneId)
    {
        if (_zoneHandles.TryGetValue(zoneId, out var handles))
        {
            foreach (var handle in handles)
            {
                Addressables.Release(handle);
            }
            _zoneHandles.Remove(zoneId);
            GameLogger.Info(LogTag.Zone, $"Released {handles.Count} assets for zone: {zoneId}");
        }
    }

    public async UniTask PreloadZoneAssetsAsync(string zoneId)
    {
        var handle = Addressables.LoadAssetsAsync<UnityEngine.Object>(zoneId, null);
        await handle.ToUniTask();
        
        if (!_zoneHandles.ContainsKey(zoneId))
            _zoneHandles[zoneId] = new List<AsyncOperationHandle>();
        
        _zoneHandles[zoneId].Add(handle);
        GameLogger.Info(LogTag.Zone, $"Preloaded assets for zone: {zoneId}");
    }
}
```

### References

- [Source: _bmad-output/game-architecture.md#Asset-Pipeline-Unity-Addressables]
- [Source: _bmad-output/plans/epics.md#Story-2.1]
- [Source: docs/init-args-guide.md#Services-globaux]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Completion Notes List

### File List

