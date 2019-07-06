using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BattleMaintaining {    
    public abstract class Cell : MonoBehaviour, ISavable {
        [SerializeField, HideInInspector] FollowPath path;

        [SerializeField, HideInInspector] float _radius;
        public float radius {
            get { return _radius; }
            protected set { _radius = value; }
        }

        [SerializeField, HideInInspector] Team _team;
        public virtual Team team {
            get { return _team; }
            set {
                if(_team != value) {
                    _team = value;
                    teamChanged?.Invoke(this);
                }
            }
        }
        public event System.Action<Cell> teamChanged;

        public Vector2 position => transform.position;

        public FollowPath GetPath() => path;

        private DominanceSupervisor dominanceSupervisor;

        /// <summary>
        /// Interted-like <see cref="team"/> property
        /// </summary>
        public Team dominantTeam => dominanceSupervisor.currentDominator;

        [SerializeField, HideInInspector] List<Cell> incomingCells = new List<Cell>();
        [SerializeField, HideInInspector] List<Cell> outcomingCells = new List<Cell>();

        public IReadOnlyCollection<Cell> incoming => incomingCells;
        public IReadOnlyCollection<Cell> outcoming => outcomingCells;

        public static void Connect(Cell @out, Cell @in) {
            if(@in.outcomingCells.Contains(@out) || @out.incomingCells.Contains(@in)) // already connected inversely
                return;
            @in.incomingCells.Add(@out);
            @out.outcomingCells.Add(@in);
        }

        public static void Disconnect(Cell a, Cell b) {
            a.outcomingCells.Remove(b);
            a.incomingCells.Remove(b);
            b.outcomingCells.Remove(a);
            b.incomingCells.Remove(a);
        }

        public IReadOnlyCollection<Cell> GetBackwardCells() => GetBackwardCells(this.team);

        public IReadOnlyCollection<Cell> GetBackwardCells(Team relativeTo) {
            return relativeTo == Team.Allies ? incoming : outcoming;
        }

        public IReadOnlyCollection<Cell> GetForwardCells() => GetForwardCells(this.team);

        public IReadOnlyCollection<Cell> GetForwardCells(Team relativeTo) {
            return relativeTo == Team.Allies ? outcoming : incoming;
        }

        /// <summary>
        /// If there are at least <paramref name="level"/> teammate cells in front of this cell
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool HasProtection(int level) => GetForwardCells().Count(c => c.team == this.team) >= level;

        public virtual void Generate(FollowPath path, float radius) {
            this.path = path;
            this.radius = radius;
        }

        public bool IsPointInside(Vector2 point) {
            return IsPointInside(point, this.radius);
        }

        public bool IsPointInside(Vector2 point, float overrideRadius) {
            return (this.position - point).CompareLength(overrideRadius) < 0;
        }

        protected virtual void Awake() {
            dominanceSupervisor = new DominanceSupervisor(this);
        }

        protected virtual void Update() {
            dominanceSupervisor.Update();
        }

        public virtual object CaptureSnapshot() {
            return new Snapshot {
                team = team,
                dominanceSupervisorSnapshot = dominanceSupervisor.CaptureSnapshot()
            };
        }

        public virtual void RestoreSnapshot(object data) {
            Snapshot snapshot = (Snapshot)data;
            _team = snapshot.team;
            dominanceSupervisor.RestoreSnapshot(snapshot.dominanceSupervisorSnapshot);
        }

        [System.Serializable]
        private struct Snapshot {
            public Team team;
            public object dominanceSupervisorSnapshot;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected() {
            Color color1 = team == Team.Allies ? Color.green : Color.red;
            Color color2 = color1;
            color2.a = 0.1f;
            Handles.color = color1;
            Handles.DrawWireDisc(transform.position, Vector3.forward, radius);
            Handles.color = color2;
            Handles.DrawSolidDisc(transform.position, Vector3.forward, radius);

            Gizmos.color = Color.white;
            foreach(var i in incoming) {
                DrawArrows(i.transform.position, transform.position);
            }
            foreach(var o in outcoming) {
                DrawArrows(transform.position, o.transform.position);
            }

            if(dominanceSupervisor != null) {
                Handles.color = Color.green * (dominantTeam == Team.Allies ? 1 : 0.5f);
                DrawArc(Vector3.up, dominanceSupervisor.alliesWeight / DominanceSupervisor.maxWeight * 360, radius + 1, 1.3f);
                Handles.color = Color.red * (dominantTeam == Team.Enemies ? 1 : 0.5f);
                DrawArc(Vector3.up, dominanceSupervisor.enemiesWeight / DominanceSupervisor.maxWeight * -360, radius + 1, 1.3f);
            }
        }

        private void DrawArrows(Vector3 from, Vector3 to) {
            const float angle = 15;
            const float length = 8;
            Vector3 vector = to - from;
            Vector3 arrowLine1 = Quaternion.AngleAxis(180 - angle, Vector3.forward) * vector.normalized * length;
            Vector3 arrowLine2 = Quaternion.AngleAxis(180 + angle, Vector3.forward) * vector.normalized * length;
            Vector3 arrowOrigin = from + vector * 0.5f;
            for(float t = -1; t < 1; t += 0.49f) {
                Vector3 offset = Vector2.Perpendicular(vector).normalized;
                Gizmos.DrawRay(arrowOrigin + offset * t, arrowLine1);
                Gizmos.DrawRay(arrowOrigin + offset * t, arrowLine2);
                Gizmos.DrawRay(from + offset * t, vector);
            }
        }

        private void DrawArc(Vector3 from, float angle, float radius, float thickness) {
            for(float r = radius; r < radius + thickness; r += 0.2f) {
                Handles.DrawWireArc(transform.position, Vector3.forward, from, angle, r);
            }
        }
#endif
    }
}