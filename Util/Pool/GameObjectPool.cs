using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool {
    private static Dictionary<GameObject, GameObjectPool> pools = new Dictionary<GameObject, GameObjectPool>();

    public static GameObjectPool Get(GameObject original) {
        if(original == null)
            return null;

        GameObjectPool pool;
        if(pools.TryGetValue(original, out pool)) {
            return pool;
        }
        return (pools[original] = new GameObjectPool(original));
    }

    private class PoolEntry {
        public GameObject gameObject;
        public PoolObject[] poolObjects;
        public bool isReleased;

        public Transform transform => gameObject.transform;

        public PoolEntry(GameObject gameObject, GameObjectPool pool) {
            this.gameObject = gameObject;
            poolObjects = gameObject.GetComponentsInChildren<PoolObject>(true);
            foreach(var obj in poolObjects) {
                obj.OnCreatedInPool(pool);
            }
            isReleased = false;
        }

        public void OnTaken() {
            isReleased = false;
            gameObject.SetActive(true);
            foreach(var obj in poolObjects) {
                obj.OnTaken();
            }
        }

        public void Release() {
            if(isReleased) {
                return;
            }
            isReleased = true;
            foreach(var obj in poolObjects) {
                obj.OnReleased();
            }
            gameObject.SetActive(false);
        }
    }

    private static Transform _globalRoot;
    private static Transform globalRoot => _globalRoot != null ? _globalRoot : (_globalRoot = new GameObject("Object Pools").transform);
    private Dictionary<GameObject, PoolEntry> entries = new Dictionary<GameObject, PoolEntry>();
    private GameObject original;
    private Transform root;

    private const int callsToClearUnloadedEntries = 100;
    private int takeEntryCalls;

    private GameObjectPool(GameObject original) {
        this.original = original;
        string name = original.name + " Pool";
        root = new GameObject(name).transform;
        root.parent = globalRoot;
    }

    public GameObject Take() {
        var entry = TakeEntry();
        entry.OnTaken();
        return entry.gameObject;
    }

    public GameObject Take(Vector3 setPosition, Quaternion setRotation, Transform setParent) {
        var entry = TakeEntry();
        entry.transform.parent = setParent;
        entry.transform.SetPositionAndRotation(setPosition, setRotation);
        entry.OnTaken();
        return entry.gameObject;
    }

    public GameObject Take(Vector3 setPosition, Quaternion setRotation) {
        var entry = TakeEntry();
        entry.transform.SetPositionAndRotation(setPosition, setRotation);
        entry.OnTaken();
        return entry.gameObject;
    }

    private PoolEntry TakeEntry() {
        if(takeEntryCalls++ >= callsToClearUnloadedEntries) {
            RemoveUnloadedEntries();
        }
        
        PoolEntry result = FindReleasedEntry() ?? CreateNewEntry();
        result.transform.parent = root;
        return result;
    }

    private PoolEntry FindReleasedEntry() {
        foreach(var entry in entries.Values) {
            if(entry.isReleased && entry.gameObject)
                return entry;
        }
        return null;
    }

    // PoolObject.OnDestroy() calls the Remove() method that removes the gameobject from the pool.
    // But gameobjects that do not contain any PoolObject components are not removed from the pool automatically.
    // We need to remove them manually.
    private void RemoveUnloadedEntries() {
        var list = new List<GameObject>();
        foreach(GameObject key in entries.Keys) {
            if(!key) {
                list.Add(key);
            }
        }
        foreach(var go in list) {
            entries.Remove(go);
        }
    }

    private PoolEntry CreateNewEntry() {
        var gameObject = Object.Instantiate(original, root);
        var entry = new PoolEntry(gameObject, this);
        entries.Add(gameObject, entry);
        return entry;
    }

    public void Release(GameObject obj) {
        PoolEntry entry;
        if(entries.TryGetValue(obj, out entry)) {
            entry.Release();
            entry.transform.parent = root;
        }
        else {
            throw new System.ArgumentException("Attempting to release an object that is not in the pool");
        }
    }

    public void Remove(GameObject obj) {
        entries.Remove(obj);
    }
}
