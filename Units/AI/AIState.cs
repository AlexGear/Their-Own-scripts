using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public static class AIStateExtension {
        public static bool IsCombatState(this AIState state, bool treatUncheckedWanderingPointAsInCombat = false) {
            if(state is TargetSearchState || state is MoveToPositionState || state is DamagedNewTargetSearchState)
                return false;

            var wanderingState = state as TargetSearchWanderingState;
            if(wanderingState != null) {
                if(wanderingState.hasCheckedSearchPoint || !treatUncheckedWanderingPointAsInCombat)
                    return false;
            }

            return true;
        }
    }

    public class AIState {
        public virtual void Enter() { }
        public virtual void Think() { }
        public virtual void Leave() { }

        public override string ToString() {
            return GetType().Name;
        }

        public virtual void DrawGizmos() { }
    }

    public abstract class IntervalThinkState : AIState {
        protected Timer thinkTimer;
        public IntervalThinkState(float interval) {
            thinkTimer = new Timer(interval);
        }

        public override void Think() {
            if(thinkTimer.Tick()) {
                IntervalThink();
            }
        }

        public abstract void IntervalThink();

        public override string ToString() {
            return base.ToString() + $"\nInterval Think Timer: {thinkTimer}";
        }
    }
}