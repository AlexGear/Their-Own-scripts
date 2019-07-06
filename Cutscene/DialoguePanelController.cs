using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DialoguePanelController : PlayableAsset {
    public enum State { Enabled, Disabled }
    public State setState = State.Enabled;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
        var template = new DialoguePanelControllerPlayable() {
            enabled = (setState == State.Enabled)
        };
        return ScriptPlayable<DialoguePanelControllerPlayable>.Create(graph, template);
    }
}

public class DialoguePanelControllerPlayable : PlayableBehaviour {
    public bool enabled;

    public override void OnBehaviourPlay(Playable playable, FrameData info) {
        if(UI.instance != null) {
            if(enabled) {
                UI.instance?.ShowDialoguePanel();
            }
            else {
                UI.instance?.HideDialoguePanel();
            }
        }

        base.OnBehaviourPlay(playable, info);
    }
}