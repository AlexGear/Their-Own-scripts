using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoolObject : MonoBehaviour {
    private GameObjectPool pool;
    public void OnCreatedInPool(GameObjectPool pool) {
        this.pool = pool;
    }   
    
    public virtual void OnTaken() {
        this.enabled = true;
    }

    public virtual void OnReleased() {
        this.enabled = false;
    }

    public void Release() {
        CancelInvoke();
        StopAllCoroutines();
        pool.Release(this.gameObject);
    }

    public void Release(float time) {
        StartCoroutine(ReleaseInSeconds(time));
    }

    private IEnumerator ReleaseInSeconds(float time) {
        yield return new WaitForSeconds(time);
        Release();
    }

    protected virtual void OnDestroy() {
        if(pool != null) {
            pool.Remove(this.gameObject);
        }
    }
}
