using UnityEngine;
using UnityEngine.U2D;

public class ProjectileSpriteShape : MonoBehaviour
{
    public SpriteShapeController fillSpriteShape;
    public SpriteShapeController edgeSpriteShape;
    Rigidbody2D[] pointArray;
    private const float splineOffset = 0.5f;
    private void Start()
    {
        Rigidbody2D[] templist = GetComponentsInChildren<Rigidbody2D>();
        pointArray = new Rigidbody2D[12];
        for (int i = 0; i < pointArray.Length; i++)
        {
            pointArray[i] = templist[i + 1];
        }
    }
    private void Update()
    {
        UpdateSplines();
    }

    void UpdateSplines()
    {
        //updates splines based on the points on the physical model
        for (int i = 0; i < pointArray.Length; i++)
        {
            Vector2 localPos = fillSpriteShape.transform.InverseTransformPoint(pointArray[i].transform.position);

            //update spline's position    
            fillSpriteShape.spline.SetPosition(i, localPos);
            edgeSpriteShape.spline.SetPosition(i, localPos);

            //update spline's rotation
            Vector2 towardsCenter = (Vector2.zero - localPos).normalized;
            Vector2 lTangent = fillSpriteShape.spline.GetLeftTangent(i);
            Vector2 newRTangent = Vector2.Perpendicular(towardsCenter) * lTangent.magnitude;
            Vector2 newLTangent = Vector2.zero - newRTangent;

            fillSpriteShape.spline.SetRightTangent(i, newRTangent);
            fillSpriteShape.spline.SetLeftTangent(i, newLTangent);
            edgeSpriteShape.spline.SetRightTangent(i, newRTangent);
            edgeSpriteShape.spline.SetLeftTangent(i, newLTangent);
        }

    }
}
