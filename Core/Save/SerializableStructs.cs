using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SerializableStructs {
    [System.Serializable]
    public struct Vector2Saved {
        public float x, y;
        public Vector2Saved(Vector2 v) { x = v.x; y = v.y;  }
        public Vector2 value => new Vector2(x, y);
        public static explicit operator Vector2(Vector2Saved src) => src.value;
    }
    
    [System.Serializable]
    public struct Vector3Saved {
        public float x, y, z;
        public Vector3Saved(Vector3 v) { x = v.x; y = v.y; z = v.z; }
        public Vector3 value => new Vector3(x, y, z);
        public static explicit operator Vector3(Vector3Saved src) => src.value;
    }

    [System.Serializable]
    public struct QuaternionSaved {
        public float x, y, z, w;
        public QuaternionSaved(Quaternion q) { x = q.x; y = q.y; z = q.z; w = q.w; }
        public Quaternion value => new Quaternion(x, y, z, w);
        public static explicit operator Quaternion(QuaternionSaved src) => src.value;
    }

    [System.Serializable]
    public struct ColorSaved {
        public float r, g, b, a;
        public ColorSaved(Color c) { r = c.r; g = c.g; b = c.b; a = c.a; }
        public Color value => new Color(r, g, b, a);
        public static explicit operator Color(ColorSaved src) => src.value;
    }
}