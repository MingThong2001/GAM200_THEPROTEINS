using System.Collections.Generic;
using UnityEngine;

public class PuddleController : MonoBehaviour
{
    [Header("References")]
    public Playerline playerline;        // Reference to your Playerline script
    public Rigidbody2D playerRb;         // Player's main Rigidbody2D
    public LayerMask groundLayerMask;    // Terrain layer

    [Header("Puddle Settings")]
    [Range(0.1f, 1f)] public float heightFactor = 0.5f;
    [Range(1f, 2f)] public float widthFactor = 1.3f;

    private bool isPuddled = false;
    private List<Vector3> originalLocalPositions = new List<Vector3>();

    private void Start()
    {
        if (playerline == null)
            playerline = GetComponent<Playerline>();
        if (playerRb == null)
            playerRb = GetComponent<Rigidbody2D>();

        // Store original segment local positions
        originalLocalPositions.Clear();
        foreach (Transform seg in playerline.segments)
            originalLocalPositions.Add(seg.localPosition);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPuddled)
                ApplyPuddle();
            else
                RestoreShape();
        }
    }

    private void ApplyPuddle()
    {
        if (isPuddled) return;
        isPuddled = true;

        SquashSegments();
        playerline.UpdateShell();

        LiftPlayerToPreventPhasing(); // lift before squashing

    }

    private void RestoreShape()
    {
        if (!isPuddled) return;
        isPuddled = false;

        RestoreSegments();
        playerline.UpdateShell();
        LiftPlayerToPreventPhasing(); // lift before expanding

    }

    private void SquashSegments()
    {
        Vector3 center = GetSegmentsCenter();

        for (int i = 0; i < playerline.segments.Length; i++)
        {
            Transform seg = playerline.segments[i];
            Vector3 offset = originalLocalPositions[i] - center;
            offset.x *= widthFactor;
            offset.y *= heightFactor;
            seg.localPosition = center + offset;
        }
    }

    private void RestoreSegments()
    {
        for (int i = 0; i < playerline.segments.Length; i++)
        {
            playerline.segments[i].localPosition = originalLocalPositions[i];
        }
    }

    private Vector3 GetSegmentsCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (Transform seg in playerline.segments)
            center += seg.localPosition;
        return center / playerline.segments.Length;
    }

    private void LiftPlayerToPreventPhasing()
    {
        // Find the actual lowest segment point NOW (not predicted)
        float lowestY = float.MaxValue;
        foreach (Transform seg in playerline.segments)
        {
            float worldY = seg.position.y;
            if (worldY < lowestY) lowestY = worldY;
        }

        // Raycast downward from player center
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 20f, groundLayerMask);

        if (hit.collider != null)
        {
            float groundY = hit.point.y;
            float penetration = groundY - lowestY;

            if (penetration > 0.01f) // if any segment is below ground
            {
                playerRb.position += new Vector2(0, penetration + 0.05f); // add small buffer
            }
        }
    }
}