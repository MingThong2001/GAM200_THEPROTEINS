using System.Collections;
using UnityEngine;

public class DebugSegments : MonoBehaviour
{
    MassSegment segmentScript;
    bool activatable = true;
    private void Start()
    {
        segmentScript = GetComponent<MassSegment>();
        segmentScript.RemoveSegment();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && activatable == true)
        {
            activatable = false;
            StartCoroutine(Add());

        }
        if (Input.GetKeyDown(KeyCode.C) && activatable == true)
        {
            activatable = false;
            StartCoroutine(Remove());
        }

    }
    IEnumerator Add()
    {
        segmentScript.AddSegment();
        yield return new WaitForSeconds(0.3f);
        activatable = true;
    }

    IEnumerator Remove()
    {
        activatable = false;
        segmentScript.RemoveSegment();
        yield return new WaitForSeconds(0.3f);
        activatable = true;
    }
}
