using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentHelper : MonoBehaviour {
    private NavMeshAgent agent;
    private Rigidbody2D rb;
    private bool revertTransform;
    private bool warpNeeded;
    private bool rbWasSleeping;
    private Vector3 posBeforeRevert;

#if UNITY_EDITOR
    [System.NonSerialized] public Vector2 lastDestination2D;
#endif

    private float _agentZ;
    public float agentZ {
        get { return _agentZ; }
        set {
            _agentZ = value;
            warpNeeded = true;
        }
    }

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponentInChildren<Rigidbody2D>();
        NavMesh.onPreUpdate += OnNavMeshPreUpdate;
    }

    private void Start() {
        agentZ = NavigationMain.current.standardAgentsZ;
    }

    private void OnNavMeshPreUpdate() {
        if(revertTransform && agent.isActiveAndEnabled) {
            revertTransform = false;

            //if(rb != null) {
                //rb.simulated = false;
            //}

            var position = transform.position;
            posBeforeRevert = position;

            var rotation = transform.rotation;

            position.z = agentZ;
            rotation = Quaternion.AngleAxis(-90, transform.right) * rotation;

            transform.SetPositionAndRotation(position, rotation);
            if(warpNeeded) {
                warpNeeded = false;
                agent.Warp(position);
            }
        }
    }

    private void Update() {
        if(!agent.isActiveAndEnabled) {
            return;
        }
        var position = transform.position;
        var rotation = transform.rotation;

        //if(rb != null) {
            //rb.simulated = true;
            //if(position == posBeforeRevert) {
              //  rb.Sleep();
            //}
        //}
        
        position.z = 0;

        rotation = Quaternion.AngleAxis(90, transform.right) * rotation;
        rotation.x = 0;
        rotation.y = 0;
        rotation.Normalize();

        transform.SetPositionAndRotation(position, rotation);
        revertTransform = true;
    }

    private void OnDestroy() {
        NavMesh.onPreUpdate -= OnNavMeshPreUpdate;
    }


#if UNITY_EDITOR
    private static Vector3[] corners = new Vector3[100];
    void OnDrawGizmosSelected() {
        if(!Application.isPlaying)
            return;
        Gizmos.color = Color.green;
        Vector3 prev = transform.position;
        int n = agent.path.GetCornersNonAlloc(corners);
        for(int i = 0; i < n; i++) {
            Vector3 corner = corners[i];
            Gizmos.DrawLine(prev, corner);
            prev = corner;
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, agent.velocity);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, lastDestination2D);
    }
#endif
}
