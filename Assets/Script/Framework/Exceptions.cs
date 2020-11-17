using System;

namespace Framework
{
    public class AssetPathInvalidException : Exception
    {
        public static AssetPathInvalidException Default =
            new AssetPathInvalidException();
    }

    /// <summary>
    /// 当需要释放的bundle仍被依赖时触发
    /// </summary>
    public class BundleDependencyException : Exception
    {
        public BundleDependencyException(string bundleName, int dependencyCount) :
            base($"{bundleName} 仍有 {dependencyCount} 个bundle依赖于它，不能释放")
        { }
    }

    /// <summary>
    /// 资源未找到
    /// </summary>
    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(string msg) : base(msg) { }
    }

    /// <summary>
    /// 重复初始化时触发
    /// </summary>
    public class InitDuplicatelyException : Exception
    {
        public InitDuplicatelyException() { }
        public InitDuplicatelyException(string msg) : base(msg) { }
    }
}
