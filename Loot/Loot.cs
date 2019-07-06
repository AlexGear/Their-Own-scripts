using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class Loot : MonoBehaviour {
    [SerializeField] protected SpriteRenderer lootSpriteRenderer;
    [SerializeField] protected GameObject glow;
    protected float destroyTime = 1f;
    protected bool playerIsNear = false;
    protected bool needPressButton = false;
    protected Collider2D trigger;
    private Animator animator;
    private readonly int pickedUpHash = Animator.StringToHash("PickedUp");
    protected MainCharacter mainCharacter;

    [Serializable]
    protected struct Attraction {
        public bool enabled;
        public float radius;
        public AnimationCurve curve;
        [LabelOverride("Obstacles")] public LayerMask obstaclesLayerMask;
    }
    [SerializeField] protected Attraction attraction;

    protected virtual void Awake() {
        trigger = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Start() {
        mainCharacter = MainCharacter.current;
    }

    private void Update() {
        if (attraction.enabled) {
            FollowMainChar();
        }
        if (playerIsNear) {
            if (!needPressButton || needPressButton && MainCharacter.current.inputEnabled && Input.GetButtonDown("Use")) {
                PickUp();
            }
        }
    }

    private void FollowMainChar() {
        Vector2 direction = mainCharacter.transform.position - transform.position;
        if (direction.sqrMagnitude <= attraction.radius * attraction.radius) {
            bool playerIsAchievable = Physics2D.LinecastNonAlloc(transform.position, mainCharacter.transform.position, new RaycastHit2D[2]) == 1;
            if (playerIsAchievable) {
                float curveArg = 1 / attraction.radius * direction.magnitude;
                float curveVal = attraction.curve.Evaluate(curveArg);
                transform.position = Vector2.Lerp(transform.position, mainCharacter.transform.position, curveVal);
            }
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            playerIsNear = false;
        }
    }

    protected virtual void PickUp() {
        Destroy(gameObject, destroyTime);
        playerIsNear = false;
        trigger.enabled = false;
        lootSpriteRenderer.enabled = false;
        animator.SetBool(pickedUpHash, true);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        if (attraction.enabled) {
            Color discColor = Color.blue;
            discColor.a = 0.05f;
            Handles.color = discColor;
            Handles.DrawSolidDisc(transform.position, Vector3.forward, attraction.radius);
        }
    }
#endif
}