namespace Thunder.Sys
{
    public interface IBaseSys
    {
        void OnSceneEnter(string preScene, string curScene);
        void OnSceneExit(string curScene);
        void OnApplicationExit();
    }
}