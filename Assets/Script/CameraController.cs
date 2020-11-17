using System.Collections.Generic;

using UnityEngine;

namespace Thunder
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        private readonly Dictionary<string, BaseCamera> _Cameras = new Dictionary<string, BaseCamera>();

        private BaseCamera _CurCamera;
        private string _PreCameraType;
        public string CameraType;

        public void SwitchCamera(string cameraType)
        {
            if (_CurCamera != null)
                _CurCamera.enabled = false;
            _CurCamera = _Cameras[cameraType];
            _CurCamera.enabled = true;
        }

        private void Awake()
        {
            // ReSharper disable once LocalVariableHidesMember
            foreach (var camera in GetComponents<BaseCamera>())
            {
                camera.enabled = false;
                _Cameras.Add(camera.GetType().Name, camera);
            }
        }

        private void FixedUpdate()
        {
            if (CameraType == _PreCameraType) return;
            _PreCameraType = CameraType;
            SwitchCamera(CameraType);
        }
    }
}