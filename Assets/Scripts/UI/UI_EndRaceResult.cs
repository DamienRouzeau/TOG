using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;
using Photon.Pun;

public class UI_EndRaceResult : MonoBehaviour
{
    #region EnumArrays

    [System.Serializable] public class ScoreValueTypeCoef : RREnumArray<UI_EndRacePlayer.ValueType, float> { }

    #endregion

    public class EndPlayerData
    {
        public string sName;
        public int playerId;
        public string sTitle;
        public double treasures;
        public double kills;
        public double deaths;
        public double tourret;
        public double boatsKills;
        public double obstacles;
        public double mermaids;
        public double monsters;
        public double skulls;
        public int team;
        public int skinId;
        public bool bReady;
        public double score;
        public double malus;
        public double distance;
        public double drone;
        public double mine;
        public double superDrone;
        public double megaDroid;
        public double plasmaBomb;
        public double bomber;
        public double conveyor;
        public double droneUltra;
        public double archives;
        public double scientists;
        public double gameTime;

        private ScoreValueTypeCoef _valueTypeCoef = null;

        public long pirateIdx => _pirateIndex;
        private long _pirateIndex = -1;

        public string statsName => _statsName;
        private string _statsName;

        private float _startingScore = 0;

        public EndPlayerData( int nTeamId, long nPirateIndex, ScoreValueTypeCoef valueTypeCoef, float startingScore)
        {
            sName = GameflowBase.piratenames[nPirateIndex];
            playerId = (int)nPirateIndex;
            _statsName = $"{sName}_{nPirateIndex}";
            skinId = gamesettings_player.myself.GetSkinIndexFromName(GameflowBase.pirateskins[nPirateIndex]);
            team = nTeamId;
            distance = gameflowmultiplayer.boatDistances[team];
            _pirateIndex = nPirateIndex;
            _valueTypeCoef = valueTypeCoef;
            _startingScore = startingScore;
            CheckStats();
        }

        public void SetupFromDic(Dictionary<string, object> dic)
		{
            team = (int)(long)dic["team"];
            score = (int)(long)dic["score"];
            malus = (int)(long)dic["malus"];
            treasures = (int)(long)dic["treasures"];
            kills = (int)(long)dic["kills"];
            deaths = (int)(long)dic["deaths"];
            monsters = (int)(long)dic["monsters"];
            skinId = (int)(long)dic["skinId"];            
            if (multiplayerlobby.product == multiplayerlobby.Product.TOG)
            {
                obstacles = (int)(long)dic["obstacles"];
                boatsKills = (int)(long)dic["boatsKills"];
                tourret = (int)(long)dic["tourret"];
                mermaids = (int)(long)dic["mermaids"];
                skulls = (int)(long)dic["skulls"];
                distance = (double)Helper.GetDictionaryValue(dic, "distance");
            }
            else if (multiplayerlobby.product == multiplayerlobby.Product.KDK)
            {
                drone = (int)(long)dic["drones"];
                mine = (int)(long)dic["mines"];
                superDrone = (int)(long)dic["superDrones"];
                megaDroid = (int)(long)dic["megaDroids"];
                plasmaBomb = (int)(long)dic["plasmaBombs"];
                bomber = (int)(long)dic["bombers"];
                conveyor = (int)(long)dic["conveyors"];
                droneUltra = (int)(long)dic["droneUltras"];
            }
            else if (multiplayerlobby.product == multiplayerlobby.Product.BOD)
			{
                archives = (int)(long)dic["archives"];
                scientists = (int)(long)dic["scientists"];
                gameTime = (double)Helper.GetDictionaryValue(dic, "gameTime");
            }
            sName = (string)dic["name"];
            playerId = (int)(long)dic["playerId"];
            sTitle = (string)dic["title"];
            _statsName = $"{sName}_{playerId}";
            bReady = true;
        }

