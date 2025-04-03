using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class SpectatorManager : MonoBehaviour
{
	#region Enums

    public enum View
    {
        Auto,
        Map,
        BoatBlue,
        BoatRed,
        PlayersBoatBlue,
        PlayersBoatRed,
        Count
    }

	#endregion

	#region Properties

	[SerializeField]
    private GameObject _camerasPrefab = null;
    [SerializeField]
    private GameObject _camerasBoatPrefab = null;
    [SerializeField]
    private Transform _rootCameras = null;
    [SerializeField]
    private float _boatDistanceInDir = 20f;
    [SerializeField]
    private float _boatDistanceInHeight = 10f;

    private GameObject _goCams = null;
    private Dictionary<View,List<CinemachineVirtualCamera>> _cameras = null;
    private int _currentCameraIndex = 0;
    private View _currentView = View.Map;
    private CinemachineBrain _brain = null;

    #endregion

#if SPECTATOR_MODE
    private void Start()
    {
        Init();
    }
#endif

    private void OnDestroy()
    {
        _goCams = null;
        _cameras = null;
        _brain = null;
    }

    public void Init()
    {
        gameflowmultiplayer.isInSpectatorView = true;

        _cameras = new Dictionary<View, List<CinemachineVirtualCamera>>();

        _cameras.Add(View.Auto, null);

        if (_camerasPrefab != null)
        {
            _goCams = Instantiate(_camerasPrefab);
            _goCams.transform.SetParent(_rootCameras);
            _goCams.transform.localPosition = Vector3.zero;
            _goCams.transform.localScale = Vector3.one;
            _goCams.transform.localRotation = Quaternion.identity;
            _brain = _goCams.GetComponentInChildren<CinemachineBrain>();
            _cameras.Add(View.Map, new List<CinemachineVirtualCamera>(_goCams.GetComponentsInChildren<CinemachineVirtualCamera>(true)));
        }

        foreach (boat_followdummy boat in FindObjectsOfType<boat_followdummy>())
        {
            GameObject goCam = new GameObject("VirtualCameraOnBoat");
            goCam.transform.SetParent(boat.transform);
            goCam.transform.localScale = Vector3.one;
            View viewBoat = View.BoatBlue;
            View viewPlayers = View.PlayersBoatBlue;
            switch (boat.teamColor)
            {
                case boat_followdummy.TeamColor.Blue:
                    viewBoat = View.BoatBlue;
                    viewPlayers = View.PlayersBoatBlue;
                    break;
                case boat_followdummy.TeamColor.Red:
                    viewBoat = View.BoatRed;
                    viewPlayers = View.PlayersBoatRed;
                    break;
            }

            goCam.transform.localPosition = new Vector3(-15f, 5f, 10f);
            goCam.transform.localRotation = Quaternion.Euler(5f, 140f, 0f);
            _cameras.Add(viewBoat, new List<CinemachineVirtualCamera>());
            _cameras[viewBoat].Add(goCam.AddComponent<CinemachineVirtualCamera>());

            if (_camerasBoatPrefab != null)
            {
                _goCams = Instantiate(_camerasBoatPrefab);
                _goCams.transform.SetParent(boat.GetComponentInChildren<boat_sinking>(true).transform);
                _goCams.transform.localPosition = Vector3.zero;
                _goCams.transform.localScale = Vector3.one;
                _goCams.transform.localRotation = Quaternion.identity;
                _cameras[viewBoat].AddRange(_goCams.GetComponentsInChildren<CinemachineVirtualCamera>(true));
            }

            Camera[] camArray = boat.GetComponentsInChildren<Camera>(true);
            if (camArray != null)
            {
                foreach (Camera cam in camArray)
                {
                    if (cam != null)
                    {
                        if (_brain == null && cam.gameObject.activeInHierarchy)
                        {
                            _brain = cam.gameObject.AddComponent<CinemachineBrain>();
                        }
                        else
                        {
                            if (_cameras.ContainsKey(viewPlayers) == false)
                                _cameras.Add(viewPlayers, new List<CinemachineVirtualCamera>());
                            _cameras[viewPlayers].Add(cam.gameObject.AddComponent<CinemachineVirtualCamera>());
                            cam.enabled = false;
                        }
                    }
                }
            }
        }

        LensSettings lens = new LensSettings(86f,0f,0.01f, 1800f, 0f);

        foreach (var keyval in _cameras)
        {
            if (keyval.Value != null && keyval.Value.Count > 0)
            {
                foreach (var cam in keyval.Value)
                {
                    if (cam != null)
                        cam.m_Lens = lens;
                }
            }
        }

        UpdateCamera();
    }

    private View NextView()
    {
        for(int view=0; view < (int)View.Count; ++view)
        {
            int next = (int)(_currentView + view + 1) % (int)View.Count;
            View newView = (View)next;
            if (_cameras.ContainsKey(newView))
                return newView;
        }
        return _currentView;
    }

    private View PreviousView()
    {
        for (int view = 0; view < (int)View.Count; ++view)
        {
            int previous = (int)(_currentView + (int)View.Count - view - 1) % (int)View.Count;
            View previousView = (View)previous;
            if (_cameras.ContainsKey(previousView))
                return previousView;
        }
        return _currentView;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameflowmultiplayer.isInSpectatorView)
            return;

        UpdateBoatVirtualCameras();

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _currentView = NextView();
            _currentCameraIndex = 0;
            UpdateCamera();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _currentView = PreviousView();
            _currentCameraIndex = 0;
            UpdateCamera();
        }

        if (_currentView != View.Auto)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (_currentCameraIndex > 0)
                    _currentCameraIndex--;
                else
                    _currentCameraIndex = _cameras[_currentView].Count - 1;
                UpdateCamera();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _currentCameraIndex++;
                if (_currentCameraIndex >= _cameras[_currentView].Count)
                    _currentCameraIndex = 0;
                UpdateCamera();
            }
        }
    }

    private void UpdateCamera()
    {
        bool isAuto = _currentView == View.Auto;
        foreach (var keyval in _cameras)
        {
            bool isCurrentView = keyval.Key == _currentView;
            List<CinemachineVirtualCamera> cameras = keyval.Value;
            if (cameras != null)
            {
                for (int i = 0; i < cameras.Count; ++i)
                    cameras[i].gameObject.SetActive(isAuto || (isCurrentView && _currentCameraIndex == i));
            }
        }
    }

    private void UpdateBoatVirtualCameras()
    {
        if (_cameras != null && _cameras.ContainsKey(View.BoatBlue) && _cameras.ContainsKey(View.BoatRed))
        {
            Vector3 bluePos = RaceManager.myself.boats[boat_followdummy.TeamColor.Blue].transform.position;
            Vector3 redPos = RaceManager.myself.boats[boat_followdummy.TeamColor.Red].transform.position;
            Vector3 dir = (redPos - bluePos).normalized;
            _cameras[View.BoatRed][0].transform.position = redPos + dir * _boatDistanceInDir + Vector3.up * _boatDistanceInHeight;
            _cameras[View.BoatRed][0].transform.LookAt(bluePos);
            _cameras[View.BoatBlue][0].transform.position = bluePos - dir * _boatDistanceInDir + Vector3.up * _boatDistanceInHeight;
            _cameras[View.BoatBlue][0].transform.LookAt(redPos);
        }
    }
}
