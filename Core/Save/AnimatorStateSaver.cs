using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorStateSaver : Saver {
    [System.Serializable]
    private struct AnimatorSaved {
        [System.Serializable]
        public struct LayerState {
            public int nameHash;
            public float normalizedTime;
        }
        public Dictionary<int, LayerState> layerStates;
        public Dictionary<int, object> parameterValues;
    }

    [System.Serializable]
    public struct AnimatorParameter {
        public int nameHash;
        public AnimatorControllerParameterType type;
    }

    [SerializeField, HideInInspector] int[] saveLayers;
    [SerializeField, HideInInspector] AnimatorParameter[] saveParameters;

#if UNITY_EDITOR
    protected override void Reset() {
        base.Reset();
        var animator = GetComponent<Animator>();
        saveLayers = Enumerable.Range(0, animator.layerCount).ToArray();
        var parameters = new List<AnimatorParameter>();
        foreach(var parameter in animator.parameters) {
            parameters.Add(new AnimatorParameter {
                nameHash = parameter.nameHash,
                type = parameter.type
            });
        }
        saveParameters = parameters.ToArray();
    }
#endif

    public override void OnLoad(object data) {
        var animatorData = (AnimatorSaved)data;
        var animator = GetComponent<Animator>();

        foreach(var parameter in saveParameters) {
            object parameterValue = animatorData.parameterValues[parameter.nameHash];
            switch(parameter.type) {
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameter.nameHash, (bool)parameterValue);
                    break;
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameter.nameHash, (float)parameterValue);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameter.nameHash, (int)parameterValue);
                    break;
            }
        }
        foreach(int layer in saveLayers) {
            var layerState = animatorData.layerStates[layer];
            animator.Play(layerState.nameHash, layer, layerState.normalizedTime);
        }
    }

    public override object OnSave() {
        var animatorData = new AnimatorSaved {
            layerStates = new Dictionary<int, AnimatorSaved.LayerState>(),
            parameterValues = new Dictionary<int, object>()
        };
        var animator = GetComponent<Animator>();

        foreach(int layer in saveLayers) {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            animatorData.layerStates[layer] = new AnimatorSaved.LayerState {
                nameHash = stateInfo.shortNameHash,
                normalizedTime = stateInfo.normalizedTime
            };
        }
        foreach(var parameter in saveParameters) {
            object parameterValue = null;
            switch(parameter.type) {
                case AnimatorControllerParameterType.Bool:
                    parameterValue = animator.GetBool(parameter.nameHash);
                    break;
                case AnimatorControllerParameterType.Float:
                    parameterValue = animator.GetFloat(parameter.nameHash);
                    break;
                case AnimatorControllerParameterType.Int:
                    parameterValue = animator.GetInteger(parameter.nameHash);
                    break;
            }
            animatorData.parameterValues[parameter.nameHash] = parameterValue;
        }

        return animatorData;
    }
}
