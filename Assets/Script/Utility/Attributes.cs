using System;

namespace Thunder
{
    public class DontGenerateWrapAttribute : Attribute { }

    public class GenerateWrapAttribute : Attribute { }

    public class DontInjectAttribute : Attribute { }

    public class PreferenceAsset : Attribute { }
}