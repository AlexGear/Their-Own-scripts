using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraShake : MonoBehaviour {
    [SerializeField] private float power = 1;
    [SerializeField] private float shakeSpeed = 1;
    [SerializeField] private float duration = 1;
    [SerializeField] private float motionBlur = 1;
    [SerializeField, LabelOverride("Frequency (Omni Only)")] private float frequency = 50;

    private Transform mainCamera;
    private CameraMotionBlur motionBlurEffect;
    private Vector3 lastShakeDelta;

    private const float maxBlur = 24f;

    private abstract class Shake {
        public float shakeSpeed, duration, motionBlur;
        public float startTime;

        public bool Update(Transform camera, CameraMotionBlur motionBlurEffect) {
            float expiredTime = Time.time - startTime;
            float progress = Mathf.Clamp01(expiredTime / duration);
            if(expiredTime >= duration) {
                return false;
            }
            camera.position += GetDeviation(progress);
            float blur = motionBlur * (1f - progress) * maxBlur;
            motionBlurEffect.velocityScale = Mathf.Min(maxBlur, motionBlurEffect.velocityScale + blur);
            return true;
        }

        protected abstract Vector3 GetDeviation(float progress);
    }

    private class Omni : Shake {
        public float power;
        public float frequency;

        private Vector3 vector;
        private float vectorUpdateTimer = 0;

        private float period => 1 / frequency;

        protected override Vector3 GetDeviation(float progress) {
            UpdateVector();
            float t = (1f - progress) * Mathf.Clamp01(Time.deltaTime * shakeSpeed);
            return vector * t;
        }

        private void UpdateVector() {
            vectorUpdateTimer -= Time.deltaTime;
            if(vectorUpdateTimer <= 0) {
                vectorUpdateTimer = period;
                vector = Random.insideUnitCircle.normalized * power;
            }
        }
    }

    private class Directed : Shake {
        public Vector2 vector;

        protected override Vector3 GetDeviation(float progress) {
            float t = (1f - progress) * Mathf.Clamp01(Time.deltaTime * shakeSpeed);
            return vector * t;
        }
    }

    private List<Shake> shakes = new List<Shake>();

    void Start() {
        mainCamera = Camera.main.transform;
        motionBlurEffect = ImageEffectsManager.current.cameraMotionBlur;
    }

    public void ShakeCameraOmni(float power, float shakeSpeed, float duration, float motionBlur, float frequency) {
        shakes.Add(new Omni {
            power = power,
            shakeSpeed = shakeSpeed,
            duration = duration,
            startTime = Time.time,
            motionBlur = motionBlur,
            frequency = frequency
        });
    }

    public void ShakeCameraOmni() {
        ShakeCameraOmni(this.power, this.shakeSpeed, this.duration, this.motionBlur, this.frequency);
    }

    public void ShakeCameraOmni_ForAnim() {
        ShakeCameraOmni();
    }

    public void ShakeCameraKnockback(Vector3 direction, float power, float shakeSpeed, float duration, float motionBlur) {
        shakes.Add(new Directed {
            vector = direction.normalized * power,
            shakeSpeed = shakeSpeed,
            duration = duration,
            startTime = Time.time,
            motionBlur = motionBlur
        });
    }

    public void ShakeCameraKnockback(Vector3 direction) {
        ShakeCameraKnockback(direction, this.power, this.shakeSpeed, this.duration, this.motionBlur);
    }

    private void Update() {
        motionBlurEffect.velocityScale = 0;
        mainCamera.position -= lastShakeDelta;
    }

    private void LateUpdate() {
        Vector3 originalPos = mainCamera.position;

        var shakesToRemove = new List<Shake>();
        foreach(var shake in shakes) {
            if(!shake.Update(mainCamera, motionBlurEffect)) {
                shakesToRemove.Add(shake);
            }
        }
        foreach(var shakeToRemove in shakesToRemove) {
            shakes.Remove(shakeToRemove);
        }

        lastShakeDelta = mainCamera.position - originalPos;
    }
}
