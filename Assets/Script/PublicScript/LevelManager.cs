using System.Collections.Generic;

namespace Thunder.PublicScript
{
    /// <summary>
    /// dependence:
    /// DatabaseManager
    /// SaveManager
    /// </summary>
    public class LevelManager
    {
        public struct LevelParam
        {
            public int index;
            public string name;
            public string arg;
            public string modeType;

            public LevelParam(int index, string name, string arg, string modeType)
            {
                this.index = index;
                this.name = name;
                this.arg = arg;
                this.modeType = modeType;
            }
        }

        private const string NAME = "name";
        private const string ARG = "arg";
        private const string MODE_TYPE = "mode_type";
        public LevelParam[] levels;

        public LevelManager()
        {
            Load();
        }

        private void Load()
        {
            List<LevelParam> levels = new List<LevelParam>();
            int count = 0;
            foreach (var item in Sys.Stable.dataBase["level"])
            {
                levels.Add(new LevelParam(count, item[NAME] as string, item[ARG] as string, item[MODE_TYPE] as string));
                count++;
            }
            this.levels = levels.ToArray();
        }

        public LevelParam LevelComplete(int index)
        {
            Sys.Stable.saveManager.levelComplete.Add(index);
            if (index < levels.Length - 1)
                return levels[index + 1];
            else
                return default;
        }
    }
}
