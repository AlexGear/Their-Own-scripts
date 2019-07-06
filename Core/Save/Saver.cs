using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public abstract class Saver : MonoBehaviour {
    public string id;

#if UNITY_EDITOR
    [SerializeField]
    [LabelOverride("Keep ID on duplicating")]
    private bool isPersistent = false;

    protected virtual void Reset() {
        instanceID = 0;
        Awake();
    }

    // Catch duplication
    [SerializeField]
    [HideInInspector]
    private int instanceID = 0;
    void Awake() {
        if(Mathf.Approximately(Time.time, 0)    // exclude false positive triggering when opening scene
            || isPersistent || Application.isPlaying)
            return;

        if(instanceID != GetInstanceID() && GetInstanceID() < 0) {
            instanceID = GetInstanceID();
            id = GenerateID();
        }
    }

    private string GenerateID() => GetInstanceID().ToString();
#endif

    protected virtual void OnDestroy() {
#if UNITY_EDITOR
        if(!Application.isPlaying)
            return;
#endif
        if(SaveSystem.instance != null) {
            SaveSystem.instance.Unregister(id, this);
        }
    }

    public abstract void OnLoad(object data);
    public abstract object OnSave();
}