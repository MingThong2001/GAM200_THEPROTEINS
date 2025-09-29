using System.Collections.Generic;
using UnityEngine;

public class ChangeVolume : MonoBehaviour
{
    SpringJoint2D[] springJointList;
    List<float> originalDistanceList = new List<float>();
    List<float> modifiedDistanceList = new List<float>();
    List<float> originalFrequencyList = new List<float>();
    List<float> originalDampList = new List<float>();
    public GameObject firePoint;
    float firePointOriY;
    public float oriSize = 1f;
    public float shrinkSize = 0.5f;
    public float growSize = 2f;
    public float lowerStr = 0.5f;
    public float oriStr = 1f;

    public MassSegment massSegment;
    private int lastSegmentcount;
    void Start()
    {
        springJointList = GetComponentsInChildren<SpringJoint2D>();
        foreach (SpringJoint2D joint in springJointList)
        {
            originalDistanceList.Add(joint.distance);
            modifiedDistanceList.Add(joint.distance);
            originalFrequencyList.Add(joint.frequency);
            originalDampList.Add(joint.dampingRatio);
        }
        firePointOriY = firePoint.transform.localPosition.y;
        massSegment = GetComponentInParent<MassSegment>();
      
    }

    void Update()
    {
        //if (Input.GetKey(KeyCode.X))
        //{
         
        //    Change(growSize);
            

        //}
        //if (Input.GetKey(KeyCode.C))
        //{
        //    Change(shrinkSize);
        //}
        //if (Input.GetKey(KeyCode.V))
        //{
        //    Change(oriSize);
        //}
        if (Input.GetKey(KeyCode.N))
        {
            ChangeJointStr(oriStr);
        }
        if (Input.GetKey(KeyCode.M))
        {
            ChangeJointStr(lowerStr);
        }

        //if (massSegment == null) return;

        //int currentSegmentCount = massSegment.gloomassStats.currentSegments;
        //if (currentSegmentCount != lastSegmentcount)
        //{
        //    float scalefactor = GetvaluescalefromSegment();
        //    Change(scalefactor);
        //    ChangeJointStr(oriStr);

        //    lastSegmentcount = currentSegmentCount;
        //}

    }



    public void Change(float volumeFactor)
    {
        int i = 0;
        foreach (SpringJoint2D joint in springJointList)
        {
            joint.distance = modifiedDistanceList[i] + (volumeFactor - 1) * originalDistanceList[i];
            modifiedDistanceList[i] = joint.distance;
            i++;
        }
    }

    public void AdjustFirePointDistance(float volumeFactor)
    {
        //firePoint.transform.localPosition = new Vector2(0, firePointOriY * volumeFactor);
    }

    public void ChangeJointStr(float value)
    {
        int i = 0;
        foreach (SpringJoint2D joint in springJointList)
        {
            joint.frequency = originalFrequencyList[i] * value;
            joint.dampingRatio = originalDampList[i] + value;
            i++;
        }
    }

    void SquishDown()
    {
        foreach (SpringJoint2D joint in springJointList)
        {
            if (joint.transform.position.y >= this.transform.position.y)
            {

            }

        }
    }

    #region Volume Scale From Segments
    //public float GetvaluescalefromSegment()
    //{
    //    if (massSegment == null)
    //    {
    //        return 1f;
    //    }

    //    float volumeRatio = (float)(massSegment.gloomassStats.currentSegments - massSegment.gloomassStats.minSegments) / (massSegment.gloomassStats.maxSegments - massSegment.gloomassStats.minSegments);
    //    //Smooth interpolation.
    //    return Mathf.Lerp(shrinkSize, growSize, volumeRatio);
    //}
    #endregion
}
