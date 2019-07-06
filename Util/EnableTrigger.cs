using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Light2D;
using System;

[RequireComponent(typeof(Collider2D))]
public class EnableTrigger : MonoBehaviour {
    [SerializeField] List<GameObject> ignoreChildren = new List<GameObject>();
    [SerializeField] LayerMask activatorsLayerMask = 1 << 8; // Allies

    private GameObject[] gameObjects;

    private Collider2D trigger;
    private List<Collider2D> touching = new List<Collider2D>();

    private bool active;

    private const float toggleCooldown = 1f;

    private void Awake() {
        trigger = GetComponent<Collider2D>();
        active = this.gameObject.activeSelf;
    }

    private void Init() {
        var gameObjectsList = new List<GameObject>();
        foreach(Transform child in transform) {
            if(ignoreChildren.Contains(child.gameObject) || !child.gameObject.activeSelf)
                continue;

            gameObjectsList.Add(child.gameObject);
        }
        gameObjects = gameObjectsList.ToArray();
    }

    IEnumerator Start() {
        yield return null;
        yield return null;
        yield return null;

        Init();

        if(!active) {
            UpdateState();
        }

        bool oldActive = active;
        while(true) {
            if(active) {
                touching.RemoveAll(x => x == null || !x.isActiveAndEnabled || !x.IsTouching(trigger));
                if(touching.Count == 0) {
                    active = false;
                }
            }

            if(active != oldActive) {
                oldActive = active;
                if(!MainCharacter.current.isDead) { // preventing objects disappear on player's death
                    UpdateState();
                }
                yield return new WaitForSeconds(toggleCooldown);
            }
            else yield return null;
        }
    }

    private void UpdateState() {
        for(int i = 0; i < gameObjects.Length; i++)
            gameObjects[i].SetActive(active);
    }

    private void Enable() => active = true;
    private void Disable() => active = false;

    private void OnTriggerEnter2D(Collider2D collision) {
        bool tagMatches = collision.CompareTag("XrayTrigger") || collision.CompareTag("Player");
        if(tagMatches && 0 != ((1 << collision.gameObject.layer) & activatorsLayerMask)) {
            touching.Add(collision);
            Enable();
        }
    }
}
