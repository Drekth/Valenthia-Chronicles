using System.Collections.Generic;
using UnityEngine;

// Which system is currently hiding an occluder. Used as a bitmask so the two occlusion layers
// compose instead of fighting: the occluder stays hidden while ANY source requests it, and only
// becomes visible again once EVERY source has released it.
[System.Flags]
public enum FadeSource
{
    None = 0,
    Camera = 1,   // micro layer: camera-to-player sweep (CameraOcclusionFader)
    Interior = 2  // macro layer: player inside a building (BuildingZone)
}

// Per-object fade driver for anything that can hide the player (roofs, walls, tall props).
// Smoothly drives the shared dither shader's _FadeAmount from 0 (opaque) to 1 (fully dithered out)
// through a per-renderer MATERIAL INSTANCE — not a MaterialPropertyBlock, whose overrides the SRP
// Batcher drops for properties declared outside UnityPerMaterial. Material instances are created
// lazily on the first fade, so occluders that never trigger cost nothing. Once fully hidden the
// renderers are switched off to skip drawing entirely. Both occlusion layers drive this through
// SetFaded(source, hidden) and their requests are OR-ed together.
[DisallowMultipleComponent]
public class OccluderFadeController : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private static readonly int FadeAmountId = Shader.PropertyToID("_FadeAmount");

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Request (Hidden = true) or release (Hidden = false) the fade for one source. Cheap and
    // idempotent: callers may invoke it every frame. The occluder fades while any source holds it.
    public void SetFaded(FadeSource Source, bool Hidden)
    {
        FadeSource NewSources = Hidden ? (ActiveSources | Source) : (ActiveSources & ~Source);
        if (NewSources == ActiveSources)
        {
            return;
        }

        ActiveSources = NewSources;
        TargetFade = ActiveSources != FadeSource.None ? 1.0f : 0.0f;
        enabled = true;
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        CacheRenderers();
    }

    private void Update()
    {
        if (Mathf.Approximately(CurrentFade, TargetFade))
        {
            CurrentFade = TargetFade;
            ApplyFade(CurrentFade);
            // Settled — stop ticking until the next SetFaded.
            enabled = false;
            return;
        }

        float Speed = FadeDuration > 0.0f ? (1.0f / FadeDuration) : MaxInstantSpeed;
        CurrentFade = Mathf.MoveTowards(CurrentFade, TargetFade, Speed * Time.deltaTime);
        ApplyFade(CurrentFade);
    }

    private void CacheRenderers()
    {
        if (Renderers == null || Renderers.Length == 0)
        {
            Renderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    // Instantiates the per-renderer materials the first time we actually need to write _FadeAmount.
    private void EnsureFadeMaterials()
    {
        if (FadeMaterials != null)
        {
            return;
        }

        List<Material> Found = new List<Material>();
        if (Renderers != null)
        {
            foreach (Renderer Target in Renderers)
            {
                if (Target == null)
                {
                    continue;
                }

                // Reading .materials instantiates the shared materials; the renderer now draws these.
                foreach (Material Instance in Target.materials)
                {
                    if (Instance != null && Instance.HasProperty(FadeAmountId))
                    {
                        Found.Add(Instance);
                    }
                }
            }
        }

        FadeMaterials = Found.ToArray();
    }

    private void ApplyFade(float Value)
    {
        bool FullyHidden = Value >= 1.0f;

        // Skip drawing once fully dithered out; re-enable while visible or fading.
        if (Renderers != null)
        {
            foreach (Renderer Target in Renderers)
            {
                if (Target != null)
                {
                    Target.enabled = !FullyHidden;
                }
            }
        }

        if (FullyHidden)
        {
            return;
        }

        // Fully opaque and never faded yet — the shared material already reads 0, nothing to do.
        if (Value <= 0.0f && FadeMaterials == null)
        {
            return;
        }

        EnsureFadeMaterials();
        foreach (Material Instance in FadeMaterials)
        {
            Instance.SetFloat(FadeAmountId, Value);
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    // Fallback fade rate when FadeDuration is 0 (treat as effectively instant).
    private const float MaxInstantSpeed = 1000.0f;

    [Header("Fade")]
    // Seconds for a full opaque <-> hidden transition.
    [SerializeField] private float FadeDuration = 0.3f;

    [Header("Renderers (auto-filled from children if empty)")]
    [SerializeField] private Renderer[] Renderers;

    private Material[] FadeMaterials;
    private FadeSource ActiveSources;
    private float CurrentFade;
    private float TargetFade;
}