        public bool CheckStats()
        {
            Dictionary<string, double> stats = RRStats.RRStatsManager.instance.GetStats(_statsName);
            if( stats == null ) // no stats yet
            {
                return false;
            }
            treasures = stats.ContainsKey(gamesettings.STAT_POINTS) ? stats[gamesettings.STAT_POINTS] : 0;
            kills = stats.ContainsKey(gamesettings.STAT_KILLS) ? stats[gamesettings.STAT_KILLS] : 0;
            deaths = stats.ContainsKey(gamesettings.STAT_DEATH) ? stats[gamesettings.STAT_DEATH] : 0;
            obstacles = stats.ContainsKey(gamesettings.STAT_LAVA) ? stats[gamesettings.STAT_LAVA] : 0;
            //obstacles += stats.ContainsKey(gamesettings.STAT_TURRETS) ? stats[gamesettings.STAT_TURRETS] : 0; // don't add STAT_TURRETS 2 times
            tourret = stats.ContainsKey(gamesettings.STAT_TURRETS) ? stats[gamesettings.STAT_TURRETS] : 0;
            boatsKills = stats.ContainsKey(gamesettings.STAT_BOATSKILLER) ? stats[gamesettings.STAT_BOATSKILLER] : 0;
            mermaids = stats.ContainsKey(gamesettings.STAT_MERMAIDS) ? stats[gamesettings.STAT_MERMAIDS] : 0;
            monsters = stats.ContainsKey(gamesettings.STAT_MONSTERS) ? stats[gamesettings.STAT_MONSTERS] : 0;
            skulls = stats.ContainsKey(gamesettings.STAT_SKULLS) ? stats[gamesettings.STAT_SKULLS] : 0;
            archives = stats.ContainsKey(gamesettings.STAT_ARCHIVES) ? stats[gamesettings.STAT_ARCHIVES] : 0;
            scientists = stats.ContainsKey(gamesettings.STAT_SCIENTISTS) ? stats[gamesettings.STAT_SCIENTISTS] : 0;

            drone = stats.ContainsKey(Health.HealtObjectType.drone.ToString()) ? stats[Health.HealtObjectType.drone.ToString()] : 0;
            mine = stats.ContainsKey(Health.HealtObjectType.mine.ToString()) ? stats[Health.HealtObjectType.mine.ToString()] : 0;
            superDrone = stats.ContainsKey(Health.HealtObjectType.superDrone.ToString()) ? stats[Health.HealtObjectType.superDrone.ToString()] : 0;
            megaDroid = stats.ContainsKey(Health.HealtObjectType.megaDroid.ToString()) ? stats[Health.HealtObjectType.megaDroid.ToString()] : 0;
            plasmaBomb = stats.ContainsKey(Health.HealtObjectType.plasmaBomb.ToString()) ? stats[Health.HealtObjectType.plasmaBomb.ToString()] : 0;
            bomber = stats.ContainsKey(Health.HealtObjectType.bomber.ToString()) ? stats[Health.HealtObjectType.bomber.ToString()] : 0;
            conveyor = stats.ContainsKey(Health.HealtObjectType.conveyor.ToString()) ? stats[Health.HealtObjectType.conveyor.ToString()] : 0;
            droneUltra = stats.ContainsKey(Health.HealtObjectType.droneUltra.ToString()) ? stats[Health.HealtObjectType.droneUltra.ToString()] : 0;

            ComputeScore(_valueTypeCoef);
            bReady = true;
            Debug.Log($"[END_RESULT] player {sName} skinId {skinId} team {team} is ready! score {score}");
            return true;
        }

        public double ComputeScore(ScoreValueTypeCoef valueTypeCoef)
		{
            score = _startingScore;
            malus = -deaths * valueTypeCoef[UI_EndRacePlayer.ValueType.Deaths];
            if (valueTypeCoef != null)
            {
                score += treasures * valueTypeCoef[UI_EndRacePlayer.ValueType.Treasures];
                score += kills * valueTypeCoef[UI_EndRacePlayer.ValueType.Kills];
                score += deaths * valueTypeCoef[UI_EndRacePlayer.ValueType.Deaths];
                score += obstacles * valueTypeCoef[UI_EndRacePlayer.ValueType.Obstacles];
                score += tourret * valueTypeCoef[UI_EndRacePlayer.ValueType.Turrets];
                score += boatsKills * valueTypeCoef[UI_EndRacePlayer.ValueType.BoatKills];
                score += mermaids * valueTypeCoef[UI_EndRacePlayer.ValueType.Mermaids];
                score += monsters * valueTypeCoef[UI_EndRacePlayer.ValueType.Monsters];
                score += skulls * valueTypeCoef[UI_EndRacePlayer.ValueType.Skulls];
                score += drone * valueTypeCoef[UI_EndRacePlayer.ValueType.Drone];
                score += mine * valueTypeCoef[UI_EndRacePlayer.ValueType.Mine];
                score += superDrone * valueTypeCoef[UI_EndRacePlayer.ValueType.SuperDrone];
                score += megaDroid * valueTypeCoef[UI_EndRacePlayer.ValueType.MegaDroid];
                score += plasmaBomb * valueTypeCoef[UI_EndRacePlayer.ValueType.PlasmaBomb];
                score += bomber * valueTypeCoef[UI_EndRacePlayer.ValueType.Bomber];
                score += conveyor * valueTypeCoef[UI_EndRacePlayer.ValueType.Conveyor];
                score += droneUltra * valueTypeCoef[UI_EndRacePlayer.ValueType.DroneUltra];
                score += archives * valueTypeCoef[UI_EndRacePlayer.ValueType.Archives];
                score += scientists * valueTypeCoef[UI_EndRacePlayer.ValueType.Scientists];
            }
            return score;
        }
    }

    public class EndPlayerComparer : IComparer<EndPlayerData>
    {
        public int Compare(EndPlayerData x, EndPlayerData y)
        {
            return (int)(y.score - x.score);
        }
    }

