using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Playerline : MonoBehaviour
{
    
    //Collect child transform points.
    public Transform[] segments;

    [Header("Collider Settings")]
    public float shellThickness = 0.35f; //How thick the collider should be.
    public float paddingFromSegments = 0.1f; //Extra padding around each segment.
    public LayerMask groundLayerMask; //Set this layer to ground so it wont phase into the ground.

    [Header("UpdateFrame")]
    public bool updateEveryFrame = true; //To check against every fixed frames.

    [Header("Visualization")]
    public bool showShell = true;
    public Color shellColor = new Color(0, 0.5f, 1f, 0.5f);

    //Collider (Shell) Settings
    private GameObject shellObject; //To hold the collider and line.
    public EdgeCollider2D edgeCollider; //Define the outer collision shape through edgecollider.
    private Rigidbody2D shellRb; //To control how collider moves.
    private LineRenderer lineRenderer; //For visual feedback.
    private int frameSkip = 0; //TO keep tracked of skipped frames (This is to check for performance.)

    void Start()
    {
        CreateShell();
    }

    //To intialize the shell.
    void CreateShell()
    {
        //GameObject Settings.
        shellObject = new GameObject("SquishyShell");
        shellObject.transform.parent = transform;
        shellObject.layer = LayerMask.NameToLayer("PlayerShell");


        //EdgeCollider Settings.
        edgeCollider = shellObject.AddComponent<EdgeCollider2D>();
        edgeCollider.edgeRadius = shellThickness; //Thickeness of the collider border.

        //Shell RB Settings.
        shellRb = shellObject.AddComponent<Rigidbody2D>();
        shellRb.bodyType = RigidbodyType2D.Dynamic;
        shellRb.mass = 1f;
        shellRb.gravityScale = 1f;
        shellRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        shellRb.interpolation = RigidbodyInterpolation2D.Interpolate; //Make it smooth between frames (Need it cuz we are a slime).
       

        //Ignore each other to prevent any unintended or weird behavior.
        foreach (Transform seg in segments)
        {
            Collider2D segCol = seg.GetComponent<Collider2D>();
            if (segCol != null)
            {
                Physics2D.IgnoreCollision(edgeCollider, segCol, true);
            }
        }
        edgeCollider.includeLayers = groundLayerMask;


        //This is for visualiztion.
        if (showShell)
        {
            lineRenderer = shellObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = shellColor;
            lineRenderer.endColor = shellColor;
            lineRenderer.startWidth = shellThickness * 2f;
            lineRenderer.endWidth = shellThickness * 2f;
            lineRenderer.sortingOrder = 15;
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = true;
        }

        UpdateShell();
    }

    //Physic Update
    void FixedUpdate()
    {
        //So if updateeveryframe is true, we update the shape of the shell.
        if (updateEveryFrame)
        {
            UpdateShell();
        }
        else
        {
            frameSkip++;
            if (frameSkip >= 2)
            {
                UpdateShell();
                frameSkip = 0;
            }
        }
    }

    public void UpdateShell()
    {
        if (segments.Length == 0) return;

        //Get shell points that follow the segment positions.
        List<Vector2> shellPoints = GetSegmentOutline();

        //Close the loop by adding first point at the end.
        shellPoints.Add(shellPoints[0]);

        //Convert to local space for EdgeCollider.
        Vector2[] localPoints = new Vector2[shellPoints.Count];
        for (int i = 0; i < shellPoints.Count; i++)
        {
            localPoints[i] = shellObject.transform.InverseTransformPoint(shellPoints[i]);
        }
        edgeCollider.points = localPoints;

        //Update the line visualization.
        if (showShell && lineRenderer != null)
        {
            lineRenderer.positionCount = shellPoints.Count;
            for (int i = 0; i < shellPoints.Count; i++)
            {
                lineRenderer.SetPosition(i, shellPoints[i]);
            }
        }
    }

    List<Vector2> GetSegmentOutline()
    {
        List<Vector2> points = new List<Vector2>();

        //G through the segmenet in oreer.
        foreach (Transform seg in segments)
        {
            if (seg == null) continue;

            // Get segment position.
            Vector2 segPos = seg.position;

            //Add some paddin.g
            if (paddingFromSegments > 0)
            {
                Vector2 center = GetCenter();
                Vector2 directionFromCenter = (segPos - center).normalized;
                segPos += directionFromCenter * paddingFromSegments;
            }

            points.Add(segPos);
        }

        return points;
    }

    Vector2 GetCenter()
    {
        Vector2 sum = Vector2.zero;
        int count = 0;

        //Get the centre formula by averaging it out.
        foreach (Transform seg in segments)
        {
            if (seg != null)
            {
                sum += (Vector2)seg.position;
                count++;
            }
        }
        //If there is segment, return their centre if not return 0.
        return count > 0 ? sum / count : Vector2.zero;
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || edgeCollider == null) return;

        Gizmos.color = Color.cyan;
        Vector2[] points = edgeCollider.points;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 p1 = shellObject.transform.TransformPoint(points[i]);
            Vector3 p2 = shellObject.transform.TransformPoint(points[i + 1]);
            Gizmos.DrawLine(p1, p2);
        }
    }
}