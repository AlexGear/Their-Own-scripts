using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TypeRestrict : PropertyAttribute {
    public Type type;
    public TypeRestrict(Type type) {
        this.type = type;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TypeRestrict))]
    public class ISavablePropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var propertyAttribute = this.attribute as TypeRestrict;
            EditorGUI.ObjectField(position, property, propertyAttribute.type, label);
        }
    }
#endif
}