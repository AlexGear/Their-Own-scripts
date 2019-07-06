using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MoveToTarget : PlayableAsset {
    public ExposedReference<UnityEngine.AI.NavMeshAgent> agent;
    public bool disableAgentAtEnd = false;
    public ExposedReference<Transform> target;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
        var template = new MoveToTargetPlayable {
            agent = agent.Resolve(graph.GetResolver()),
            disableAgentAtEnd = disableAgentAtEnd,
            target = target.Resolve(graph.GetResolver())
        };
        return ScriptPlayable<MoveToTargetPlayable>.Create(graph, template);
    }
}

public class MoveToTargetPlayable : PlayableBehaviour {
    public UnityEngine.AI.NavMeshAgent agent;
    public bool disableAgentAtEnd = false;
    public Transform target;

    public override void OnBehaviourPlay(Playable playable, FrameData info) {
        if(agent != null && target != null) {
            agent.enabled = true;
            agent.SetDestination2D(target.position);
        }
        base.OnBehaviourPlay(playable, info);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info) {
        if(agent != null && disableAgentAtEnd) {
            agent.enabled = false;
        }

        base.OnBehaviourPause(playable, info);
    }
}