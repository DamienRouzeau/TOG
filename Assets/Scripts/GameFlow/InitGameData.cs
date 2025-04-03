using AllMyScripts.Common.Tools;
using System.Collections.Generic;

public class InitGameData
{
    public class InitPlayerData
	{
        public int id = 0;
        public string name = null;
        public string machine = null;
        public int team = 0;

        public InitPlayerData(Dictionary<string, object> dic)
        {
            id = DicTools.GetValueInt32(dic, "id");
            name = DicTools.GetValueString(dic, "name");
            machine = DicTools.GetValueString(dic, "machine");
            team = DicTools.GetValueInt32(dic, "team");
        }
    }

    public static InitGameData instance = null;

    public multiplayerlobby.Product initProduct = multiplayerlobby.Product.TOG;
    public multiplayerlobby.GameMode initMode = multiplayerlobby.GameMode.Normal;
    public multiplayerlobby.SkinTheme initTheme = multiplayerlobby.SkinTheme.Normal;
    public int initDuration = 0;
    public int initLevel = 0;
    public int initWave = 0;
    public bool initNext = false;
    public List<InitPlayerData> playerDatas;

    public static void Create(Dictionary<string, object> dic)
	{
        instance = new InitGameData(dic);
    }

    public InitGameData(Dictionary<string, object> dic)
	{
        initProduct = DicTools.GetValueStringAsEnum<multiplayerlobby.Product>(dic, "product", initProduct);
        initMode = DicTools.GetValueStringAsEnum<multiplayerlobby.GameMode>(dic, "mode", initMode);
        initTheme = DicTools.GetValueStringAsEnum<multiplayerlobby.SkinTheme>(dic, "theme", initTheme);
        initDuration = DicTools.GetValueInt32(dic, "duration");
        initLevel = DicTools.GetValueInt32(dic, "level");
        initWave = DicTools.GetValueInt32(dic, "wave");
        initNext = DicTools.GetValueBool(dic, "next");
        List<object> playerList = DicTools.GetValueList(dic, "players");
        if (playerList != null)
		{
            playerDatas = new List<InitPlayerData>();
            foreach (object o in playerList)
			{
                playerDatas.Add(new InitPlayerData(o as Dictionary<string, object>));
            }
		}
    }

    ~InitGameData()
	{
        if (instance == this)
            instance = null;
    }

    public string GetLevelListId()
	{
        string levelListId = null;
        switch (initProduct)
		{
            case multiplayerlobby.Product.TOG:
                switch (initMode)
				{
                    case multiplayerlobby.GameMode.Normal:
                        levelListId = "Arcade";
                        break;
                    case multiplayerlobby.GameMode.Endless:
                        levelListId = "Endless";
                        break;
                    case multiplayerlobby.GameMode.Kid:
                        levelListId = "Kid_Races";
                        break;
                }
                break;
            case multiplayerlobby.Product.KDK:
                levelListId = "TowerDef_Endless";
                break;
            case multiplayerlobby.Product.BOD:
                levelListId = "BOD_Levels";
                break;
		}
        return levelListId;
    }

    public string GetPlayerName(int id)
	{
        if (playerDatas != null)
		{
            foreach (InitPlayerData player in playerDatas)
			{
                if (player.id == id)
                    return player.name;
			}
		}
        return null;
	}
}
