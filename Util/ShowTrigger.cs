using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Light2D;

[RequireComponent(typeof(Collider2D))]
public class ShowTrigger : MonoBehaviour {
    [SerializeField] private List<GameObject> ignoreObjects = new List<GameObject>();

    private Renderer[] renderers;
    private CustomSprite[] customSprites;

    private bool setEnabled = false;

    private const float toggleCooldown = 1f;

    private void Init() {
        var renderersList = new List<Renderer>();
        var customSpritesList = new List<CustomSprite>();

        foreach(var renderer in GetComponentsInChildren<Renderer>(true)) {
            if(ignoreObjects.Contains(renderer.gameObject))
                continue;

            var customSprite = renderer.GetComponent<CustomSprite>();
            if(customSprite != null) {
                if(customSprite.enabled) {
                    customSpritesList.Add(customSprite);
                }
                continue;
            }

            if(renderer.enabled) {
                renderersList.Add(renderer);
            }
        }

        renderers = renderersList.ToArray();
        customSprites = customSpritesList.ToArray();
    }
    
    IEnumerator Start() {
        yield return null;
        yield return null;

        Init();

        if(!setEnabled) {
            UpdateState();
        }

        bool oldEnabled = setEnabled;
        while(true) {
            if(setEnabled != oldEnabled) {
                oldEnabled = setEnabled;
                if(!MainCharacter.current.isDead) { // preventing objects disappear on player's death
                    UpdateState();
                }
                yield return new WaitForSeconds(toggleCooldown);
            }
            else yield return null;
        }
    }

    private void UpdateState() {
        for(int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = setEnabled;
        for(int i = 0; i < customSprites.Length; i++)
            customSprites[i].enabled = setEnabled;
    }

    private void EnableRenderers() => setEnabled = true;
    private void DisableRenderers() => setEnabled = false;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            EnableRenderers();
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            DisableRenderers();
        }
    }
}
