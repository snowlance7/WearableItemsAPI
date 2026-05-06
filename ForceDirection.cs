using UnityEngine;

internal class ForceDirection : MonoBehaviour
{
    private RectTransform rt;
    public float forcedZRotation = 0f;

    private Vector3 rot = Vector3.zero;

    void Awake()
    {
        rot.z = forcedZRotation;
        rt = GetComponent<RectTransform>();
    }
	
	void Update () 
    {
        if (!Application.isPlaying)
        rot.z = forcedZRotation;

        if (rt.eulerAngles != rot)
            rt.eulerAngles = rot;
    }
}
