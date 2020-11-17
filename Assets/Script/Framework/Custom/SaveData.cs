using Newtonsoft.Json;

namespace Framework
{
    public class SaveData
    {
        [JsonIgnore] public static SaveData Ins;

        // 添加需要保存的字段
    }
}
