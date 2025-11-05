using System.Collections.Generic;
using UnityEngine;

public class PuddleController : MonoBehaviour
{
    [Header("References")]
    public Playerline playerline;        
    public Rigidbody2D playerRb;         
    public LayerMask groundLayerMask;    
    public AudioManager audioManager;

    [Header("Puddle Settings")]
    [Range(0.1f, 1f)] public float heightFactor = 0.5f;
    [Range(1f, 2f)] public float widthFactor = 1.3f;

    private bool isPuddled = false;
    private List<Vector3> originalLocalPositions = new List<Vector3>(); //To store the original local positions of the player segments.

    private void Start()
    {
        //Get respective components.
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        if (playerline == null)
            playerline = GetComponent<Playerline>();
        if (playerRb == null)
            playerRb = GetComponent<Rigidbody2D>();

        //Store original local position
        originalLocalPositions.Clear();
        foreach (Transform seg in playerline.segments)
            originalLocalPositions.Add(seg.localPosition);
    }

    //Toggle E to activate/deactivate the puddle.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPuddled)
            {
                ApplyPuddle();
                if (audioManager != null)
                {
                    audioManager.PlaySFX(audioManager.applyPuddle);

                }
            }


            else
            {
                RestoreShape();

                if (audioManager != null)
                {
                    audioManager.PlaySFX(audioManager.restorePuddle);

                }
            }    
        }
    }

    private void ApplyPuddle()
    {
        if (isPuddled) return; //Do nothing if its puddle.
        isPuddled = true;

        SquashSegments(); //Squeeze the segment according to designated factors.

        playerline.UpdateShell(); //Update the shape of the shell collider.

        LiftPlayerToPreventPhasing(); //Life players so to prevent player phasing into the terrarin (safety net).

    }

    private void RestoreShape()
    {
        if (!isPuddled) return; //Do nothing if is not puddled.
        isPuddled = false;

        RestoreSegments();
        playerline.UpdateShell();
        LiftPlayerToPreventPhasing();

    }

    //Safety net so taht the player will go back to whatever position it is.
    private void EnsureOriginalPositions()
    {
        if (playerline == null || playerline.segments == null) return;

        //If stored original positions count does not match the segment count, refresh them by clearing and adding them again.
        if (originalLocalPositions.Count != playerline.segments.Length)
        {
            originalLocalPositions.Clear();
            foreach (Transform seg in playerline.segments)
                originalLocalPositions.Add(seg.localPosition);
        }
    }

    //This is what make thaem a puddle.
    private void SquashSegments()
    {
        EnsureOriginalPositions();
        Vector3 center = GetSegmentsCenter(); //Find the center point.

        //Loop through all the points and apply squash on them.
        for (int i = 0; i < playerline.segments.Length; i++)
        {
            Transform seg = playerline.segments[i];
            Vector3 offset = originalLocalPositions[i] - center; //We first find the vector from the center to the point.
            offset.x *= widthFactor; //We apply the horizontal stretch.
            offset.y *= heightFactor; //Veticle stretech. 
            seg.localPosition = center + offset; //Update the segment position.
        }
    }

    private void RestoreSegments()
    {
        EnsureOriginalPositions(); //Make sure the position are correct.

        //reset each segment to its original local position.
        for (int i = 0; i < playerline.segments.Length; i++)
        {
            playerline.segments[i].localPosition = originalLocalPositions[i];
        }
    }

    //WE get the segment centre here.
    private Vector3 GetSegmentsCenter()
    {
        //Sum up all the local points then we divide by the numbers of child so to get an average.
        Vector3 center = Vector3.zero;
        foreach (Transform seg in playerline.segments)
            center += seg.localPosition;
        return center / playerline.segments.Length;
    }

    private void LiftPlayerToPreventPhasing()
    {
        //Find the lowest Y postion .
        float lowestY = float.MaxValue;
        foreach (Transform seg in playerline.segments)
        {
            float worldY = seg.position.y;
            if (worldY < lowestY) lowestY = worldY;
        }

        //We raycast downward to detect ground.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 20f, groundLayerMask);

        if (hit.collider != null)
        {
            float groundY = hit.point.y;
            float penetration = groundY - lowestY; //How much the player is below the ground.

            if (penetration > 0.01f) //So if any player is below the ground, we lift them up.
            {
                playerRb.position += new Vector2(0, penetration + 0.05f); 
            }
        }
    }
}