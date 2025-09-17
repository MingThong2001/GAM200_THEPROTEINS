using System.Collections.Generic;
using UnityEngine;

public class ChangeVolume : MonoBehaviour
{
    SpringJoint2D[] springJointList;
    List<float> originalDistanceList = new List<float>();

    void Start()
    {
        springJointList = GetComponentsInChildren<SpringJoint2D>();
        foreach (SpringJoint2D joint in springJointList)
        {
            originalDistanceList.Add(joint.distance);
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Y))
        {
            Change(2f);
        }
        if (Input.GetKey(KeyCode.C))
        {
            Change(0.5f);
        }
        if (Input.GetKey(KeyCode.V))
        {
            Change(1f);
        }
        if (Input.GetKey(KeyCode.N))
        {
            ChangeJointStr(0.01f);
        }
        if (Input.GetKey(KeyCode.M))
        {
            ChangeJointStr(-0.01f);
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
            joint.frequency = joint.frequency + value;
            joint.dampingRatio = joint.dampingRatio + value;
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
