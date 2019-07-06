using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(SpriteRenderer))]
public class LaserDesignator : MonoBehaviour {
    [SerializeField] LayerMask stopMask;
    [SerializeField] float maxDistance = 50;

    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        if(lineRenderer != null) {
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
        }
    }

    private void Update() {
        var hit = Physics2D.Raycast(transform.parent.position, transform.up, maxDistance, stopMask);
        if(hit.collider == null) {
            if(lineRenderer == null)
                spriteRenderer.enabled = false;
            transform.position = transform.parent.position + transform.up * maxDistance;
        }
        else {
            spriteRenderer.enabled = true;
            transform.position = hit.point;
        }
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer() {
        if(lineRenderer == null)
            return;

        lineRenderer.SetPosition(0, transform.parent.position);
        lineRenderer.SetPosition(1, transform.position);
    }
}
