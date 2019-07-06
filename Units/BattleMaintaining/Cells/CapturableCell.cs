using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BattleMaintaining {
    public partial class CapturableCell : Cell {
        private CapturableCellTriggerHelper helper;
        
        private const float captureDuration = 5f;
        private const float captureUnitMultiplier = 1f;
        private float captureProgress;

        private Timer captureCheckTimer;

        public override void Generate(FollowPath path, float radius) {
            base.Generate(path, radius);

            team = Team.Enemies;
            var trigger = gameObject.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = radius;
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if(helper != null) {
                helper?.OnTriggerEnter(collision);
            }
        }

        protected override void Awake() {
            base.Awake();
            captureCheckTimer = new Timer(0.4f);
            helper = new CapturableCellTriggerHelper(GetComponent<Collider2D>());
        }

        protected override void Update() {
            base.Update();
            if(helper == null) return;

            helper.Update();
            UpdateCaptureProgress();

            if(captureCheckTimer.Tick()) {
                if(captureProgress >= captureDuration - 0.1f) {
                    CaptureBy(team.Opposite());
                }
            }
        }

        private void CaptureBy(Team newTeam) {
            captureProgress = 0;
            team = newTeam;
        }

        private void UpdateCaptureProgress() {
            captureProgress += CalculateProgressDelta(Time.deltaTime);
            captureProgress = Mathf.Clamp(captureProgress, 0, captureDuration);
        }

        private float CalculateProgressDelta(float deltaTime) {
            const int minInvaders = 3;

            int owners = CountUnitsOnOppositeCellsContinuously(team);
            int invaders = CountUnitsOnOppositeCellsContinuously(team.Opposite());

            if(invaders < minInvaders)
                invaders = 0; // do not allow a group of less than <minInvaders> invaders to capture the cell

            float progressDelta = (invaders - owners) * captureUnitMultiplier * deltaTime;
            return progressDelta;
        }
        
        private int CountUnitsOnOppositeCellsContinuously(Team unitsTeam) {
            int sum = helper.UnitsCount(unitsTeam);
            
            foreach(var cell in GetForwardCells(unitsTeam)) {
                if(cell.team == unitsTeam) // count only opposite cells
                    continue; 
                if(!(cell is CapturableCell))
                    continue;

                var capturable = (CapturableCell)cell;
                if(capturable.helper.alliesCount == 0)
                    continue;
                sum += capturable.CountUnitsOnOppositeCellsContinuously(unitsTeam);
            }
            return sum;
        }

        public override object CaptureSnapshot() {
            return new Snapshot {
                captureProgress = captureProgress,
                baseSnapshot = base.CaptureSnapshot()
            };
        }

        public override void RestoreSnapshot(object data) {
            Snapshot snapshot = (Snapshot)data;
            captureProgress = snapshot.captureProgress;
            base.RestoreSnapshot(snapshot.baseSnapshot);
        }

        [System.Serializable]
        private struct Snapshot {
            public float captureProgress;
            public object baseSnapshot;
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected() {
            base.OnDrawGizmosSelected();
            Color color = Color.grey;
            color.a = 0.23f;
            Handles.color = color;
            float angle = captureProgress * 360 / captureDuration;
            Handles.DrawSolidArc(position, Vector3.forward, Vector3.up, angle, radius);
        }
#endif
    }
}