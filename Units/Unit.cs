using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
    [SerializeField]
    [LabelOverride("Maximum Health")]
    private float _maxHealth = 100f;

    [SerializeField]
    [LabelOverride("Health")]
    private float _health = 100;

    protected enum ActionOnDeath { MakeInactive, Destroy }
    [SerializeField] protected ActionOnDeath actionOnDeath;

    [SerializeField] float activeDistance = 70f;

    [SerializeField] CurveAsset updateInterval;

    public new Collider2D collider { get; private set; }

    private AmmoBag _ammoBag = null;
    public AmmoBag ammoBag => _ammoBag != null ? _ammoBag : (_ammoBag = GetComponentInChildren<AmmoBag>());

    public bool isDead { get; protected set; } = false;

    [NonSerialized] public bool usedInPool = false;

    public event Action<Unit> Died;
    public event Action<Unit, float> HealthChanged;
    public event Action<Unit, float> DamageApplied;

    public AI.UnitAI ai { get; protected set; } = null;

    public Vector2 position {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public Quaternion rotation {
        get { return transform.rotation; }
        set { transform.rotation = value; }
    }
    public Vector2 forward => transform.up;

    public float maxHealth {
        get { return _maxHealth; }
        set {
            _maxHealth = value;
            if(health > _maxHealth) {
                health = _maxHealth;
            }
        }
    }

    public float health {
        get { return _health; }
        set {
            if(isDead) {
                return;
            }
            value = CalculateNewHealth(value);
            if(_health == value) {
                return;
            }

            float delta = value - _health;
            _health = value;
            OnHealthChanged(delta);
            HealthChanged?.Invoke(this, delta);

            if(_health <= 0f) {
                isDead = true;
                OnDied();
                Died?.Invoke(this);
            }
        }
    }

    public const int alliesLayer = 8;
    public const int enemiesLayer = 13;

    protected static ICollection<Unit> allUnits = new HashSet<Unit>();

    private bool activeSelf = false;
    private bool skipActiveSelfSet = false;

    private Timer updateTimer;
    
    protected virtual void Awake() {
        collider = GetComponentInChildren<Collider2D>();
        ai = CreateAI();

        allUnits.Add(this);
    }

    protected virtual void Start() {
        if(updateInterval != null) {
            updateTimer = new Timer(GetUpdateInterval());
        }
    }

    protected virtual AI.UnitAI CreateAI() => null;

    protected virtual void OnDestroy() {
        allUnits.Remove(this);
    }

    private void OnEnable() {
        if(skipActiveSelfSet) {
            skipActiveSelfSet = false;
            return;
        }
        activeSelf = true;
    }

    private void OnDisable() {
        if(skipActiveSelfSet) {
            skipActiveSelfSet = false;
            return;
        }
        activeSelf = false;
    }

    public void UpdateDistanceActiveness(Vector2 mainCharacterPosition) {
        if(!activeSelf)
            return;

        Vector2 vector = this.position - mainCharacterPosition;
        bool setActive = vector.CompareLength(activeDistance) < 0;
        if(gameObject.activeSelf != setActive) {
            if(setActive == false && (ai != null && ai.IsInCombat(true)))
                return;

            skipActiveSelfSet = true;
            gameObject.SetActive(setActive);
            skipActiveSelfSet = false;
        }
    }

    private void Update() {
        if(!isDead) {
            if(updateInterval == null || updateTimer == null) {
                OnUpdate();
            }
            else if(updateTimer.Tick()) {
                updateTimer.interval = GetUpdateInterval();
                OnUpdate();
            }
        }
    }

    protected virtual void OnUpdate() {
        ai?.Think();
    }

    private float GetUpdateInterval() {
        const float thresholdDistance = 35;
        if(updateInterval == null) {
            return 0;
        }
        float sqrDistance = (position - MainCharacter.current.position).sqrMagnitude;
        if(sqrDistance < thresholdDistance * thresholdDistance) {
            return 0;
        }
        float distance = Mathf.Sqrt(sqrDistance);
        return updateInterval.Evaluate(distance);
    }

    public virtual void ApplyDamage(float value, Unit source) {
        health -= value;
        if(health > 0) {
            ai?.OnDamageTaken(value, source);
            DamageApplied?.Invoke(this, value);
        }
    }

    protected virtual float CalculateNewHealth(float rawNewHealth) {
        return Mathf.Min(rawNewHealth, _maxHealth);
    }

    protected virtual void OnHealthChanged(float delta) {
    }

    public void Revive() {
        if(!isDead) {
            Debug.LogWarning($"Trying to revive an alive unit {name}");
            return;
        }
        OnRevive();
    }

    protected virtual void OnRevive() {
        isDead = false;
        health = maxHealth;
        gameObject.SetActive(true);
        Awake();
        Start();
    }

    protected virtual void OnDied() {
        if(actionOnDeath == ActionOnDeath.MakeInactive) {
            this.gameObject.SetActive(false);
        }
        else {
            Destroy(this.gameObject);
        }
        allUnits.Remove(this);
    }

    private static Collider2D[] overlapResults = new Collider2D[80];

    public static ISet<Unit> GetInRadius(Vector2 point, float radius, int layerMask = Physics.DefaultRaycastLayers,
            int obstaclesLayerMask = Physics.IgnoreRaycastLayer)
    {
        return GetInRadius<Unit>(point, radius, layerMask, obstaclesLayerMask);
    }

    public static ISet<TUnit> GetInRadius<TUnit>(Vector2 point, float radius, int layerMask = Physics.DefaultRaycastLayers,
            int obstaclesLayerMask = Physics.IgnoreRaycastLayer) where TUnit : Unit
    {
        int n = Physics2D.OverlapCircleNonAlloc(point, radius, overlapResults, layerMask);
        var targetUnits = new HashSet<TUnit>();
        for(int i = 0; i < n; i++) {
            var unit = overlapResults[i].GetComponentInParent<TUnit>();
            if(unit != null && unit.IsVisibleFromPoint(point, obstaclesLayerMask)) {
                targetUnits.Add(unit);
            }
        }
        return targetUnits;
    }

    public static ISet<Unit> GetWithinArea(Collider2D area, int layerMask = Physics.DefaultRaycastLayers)
    {
        return GetWithinArea<Unit>(area, layerMask);
    }

    public static ISet<TUnit> GetWithinArea<TUnit>(Collider2D area, int layerMask = Physics.DefaultRaycastLayers) 
        where TUnit : Unit
    {
        var contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = layerMask,
            useTriggers = true
        };
        int n = area.OverlapCollider(contactFilter, overlapResults);
        var units = new HashSet<TUnit>();
        for (int i = 0; i < n; i++)
        {
            var unit = overlapResults[i].GetComponentInParent<TUnit>();
            if (unit != null)
            {
                units.Add(unit);
            }
        }
        return units;
    }

    private static Vector2[] castPoints = new Vector2[4];
    private static Vector2[] castVectors = new Vector2[4];
    private static float[] castVectorsSqrLengths = new float[4];

    public bool IsVisibleFromPoint(Vector2 point, int obstaclesLayerMask = Physics.IgnoreRaycastLayer, bool straightLine = false) {
        if(obstaclesLayerMask == Physics.IgnoreRaycastLayer || obstaclesLayerMask == 0) {
            return true;
        }
        if(straightLine)
            return Physics2D.Linecast(point, collider.transform.position, obstaclesLayerMask).collider == null;

        //Vector2 pos = collider.transform.position;
        Bounds bounds = collider.bounds;
        bounds.Expand(-0.2f);
        if(bounds.Contains(point))
            return true;

        castPoints[0] = new Vector2(bounds.min.x, bounds.min.y);
        castPoints[1] = new Vector2(bounds.min.x, bounds.max.y);
        castPoints[2] = new Vector2(bounds.max.x, bounds.min.y);
        castPoints[3] = new Vector2(bounds.max.x, bounds.max.y);

        float maxSqrLength = 0, minSqrLength = float.PositiveInfinity;
        int maxLengthIndex = 0, minLengthIndex = 0;
        for(int i = 0; i < 4; i++) {
            castVectors[i] = castPoints[i] - point;
            float sqrLength = castVectors[i].sqrMagnitude;
            castVectorsSqrLengths[i] = sqrLength;

            if(sqrLength > maxSqrLength) {
                maxSqrLength = sqrLength; maxLengthIndex = i;
            }
            if(sqrLength < minSqrLength) {
                minSqrLength = sqrLength; minLengthIndex = i;
            }
        }

        float secondNearestSqrLength = float.PositiveInfinity;
        int secondNearestIndex = 0;
        for(int i = 0; i < 4; i++) {
            if(i != maxLengthIndex && i != minLengthIndex) {
                float sqrLength = castVectorsSqrLengths[i];
                if(sqrLength < secondNearestSqrLength) {
                    secondNearestSqrLength = sqrLength; secondNearestIndex = i;
                }
            }
        }
        int castPointIndex1 = secondNearestIndex;

        int thirdNearestIndex = 6 - maxLengthIndex - minLengthIndex - secondNearestIndex;   // 6 is sum of all possible indices: 0+1+2+3
#if UNITY_EDITOR
        if(thirdNearestIndex < 0 || thirdNearestIndex > 3) {
            Debug.DrawLine(point, transform.position, Color.red, 1000);
            Debug.DrawLine(castPoints[0], castPoints[1], Color.white, 1000);
            Debug.DrawLine(castPoints[1], castPoints[2], Color.white, 1000);
            Debug.DrawLine(castPoints[2], castPoints[3], Color.white, 1000);
            Debug.DrawLine(castPoints[3], castPoints[0], Color.white, 1000);
            Debug.Log($"Oops, thirdNearestIndex is out of range (" +
                $"{nameof(thirdNearestIndex)}={thirdNearestIndex}, {nameof(maxLengthIndex)}={maxLengthIndex}, " +
                $"{nameof(minLengthIndex)}={minLengthIndex}, {nameof(secondNearestIndex)}={secondNearestIndex}, " +
                $"{nameof(maxSqrLength)}={maxSqrLength}, {nameof(minSqrLength)}={minSqrLength}," +
                $"{nameof(secondNearestSqrLength)}={secondNearestSqrLength}", this);
        }
#endif
        Vector2 firstCastVector = castVectors[castPointIndex1].normalized;
        Vector2 otherCastVector1 = castVectors[minLengthIndex].normalized;
        Vector2 otherCastVector2 = castVectors[thirdNearestIndex].normalized;

        int castPointIndex2 = thirdNearestIndex;
        if(Vector2.Dot(firstCastVector, otherCastVector1) < Vector2.Dot(firstCastVector, otherCastVector2)) { // comparing angles and choosing the biggest
            castPointIndex2 = minLengthIndex;
        }

        if(Physics2D.Linecast(point, castPoints[castPointIndex1], obstaclesLayerMask).collider == null) return true;
        if(Physics2D.Linecast(point, castPoints[castPointIndex2], obstaclesLayerMask).collider == null) return true;
        return false;
    }

    public bool IsVisibleFromPoint_old(Vector2 point, int obstaclesLayerMask = Physics.IgnoreRaycastLayer) {
        if(obstaclesLayerMask == Physics.IgnoreRaycastLayer) {
            return true;
        }
        Vector2 pos = collider.transform.position;
        /*Vector2[] controlPoints = {
            pos,
            new Vector2(pos.x, collider.bounds.min.y),
            new Vector2(pos.x, collider.bounds.max.y),
            new Vector2(collider.bounds.min.x, pos.y),
            new Vector2(collider.bounds.max.x, pos.y)
        };

        foreach(Vector2 dot in controlPoints) {
            var hitCollider = Physics2D.Linecast(point, dot, obstaclesLayerMask).collider;
            if(hitCollider == null) {
                return true;
            }
        }*/

        //if(Physics2D.Linecast(point, pos, obstaclesLayerMask).collider == null) return true;
        if(Physics2D.Linecast(point, new Vector2(pos.x, collider.bounds.min.y), obstaclesLayerMask).collider == null) return true;
        if(Physics2D.Linecast(point, new Vector2(pos.x, collider.bounds.max.y), obstaclesLayerMask).collider == null) return true;
        if(Physics2D.Linecast(point, new Vector2(collider.bounds.min.x, pos.y), obstaclesLayerMask).collider == null) return true;
        if(Physics2D.Linecast(point, new Vector2(collider.bounds.max.x, pos.y), obstaclesLayerMask).collider == null) return true;
        return false;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected() {
        ai?.DrawGizmos();
    }
#endif
}
