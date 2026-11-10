using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 targetPos = Camera.main.transform.position;
        targetPos.y = transform.position.y;

        transform.LookAt(targetPos);
    }
}