    [System.Serializable]
    public class TeamPanel
    {
        public RectTransform panelRoot;
        public TMP_Text score;
        public TMP_Text result;
        public RectTransform playersRoot;
        public GameObject distanceRoot;
        public TextMeshProUGUI distanceValue;
    }

    [System.Serializable]
    public class LayoutData
    {
        public Vector3 rootPos;
        public float panelX;
        public float panelY;
        public float panelScale;
        public Vector2 gridSpace;
        public int lineCount;
        public float cameraSize;
    }

    public static UI_EndRaceResult myself = null;

    public LayoutData[] layoutDatas = new LayoutData[4];
    public TeamPanel[] teamsPanel;
    public UI_EndRacePlayer racePlayerPrefab;
    public UI_EndRacePlayer raceLosePlayerPrefab;
    public TMP_Text totalTreasurePercent;
    public Camera m_renderCamera;
    public int resWidth = 1920, resHeight = 1080;

    public bool playCaptainAudioAtInitScreen = true;
    [SerializeField]
    private GameObject _buttonsGroup = null;
    [SerializeField]
    private Button _nextLevelButton = null;
    [SerializeField]
    private Button _replayLevelButton = null;
    [SerializeField]
    private Button _continueLevelButton = null;
    [SerializeField]
    private Button _quitLevelButton = null;
    [SerializeField]
    private GameObject _otherTeam = null;
    [SerializeField]
    private GameObject _waitMasterMessage = null;
    [SerializeField]
    private Image _backgroundImage = null;
    [SerializeField]
    private UI_Skulls _skulls = null;
    [SerializeField]
    private TextMeshProUGUI _waveCounter = null;
    [SerializeField]
    private TextMeshProUGUI _continueBtnText = null;
    [SerializeField]
    private TextMeshProUGUI _totalGold = null;
    [SerializeField]
    private float _baseScore = 0f;

    [Header("Mission")]
    [SerializeField]
    private TextMeshProUGUI _missionCompletion = null;
    [SerializeField]
    private TextMeshProUGUI _missionDuration = null;

    private List<EndPlayerData>[] playerList;
    private bool _allStatsOk = false;
    private bool _isStopGameCounterCalled = false;
    private bool _resultWin = false;
    private float _elapsedTime = 0f;

    // Start is called before the first frame update
    void Awake()
    {
        myself = this;

		if (GameflowBase.instance == null)
			return;

        InitData();
    }

	private void OnDestroy()
	{
        playerList = null;
    }

