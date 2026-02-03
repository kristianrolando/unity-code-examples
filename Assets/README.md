# Unity Core Scripts (C#)

A collection of modular, production-oriented Unity core scripts designed to be reused across projects. This repository focuses on clean architecture, low coupling, and practical patterns commonly used in real-world Unity codebases.

> **Note:** This is a **pure script collection**. There is no functional demo scene or pre-wired sample setup included—these scripts are intended to be reviewed as code and integrated into an existing project as needed.

---

## What’s Included

Scripts live under:

```
Assets/_Game/Scripts
```

High-level modules:

* **Core**

  * **Object Pooling** (pooling primitives + manager pattern)
  * **Singleton** (multiple lifecycle variants + validation)
  * **EventBus** (decoupled event-driven communication)
  * **ServiceLocator** (central service registry & resolution)
* **SaveSystem**

  * Interfaces + handlers + storage strategies
  * JSON file storage + encryption wrapper + hybrid storage
  * Unity type converters for consistent serialization
* **Utilities**

  * Coroutine helpers (for non-MonoBehaviour contexts)
  * Debug helpers & visualization utilities
  * Scene loading helper
  * Screenshot and transparency utilities

---

## Folder Structure

```
Assets/_Game
  ├─ Scenes
  │   └─ SampleScene
  └─ Scripts
      ├─ Core
      │   ├─ Object Pooling
      │   │   ├─ ObjectPool
      │   │   ├─ ObjectPoolManager
      │   │   └─ PoolableKey
      │   ├─ Singleton
      │   │   ├─ BaseSingleton
      │   │   ├─ SingletonGlobal
      │   │   ├─ SingletonScene
      │   │   ├─ SingletonTransient
      │   │   └─ SingletonValidator
      │   ├─ EventBus
      │   └─ ServiceLocator
      ├─ SaveSystem
      │   ├─ BaseSaveHandler
      │   ├─ EncryptedStorage
      │   ├─ HybridStorage
      │   ├─ ISaveable
      │   ├─ ISaveStorage
      │   ├─ JsonFileStorage
      │   ├─ SaveContainer
      │   ├─ SaveManager
      │   ├─ SaveSystem
      │   └─ UnityConverters
      └─ Utilities
          ├─ CoroutineHelper
          ├─ CoroutineRunner
          ├─ DebugShapeVisualizer
          ├─ DebugX
          ├─ SceneLoader
          ├─ ScreenshotTool
          └─ TransparencyUtility
```

> The `SampleScene` folder may exist in the project structure, but **it is not meant to be a runnable demo**. The scripts are intentionally not wired to any scene objects.

---

## Design Goals

* **Modular & scalable**: each system can be adopted independently.
* **Low coupling**: communication through interfaces/events instead of hard references.
* **Maintainable architecture**: clear responsibilities, predictable lifecycles.
* **Practical patterns**: implementation style mirrors typical production Unity projects.

---

## Module Overview

### Object Pooling

Provides an object pooling foundation for runtime-heavy objects (VFX, projectiles, UI popups, enemies, etc.).

Typical responsibilities include:

* key-based prefab identification
* instance reuse / return-to-pool workflow
* centralized pool management via a manager-style entry point

### Singleton (Lifecycle Variants)

Multiple singleton flavors to match real needs rather than forcing a one-size-fits-all singleton approach:

* global (persistent across scenes)
* scene-scoped
* transient/re-creatable (useful for sandbox/testing tooling)
* validation utilities to detect duplicates or incorrect wiring early

### EventBus

A lightweight event-driven communication layer to reduce coupling:

* publishers do not need to know subscribers
* keeps gameplay/UI/system signals clean and refactor-friendly

### ServiceLocator

A central registry to resolve cross-cutting services while avoiding scattered lookups.

Useful for:

* game-wide services (save, audio routing, configuration, analytics adapters)
* reducing manual reference wiring across many prefabs/scenes

### Save System

A flexible save/load foundation built around interfaces and storage strategies:

* `ISaveable` defines saveable state boundaries
* a manager orchestrates the save/load flow
* storage implementations can be swapped (JSON, encrypted wrapper, hybrid composition)
* converters help serialize common Unity types consistently

### Utilities

A set of small helpers used in day-to-day development:

* coroutine helpers for non-MonoBehaviour contexts
* debug helpers and visualization utilities
* scene loading helper
* screenshot and transparency utilities

---

## Dependency: Newtonsoft JSON (Required)

To ensure serialization features work correctly, install the following Unity package:

**Package name**

```
com.unity.nuget.newtonsoft-json
```

**How to install (Unity Package Manager)**

1. Open **Window → Package Manager**

2. Click **+** → **Add package by name...**

3. Paste:

   ```
   com.unity.nuget.newtonsoft-json
   ```

4. Install

---

## Integration Notes

* These scripts are intended to be integrated into your own project’s composition/wiring layer.
* If you adopt the Save System, ensure your saveable objects follow the provided interfaces and that you select the appropriate storage strategy for your g
