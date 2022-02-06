using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limiter : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Camera cameraBounds;
    private Rect limit;

    void LateUpdate()
    {
        float ratio = Screen.width / (float)Screen.height;
        Vector2 halfTargetSize2D = target.transform.lossyScale / 2;
        var size = new Vector2(cameraBounds.orthographicSize * ratio, cameraBounds.orthographicSize) - halfTargetSize2D;
        Vector2 cameraPos2D = cameraBounds.transform.position;
        limit = new Rect(cameraPos2D - size, size * 2);
        Debug.Log(limit);

        if (target.position.x < limit.xMin)
        {
            target.position = new Vector3(limit.xMin, target.position.y, target.position.z);
        }
        if (target.position.y < limit.yMin)
        {
            target.position = new Vector3(target.position.x, limit.yMin, target.position.z);
        }
        if (target.position.x > limit.xMax)
        {
            target.position = new Vector3(limit.xMax, target.position.y, target.position.z);
        }
        if (target.position.y > limit.yMax)
        {
            target.position = new Vector3(target.position.x, limit.yMax, target.position.z);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(limit.xMin, limit.yMin, 1), new Vector3(limit.xMax, limit.yMin, 1));
        Gizmos.DrawLine(new Vector3(limit.xMax, limit.yMin, 1), new Vector3(limit.xMax, limit.yMax, 1));
        Gizmos.DrawLine(new Vector3(limit.xMax, limit.yMax, 1), new Vector3(limit.xMin, limit.yMax, 1));
        Gizmos.DrawLine(new Vector3(limit.xMin, limit.yMax, 1), new Vector3(limit.xMin, limit.yMin, 1));
    }
}
