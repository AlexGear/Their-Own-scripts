using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowProjector : MonoBehaviour {
    [SerializeField] AnimationCurve heightToAlpha = AnimationCurve.Linear(18, 90, 0, 170);
    [SerializeField] AnimationCurve heightToScale = AnimationCurve.Linear(30, 1.6f, 0, 1f);

    private Light2D.LightObstacleSprite obstacleSprite;

    private void Update() {
        if(obstacleSprite == null) {
            obstacleSprite = GetComponentInChildren<Light2D.LightObstacleSprite>();
            if(obstacleSprite == null) {
                Debug.LogError("No Light Obstacle Sprite found");
                DestroyImmediate(this);
                return;
            }
        }
        float height = -transform.position.z;
        obstacleSprite.Color.a = heightToAlpha.Evaluate(height) / 255f;

        float scale = heightToScale.Evaluate(height);
        obstacleSprite.transform.localScale = new Vector3(scale, scale, scale);
    }
}
