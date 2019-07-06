using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[AddComponentMenu("Abilities/SlowMotionAbility")]
public class SlowMotionAbility : ContinuousAbility {
    [SerializeField] [Range(0, 1)] private float factor = 1;
    [SerializeField] private AnimationCurve activationAnim = AnimationCurve.Constant(0, 1, 1);
    [SerializeField] private AnimationCurve deactivationAnim = AnimationCurve.Constant(0, 1, 1);
    private VignetteAndChromaticAberration imageEffect;

    private float startTimeScale;
    private float startFixedTimestep;
    private float startMoveSpeed;
    
    private float vignetting;
    private float blurredCorners;
    private float chromaticAberration;

    private MainCharacter mainCharacter;
    private float animTime, animDuration = 0;

    protected override void Start() {
        base.Start();
        imageEffect = ImageEffectsManager.current.vignetteAndChromaticAberration;
        mainCharacter = unit as MainCharacter;
        vignetting = imageEffect.intensity;
        blurredCorners = imageEffect.blur;
        chromaticAberration = imageEffect.chromaticAberration;
    }

    protected override void Update() {
        base.Update();
        if (isActive) {
            UpdateScale(Time.deltaTime, activationAnim);
        }
        else if(animDuration != 0) {
            UpdateScale(Time.deltaTime, deactivationAnim);
        }
    }

    protected override void OnActivated() {
        animTime = 0;
        animDuration = activationAnim.keys.Last().time;
        if (mainCharacter != null) {
            startTimeScale = Time.timeScale;
            startFixedTimestep = Time.fixedDeltaTime;
            startMoveSpeed = mainCharacter.movespeed;
        }
        if (imageEffect != null) {
            imageEffect.intensity = 0;
            imageEffect.blur = 0;
            imageEffect.chromaticAberration = 0;
            imageEffect.enabled = true;
        }
        UpdateScale(0, activationAnim);
    }

    protected override void OnDeactivated() {
        animTime = 0;
        animDuration = deactivationAnim.keys.Last().time;
    }

    public void UpdateScale(float deltaTime, AnimationCurve curve) {
        animTime += deltaTime;
        if (animTime > animDuration) {
            animTime = animDuration;
        }
        float t = curve.Evaluate(animTime);

        if (mainCharacter != null) {
            Time.timeScale = Mathf.Lerp(startTimeScale, startTimeScale * factor, t);
            Time.fixedDeltaTime = Mathf.Lerp(startFixedTimestep, startFixedTimestep * factor, t);
            mainCharacter.movespeed = Mathf.Lerp(startMoveSpeed, startMoveSpeed * factor * 8, t);
        }
        if (imageEffect != null) {
            imageEffect.intensity = Mathf.Lerp(0, vignetting, t);
            imageEffect.blur = Mathf.Lerp(0, blurredCorners, t);
            imageEffect.chromaticAberration = Mathf.Lerp(0, chromaticAberration, t);
        }
    }

    protected override void UpdateCharge() {
        if (isActive) {
            charge -= Time.unscaledDeltaTime / maxDuration;
        }
        else {
            charge += Time.unscaledDeltaTime / rechargeDuration;
        }
    }
}
