using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCheckpoint : MonoBehaviour {
    public void Save() {
        if(SaveSystem.instance == null) {
            Debug.LogWarning("SaveSystem instance is null");
            return;
        }
        SaveSystem.instance.Save();
    }
}
