using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject FollowTarget;
    public bool OffsetFollow;
    public float OffsetLerp;
    public float MoveDampTime;

    private Vector3 targetPrePosition = Vector2.zero;

    public void LateUpdate()
    {
        if (FollowTarget == null) return;
        if (OffsetFollow)
        {
            Vector3 tempPosition = transform.position;
            tempPosition += FollowTarget.transform.position - targetPrePosition;

            Vector3 newPosition = Vector2.Lerp(FollowTarget.transform.position, Tool.Tools.GetMousePosition(), OffsetLerp);

            Vector3 temp = new Vector3();
            tempPosition = Vector3.SmoothDamp(tempPosition, newPosition, ref temp, MoveDampTime);
            tempPosition.z = -10;
            transform.position = tempPosition;

            targetPrePosition = FollowTarget.transform.position;
        }
        else
        {
            transform.position = FollowTarget.transform.position + Vector3.forward * (-10);
        }
    }
}

