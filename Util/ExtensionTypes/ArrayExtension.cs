using UnityEngine;

public static class ArrayExtension {
    public static T GetRandomItem<T>(this T[] array) {
        int index = Random.Range(0, array.Length);
        return array[index];
    }
}