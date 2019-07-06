using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blood : MonoBehaviour {
    [SerializeField] GameObject bloodStain;
    [SerializeField] GameObject bloodDrops;
    [SerializeField] GameObject bloodBurst;
    [SerializeField] Transform bloodRoot;
    [SerializeField] float minBloodstainScale = 0.3f;
    [SerializeField] float maxBloodstainScale = 0.5f;

    private static Transform _bloodBurstsRoot;
    private static Transform bloodBurstsRoot => _bloodBurstsRoot != null ? _bloodBurstsRoot :
        (_bloodBurstsRoot = new GameObject("BloodBursts").transform);

    private GameObjectPool _pool;
    private GameObjectPool bloodStainPool => _pool != null ? _pool : (_pool = GameObjectPool.Get(bloodStain));
    private Queue<GameObject> bloodStains = new Queue<GameObject>();
    private const int maxBloodStains = 3;
    
    public void AddNewBloodStain(Vector2 position, Vector2 normal) {
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, normal);

        if(bloodStain != null && bloodRoot != null) {
            GameObject instance = GetBloodStain(position, rotation);
            float scale = Random.Range(minBloodstainScale, maxBloodstainScale);
            instance.transform.localScale = new Vector2(scale, scale);
        }
        if(bloodDrops != null) {
            Instantiate(bloodDrops, position, rotation);
        }
    }

    private GameObject GetBloodStain(Vector2 position, Quaternion rotation) {
        if(bloodStains.Count >= maxBloodStains) {
            return WrapQueueAroundAndGetBloodStain();
        }
        GameObject instance = bloodStainPool.Take(position, rotation, bloodRoot);
        instance.name += name;
        bloodStains.Enqueue(instance);
        return instance;
    }

    private GameObject WrapQueueAroundAndGetBloodStain() {
        var result = bloodStains.Dequeue();
        bloodStains.Enqueue(result);
        return result;
    }
    
    public void ClearBloodStains() {
        while(bloodStains.Count != 0) {
            bloodStainPool.Release(bloodStains.Dequeue());
        }
    }

    public void SpawnBloodBurst() {
        Instantiate(bloodBurst, transform.position, Quaternion.identity, bloodBurstsRoot);
    }
}
