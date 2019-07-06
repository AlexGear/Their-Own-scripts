using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Armament
{
    public class Hook : MonoBehaviour
    {
        [SerializeField] float radius = 0.5f;
        [SerializeField] float launchDistance = 13;
        [SerializeField] float launchSpeed = 45;
        [SerializeField] float unitPullingSpeed = 10;
        [SerializeField] float retractSpeed = 20;
        [SerializeField] LayerMask layerMask;
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] Renderer hookRenderer;

        public enum State { Retracted, Launched, Attached, Retracting }
        public State state { get; private set; }
        public bool isRetracted => state == State.Retracted;
        public Unit pulledUnit { get; private set; }

        private static RaycastHit2D[] overlapResults = new RaycastHit2D[10];
        private Transform origin;
        private Vector3 launchDirection;

        public void Launch(Transform origin, Vector2 direction)
        {
            this.state = State.Launched;
            this.origin = origin;
            this.launchDirection = direction.normalized;
            hookRenderer.enabled = true;
            lineRenderer.enabled = true;
            Update();
        }

        public void StartRetracting()
        {
            if (state == State.Retracted)
            {
                return;
            }
            state = State.Retracting;
            pulledUnit = null;
        }

        private void Attach(Unit unit)
        {
            state = State.Attached;
            pulledUnit = unit;
            transform.position = unit.position;
        }

        private void Retracted()
        {
            state = State.Retracted;
            hookRenderer.enabled = false;
            lineRenderer.enabled = false;
        }

        private void ProcessLaunching(float deltaTime)
        {
            var currentPos = transform.position;
            float distance = launchSpeed * deltaTime;
            int n = Physics2D.CircleCastNonAlloc(currentPos, radius, launchDirection, overlapResults, distance, layerMask);
            if (n == 0)
            {
                transform.position = currentPos + launchDirection * launchSpeed * deltaTime;
                if ((transform.position - origin.position).CompareLength(launchDistance) > 0)
                {
                    StartRetracting();
                }
                return;
            }

            Unit unit = CaptureUnit(overlapResults, n);
            if (unit == null)
            {
                StartRetracting();
            }
            else
            {
                Attach(unit);
            }
        }

        private Unit CaptureUnit(RaycastHit2D[] hits, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                var unit = hit.transform.GetComponentInParent<Unit>();
                if (unit.Is())
                {
                    return unit;
                }
            }
            return null;
        }

        private void ProcesRetracting(float deltaTime)
        {
            var vector = origin.position - transform.position;
            if (vector.CompareLength(0.5f) < 0)
            {
                Retracted();
                return;
            }
            transform.position = Vector2.MoveTowards(transform.position, origin.position, retractSpeed * deltaTime);
        }

        private void ProcessUnitPulling(float deltaTime)
        {
            if (!pulledUnit.Is())
            {
                StartRetracting();
                return;
            }
            float unitDistance = Vector2.Distance(pulledUnit.position, origin.position);
            unitDistance -= unitPullingSpeed * deltaTime;
            if (unitDistance < 1f)
            {
                StartRetracting();
                return;
            }
            var vector = pulledUnit.position - (Vector2)origin.position;
            vector = vector.normalized * unitDistance;
            var position = (Vector2)origin.position + vector;
            if (pulledUnit?.ai?.navAgent != null)
            {
                if (pulledUnit.ai.navAgent.GetTraversablePosition(position, out var newPos, 1f))
                {
                    position = newPos;
                }
            }
            transform.position = pulledUnit.position = position;
            if (pulledUnit?.ai?.navAgent != null)
            {
                pulledUnit.ai.navAgent.Warp(position);
            }
        }

        private void Update()
        {
            switch (state)
            {
                case State.Launched:
                    ProcessLaunching(Time.deltaTime);
                    break;

                case State.Attached:
                    ProcessUnitPulling(Time.deltaTime);
                    break;

                case State.Retracting:
                    ProcesRetracting(Time.deltaTime);
                    break;
            }

            if (state != State.Retracted)
            {
                lineRenderer.SetPosition(0, origin.position);
                lineRenderer.SetPosition(1, transform.position);
                transform.rotation = Quaternion2D.LookRotation(transform.position - origin.position);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Handles.color = new Color(1, 0, 0, 0.1f);
            Handles.DrawSolidDisc(transform.position, Vector3.forward, radius);
        }
#endif
    }
}