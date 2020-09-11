using System;
using UnityEngine;

namespace Thunder.Utility.PostProcessing
{
    [Serializable]
    public class MotionBlur : BasePostProcessing
    {
        [Range(0, 1f)]
        public float BlurAmount = 0.5f;
        public Camera Camera;
        private Matrix4x4 _PreVpMatrix;

        public override void Process(RenderTexture source, RenderTexture dest)
        {
            _Mat.SetFloat("_BlurSize", BlurAmount);
            _Mat.SetMatrix("_PreVPMatrix", _PreVpMatrix);
            Matrix4x4 curVpMatrix = Camera.projectionMatrix * Camera.worldToCameraMatrix;
            Matrix4x4 curVpMatrixInverse = curVpMatrix.inverse;
            _Mat.SetMatrix("_CurVPMatrixInverse", curVpMatrixInverse);
            _PreVpMatrix = curVpMatrix;
            RenderTexture tempTexture = RenderTexture.GetTemporary(source.width, source.height, 0);
            Graphics.Blit(source, dest, _Mat);
        }
    }
}