	private void Update()
    {
        if (GameflowBase.instance == null)
            return;

        if ( !_allStatsOk)
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > 1f)
            {
                _allStatsOk = CheckAllStatsOk();
                if (!_allStatsOk && _elapsedTime > 3f)
                    _allStatsOk = true;
                if (_allStatsOk)
                    InitScreen();
            }
        }
    }

    public void SetResult(bool win)
	{
        _resultWin = win;
    }

    public void InitData(bool inLauncher = false, string levelName = null, int numWave = 0, int teamGold = 0)
	{
        playerList = new List<EndPlayerData>[2];
        for (int i = 0; i < 2; i++)
        {
            playerList[i] = new List<EndPlayerData>();
        }

        ScoreValueTypeCoef scoreCoef = inLauncher ? null : gamesettings_general.myself.scoreValueTypeCoef;
        int teamListACount = GameflowBase.teamlistA.Length;
        int teamListBCount = GameflowBase.teamlistB.Length;

        Debug.Log($"[END_RESULT] teamListACount {teamListACount} - teamListBCount {teamListBCount}");

        // team A
        for (int i = 0; i < teamListACount; i++)
        {
            long numPlayer = GameflowBase.teamlistA[i];
            Debug.Log($"[END_RESULT] teamListA idx {i} numPlayer {numPlayer}");
            if (numPlayer == -1)
            {
                // AIBoat
                if (i == 0 && _otherTeam != null)
                    _otherTeam.SetActive(false);
                continue;
            }
            EndPlayerData endPlayerData = new EndPlayerData(0, numPlayer, scoreCoef, _baseScore);
            playerList[0].Add(endPlayerData);
        }

        // team B
        for (int i = 0; i < teamListBCount; i++)
        {
            long numPlayer = GameflowBase.teamlistB[i];
            Debug.Log($"[END_RESULT] teamListB idx {i} numPlayer {numPlayer}");
            if (numPlayer == -1)
            {
                // AIBoat
                if (i == 0 && _otherTeam != null)
                    _otherTeam.SetActive(false);
                continue;
            }

            EndPlayerData endPlayerData = new EndPlayerData(1, numPlayer, scoreCoef, _baseScore);
            playerList[1].Add(endPlayerData);
        }

        _allStatsOk = false;

        _replayLevelButton.gameObject.SetActive(false);
        _continueLevelButton?.gameObject.SetActive(false);
        _nextLevelButton.gameObject.SetActive(false);
        _quitLevelButton.gameObject.SetActive(false);
        _waitMasterMessage.SetActive(false);

        if (_backgroundImage != null)
        {
            if (multiplayerlobby.product == multiplayerlobby.Product.TOG)
            {
                if (levelName == null)
                    levelName = gameflowmultiplayer.currentLevelName;
            }
            if (multiplayerlobby.product == multiplayerlobby.Product.KDK)
            {
                if (levelName == null)
                    levelName = "KDK";
            }
            if (multiplayerlobby.product == multiplayerlobby.Product.BOD)
            {
                if (levelName == null)
                    levelName = "BOD";
            }

            _backgroundImage.sprite = gamesettings_endscreen.myself.GetBackgroundFromLevelName(levelName);
            _backgroundImage.gameObject.SetActive(false);
        }

        if (multiplayerlobby.product == multiplayerlobby.Product.KDK)
        {
            int currentWave = inLauncher ? numWave : TowerDefManager.myself.currentWave;
            if (_waveCounter != null)
            {   
                _waveCounter.text = currentWave.ToString();
            }
            if (_totalGold != null)
            {
                int gold = inLauncher ? teamGold : Player.myplayer.teamGold;
                _totalGold.text = gold.ToString();
            }
            if (_continueBtnText != null)
            {
                if (TowerDefManager.keepLastWaveOnContinue)
                {
                    _continueBtnText.text = RRLib.RRLanguageManager.instance.GetString("str_btn_restartlevel");
                }
                else
                {
                    string wave = ((currentWave + 1) / 2).ToString();
                    _continueBtnText.text = RRLib.RRLanguageManager.instance.GetString("str_kdk_ui_restartwave").Replace("%WAVE%", wave);
                }
            }
        }

#if USE_DEMO_MODE
        if (!inLauncher)
        {
            int count = GameLoader.GetDemoCounter();
            GameLoader.SetDemoCounter(count - 1);
        }
#endif

        if (_skulls != null)
		{
            _skulls.gameObject.SetActive(false);
        }
        
    }

    private void ComputeMissionData()
	{
        if (multiplayerlobby.product == multiplayerlobby.Product.BOD)
        {
            List<string> playerNames = new List<string>();
            for (int i = 0; i < playerList.Length; i++)
            {
                for (int j = 0; j < playerList[i].Count; j++)
                {
                    if (!string.IsNullOrEmpty(playerList[i][j].statsName))
                        playerNames.Add(playerList[i][j].statsName);
                }
            }

            int countDNA = 0;
            foreach (string name in playerNames)
            {
                Dictionary<string, double> stats = RRStats.RRStatsManager.instance.GetStats(name);
                if (stats != null)
                {
                    countDNA += stats.ContainsKey(gamesettings.STAT_POINTS) ? (int)stats[gamesettings.STAT_POINTS] : 0;
                }
            }

            int maxDNA = CollectableCounter.GetCounterFromType(CollectableCounter.CollectableType.DNA);
            Debug.Log($"DNA {countDNA}/{maxDNA}");
            //GameflowBOD.completion = 50 + Mathf.RoundToInt(50f * (float)countDNA / (float)maxDNA);
            SetMissionData(GameflowBOD.completion, TowerDefManager.myself.gameTime);
        }
    }

    public void SetMissionData(float completion, float duration)
    {
        if (_missionCompletion != null)
            _missionCompletion.text = $"{GameflowBOD.completion}% {RRLib.RRLanguageManager.instance.GetString("str_queststatedone")}";
        if (_missionDuration != null)
        {
            int minutes = Mathf.FloorToInt(duration) / 60;
            int seconds = Mathf.FloorToInt(duration) % 60;
            int ms = Mathf.FloorToInt(duration * 100f) % 100;
            _missionDuration.text = $"{minutes:00}:{seconds:00}:{ms:00}";
        }
    }

    public EndPlayerData GetPlayerData(int numPlayer)
	{
        for( int i = 0; i < playerList.Length; ++i)
		{
            foreach (EndPlayerData data in playerList[i])
			{
                if (data.pirateIdx == numPlayer)
                    return data;
			}
		}
        return null;
	}

    public void ApplyChoiceButton(string levelName)
	{
        if (!_isStopGameCounterCalled)
        {
            CancelInvoke("StopGameCounter");
            StopGameCounter();
        }        
        if (!string.IsNullOrEmpty(levelName))
		{
            LaunchSceneASync launcher = GetComponent<LaunchSceneASync>();
            if (launcher != null)
            {
                launcher.sceneName = levelName;
                launcher.LaunchScene();
            }
            else
			{
                Debug.LogError("Missing LaunchSceneASync component!");
			}
        }
	}

    private bool CheckAllStatsOk()
    {
        _allStatsOk = true;

        if( playerList==null )
        {
            return false;
        }

        for( int i=0; i<playerList.Length; i++ )
        {
            for( int j=0; j<playerList[i].Count; j++ )
            {
                if( !playerList[i][j].bReady)
                {
                    _allStatsOk = playerList[i][j].CheckStats() && _allStatsOk;
                }
            }
        }

        // TODO Compute Title for each player/Team
        return _allStatsOk;
    }

    public void InitScreen(bool inLauncher = false, int maxGold = 0)
    {
        if (!inLauncher)
        {
            // Compute Title for each player
            ComputePlayerTitle();
            ComputeMissionData();
        }

        int nTeam = GameflowBase.myTeam;
        int[] teamPositions = new int[] { 0, 0 };
        double[] teamScores = new double[] { 0, 0 };
        for (int i = 0; i < playerList.Length; i++)
        {
            teamPositions[i] = gameflowmultiplayer.GetTeamPosition(i);
            playerList[i].Sort(new EndPlayerComparer());
            for (int j = 0; j < playerList[i].Count; j++)
            {
                EndPlayerData data = playerList[i][j];
                if (!inLauncher)
                    apicalls.myself?.PrepareJson(data.pirateIdx, data, teamPositions[i] == 0);
                teamScores[i] += data.treasures;
            }
        }

#if USE_STANDALONE
        // Progression
        if (GameflowBase.allFlows.Length < 2 && _skulls != null)
        {
            if (_skulls != null)
                _skulls.gameObject.SetActive(true);

            string levelId = gamesettings_general.myself.levelSettings.GetLevelIdFromListIndex(gameflowmultiplayer.levelListId, gameflowmultiplayer.levelIndex);
            Debug.Assert(levelId != null);
            SaveManager.ProgressionLevelData level = SaveManager.myself.profile.progression.GetLevelFromId(levelId);
            Debug.Assert(level != null);

            bool win = teamPositions[nTeam] == 0;

            // Skulls animation
            for (int i = 0; i < level.unlockedSkulls.Length; ++i)
            {
                bool justUnlocked = gameflowmultiplayer.unlockedSkullsInRace[i];
                bool unlocked = level.unlockedSkulls[i] && (win || !justUnlocked);
                if (justUnlocked)
                    _skulls.SetSkullDisplayWithDelay(i, unlocked, true, i * 0.5f + 0.5f);
                else
                    _skulls.SetSkullDisplay(i, unlocked, false);
            }

            if (win)
			{
                level.bestScore = Mathf.Max(level.bestScore, (int)teamScores[nTeam]);
                SaveManager.myself.profile.coins += gameflowmultiplayer.GetBoat(nTeam).wonGold;
                SaveManager.myself.Save();
            }
            else
			{
                SaveManager.myself.Load();
            }
        }
        else if (GameflowBase.allFlows.Length >= 2)
		{
            if (_skulls != null)
                _skulls.gameObject.SetActive(false);

            bool win = true; // teamPositions[nTeam] == 0; // We always win in multi
            if (win)
            {
                Dictionary<string, double> stats = RRStats.RRStatsManager.instance.GetStats(GameflowBase.myStatsName);
                if (stats != null)
                {
                    int coins = stats.ContainsKey(gamesettings.STAT_POINTS) ? (int)stats[gamesettings.STAT_POINTS] : 0;
                    SaveManager.myself.profile.coins += coins;
                    SaveManager.myself.Save();
                }
            }
            else
            {
                SaveManager.myself.Load();
            }
        }
#endif

        bool onlyOneTeam = false;
        // Determine best team
        int bestTeam = 0;
        if (GameflowBase.GetActorCountInTeam(0) == 0 || GameflowBase.GetActorCountInTeam(1) == 0)
        {
            if (GameflowBase.GetActorCountInTeam(nTeam) == 0)
                nTeam = 1 - nTeam;
            bestTeam = nTeam;
            onlyOneTeam = true;
        }
        else if (teamPositions[0] != 0)
        {
            if (teamPositions[1] == 0)
                bestTeam = 1;
            else if (teamScores[1] > teamScores[0])
                bestTeam = 1;
        }

        if (!inLauncher)
        {
            apicalls.myself?.SendLeaderboard();
            int myTeamPosition = teamPositions[nTeam];
            gameflowmultiplayer.amWinnerTeam = myTeamPosition == 0;
            if (playCaptainAudioAtInitScreen)
            {
                CaptainVoiceOver captainVoiceOver = GameObject.FindObjectOfType<CaptainVoiceOver>();
                if (captainVoiceOver != null)
                {
                    switch (myTeamPosition)
                    {
                        case -1:
                            captainVoiceOver.PlayVoice(CaptainVoiceOver.CaptainVoices.timeout);
                            break;
                        case 0:
                            captainVoiceOver.PlayVoice(CaptainVoiceOver.CaptainVoices.victory);
                            break;
                        case 1:
                            captainVoiceOver.PlayVoice(CaptainVoiceOver.CaptainVoices.second);
                            break;
                    }
                }
            }

            if (gameflowmultiplayer.myCaptain != null)
            {
                Animator animator = gameflowmultiplayer.myCaptain.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger(gameflowmultiplayer.amWinnerTeam ? "Win" : "Lose");
                }
            }
        }

        float totalGold = maxGold;
        if (totalGold <= 0f && RaceManager.myself != null)
            totalGold = (float)RaceManager.myself.totalGold;

        if (multiplayerlobby.product == multiplayerlobby.Product.TOG)
        {
            InitTeam(0, bestTeam, teamScores, totalGold, gameflowmultiplayer.boatDistances[bestTeam], onlyOneTeam);
            if (!onlyOneTeam)
                InitTeam(1, 1 - bestTeam, teamScores, totalGold, gameflowmultiplayer.boatDistances[1 - bestTeam], false);
        }
        else
        {
            InitTeam(0, bestTeam, teamScores, totalGold, 0f, true);
        }

        if (totalTreasurePercent != null)
        {
            double totalScore = teamScores[0] + teamScores[1];
            int percent = Mathf.CeilToInt((float)totalScore * 100f / totalGold);
            percent = Mathf.Min(percent, 99); // never reach 100%
            totalTreasurePercent.text = totalScore + " (" + percent.ToString() + "%)";
        }

        if (!inLauncher)
        {
#if !USE_STANDALONE && !USE_LAUNCHER
            Invoke("TakeScreenShoot", 1f);
#endif
            Invoke("SetupLevelButtons", gamesettings.myself.delayToShowButtonsAtEndRace);
        }
    }

    private void SetupLevelButtons()
	{
        if (PhotonNetworkController.IsMaster())
        {
#if USE_BOD
            _replayLevelButton.gameObject.SetActive(false);
            _nextLevelButton.gameObject.SetActive(false);
            _quitLevelButton.gameObject.SetActive(true);
            _continueLevelButton.gameObject.SetActive(false);
#elif USE_KDK
            bool showNextRace = false;
            if (TowerDefManager.myself != null)
                showNextRace = TowerDefManager.myself.CanShowNextRace();

            if (_resultWin)
			{
                _replayLevelButton.gameObject.SetActive(false);
                _nextLevelButton.gameObject.SetActive(showNextRace);
                _quitLevelButton.gameObject.SetActive(!showNextRace);
                _continueLevelButton.gameObject.SetActive(false);
            }
            else
			{
                int continueMinWave = TowerDefManager.keepLastWaveOnContinue ? 0 : 2;
                float minRemainingTime = TowerDefManager.myself != null ? TowerDefManager.myself.nextRaceMinRemainingTime : 100f;
                bool canPlayAgain = multiplayerlobby.endlessDuration > minRemainingTime;
                _replayLevelButton.gameObject.SetActive(canPlayAgain && !showNextRace);
                _nextLevelButton.gameObject.SetActive(showNextRace);
                _quitLevelButton.gameObject.SetActive(!canPlayAgain);
                _continueLevelButton.gameObject.SetActive(canPlayAgain && TowerDefManager.myself.currentWave > continueMinWave);
            }
#else
#if USE_STANDALONE
            bool showReplayButton = false;
            bool existNextLevel = false;
#else
            string listId = gameflowmultiplayer.levelListId;
            int listIdx = gameflowmultiplayer.levelIndex + 1;
            bool showReplayButton = !apicalls.isDemoGame && multiplayerlobby.canPlayNextRace && gamesettings.myself.showReplayButton;
            bool existNextLevel = gamesettings_general.myself.levelSettings.GetLevelIdFromListIndex(listId, listIdx) != null;
#endif
            _replayLevelButton.gameObject.SetActive(showReplayButton);
            _nextLevelButton.gameObject.SetActive(existNextLevel && multiplayerlobby.canPlayNextRace && gamesettings.myself.showNextButton);
            _quitLevelButton.gameObject.SetActive(true);
#if USE_STANDALONE
            _buttonsGroup.transform.localPosition += Vector3.up * 300f;
#endif
#endif
        }
        else
		{
            _waitMasterMessage.SetActive(true);
#if USE_STANDALONE
            _waitMasterMessage.transform.localPosition += Vector3.up * 300f;
#endif
        }

        Invoke("StopGameCounter", gamesettings_general.myself.delayToStopGameAfterButtonAppearance);
    }

    private void StopGameCounter()
	{
        if (!_isStopGameCounterCalled)
        {
            apicalls.myself?.StopGameCounter();
            _isStopGameCounterCalled = true;
        }
    }

    private void ComputePlayerTitle()
    {
        string[] sStatsArray = new string[]
        {
            gamesettings.STAT_POINTS,
            gamesettings.STAT_CHESTS,
            //gamesettings.STAT_SAILOR,
            //gamesettings.STAT_SKIMMER,
            gamesettings.STAT_TURRETS,
            gamesettings.STAT_LAVA,
            gamesettings.STAT_KILLS,
            gamesettings.STAT_BOATSKILLER,
            //gamesettings.STAT_CANONBAIT,
            gamesettings.STAT_BIRDS,
            gamesettings.STAT_MERMAIDS,
            gamesettings.STAT_MONSTERS,
            gamesettings.STAT_SKULLS,
            gamesettings.STAT_ARCHIVES,
            gamesettings.STAT_SCIENTISTS
        };

        List<string> statsList = new List<string>(sStatsArray);

#if USE_KDK
        statsList.Add(Health.HealtObjectType.drone.ToString());
        statsList.Add(Health.HealtObjectType.mine.ToString());
        statsList.Add(Health.HealtObjectType.superDrone.ToString());
        statsList.Add(Health.HealtObjectType.megaDroid.ToString());
        statsList.Add(Health.HealtObjectType.plasmaBomb.ToString());
        statsList.Add(Health.HealtObjectType.bomber.ToString());
        statsList.Add(Health.HealtObjectType.conveyor.ToString());
        statsList.Add(Health.HealtObjectType.droneUltra.ToString());
#endif

        List<string> playerNames = new List<string>();
        for( int i=0; i<playerList.Length; i++ )
        {
            for (int j = 0; j < playerList[i].Count; j++)
            {
                if (!string.IsNullOrEmpty(playerList[i][j].statsName))
                    playerNames.Add(playerList[i][j].statsName);
            }
        }

        int currentStat = 0;
        while( currentStat < statsList.Count && playerNames.Count > 0 )
        {
            string sPlayer = RRStats.RRStatsManager.instance.GetBestListenerForStatInList(statsList[currentStat], playerNames);
            if( !string.IsNullOrEmpty(sPlayer))
            {
                SetPlayerTitle(sPlayer, statsList[currentStat]);
                playerNames.Remove(sPlayer);
            }
            currentStat++;
        }

        for( int i=0; i<playerNames.Count; i++ )
        {
            SetPlayerTitle(playerNames[i], "default");
        }
    }

    private void SetPlayerTitle( string sPlayer, string sStat )
    {
#if USE_KDK || USE_BOD
        string defaultTitleId = "str_kdk_title_default";
#else
        string defaultTitleId = "str_title_default";
#endif
        string sTitle = "";
        if( sStat == "default")
        {
            sTitle = RRLib.RRLanguageManager.instance.GetString(defaultTitleId);
        }
        else
        {
#if USE_KDK || USE_BOD
            string s = "str_kdk_title_" + sStat + "_0" + UnityEngine.Random.Range(0, 2);
#else
            string s = "str_title_" + sStat + "_0" + UnityEngine.Random.Range(0, 2);
#endif
            sTitle = RRLib.RRLanguageManager.instance.GetString(s);
            if (string.IsNullOrEmpty(sTitle))
                sTitle = RRLib.RRLanguageManager.instance.GetString(defaultTitleId);
        }

        bool bSet = false;
        int nTeamId = 0;
        while (!bSet && nTeamId < playerList.Length)
        {
            int index = 0;
            while (!bSet && index < playerList[nTeamId].Count )
            {
                if(playerList[nTeamId][index].statsName==sPlayer )
                {
                    bSet = true;
                    playerList[nTeamId][index].sTitle = sTitle;
                }
                else
                {
                    index++;
                }
            }
            if( !bSet)
            {
                nTeamId++;
            }
        }
    }

    private void InitTeam( int nTeamIndex,  int nTeamId, double[] teamScores, float totalGold, float distance, bool onlyOneTeam)
    {
        if (nTeamIndex >= teamsPanel.Length)
            return;

        TeamPanel teamPanel = teamsPanel[nTeamIndex];

        int percent = Mathf.CeilToInt((float)teamScores[nTeamId] * 100f / totalGold);
        percent = Mathf.Min(percent, 99); // never reach 100%
        if (teamPanel.score != null)
            teamPanel.score.text = teamScores[nTeamId].ToString() + " ( " + percent.ToString() + " % ) ";
        if (teamPanel.distanceRoot != null)
            teamPanel.distanceRoot.SetActive(distance > 0);
        if (teamPanel.distanceValue != null)
            teamPanel.distanceValue.text = $"{(int)distance}m";

        int teamPosition = gameflowmultiplayer.GetTeamPosition(nTeamId);
        UI_EndRacePlayer playerTeamPrefab = racePlayerPrefab;

        if (teamPosition==0) // win
        {
            string key = distance > 0 ? "str_drawposfirst" : "str_win";
            if (teamPanel.result != null)
                teamPanel.result.text = RRLib.RRLanguageManager.instance.GetString(key);
        }
        else /*if(teamScores[nTeamId] == teamScores[1 - nTeamId]) // draw
        {
            teamsPanel[nTeamIndex].result.text = RRLib.RRLanguageManager.instance.GetString("str_draw");
        }
        else // loose*/
        {
            playerTeamPrefab = raceLosePlayerPrefab;
            string key = distance > 0 ? "str_drawpossecond" : "str_loose";
            if (teamPanel.result != null)
                teamPanel.result.text = RRLib.RRLanguageManager.instance.GetString(key);
        }

        // set players
        if (multiplayerlobby.product == multiplayerlobby.Product.KDK || multiplayerlobby.product == multiplayerlobby.Product.BOD)
        {
            int playerCount = playerList[nTeamId].Count;
            int numLayout = Mathf.Clamp(playerCount - 1, 0, layoutDatas.Length - 1);
            LayoutData data = layoutDatas[numLayout];
            teamsPanel[nTeamIndex].panelRoot.localPosition = data.rootPos;
            RectTransform panelRT = teamsPanel[nTeamIndex].playersRoot;
            panelRT.anchoredPosition = new Vector2(data.panelX, data.panelY);
            panelRT.localScale = Vector3.one * data.panelScale;
            GridLayoutGroup grid = panelRT.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                grid.spacing = data.gridSpace;
                grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                grid.constraintCount = data.lineCount;
            }
            for (int i = 0; i < playerCount; i++)
            {
                UI_EndRacePlayer endRacePlayer = GameObject.Instantiate<UI_EndRacePlayer>(playerTeamPrefab, teamsPanel[nTeamIndex].playersRoot);
                endRacePlayer.Setup(playerList[nTeamId][i]);
            }
        }
        else
        {
            int playerCount = playerList[nTeamId].Count;
            int playerFirsLineCount = GetPlayerCountInFirstLine(playerCount, onlyOneTeam);
            int playerSecondLineCount = playerCount - playerFirsLineCount;
            bool onlyOneLine = playerSecondLineCount == 0;
            for (int i = 0; i < playerCount; i++)
            {
                bool firstLine = i < playerFirsLineCount;

                float distPlayers = 475f - 30f * playerFirsLineCount;
                if (onlyOneTeam && onlyOneLine)
                    distPlayers *= 2f;

                float verticalDistPlayers = 500f;
                if (onlyOneTeam)
                    verticalDistPlayers += 50f;

                float x = firstLine ? GetPositionInLine(playerFirsLineCount, i, distPlayers) : GetPositionInLine(playerSecondLineCount, i - playerFirsLineCount, distPlayers);
                float y = firstLine ? 0f : -verticalDistPlayers;
                UI_EndRacePlayer endRacePlayer = GameObject.Instantiate<UI_EndRacePlayer>(playerTeamPrefab, teamsPanel[nTeamIndex].playersRoot);
                RectTransform rt = endRacePlayer.GetRT();
                rt.anchoredPosition = new Vector2(x, y);

                if (onlyOneLine && onlyOneTeam)
                    rt.localScale = Vector3.one;
                
                endRacePlayer.Setup(playerList[nTeamId][i]);
            }
        }
    }

    private int GetPlayerCountInFirstLine(int playerCount, bool onlyOneTeam)
	{
        int maxPlayerByLine = onlyOneTeam ? 5 : 3;
        if (playerCount <= maxPlayerByLine)
            return playerCount;
        return (playerCount + 1) / 2;
    }

    private float GetPositionInLine(int count, int index, float distanceBetweenPlayer)
    {
        float total = (count - 1) * distanceBetweenPlayer;
        float half = total * 0.5f;
        return -half + index * distanceBetweenPlayer;
    }

    private void TakeScreenShoot()
	{
        // generate structure
        string rootpath = Application.dataPath + "/../PDFFiles";
        if (!Directory.Exists(rootpath))
            Directory.CreateDirectory(rootpath);
        // sessions day
        rootpath += "/" + System.DateTime.Now.ToString("yyyy_MM_dd");
        if (!Directory.Exists(rootpath))
            Directory.CreateDirectory(rootpath);
        // session itself
        rootpath += "/" + System.DateTime.Now.ToString("T").Replace(":", "_");
        if (!Directory.Exists(rootpath))
            Directory.CreateDirectory(rootpath);

        TakeScreenShootAtPath(rootpath, 0);
    }

    public void TakeScreenShootAtPath(string rootpath, int index)
    {
        if (_backgroundImage != null)
            _backgroundImage.gameObject.SetActive(true);

        float scale = transform.lossyScale.x;
        m_renderCamera.orthographicSize *= scale;

        RenderTexture rt = new RenderTexture( resWidth, resHeight, 24);
        m_renderCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        m_renderCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        m_renderCamera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToJPG();
        string filename = rootpath != null ? $"{rootpath}/EndScreen_{index}.jpg" : "EndScreen.jpg";
        File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));

        if (_backgroundImage != null)
            _backgroundImage.gameObject.SetActive(false);

        string spriteTitle = null;
        if (multiplayerlobby.product == multiplayerlobby.Product.TOG)
            spriteTitle = "Pdf/PdfTitle";

        if (rootpath != null)
            PdfGenerator._CreatePDFwithScreenShoot(bytes, resWidth, resHeight, rootpath, "/EndScreen_" + index, spriteTitle);
        else
            PdfGenerator.CreatePDFwithScreenShoot(bytes, resWidth, resHeight, spriteTitle);
    }
}
