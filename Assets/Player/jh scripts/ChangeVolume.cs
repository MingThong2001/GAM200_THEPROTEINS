using System.Collections.Generic;
using UnityEngine;

public class ChangeVolume : MonoBehaviour
{
    SpringJoint2D[] springJointList;
    List<float> originalDistanceList = new List<float>();
    List<float> originalFrequencyList = new List<float>();
    List<float> originalDampList = new List<float>();
    public float oriSize = 1f;
    public float shrinkSize = 0.5f;
    public float growSize = 2f;
    public float lowerStr = 0.5f;
    public float oriStr = 1f;


    void Start()
    {
        springJointList = GetComponentsInChildren<SpringJoint2D>();
        foreach (SpringJoint2D joint in springJointList)
        {
            originalDistanceList.Add(joint.distance);
            originalFrequencyList.Add(joint.frequency);
            originalDampList.Add(joint.dampingRatio);
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.X))
        {
            Change(growSize);
        }
        if (Input.GetKey(KeyCode.C))
        {
            Change(shrinkSize);
        }
        if (Input.GetKey(KeyCode.V))
        {
            Change(oriSize);
        }
        if (Input.GetKey(KeyCode.N))
        {
            ChangeJointStr(oriStr);
        }
        if (Input.GetKey(KeyCode.M))
        {
            ChangeJointStr(lowerStr);
        }


    }

    void Change(float volumeFactor)
    {
        int i = 0;
        foreach (SpringJoint2D joint in springJointList)
        {
            joint.distance = originalDistanceList[i] * volumeFactor;
            i++;
        }
    }
    void ChangeJointStr(float value)
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
}
