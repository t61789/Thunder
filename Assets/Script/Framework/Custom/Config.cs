using System;
using System.IO;

namespace Framework
{
    public static class Config
    {
        public static string UiFramworkBaseObjName = "Canvas";
        public static string ConfigXmlPath;
        public static string ItemInfoTableName;
        public static int PackageItemInfoBuffer;

        /// <summary>
        /// 用于初始化对象
        /// </summary>
        /// <returns>需要加入核心生命周期的继承了IBaseSys的对象</returns>
        public static IBaseSys[] Init()
        {
            // 在这里进行初始化


            return new IBaseSys[]
            {
                // 添加新的系统
            };
        }
    }
}
