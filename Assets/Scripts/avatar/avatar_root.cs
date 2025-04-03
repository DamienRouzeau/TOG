using UnityEngine;

public class avatar_root : MonoBehaviour
{
    public static avatar_root myself;

    public GameObject teleporterTarget => _teleporterTarget;
    public Player.WeaponType defaultLeftWeapon => _defaultLeftWeapon;
    public Player.WeaponType defaultRightWeapon => _defaultRightWeapon;

    [SerializeField]
    private GameObject _teleporterTarget = null;
    [SerializeField]
    private Player.WeaponType _defaultLeftWeapon = Player.WeaponType.TOG_Biggun;
    [SerializeField]
    private Player.WeaponType _defaultRightWeapon = Player.WeaponType.Musket;

    private void Awake()
    {
        myself = this;
	}

    public static bool AttachPlayersToStartHub()
    {
        Debug.Log("AttachPlayersToStartHub");
        // Our player
        if (Player.myplayer.gameObject.transform.parent != myself.transform)
            Player.myplayer.gameObject.transform.SetParent(myself.transform);
        // Other players
        Player_avatar[] allAvatars = Player.myplayer.avatars;
        foreach (Player_avatar avatar in allAvatars)
        {
            if (avatar.actornumber >= 0)
            {
                avatar.SetName(GameflowBase.piratenames[avatar.actornumber]);
                if (avatar.gameObject.transform.parent != myself.transform)
                    avatar.gameObject.transform.SetParent(myself.transform);
                avatar.gameObject.SetActive(true);
            }
        }
        return true;
    }

    public static bool AttachPlayersToBoats(boat_followdummy [] boats)
    {
        if (boats[0] == null) 
            return false;

        // Our player
        int team = GameflowBase.myTeam;

        Debug.Log("AttachPlayersToBoats " + team);

        boat_sinking bs = boats[team].gameObject.GetComponentInChildren<boat_sinking>();

        Player.myplayer.InitTeam(team);
        Player.myplayer.RemoveDagger();

        if (Player.myplayer.transform.parent != bs.transform)
            Player.myplayer.transform.SetParent(bs.transform);

        if (pointfromhand.teleporttarget == null)
		{
            Player.myplayer.InitTeleporterTarget(myself);
            Player.myplayer.ChangeTypeOfWeaponData(0, Player.WeaponType.Musket);
            Player.myplayer.ChangeTypeOfWeaponData(1, Player.WeaponType.Musket);
        }

        pointfromhand.teleporttarget.transform.SetParent(Player.myplayer.transform.parent);

        // Other players
        Player_avatar[] allAvatars = Player.myplayer.avatars;
        foreach (Player_avatar avatar in allAvatars)
        {
            if (avatar.actornumber >= 0)
            {
                team = GameflowBase.allteams[avatar.actornumber];
                bs = boats[team].gameObject.GetComponentInChildren<boat_sinking>();
                
                avatar.InitTeam(team);

                if (avatar.gameObject.transform.parent != bs.transform)
                    avatar.gameObject.transform.SetParent(bs.transform);
                avatar.gameObject.SetActive(true);
            }
        }

        // Quests
        if (RaceManager.myself != null)
            RaceManager.myself.ShowQuestDisplay((boat_followdummy.TeamColor)team);

        if (UI_Tutorial.myself != null)
            UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.PlayerOnBoat);

        return true;
    }

    public static bool AttachPlayersToBase()
    {
        Debug.Log("AttachPlayersToBase");
        Player.myplayer.InitTeam(0);
        Player.myplayer.RemoveDagger();

        if (pointfromhand.teleporttarget == null)
        {
            Player.myplayer.InitTeleporterTarget(myself);
        }

        Player.myplayer.ChangeTypeOfWeaponData(0, myself.defaultLeftWeapon);
        Player.myplayer.ChangeTypeOfWeaponData(1, myself.defaultRightWeapon);

        pointfromhand.teleporttarget.transform.SetParent(Player.myplayer.transform.parent);

        // Other players
        Player_avatar[] allAvatars = Player.myplayer.avatars;
        foreach (Player_avatar avatar in allAvatars)
        {
            if (avatar.actornumber >= 0)
            {
                avatar.SetName(GameflowBase.piratenames[avatar.actornumber]);
                avatar.InitTeam(0);
                bool visible = GameflowBase.IsNumActorVisible(avatar.actornumber);
                avatar.gameObject.SetActive(visible);
            }
        }

        return true;
    }



}
