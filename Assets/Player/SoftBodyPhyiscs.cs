using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.U2D;

public class SoftBodyPhyiscs : MonoBehaviour
{
    
    //Constant is used to define an offeset for spline points when they are too close.
    #region Constants 
    private const float splineOffset = 0.5f;
    #endregion

    #region Fields
    [SerializeField]
    public SpriteShapeController spriteShape; //To control the sprite's shape.

    [SerializeField]
    public Transform[] points; //Array of points represent the vertices of the body.
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        UpdateVerticies(); //Call to initialize the spline positions based on the points.
    }

    public void Update()
    {
        UpdateVerticies(); //Continously update the spline vertices to match the point's positions.
    }
    #endregion

    #region privateMethods

    //To update the spline's vertex positions based on the points.
    private void UpdateVerticies()  
    {
        for (int i = 0; i < points.Length - 1; i++) //Loop through each point and update the corresponding vertex in the sprite shape.
        {
            Vector2 vertex = points[i].localPosition; //Get the current's point local position.
            Vector2 _towardsCenter = (Vector2.zero - vertex).normalized; //Calculate direction vector toward the center which is 0 , 0 .
            float colliderRadius = points[i].gameObject.GetComponent<CircleCollider2D>().radius; //Get the collider's radius for the current point. This is to offset the spline.

            try
            {
                spriteShape.spline.SetPosition(i, (vertex - _towardsCenter * colliderRadius)); //Update the position of the spline's vertex and adjust it by the collider radius.

            }
            catch
            {
                spriteShape.spline.SetPosition(i, (vertex - _towardsCenter * (colliderRadius + splineOffset))); //If spline points are too close, adjust the position by adding an offset to avoid overlap.
                Debug.Log("Spline points are too close to each other!");

            }
            Vector2 lt = spriteShape.spline.GetLeftTangent(i); //Calculate the left tangent of the spline based on the directio towards the center.
            Vector2 newRt = Vector2.Perpendicular(_towardsCenter) * lt.magnitude; //Calculate the right tangent as the perpendicular to the direction vector.
            Vector2 newLt = Vector2.zero - (newRt); //Calculate the new left tangent as the opposite of the right tangent.

            //Set the tangents of the spline for smooth curvature.
            spriteShape.spline.SetRightTangent(i, newRt);
            spriteShape.spline.SetLeftTangent(i, newLt);
        }
    }

    #endregion

    //To update the radius of each collider based on the distance to the update spline's position.
    public void UpdateCollider()
    {
        for (int i = 0; i < points.Length; i++) //Loop through each point and update the collider radius.
        { 
            CircleCollider2D collider = points[i].GetComponent<CircleCollider2D>(); //Get the CircleCollider2D component to the point.
            if (collider != null)
            { 
              collider.radius = Vector2.Distance(points[i].localPosition, spriteShape.spline.GetPosition(i)); //Update the collider's radius based on the distance from the point to the corresponding spline's position.
            }
        }
    }
}

    
