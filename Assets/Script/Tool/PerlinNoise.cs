using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.Tool
{
    [Serializable]
    public class PerlinNoise
    {
        [Serializable]
        public struct PerlinParam
        {
            public string Remark;
            public Vector2 StartPos;
            public Vector2 Dir;
            public float Smooth;
            public bool Enable;

            public PerlinParam(string remark, Vector2 startPos, Vector2 dir, float smooth)
            {
                Remark = remark;
                StartPos = startPos;
                Dir = dir;
                Smooth = smooth;
                Enable = true;
            }
        }

        public PerlinParam[] Perlins;

        /// <summary>
        /// 注册一个噪声单元
        /// </summary>
        /// <returns>噪声的下标</returns>
        public int RegisterParam(Vector2 startPos, Vector2 dir, float smooth)
        {
            return RegisterParam(new PerlinParam(null, startPos, dir, smooth));
        }

        /// <summary>
        /// 注册一个噪声单元
        /// </summary>
        /// <returns>噪声的下标</returns>
        public int RegisterParam(PerlinParam param)
        {
            if (Perlins == null)
                Perlins = new PerlinParam[0];

            int i = 0;
            for (; i < Perlins.Length; i++)
            {
                if (Perlins[i].Enable) continue;
                Perlins[i] = param;
                return i;
            }

            if (i != Perlins.Length) return Perlins.Length - 1;
            var temp = new PerlinParam[Perlins.Length + 1];
            Array.Copy(Perlins, temp, Perlins.Length);
            Perlins = temp;
            Perlins[Perlins.Length - 1] = param;

            return Perlins.Length - 1;
        }

        /// <summary>
        /// 注销一个噪声单元，使其不可用
        /// </summary>
        public void CancelParam(int index)
        {
            Assert.IsFalse(Perlins.OutOfRange(index), $"下标有误，范围为[0,{Perlins.Length - 1}]，而你输入了{index}");
            PerlinParam param = Perlins[index];
            if (!param.Enable) return;
            param.Enable = false;
            Perlins[index] = param;
        }

        /// <summary>
        /// 获取下一个采样点
        /// </summary>
        /// <param name="index">申请的噪音单元的下标</param>
        public float GetNext(int index)
        {
            Assert.IsFalse(Perlins.OutOfRange(index), $"下标有误，范围为[0,{Perlins.Length - 1}]，而你输入了{index}");
            PerlinParam param = Perlins[index];
            Assert.IsTrue(param.Enable, "噪声单元不可用，请重新注册");
            param.StartPos += param.Smooth * param.Dir;
            Perlins[index] = param;
            return Mathf.PerlinNoise(param.StartPos.x, param.StartPos.y);
        }
    }
}
