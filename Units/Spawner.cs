using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject unitPrefab;
    public bool spawnPeriodically = true;
    public float spawnPeriod = 5f;
    public int periodicSpawnQuantity = 1;
    public FollowPath path;

    private float timer = 0;

    private void Update() {
        if(!spawnPeriodically) {
            return;
        }
        timer += Time.deltaTime;
        if(timer >= spawnPeriod) {
            SpawnAndResetTimer(periodicSpawnQuantity);
        }
    }

    public void Spawn(int quantity) {
        Vector3 position = path?.GetWaypointByIndexClamped(0) ?? transform.position;
        Quaternion rotation = this.transform.rotation;
        for(int i = 0; i < quantity; i++) {
            GameObject unitObject = Instantiate(unitPrefab, position, rotation);
            AI.UnitAI ai = unitObject.GetComponent<Unit>()?.ai;
            if(ai != null)
                ai.followPath = path;
        }
    }

    public void SpawnAndResetTimer(int quantity) {
        Spawn(quantity);
        timer = 0;
    }
}
