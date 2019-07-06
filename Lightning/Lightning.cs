using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(LineRenderer))]
public class Lightning : MonoBehaviour {
    [SerializeField] private string resourceName;
    [SerializeField] private GameObject endFlarePrefab;
    [SerializeField] private float showDuration = 0.04f;
    [SerializeField]
    [MinMaxSlider(0, 1.5f)]
    private Vector2 maxSpreadPerMeter = new Vector2(0.15f, 0.25f);
    [SerializeField] private float pointsPerMeter = 8;
    [SerializeField] private float branchingChance = 0.6f;
    [SerializeField] private int maxBranchingOrder = 4;
    [SerializeField] private float branchWidthFactor = 0.7f;
    [SerializeField] private float minBranchLength = 1f;
    [SerializeField] private float minBranchOffset = 1f;
    [SerializeField] private float maxBranchOffset = 2f;
    [SerializeField]
    [MinMaxSlider(0, 2)]
    private Vector2 rootWidth = new Vector2(0.6f, 1.4f);

    private Coroutine strikeRoutine = null;

    private LineRenderer lineRenderer;
    private Vector3 end;

    private LightningEndFlare flare;
    private bool hideFlare;

    private int branchingOrder;
    private List<Lightning> branchPool = new List<Lightning>();
    private int branchCount;

    private Vector3[] points = new Vector3[0];
    private int pointCount;

    public float GetShowDuration() => showDuration;

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private Lightning TakeBranch() {
        if(branchCount >= branchPool.Count) {
            GameObject instance = Instantiate(Resources.Load<GameObject>(resourceName));
            instance.transform.parent = this.transform;
            LineRenderer branchRenderer = instance.GetComponent<LineRenderer>();
            branchRenderer.widthMultiplier = lineRenderer.widthMultiplier * branchWidthFactor;

            branchPool.Add(instance.GetComponent<Lightning>());
        }
        return branchPool[branchCount++];
    }

    private void BranchAimed(Vector3 start, Queue<Vector3> engageEnds) {
        Vector3 end = engageEnds.Dequeue();
        TakeBranch().GenerateAsBranch(start, end, this.branchingOrder + 1, engageEnds);
    }

    private void BranchFree(Vector3 start, Vector3 end, Vector3 offset) {
        float length = Vector3.Distance(start, end);
        if(length < minBranchLength) {
            end = start + (end - start) / length * minBranchLength;
        }
        end = Vector3.Lerp(start, end, 0.7f) + offset * Random.Range(minBranchOffset, maxBranchOffset);
        TakeBranch().GenerateAsBranch(start, end, this.branchingOrder + 1);
    }

    private bool CanBranch(Vector3 start) {
        return branchingOrder < maxBranchingOrder && Vector3.Distance(start, this.end) >= minBranchLength;
    }

    private void GenerateRecursively(int left, int right, float maxSpread, Vector3 offsetDir, Queue<Vector3> engageEnds = null) {
        if(left >= right - 1) {
            return;
        }
        if(Random.Range(0, 2) == 0) { 
			offsetDir = -offsetDir; 
		}
        Vector3 offset = offsetDir * Random.Range(0, maxSpread);
        Vector3 point = offset + (points[left] + points[right]) / 2f;

        int index = (left + right) / 2;
        points[index] = point;

        if(CanBranch(point) && Random.value <= branchingChance) {
            if(engageEnds != null && engageEnds.Any()) {
                BranchAimed(point, engageEnds);
            }
            else {
                BranchFree(point, points[right], offsetDir * maxSpread);
            }
        }
        maxSpread *= 0.5f;
        GenerateRecursively(left, index, maxSpread, offsetDir, engageEnds);
        GenerateRecursively(index, right, maxSpread, offsetDir, engageEnds);
    }

    private void HideUnusedBranches() {
        for(int i = branchCount; i < branchPool.Count; i++) {
            branchPool[i].Hide();
        }
    }

