using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class startgamebutton : MonoBehaviour
{
    public bool isDaggersEnable => _enableDaggers;

    [SerializeField]
    private bool _enableDaggers = false;
    [SerializeField]
    private GameObject[] _objectsToEnableDaggers = null;

    public GameObject left_off;
    public GameObject left_on;
    public GameObject right_off;
    public GameObject right_on;
    public GameObject left_forceshow = null;
    public GameObject right_forceshow = null;
    public RRLib.RRLocalizedTextMP left_commandtext = null;
    public RRLib.RRLocalizedTextMP right_commandtext = null;
    public Image left_commandboard = null;
    public Image right_commandboard = null;
    public string jointext = "";
    public string readytext = "";
    public Color joincolor;
    public Color readycolor;
    public Color readyothercolor;

    [SerializeField]
    private GameObject [] Team_List_A_Element = new GameObject[gameflowmultiplayer.nrplayersperteam];
    [SerializeField]
    private GameObject [] Team_List_B_Element = new GameObject[gameflowmultiplayer.nrplayersperteam];
    [SerializeField]
    private GameObject [] Team_List_A_Element_V = new GameObject[gameflowmultiplayer.nrplayersperteam];
    [SerializeField]
    private GameObject[] Team_List_B_Element_V = new GameObject[gameflowmultiplayer.nrplayersperteam];
    [SerializeField]
    private Collider[] _deskDaggerCollisions = null;
    [SerializeField]
    private GameObject _descDagger = null;
    [SerializeField]
    private Transform _daggerPos = null;
    [SerializeField]
    private float _minDaggerDistance = 0.2f;
    [SerializeField]
    private float _maxDaggerDistance = 1f;
    [SerializeField]
    private bool _useAutomaticDaggerInHands = false;
    [SerializeField]
    private GameObject[] _objectsToEnableWhenDaggerInHand = null;
    [SerializeField]
    private GameObject _popupTeamValidation = null;
    [SerializeField]
    private GameObject[] _objectsToDisableWhenValidationPopupAppear = null;
    [SerializeField]
    private TextMeshProUGUI _teamCounterA = null;
    [SerializeField]
    private TextMeshProUGUI _teamCounterB = null;
    [SerializeField]
    private Button _teamButtonA = null;
    [SerializeField]
    private Button _teamButtonB = null;
    [SerializeField]
    private Button _validateButton = null;

    private GameObject Team_List_A;
    private GameObject Team_List_B;
    private TextMeshProUGUI [] Team_List_A_Element_text = new TextMeshProUGUI[gameflowmultiplayer.nrplayersperteam];
    private TextMeshProUGUI[] Team_List_B_Element_text = new TextMeshProUGUI[gameflowmultiplayer.nrplayersperteam];
    private TextMeshProUGUI [] Team_List_A_Element_text_V = new TextMeshProUGUI[gameflowmultiplayer.nrplayersperteam];
    private TextMeshProUGUI[] Team_List_B_Element_text_V = new TextMeshProUGUI[gameflowmultiplayer.nrplayersperteam];
    private int validcounter = 0;
    private bool buttonpressed = false;
    private Player.WeaponType _weapon = Player.WeaponType.None;
    private Vector3 _daggerStartPosition = Vector3.zero;
    private Quaternion _daggerStartRotation = Quaternion.identity;
    private Transform _daggerInHand = null;
    private Vector3[] _lastDaggerInHandPos = new Vector3[3];
    private float[] _lastDaggerInHandTime = new float[3];
    private Vector3 _currentDaggerVelocity = Vector3.zero;
    private Player.WeaponPlace _lastUsedHand = Player.WeaponPlace.RightHand;
    private bool _showPopupValidation = false;

    private void Awake()
    {
        left_on.SetActive(false);
        right_on.SetActive(false);

        left_commandtext.SetTextId(jointext);
        right_commandtext.SetTextId(jointext);

        Team_List_A = gameObject.FindInChildren("Team_List_A");
        Team_List_B = gameObject.FindInChildren("Team_List_B");
        if (Team_List_A_Element.Length > 0 && Team_List_A_Element[0] == null)
            Team_List_A_Element[0] = Team_List_A.FindInChildren("List_Line_InfosDisplay_Filled");
        if (Team_List_B_Element.Length > 0 && Team_List_B_Element[0] == null)
            Team_List_B_Element[0] = Team_List_B.FindInChildren("List_Line_InfosDisplay_Filled");

        // Y = -20 -80 -140
        for (int i = 1; i < gameflowmultiplayer.nrplayersperteam; i++)
        {
            if (Team_List_A_Element.Length > i && Team_List_A_Element[i] == null)
            {
                Team_List_A_Element[i] = Instantiate(Team_List_A_Element[0]) as GameObject;
                Team_List_A_Element[i].transform.SetParent(Team_List_A_Element[0].transform.parent);
                Team_List_A_Element[i].transform.localScale = Vector3.one;
                Team_List_A_Element[i].transform.localEulerAngles = Vector3.zero;
                Team_List_A_Element[i].transform.localPosition = new Vector3(Team_List_A_Element[0].transform.localPosition.x, Team_List_A_Element[0].transform.localPosition.y - 60 * i, Team_List_A_Element[0].transform.localPosition.z);
            }

            if (Team_List_B_Element.Length > i && Team_List_B_Element[i] == null)
            {
                Team_List_B_Element[i] = Instantiate(Team_List_B_Element[0]) as GameObject;
                Team_List_B_Element[i].transform.SetParent(Team_List_B_Element[0].transform.parent);
                Team_List_B_Element[i].transform.localScale = Vector3.one;
                Team_List_B_Element[i].transform.localEulerAngles = Vector3.zero;
                Team_List_B_Element[i].transform.localPosition = new Vector3(Team_List_B_Element[0].transform.localPosition.x, Team_List_B_Element[0].transform.localPosition.y - 60 * i, Team_List_B_Element[0].transform.localPosition.z);
            }
        }
        for (int i = 0; i < gameflowmultiplayer.nrplayersperteam; i++)
        {
            Team_List_A_Element_text[i] = Team_List_A_Element[i].FindInChildren("List_PlayerName_Txt").GetComponent<TextMeshProUGUI>();
            Team_List_B_Element_text[i] = Team_List_B_Element[i].FindInChildren("List_PlayerName_Txt").GetComponent<TextMeshProUGUI>();
            Team_List_A_Element[i].SetActive(false);
            Team_List_B_Element[i].SetActive(false);
            Team_List_A_Element_text_V[i] = Team_List_A_Element_V[i].FindInChildren("List_PlayerName_Txt").GetComponent<TextMeshProUGUI>();
            Team_List_B_Element_text_V[i] = Team_List_B_Element_V[i].FindInChildren("List_PlayerName_Txt").GetComponent<TextMeshProUGUI>();
            Team_List_A_Element_V[i].SetActive(false);
            Team_List_B_Element_V[i].SetActive(false);
        }
        for (int i = gameflowmultiplayer.nrplayersperteam; i < Team_List_A_Element.Length; i++)
            Team_List_A_Element[i].SetActive(false);
        for (int i = gameflowmultiplayer.nrplayersperteam; i < Team_List_B_Element.Length; i++)
            Team_List_B_Element[i].SetActive(false);
        for (int i = gameflowmultiplayer.nrplayersperteam; i < Team_List_A_Element_V.Length; i++)
            Team_List_A_Element_V[i].SetActive(false);
        for (int i = gameflowmultiplayer.nrplayersperteam; i < Team_List_B_Element_V.Length; i++)
            Team_List_B_Element_V[i].SetActive(false);

        _daggerStartPosition = _daggerPos.position;
        _daggerStartRotation = _daggerPos.localRotation;

        _popupTeamValidation.SetActive(false);

        if (_objectsToEnableDaggers != null)
        {
            foreach (GameObject go in _objectsToEnableDaggers)
                go.SetActive(_enableDaggers);
        }

        _descDagger.SetActive(_enableDaggers);
        _daggerPos?.gameObject.SetActive(_enableDaggers);

        if (InitGameData.instance != null)
		{
            _teamButtonA.gameObject.SetActive(false);
            _teamButtonB.gameObject.SetActive(false);
        }
    }

	private void OnDestroy()
	{
    }

    private void Update()
    {
        if ((gameflowmultiplayer.myself != null) && gameflowmultiplayer.areAllTeamsSelected && !gameflowmultiplayer.myself.teamValidated)
        {
            if (!_showPopupValidation)
            {
                _showPopupValidation = true;
                if (InitGameData.instance != null)
                {
                    GameflowBase.SetMyTeam(GameflowBase.myTeam);
                    gameflowmultiplayer.SetMyValidated(true);
                    if (_validateButton != null)
						_validateButton.onClick.Invoke();
				}
                else
                {
                    _popupTeamValidation.SetActive(true);
                    if (_objectsToDisableWhenValidationPopupAppear != null)
                    {
                        foreach (GameObject go in _objectsToDisableWhenValidationPopupAppear)
                            go.SetActive(false);
                    }
                }
			}
        }

        for (int i = 0; i < GameflowBase.nrplayersperteam; i++)
        {
            if (GameflowBase.teamlistA[i] == -1)
            {
                if (Team_List_A_Element[i].activeInHierarchy)
                    Team_List_A_Element[i].SetActive(false);
                if (Team_List_A_Element_V[i].activeInHierarchy)
                    Team_List_A_Element_V[i].SetActive(false);
            }
            else
            {
                if (!Team_List_A_Element[i].activeInHierarchy)
                    Team_List_A_Element[i].SetActive(true);
                if (!Team_List_A_Element_V[i].activeInHierarchy)
                    Team_List_A_Element_V[i].SetActive(true);
                string name = GameflowBase.piratenames[GameflowBase.teamlistA[i]];
                if (name != Team_List_A_Element_text[i].text)
                    Team_List_A_Element_text[i].text = name;
                if (name != Team_List_A_Element_text_V[i].text)
                    Team_List_A_Element_text_V[i].text = name;
            }

            if (GameflowBase.teamlistB[i] == -1)
            {
                if (Team_List_B_Element[i].activeInHierarchy)
                    Team_List_B_Element[i].SetActive(false);
                if (Team_List_B_Element_V[i].activeInHierarchy)
                    Team_List_B_Element_V[i].SetActive(false);
            }
            else
            {
                if (!Team_List_B_Element[i].activeInHierarchy)
                    Team_List_B_Element[i].SetActive(true);
                if (!Team_List_B_Element_V[i].activeInHierarchy)
                    Team_List_B_Element_V[i].SetActive(true);
                string name = GameflowBase.piratenames[GameflowBase.teamlistB[i]];
                if (name != Team_List_B_Element_text[i].text)
                    Team_List_B_Element_text[i].text = name;
                if (name != Team_List_B_Element_text_V[i].text)
                    Team_List_B_Element_text_V[i].text = name;
            }
        }

        if (_enableDaggers)
        {

            if ((gameflowmultiplayer.myself != null) && (gameflowmultiplayer.myself.teamValidated))
            {
                bool exists = false;
                for (int i = 0; i < GameflowBase.nrplayersperteam; i++)
                {
                    if (GameflowBase.teamlistA[i] == PhotonNetworkController.GetPlayerId())
                        exists = true;
                    if (GameflowBase.teamlistB[i] == PhotonNetworkController.GetPlayerId())
                        exists = true;
                }
                if (!exists)
                {
                    validcounter++;
                    if (validcounter > 100)
                    {
                        if ((left_on.activeInHierarchy) || (right_on.activeInHierarchy))
                            gameflowmultiplayer.SetMyValidated(false);
                        left_off.SetActive(true);
                        left_on.SetActive(false);
                        right_off.SetActive(true);
                        right_on.SetActive(false);
                    }
                }
            }
            else
            {
                validcounter = 0;
            }

            
            if (left_forceshow != null)
            {
                if ((!left_forceshow.activeInHierarchy) && (!left_off.activeInHierarchy))
                    ForceStartButtonSettings(false, false, 0);
            }
            if (right_forceshow != null)
            {
                if ((!right_forceshow.activeInHierarchy) && (!right_off.activeInHierarchy))
                    ForceStartButtonSettings(false, false, 0);
            }
            if (_deskDaggerCollisions != null)
            {
                bool hasDaggerInHands = false;
                if (Player.myplayer.HasDaggerInRightHand())
                {
                    hasDaggerInHands = true;
                    SetDaggerInHand(Player.myplayer.GetAttachObjectOfPlace(Player.WeaponPlace.RightHand).transform);
                    _lastUsedHand = Player.WeaponPlace.RightHand;
                }
                if (Player.myplayer.HasDaggerInLeftHand())
                {
                    hasDaggerInHands = true;
                    SetDaggerInHand(Player.myplayer.GetAttachObjectOfPlace(Player.WeaponPlace.LeftHand).transform);
                    _lastUsedHand = Player.WeaponPlace.LeftHand;
                }

                if (!hasDaggerInHands)
                    _daggerInHand = null;

                foreach (Collider col in _deskDaggerCollisions)
                    col.enabled = hasDaggerInHands;
            }

            if (_daggerInHand != null)
            {
                int velocityCount = _lastDaggerInHandPos.Length - 1;
                // Shift old positions
                for (int i = 0; i < velocityCount; ++i)
                {
                    _lastDaggerInHandPos[i + 1] = _lastDaggerInHandPos[i];
                    _lastDaggerInHandTime[i + 1] = _lastDaggerInHandTime[i];
                }
                _lastDaggerInHandPos[0] = _daggerInHand.position;
                _lastDaggerInHandTime[0] = Time.time;
                Vector3 cumulatedDiff = Vector3.zero;
                for (int i = 0; i < velocityCount; ++i)
                {
                    float deltaTime = _lastDaggerInHandTime[i] - _lastDaggerInHandTime[i + 1];
                    if (deltaTime > 0f)
                        cumulatedDiff += (_lastDaggerInHandPos[i] - _lastDaggerInHandPos[i + 1]) / deltaTime;
                }
                _currentDaggerVelocity = cumulatedDiff / velocityCount;
            }

            if (_useAutomaticDaggerInHands && _daggerPos != null && gameflowmultiplayer.myself != null)
            {
                if (gameflowmultiplayer.areAllTeamsSelected)
                {
                    if (_weapon != Player.WeaponType.None)
                        RestoreWeapon(Player.myplayer.HasDaggerInLeftHand());
                    if (Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.RightHand) == Player.WeaponType.Dagger)
                    {
                        Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.RightHand);
                        _daggerInHand = null;
                    }
                    else if (Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.LeftHand) == Player.WeaponType.Dagger)
                    {
                        Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.LeftHand);
                        _daggerInHand = null;
                    }
                    _daggerPos.gameObject.SetActive(false);
                    _descDagger.gameObject.SetActive(false);
                    if (left_forceshow != null)
                        left_forceshow.GetAddComponent<attachobject>().enabled = false;
                    if (right_forceshow != null)
                        right_forceshow.GetAddComponent<attachobject>().enabled = false;
                }
                else
                {
                    Vector3 playerPos = Player.myplayer.GetFootPos();
                    playerPos.y = _daggerStartPosition.y;
                    float squareDistance = Vector3.SqrMagnitude(_daggerStartPosition - playerPos);

                    if (_daggerPos.gameObject.activeSelf)
                    {
                        bool isInsideDistance = squareDistance < _minDaggerDistance * _minDaggerDistance;
                        if (isInsideDistance)
                        {
                            _weapon = Player.myplayer.GetWeaponAtPlace(_lastUsedHand);
                            if (_weapon != Player.WeaponType.None)
                            {
                                if (Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.RightBelt) == Player.WeaponType.None)
                                    Player.myplayer.SetWeaponInPlace(_weapon, Player.WeaponPlace.RightBelt);
                                else if (Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.LeftBelt) == Player.WeaponType.None)
                                    Player.myplayer.SetWeaponInPlace(_weapon, Player.WeaponPlace.LeftBelt);
                            }
                            Player.myplayer.SetWeaponInPlace(Player.WeaponType.Dagger, _lastUsedHand);
                            SetDaggerInHand(Player.myplayer.GetAttachObjectOfPlace(_lastUsedHand).transform);

                            _daggerPos.gameObject.SetActive(false);
                            _descDagger.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        bool isOutOfMaxDistance = squareDistance > _maxDaggerDistance * _maxDaggerDistance;
                        if (isOutOfMaxDistance)
                        {
                            _daggerPos.gameObject.SetActive(true);
                            _descDagger.gameObject.SetActive(false);
                            _daggerPos.position = _daggerStartPosition;
                            _daggerPos.localRotation = _daggerStartRotation;
                            bool hasDaggerInLeftHand = Player.myplayer.HasDaggerInLeftHand();
                            _lastUsedHand = hasDaggerInLeftHand ? Player.WeaponPlace.LeftHand : Player.WeaponPlace.RightHand;
                            RestoreWeapon(hasDaggerInLeftHand);
                        }
                    }
                }
            }
        }

        int countTeamA = GameflowBase.GetActorCountInTeam(0);
        int countTeamB = GameflowBase.GetActorCountInTeam(1);

        int maxByTeam = GameflowBase.nrplayersperteam;
