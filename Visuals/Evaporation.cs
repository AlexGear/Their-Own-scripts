using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Evaporation : MonoBehaviour {
    [SerializeField] Renderer[] renderers = new Renderer[0];
    [SerializeField] Material material;
    [SerializeField, Range(0, 1)] float progress;

    private Dictionary<Renderer, Material> oldSharedMaterials = new Dictionary<Renderer, Material>();
    private float oldProgress;

    private static readonly int cutoffRefMapID = Shader.PropertyToID("_CutoffRefMap");
    private static readonly int alphaCutoffID = Shader.PropertyToID("_Cutoff");

    private void OnEnable() {
        oldSharedMaterials.Clear();

        foreach(var renderer in renderers) {
            oldSharedMaterials[renderer] = renderer.sharedMaterial;
            renderer.material = material;

            var mapOverride = GetComponent<EvaporationMapOverride>();
            if(mapOverride != null && mapOverride.map != null) {
                renderer.material.SetTexture(cutoffRefMapID, mapOverride.map);
            }
        }
    }

    private void LateUpdate() {
        foreach(var renderer in renderers) {
            renderer.material.SetFloat(alphaCutoffID, progress);
        }
    }

    private void OnDisable() {
        foreach(var pair in oldSharedMaterials) {
            pair.Key.sharedMaterial = pair.Value;
        }
    }
}
