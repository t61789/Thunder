using System;
using UnityEngine;

namespace Thunder
{
    [Serializable]
    public class MotionBlur : BasePostProcessing
    {
        private Matrix4x4 _PreVpMatrix;

        [Range(0, 1f)] public float BlurAmount = 0.5f;

        public Camera Camera;

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            _Mat.SetFloat("_BlurSize", BlurAmount);
            _Mat.SetMatrix("_PreVPMatrix", _PreVpMatrix);
            var curVpMatrix = Camera.projectionMatrix * Camera.worldToCameraMatrix;
            var curVpMatrixInverse = curVpMatrix.inverse;
            _Mat.SetMatrix("_CurVPMatrixInverse", curVpMatrixInverse);
            _PreVpMatrix = curVpMatrix;
            var tempTexture = RenderTexture.GetTemporary(source.width, source.height, 0);
            Graphics.Blit(source, dest, _Mat);
        }
    }
}