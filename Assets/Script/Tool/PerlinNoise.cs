using System;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Tool
{
    public struct PerlinNoise
    {
        private const float RandomRange = 100;

        public Vector2 StartPos;
        public Vector2 Dir;
        public float Smooth;

        /// <summary>
        /// 指定的起始位置和方向
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="dir">方向，需为归一化</param>
        /// <param name="smooth">平滑度，绝对值越小越平滑</param>
        public PerlinNoise(Vector2 startPos, Vector2 dir, float smooth)
        {
            StartPos = startPos;
            Dir = dir;
            Smooth = smooth;
        }

        /// <summary>
        /// 随机的起始位置和方向
        /// </summary>
        /// <param name="smooth">平滑度，绝对值越小越平滑</param>
        public PerlinNoise(float smooth)
        {
            StartPos = Tools.RandomVectorInCircle(RandomRange);
            Dir = Tools.RandomVectorInCircle(1).normalized;
            Smooth = smooth;
        }

        /// <summary>
        /// 获取下一个采样点
        /// </summary>
        /// <returns>噪声值，介于[0,1]内</returns>
        public float GetNext()
        {
            StartPos += Smooth * Dir;
            return Mathf.PerlinNoise(StartPos.x, StartPos.y);
        }
    }
}
