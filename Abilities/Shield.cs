using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Shield : MonoBehaviour {
    public Transform ownerTransform;
    [SerializeField] float projectorSizeFactor = 1.116f;
    [SerializeField] AnimationCurve activationRadiusAnim = AnimationCurve.Constant(0, 1, 1);
    private Projector projector;
    private float animTime, animDuration;

    private static List<Shield> shields = new List<Shield>();

    public float radius = 1f;

    private void Awake() {
        projector = GetComponentInChildren<Projector>();
    }

    private void OnEnable() {
        UpdatePosition();
        if (!shields.Contains(this)) {
            shields.Add(this);
        }
        animTime = 0;
        animDuration = activationRadiusAnim.keys.Last().time;
        UpdateSize(0f);
    }

    private void OnDisable() {
        if(shields.Contains(this)) {
            shields.Remove(this);
        }
    }

    private void Update() {
        UpdatePosition();
        UpdateSize(Time.deltaTime);
        if(projector != null) {
            UpdateProjectorSize();
        }
    }

    private void UpdatePosition() {
        if (ownerTransform != null) {
            transform.position = ownerTransform.position;
        }
    }

    private void UpdateSize(float deltaTime) {
        animTime += deltaTime;
        float scale;
        if(animTime >= animDuration || !Application.isPlaying)
            scale = radius;
        else
            scale = activationRadiusAnim.Evaluate(animTime) * radius;

        transform.localScale = new Vector3(scale, scale, scale);
    }

    private void UpdateProjectorSize() {
        float acceptableSize = projectorSizeFactor * radius;
        if (!Mathf.Approximately(projector.orthographicSize, acceptableSize)) {
            projector.orthographicSize = acceptableSize;
        }
    }

    public static bool IsPointInside(Vector2 position) {
        return CompositeShield.current.OverlapPoint(position);
    }

    public static List<Shield> OverlapShields(Vector2 position) {
        List<Shield> shields = new List<Shield>();
        foreach (Shield shield in Shield.shields) {
            PolygonCollider2D collider = shield.GetComponent<PolygonCollider2D>();
            collider.usedByComposite = false;
            if (collider.OverlapPoint(position)) {
                shields.Add(shield);
            }
            collider.usedByComposite = true;
        }
        return shields;
    }

    public static Shield OverlapShield(Vector2 position) {
        Shield result = null;
        foreach (Shield shield in shields) {
            PolygonCollider2D collider = shield.GetComponent<PolygonCollider2D>();
            collider.usedByComposite = false;
            if (collider.OverlapPoint(position)) {
                result = shield;
            }
            collider.usedByComposite = true;
        }
        return result;
    }

    static public void RepulseBullet(GameObject bullet, Vector2 point, Vector2 normal, int ricochetCount) {
        if(ricochetCount > Ammo.maxRicochetCount) {
            return;
        }
        var pool = GameObjectPool.Get(bullet);
        float deviation = Random.Range(-15, 15);
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, normal) * Quaternion.AngleAxis(deviation, Vector3.forward);
        
        var repulsedBulletObject = pool?.Take(point, rotation);
        var repulsedAmmo = repulsedBulletObject.GetComponent<Ammo>();
        repulsedAmmo.ricochetCount = ricochetCount + 1;
        //repulsedAmmo.Update();
    }
}
