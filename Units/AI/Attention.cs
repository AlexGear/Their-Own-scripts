using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public static class Attention {
        //private static Collider2D[] overlapResults = new Collider2D[100];
        private const float cooldown = 1f;
        private const float minDistance = 10f;
        //private static float attentionAttractionLastTime = float.NegativeInfinity;
        private class Epicenter {
            public Vector2 position;
            public float timestamp;
            public Epicenter(Vector2 position, float timestamp) {
                this.position = position;
                this.timestamp = timestamp;
            }
            public bool isExpired => Time.time - timestamp > cooldown;
            public bool IsClose(Vector2 position) => (position - this.position).CompareLength(minDistance) < 0;
        }
        private static List<Epicenter> epicenters = new List<Epicenter>();

        public static void Attract(Vector2 position, float radius, int layerMask) {
            var toRemove = new List<Epicenter>();
            for(int i = 0; i < epicenters.Count; i++) {
                Epicenter epicenter = epicenters[i];
                if(epicenter.isExpired) {
                    toRemove.Add(epicenter);
                    continue;
                }
                if(epicenter.IsClose(position))
                    return;
            }
            foreach(var e in toRemove)
                epicenters.Remove(e);

            epicenters.Add(new Epicenter(position, Time.time));

            DoAttractNow(position, radius, layerMask);
        }

        private static void DoAttractNow(Vector2 position, float radius, int layerMask) {
            foreach(var unit in Unit.GetInRadius<Unit>(position, radius, layerMask)) {
                unit?.ai?.AttractAttention(position);
            }
            /*int n = Physics2D.OverlapCircleNonAlloc(position, radius, overlapResults, layerMask);
            for(int i = 0; i < n; i++) {
                Unit unit = overlapResults[i].GetComponentInParent<Unit>();
                unit?.ai?.AttractAttention(position);
            }*/
        }
    }
}