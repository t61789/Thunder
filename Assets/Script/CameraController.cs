using System.Collections.Generic;
using Thunder.Utility;
using UnityEngine;

namespace Thunder
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public string CameraType;
        private string _PreCameraType;

        private BaseCamera _CurCamera;
        private readonly Dictionary<string, BaseCamera> _Cameras = new Dictionary<string, BaseCamera>();

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

