using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteRandomizer : MonoBehaviour {
#if UNITY_EDITOR
    [SerializeField] Sprite[] sprites = new Sprite[0];
    
    [LabelOverride("Click here to randomize ->")]
    [SerializeField]
    bool randomize = false;

    private SpriteRenderer spriteRenderer;

    void Awake() {
        randomize = false;
    }

    void Update() {
        if(!Application.isPlaying && randomize) {
            randomize = false;
            Randomize();
        }
    }

    private void Randomize() {
        if(spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if(spriteRenderer == null) {
                Debug.LogWarning("No SpriteRenderer attached");
                return;
            }
        }
        if(sprites.Length == 0) {
            Debug.LogWarning("No sprites are present in array");
            return;
        }
        spriteRenderer.sprite = sprites.GetRandomItem();
    }
#endif
}