#if USE_STANDALONE
        if (PhotonNetworkController.soloMode)
            maxByTeam = 1;
        else
            maxByTeam = 2;
#endif

        if (_teamCounterA != null)
            _teamCounterA.text = countTeamA + "/" + maxByTeam;
        if (_teamCounterB != null)
            _teamCounterB.text = countTeamB + "/" + maxByTeam;

        if (_teamButtonA != null)
            _teamButtonA.interactable = countTeamA < maxByTeam;
        if (_teamButtonB != null)
            _teamButtonB.interactable = countTeamB < maxByTeam;

        if (countTeamA == countTeamB && countTeamA == maxByTeam && _popupTeamValidation.activeSelf)
		{
            if (_validateButton != null)
                _validateButton.onClick.Invoke();
        }
    }

    private void RestoreWeapon(bool hasDaggerInLeftHand)
	{
        if (_weapon != Player.WeaponType.None)
        {
            if (Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.RightBelt) == _weapon)
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.RightBelt);
            else if (Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.LeftBelt) == _weapon)
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.LeftBelt);

            if (hasDaggerInLeftHand)
                Player.myplayer.SetWeaponInPlace(_weapon, Player.WeaponPlace.LeftHand);
            else
                Player.myplayer.SetWeaponInPlace(_weapon, Player.WeaponPlace.RightHand);
            _weapon = Player.WeaponType.None;
        }
        else
        {
            if (hasDaggerInLeftHand)
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.LeftHand);
            else
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.RightHand);
        }
        _daggerInHand = null;
    }

    void SetDaggerInHand(Transform trDagger)
	{
        if (_daggerInHand == null && trDagger != null)
		{
            if (_objectsToEnableWhenDaggerInHand != null)
            {
                foreach (GameObject go in _objectsToEnableWhenDaggerInHand)
                    go.SetActive(true);
            }
        }
        _daggerInHand = trDagger;
    }

    void ForceStartButtonSettings(bool left, bool right, int team)
    {
        if (gameflowmultiplayer.gameplayrunning)
            return;
        left_off.SetActive(!left);
        left_on.SetActive(left);
        right_off.SetActive(!right);
        right_on.SetActive(right);
        gameflowmultiplayer.SetMyTeam(team, true);

        if ((left) && (left_forceshow != null))
        {
            left_forceshow.SetActive(true);
            left_commandtext.SetTextId(readytext);
            left_commandboard.color = readycolor;
            right_commandboard.color = readyothercolor;
        }
        if ((right) && (right_forceshow != null))
        {
            right_forceshow.SetActive(true);
            right_commandtext.SetTextId(readytext);
            left_commandboard.color = readyothercolor;
            right_commandboard.color = readycolor;
        }

        if (left || right)
        {
            //gameflowmultiplayer.SetMyValidated(true);
        }
        else
        {
            for (int i = 0; i < GameflowBase.nrplayersperteam; i++)
            {
                if (GameflowBase.teamlistA[i] == PhotonNetworkController.GetPlayerId())
                    GameflowBase.teamlistA[i] = -1;
                if (GameflowBase.teamlistB[i] == PhotonNetworkController.GetPlayerId())
                    GameflowBase.teamlistB[i] = -1;
            }
            //gameflowmultiplayer.SetMyValidated(false);
            left_commandtext.SetTextId(jointext);
            right_commandtext.SetTextId(jointext);

            left_commandboard.color = joincolor;
            right_commandboard.color = joincolor;
        }
    }

    public void Released()
    {
        buttonpressed = false;
    }

    public void Touched(GameObject obj)
    {
        if (buttonpressed) return;
        buttonpressed = true;
        
        if (obj == left_on)
        {
            ForceStartButtonSettings(false, false, 0);
            return;
        }
        if (obj == right_on)
        {
            ForceStartButtonSettings(false, false, 0);
            return;
        }

        if (obj == left_off)
        {
            if (!HasEnoughVelocity())
            {
                Debug.Log("NOT ENOUGH VELOCITY!");
                return;
            }
            ForceStartButtonSettings(true, false, 0); 
            return;
        }
        if (obj == right_off)
        {
            if (!HasEnoughVelocity())
            {
                Debug.Log("NOT ENOUGH VELOCITY!");
                return;
            }
            ForceStartButtonSettings(false, true, 1);
            return;
        }
    }

    private bool HasEnoughVelocity()
	{
        bool isOk = _currentDaggerVelocity.sqrMagnitude > 0.5f && _currentDaggerVelocity.y < 0f;
        return isOk;

    }
}