    private void Generate(Queue<Vector3> engageEnds = null) {
        Vector3 startToEnd = end - transform.position;
        Vector3 offsetDir = new Vector3(-startToEnd.y, startToEnd.x).normalized;

        float straightDistance = startToEnd.magnitude;
        float spread = Random.Range(maxSpreadPerMeter.x, maxSpreadPerMeter.y);
        float maxSpread = straightDistance * spread;

        pointCount = 1 + Mathf.CeilToInt(straightDistance * pointsPerMeter);
        if(points.Length < pointCount) {
            points = new Vector3[pointCount];
        }
        lineRenderer.positionCount = pointCount;
        
        points[0] = transform.position;
        points[pointCount - 1] = end;

        branchCount = 0;
        GenerateRecursively(0, pointCount - 1, maxSpread, offsetDir, engageEnds);
        lineRenderer.SetPositions(points);

        HideUnusedBranches();
    }

    public void Strike() {
        if(strikeRoutine != null) {
            StopCoroutine(strikeRoutine);
            Hide();
        }
        strikeRoutine = StartCoroutine(StrikeCoroutine());
        for(int i = 0; i < branchCount; i++) {
            branchPool[i].Strike();
        }
    }

    private IEnumerator StrikeCoroutine() {
        lineRenderer.enabled = true;

        var color = Color.white;
        float growFactor = Random.Range(1.5f, 3.3f);
        bool flareVisible = false;
        for(float time = 0; time < showDuration; time += Time.deltaTime) {
            float t = time / showDuration;

            t = 1 - (1 - t) * (1 - t);
            if(branchingOrder != 0) {
                t = t * t;
            }

            float f = t * (2 * t * t - 3 * t + 2);

            color.a = 1 - t;
            if(f * growFactor > 0.7f) {
                color.a *= 2;
                lineRenderer.positionCount = pointCount;
            }
            else {
                color.a *= 0.7f;
                lineRenderer.positionCount = Mathf.Min(pointCount, (int)(f * growFactor * pointCount));
            }
            lineRenderer.SetPositions(points);
            
            if(!hideFlare && !flareVisible && f * growFactor > 0.7f) {
                ShowFlare(end);
                flareVisible = true;
            }

            lineRenderer.startColor = color;
            lineRenderer.endColor = color;


            yield return null;
        }
        Hide();
    }

    private void ShowFlare(Vector3 position) {
        if(flare == null) {
            flare = Instantiate(endFlarePrefab).GetComponent<LightningEndFlare>();
        }
        flare.transform.position = position;
        flare.Show();
    }

    private void Hide() {
        lineRenderer.enabled = false;
        if(flare != null) {
            flare.Hide();
        }
        for(int i = 0; i < branchCount; i++) {
            branchPool[i].Hide();
        }
    }

    void OnDisable() {
        lineRenderer.enabled = false;
        if(flare != null) {
            flare.Hide();
        }
    }

    public void GenerateAsRoot(Vector3 start, List<Vector3> ends, bool hideFlare = false) {
        if(ends == null || ends.Count == 0) {
            GenerateEmpty();
            return;
        }

        transform.position = start;
        lineRenderer.widthMultiplier = Random.Range(rootWidth.x, rootWidth.y);

        Queue<Vector3> engageEnds = new Queue<Vector3>(ends);
        end = engageEnds.Dequeue();

        branchingOrder = 0;
        this.hideFlare = hideFlare;

        Generate(engageEnds);
    }

    private void GenerateEmpty() {
        lineRenderer.positionCount = 0;
        pointCount = 0;
        hideFlare = true;
        for(int i = 0; i < branchCount; i++) {
            if(branchPool[i] != null) {
                branchPool[i].GenerateEmpty();
            }
        }
    }

    private void GenerateAsBranch(Vector3 start, Vector3 end, int branchingOrder, Queue<Vector3> engageEnds = null) {
        transform.position = start;
        this.end = end;
        this.branchingOrder = branchingOrder;

        bool isFree = engageEnds == null;
        hideFlare = isFree;
        lineRenderer.endWidth = isFree ? 0.2f : 1f;

        Generate(engageEnds);
    }
}
