using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class DialogueSubtitle : PlayableAsset {
    public string textID = "TEXT_ID";
    public float delayBetweenEachChar = 0.2f;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
        string localizedText = Localization.ByID(textID);
        var template = new SubtitlePlayable() {
            text = localizedText,
            delayBetweenEachChar = delayBetweenEachChar
        };
        return ScriptPlayable<SubtitlePlayable>.Create(graph, template);
    }
}

public class SubtitlePlayable : PlayableBehaviour {
    public string text;
    public float delayBetweenEachChar;

    private float timer = 0;
    private int visibleCharacters = 0;
    
    public override void PrepareFrame(Playable playable, FrameData info) {
        timer += info.deltaTime;
        int chars = Mathf.FloorToInt(timer / delayBetweenEachChar);
        chars = Mathf.Min(chars, text.Length);
        if(chars != visibleCharacters) {
            visibleCharacters = chars;
            string substring = text.Substring(0, chars);
            if(UI.instance != null) {
                UI.instance.SetDialogueSubtitles(substring);
            }
        }

        base.PrepareFrame(playable, info);
    }
}