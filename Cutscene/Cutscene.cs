using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class Cutscene : MonoBehaviour {
    [SerializeField] bool isSkippable = true;
    [SerializeField] bool saveAtEnd = true;
    [SerializeField] bool playerInputEnabled = false;

    [Header("CameraFollow settings")]
    [SerializeField]
    [Range(0, 1)]
    float cameraCursorWeight = 1f;

    [SerializeField] Transform cameraAdditionalTarget = null;
    [SerializeField]
    [Range(0, 1)]
    float cameraAdditionalTargetWeight = 0f;

    private PlayableDirector director;
    private bool isPlaying = false;
    private float prevCamCursorWeight;
    private Transform prevCamAddTarget;
    private float prevCamAddTargetWeight;

    void Awake() {
        director = GetComponent<PlayableDirector>();
    }

    public void Play() {
        director.Play();
        director.stopped += OnDirectorStopped;

        var camFollow = CameraFollow.current;
        prevCamCursorWeight = camFollow.cursorWeight;
        prevCamAddTarget = camFollow.additionalTarget;
        prevCamAddTargetWeight = camFollow.additionalTargetWeight;

        camFollow.cursorWeight = cameraCursorWeight;
        camFollow.additionalTarget = cameraAdditionalTarget;
        camFollow.additionalTargetWeight = cameraAdditionalTargetWeight;
        MainCharacter.current.cutsceneInputEnabled = playerInputEnabled;

        if(UI.instance != null) {
            UI.instance.BeginCutscene(isSkippable);
        }
        isPlaying = true;
    }

    private void OnDirectorStopped(PlayableDirector director) {
        isPlaying = false;
        director.stopped -= OnDirectorStopped;

        var camFollow = CameraFollow.current;
        camFollow.cursorWeight = prevCamCursorWeight;
        camFollow.additionalTarget = prevCamAddTarget;
        camFollow.additionalTargetWeight = prevCamAddTargetWeight;
        MainCharacter.current.cutsceneInputEnabled = true;

        if(UI.instance != null) {
            UI.instance.EndCutscene();
        }
        if(saveAtEnd && SaveSystem.instance != null) {
            SaveSystem.instance.Save();
        }
    }

    void Update() {
        if(isPlaying) {
            var camFollow = CameraFollow.current;
            camFollow.cursorWeight = cameraCursorWeight;
            camFollow.additionalTargetWeight = cameraAdditionalTargetWeight;

            if(isSkippable && Input.GetKeyDown(KeyCode.Escape)) {
                Skip();
            }
        }
    }

    private void Skip() {
        director.timeUpdateMode = DirectorUpdateMode.Manual;
        float deltaTime = (float)(director.duration - director.time) + 0.01f;
        GetComponent<Animator>().Update(deltaTime);
        director.playableGraph.Evaluate(deltaTime);
        
        if(UI.instance != null) {
            UI.instance.ShowCutsceneSkipBlackout();
        }
    }
}
