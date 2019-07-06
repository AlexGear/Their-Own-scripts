using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class ImageEffectsManager : MonoBehaviour {
    public static ImageEffectsManager current { get; private set; }

    public Bloom bloom { get; private set; }
    public VignetteAndChromaticAberration vignetteAndChromaticAberration { get; private set; }
    public CameraMotionBlur cameraMotionBlur { get; private set; }

    private void Awake() {
        current = this;
        bloom = GetComponent<Bloom>();
        vignetteAndChromaticAberration = GetComponent<VignetteAndChromaticAberration>();
        cameraMotionBlur = GetComponent<CameraMotionBlur>();
    }
}
