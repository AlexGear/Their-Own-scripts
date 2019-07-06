using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI {
    public abstract class CommandedUnitAI : SightedUnitAI {
        private Timer commanderRefreshTimer = new Timer(3f);

        private Commander _commander;
        public Commander commander {
            get { return _commander; }
            set {
                if(value != _commander) {
                    _commander?.RemoveCommandedAI(this);
                    _commander = value;
                    _commander?.AddCommandedAI(this);
                }
            }
        }
        public bool isCommanderSelf => _commander?.ai == this;

        public abstract bool isMelee { get; }

        public CommandedUnitAI(Unit owner, NavMeshAgent navAgent, UnitStats stats)
            : base(owner, navAgent, stats) 
        {
            commander = Commander.FindCommanderFor(this);
        }

        public override void Think() {
            if(commanderRefreshTimer.Tick()) {
                commander = Commander.FindCommanderFor(this);
            }
            if(isCommanderSelf) {
                commander.Think();
            }
            base.Think();
        }

        public override string ToString() {
            return $"Commander: {commander.ai.owner}\nIsCommanderSelf: {isCommanderSelf}\n" +
                $"CommanderRefreshTimer: {commanderRefreshTimer}\n" + base.ToString();
        }

#if UNITY_EDITOR
        public override void DrawGizmos() {
            base.DrawGizmos();
            commander?.DrawGizmos();
        }
#endif
    }
}