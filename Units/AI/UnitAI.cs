using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI {
    public abstract class UnitAI {
        public Unit owner { get; protected set; }
        public NavMeshAgent navAgent { get; protected set; }

        public Collider2D collider => owner.collider;

        private readonly UnitPathFollowing pathFollowing;
        public virtual FollowPath followPath {
            get { return pathFollowing.path; }
            set { pathFollowing.path = value; }
        }
        public virtual FollowPath secondaryFollowPath {
            get { return pathFollowing.secondaryPath; }
            set { pathFollowing.secondaryPath = value; }
        }

        public AIState upcomingState { get; private set; }
        public AIState quitState { get; private set; }

#if UNITY_EDITOR
        private struct StateQueueEntry {
            public readonly AIState state;
            public readonly int i;
            public StateQueueEntry(AIState state, int i) { this.state = state; this.i = i; }
            public override string ToString() => $"{i}: {state?.GetType()?.Name ?? "<NULL STATE>"}";
        }
        private const int maxPrevStates = 10;
        private Queue<StateQueueEntry> prevStates = new Queue<StateQueueEntry>();
        private int prevStatesCounter = 0;
#endif

        private AIState _state;
        public virtual AIState state {
            get { return _state; }
            set {
                if(value != _state) {
#if UNITY_EDITOR
                    if(prevStates.Count == maxPrevStates)
                        prevStates.Dequeue();
                    prevStates.Enqueue(new StateQueueEntry(_state, prevStatesCounter++));
#endif

                    upcomingState = value;
                    _state?.Leave();
                    upcomingState = null;

                    quitState = _state;
                    _state = value;
                    _state?.Enter();
                    quitState = null;
                }
            }
        }
        
        public UnitAI(Unit owner, NavMeshAgent navAgent) {
            this.owner = owner;
            this.navAgent = navAgent;
            owner.Died += OnOwnerDied;
            pathFollowing = new UnitPathFollowing(this);
        }

        private void OnOwnerDied(Unit unit) {
            state = null;
        }

        public bool IsInCombat(bool treatUncheckedWanderingPointAsInCombat = false) {
            return state.IsCombatState(treatUncheckedWanderingPointAsInCombat);
        }

        public virtual void AttractAttention(Vector2 position) {
        }

        public virtual void Think() {
            _state?.Think();
            pathFollowing.Think();
        }

        public virtual void OnDamageTaken(float value, Unit source) {
        }

        public override string ToString() {
            string stateText = state?.ToString() ?? "<NULL STATE>";
            string result = $"Path: {followPath}\n\nState: {stateText}";
#if UNITY_EDITOR
            result += $"\n\nPrev {maxPrevStates} States:\n" + string.Join("\n", prevStates.Reverse());
#endif
            return result;
        }

#if UNITY_EDITOR
        private static GUIStyle _gizmosStyle;
        private static GUIStyle gizmosStyle {
            get {
                if(_gizmosStyle != null) {
                    return _gizmosStyle;
                }
                _gizmosStyle = new GUIStyle();
                var background = new Texture2D(1, 1);
                background.SetPixel(0, 0, Color.white);
                background.wrapMode = TextureWrapMode.Repeat;
                _gizmosStyle.normal.background = background;
                return _gizmosStyle;
            }
        }

        public virtual void DrawGizmos() {
            UnityEditor.Handles.Label(owner.position, this.ToString(), gizmosStyle);
            state?.DrawGizmos();
        }
#endif
    }
}