using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class NavMeshAgentExtensions {
    public static bool Raycast2D(this NavMeshAgent nmAgent, Vector3 targetPosition, out NavMeshHit hit) {
        targetPosition.z = nmAgent.GetAgentZ();
        return nmAgent.Raycast(targetPosition, out hit);
    }

    public static bool CanReach(this NavMeshAgent nmAgent, Vector3 position) {
        return CanReach(nmAgent, position, out _);
    }

    public static bool CanReach(this NavMeshAgent nmAgent,  Vector3 position, out NavMeshPath path) {
        path = new NavMeshPath();
        position.z = nmAgent.GetAgentZ();
        return nmAgent.CalculatePath(position, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    public static bool SetPath2D(this NavMeshAgent nmAgent, NavMeshPath path) {
#if UNITY_EDITOR
        var helper = nmAgent.GetComponent<NavAgentHelper>();
        if(helper != null) helper.lastDestination2D = path.corners.Last();
#endif
        return nmAgent.SetPath(path);
    }

    public static float GetRemainingDistance(this NavMeshAgent nmAgent) {
        if(!nmAgent.pathPending) {
            float nmAgentRemainingDist = nmAgent.remainingDistance;
            if(!float.IsInfinity(nmAgentRemainingDist)) {
                return nmAgentRemainingDist;
            }
        }
        return Vector2.Distance(nmAgent.nextPosition, nmAgent.destination);
    }

    public static bool ReachedDestination(this NavMeshAgent nmAgent, float distanceTolerance) {
        return nmAgent.GetRemainingDistance() < distanceTolerance;
    }
    
    public static int GetAgentTypeIDFromName(string agentTypeName) {
        int count = NavMesh.GetSettingsCount();
        for(int i = 0; i < count; i++) {
            int id = NavMesh.GetSettingsByIndex(i).agentTypeID;
            if(agentTypeName == NavMesh.GetSettingsNameFromID(id)) {
                return id;
            }
        }
        return -1;
    }

    public static void SetShieldHolder(this NavMeshAgent nmAgent, bool isShieldHolder = true) {
        var helper = nmAgent.GetComponent<NavAgentHelper>();
        if(!helper) {
            Debug.LogError("No NavAgentHelper is attached to " + nmAgent.name);
            return;
        }
        helper.agentZ = isShieldHolder ? NavigationMain.current.shieldHoldersZ : NavigationMain.current.standardAgentsZ;
    }

    public static Lazy<int> traversableMask = new Lazy<int>(() => 1 << NavMesh.GetAreaFromName("Walkable"));

    public static bool GetTraversablePosition(this NavMeshAgent nmAgent, Vector3 originalPosition, out Vector2 result, float maxDistance) {
        originalPosition.z = nmAgent.GetAgentZ();
        if(!NavMesh.SamplePosition(originalPosition, out var hit, maxDistance, traversableMask.Value)) {
            result = default;
            return false;
        }
        result = hit.position;
        return true;
    }

    public static bool SetDestination2D(this NavMeshAgent nmAgent, Vector2 destination, bool startInstantly = false) {
        if(!nmAgent.isActiveAndEnabled) {
            return false;
        }
        float z = nmAgent.GetAgentZ();
        var destination3D = new Vector3(destination.x, destination.y, z);
        if(startInstantly) {
            var path = new NavMeshPath();
            if(nmAgent.CalculatePath(destination, path)) {
                nmAgent.SetPath2D(path);
            }
        }
        /*NavMeshHit hit;
        if(NavMesh.SamplePosition(destination3D, out hit, 1f, NavMesh.AllAreas)) {
            destination3D = hit.position;
        }*/
#if UNITY_EDITOR
        var helper = nmAgent.GetComponent<NavAgentHelper>();
        if(helper != null) helper.lastDestination2D = destination3D;
#endif
        return nmAgent.SetDestination(destination3D);
    }

    public static float GetAgentZ(this NavMeshAgent nmAgent) {
        return nmAgent.GetComponent<NavAgentHelper>()?.agentZ ?? NavigationMain.current.standardAgentsZ;
    }

    public static bool SamplePosition(this NavMeshAgent nmAgent, Vector3 position, float maxDistance, out Vector2 result) {
        position.z = nmAgent.GetAgentZ();
        NavMeshHit hit;
        if(!NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas)) {
            result = position;
            return false;
        }
        result = hit.position;
        return true;
    }
}
