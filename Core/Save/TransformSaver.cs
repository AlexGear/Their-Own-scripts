using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SerializableStructs;

public class TransformSaver : Saver {
    [System.Serializable]
    private struct TransformSaved {
        public Vector3Saved localPosition, localScale;
        public QuaternionSaved localRotation;
    }

    public override void OnLoad(object data) {
        TransformSaved transformSaved = (TransformSaved)data;
        transform.localPosition = (Vector3)transformSaved.localPosition;
        transform.localScale = (Vector3)transformSaved.localScale;
        transform.localRotation = (Quaternion)transformSaved.localRotation;
    }

    public override object OnSave() {
        return new TransformSaved {
            localPosition = new Vector3Saved(transform.localPosition),
            localScale = new Vector3Saved(transform.localScale),
            localRotation = new QuaternionSaved(transform.localRotation)
        };
    }
}
