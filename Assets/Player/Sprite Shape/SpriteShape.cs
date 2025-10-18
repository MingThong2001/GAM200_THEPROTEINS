using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteShape : MonoBehaviour
{
    public SpriteShapeController fillSpriteShape;
    public SpriteShapeController edgeSpriteShape;
    public Transform tentaTransform;
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
        Rigidbody2D tallestPoint1 = null;
        float tallestY = float.NegativeInfinity;
        //updates splines based on the points on the physical model
        for (int i = 0; i < pointArray.Length; i++)
        {
            Vector2 localPos = fillSpriteShape.transform.InverseTransformPoint(pointArray[i].transform.position);
            //Vector2 localPos2 = new Vector2();
            //if (i != pointArray.Length)
            //{
            //    localPos2 = fillSpriteShape.transform.InverseTransformPoint(pointArray[i + 1].transform.position);
            //}
            //else
            //{
            //    localPos2 = fillSpriteShape.transform.InverseTransformPoint(pointArray[0].transform.position);
            //}
            //Vector2 localPos = pointArray[i].localPosition; //doesnt work :(

            //try
            //{
            //    spriteShapeController.spline.SetPosition(i, localPos);
            //}
            //catch
            //{
            //    Debug.Log("Spline points are too close together, recalculating");
            //    spriteShapeController.spline.SetPosition(i, localPos * splineOffset);
            //}
            //update spline's position    
            fillSpriteShape.spline.SetPosition(i, localPos);
            edgeSpriteShape.spline.SetPosition(i, localPos);

            //find tallest each frame
            float currentY = pointArray[i].transform.position.y;
            if (currentY > tallestY)
            {
                tallestY = currentY;
                tallestPoint1 = pointArray[i];
            }

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
        if (tallestPoint1 != null)
        {
            tentaTransform.position = tallestPoint1.position;
        }
        
        //take tallest point and parent point to them idk

    }
}
