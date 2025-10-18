using System.Collections.Generic;
using UnityEngine;

public class PuddleController : MonoBehaviour
{
    private bool isPuddled = false;
    private List<Transform> segments = new List<Transform>();
    private List<Vector3> originalLocalPositions = new List<Vector3>();

    [Header("Puddle Shape Settings")]
    [Range(0.1f, 1f)]
    public float heightFactor = 0.5f;  // vertical compression (0.5 = half as tall)
    [Range(1f, 2f)]
    public float widthFactor = 2f;   // horizontal stretch (1.3 = 30% wider)

    private void Start()
    {
        //This is to collect all the children of the player.
        segments.Clear();
        foreach (Transform child in transform)
        {
            segments.Add(child);
            originalLocalPositions.Add(child.localPosition);
        }
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

    //Convert normal state to puddle state.
    private void ApplyPuddle()
    {
        if (isPuddled) return;
        isPuddled = true;

        //Get the average local position of all segments.
        Vector3 center = Vector3.zero;
        foreach (Transform seg in segments)
            center += seg.localPosition;
        center /= segments.Count;

        //We squished vertically and expand horizontally to get the squish feelings.
        for (int i = 0; i < segments.Count; i++)
        {
            Transform seg = segments[i];
            Vector3 pos = originalLocalPositions[i];
            Vector3 offset = pos - center;

            offset.x *= widthFactor;
            offset.y *= heightFactor;

            seg.localPosition = center + offset;
        }
    }

    //We restore the cshape to the original.
    private void RestoreShape()
    {
        if (!isPuddled) return;
        isPuddled = false;

        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].localPosition = originalLocalPositions[i];
        }
    }
}
