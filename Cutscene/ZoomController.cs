using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ZoomController : PlayableAsset {
    public float zoomCameraSize = 10f;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
        var template = new ZoomControllerPlayable() {
            zoomCameraSize = zoomCameraSize
        };
        return ScriptPlayable<ZoomControllerPlayable>.Create(graph, template);
    }
}

public class ZoomControllerPlayable : PlayableBehaviour {
    public float zoomCameraSize;

    public override void OnBehaviourPlay(Playable playable, FrameData info) {
        if(CameraFollow.current != null)
            CameraFollow.current.ZoomCamera(zoomCameraSize);
        base.OnBehaviourPlay(playable, info);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info) {
        if(CameraFollow.current != null)
            CameraFollow.current.ResetZoom();
        base.OnBehaviourPause(playable, info);
    }
}
