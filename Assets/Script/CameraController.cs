using Assets.Script.Turret;
using UnityEngine;

namespace Assets.Script
{
    public class CameraController : MonoBehaviour
    {
        public Ship FollowTarget
        {
            get => followTarget;
            set
            {
                if (value != null)
                    curCamera.orthographicSize = value.GetComponent<SpriteRenderer>().bounds.size.y * SizeCoe;
                followTarget = value;
            }
        }
        public bool OffsetFollow;
        public float OffsetLerp;
        public float MoveDampTime;
        public float SizeCoe = 1;

        private Vector3 targetPrePosition = Vector2.zero;
        private Ship followTarget;
        private Camera curCamera;

        private void Awake()
        {
            curCamera = GetComponent<Camera>();
        }

        public void LateUpdate()
        {
            if (FollowTarget == null) return;
            if (OffsetFollow)
            {
                Vector3 tempPosition = transform.position;
                tempPosition += FollowTarget.trans.position - targetPrePosition;

                Vector3 newPosition = Vector2.Lerp(FollowTarget.trans.position, FollowTarget.aimmingPos, OffsetLerp);

                Vector3 temp = new Vector3();
                tempPosition = Vector3.SmoothDamp(tempPosition, newPosition, ref temp, MoveDampTime);
                tempPosition.z = -10;
                transform.position = tempPosition;

                targetPrePosition = FollowTarget.trans.position;
            }
            else
            {
                transform.position = FollowTarget.trans.position + Vector3.forward * (-10);
            }
        }
    }
}

