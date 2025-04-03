// Created by Raph <raph_ben@hotmail.com>
//
// Date: 2020/04/16

using System.Collections;
using UnityEngine;

public class StartWithoutVR : MonoBehaviour
{
    private Controllers _controllers = null;
    private Camera _cam = null;
    private Weapon _weapon = null;
    private Vector3 _startRotateMousePos;
    private Quaternion _startRotateCamRotation;
    private Transform _trWeapon;
    private Transform _trToMove = null;
    private bool _usePlayerInNoVR = true;

    private void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        while (Player.myplayer == null)
            yield return null;
        Player player = Player.myplayer;
        _trToMove = player.transform;

        if (GameflowBase.isInSpectatorView)
        {
            // Disable the player camera
            player.GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }
        else
        {
            if (_usePlayerInNoVR)
            {
                player.gameObject.AddComponent<AIPlayer>();
                _weapon = player.GetComponentInChildren<Weapon>(true);
                _cam = player.GetComponentInChildren<Camera>(true);
                _trWeapon = _weapon?.transform;
            }
            else
            {
                // Disable this player
                player.gameObject.SetActive(false);
                // Create a camera
                GameObject goCam = new GameObject("CameraWithoutVR");
                _cam = goCam.AddComponent<Camera>();
                goCam.transform.SetParent(player.transform.parent, false);
                goCam.transform.localPosition = new Vector3(0, 2f, 0f);
                goCam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                goCam.transform.localScale = Vector3.one;
                goCam.AddComponent<AudioListener>();
                _trToMove = _cam.transform;

                // Weapon
                _weapon = player.GetComponentInChildren<Weapon>(true);
                if (_weapon != null)
                {
                    GameObject goWeapon = new GameObject("Weapon");
                    _trWeapon = goWeapon.transform;
                    _trWeapon.SetParent(goCam.transform);
                    _trWeapon.localPosition = new Vector3(0, -0.5f, 1);
                    _trWeapon.localRotation = Quaternion.identity;
                    _trWeapon.localScale = Vector3.one;
                    _weapon.transform.SetParent(_trWeapon);
                    _weapon.transform.localPosition = Vector3.zero;
                    _weapon.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    _weapon.transform.localScale = Vector3.one;
                    _controllers = _weapon.gameObject.AddComponent<Controllers>();
                    _controllers.isAWeapon = true;
                    _controllers.weapon = _weapon;
                    _weapon.enabled = true;
                }
            }
        }
    }

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.End))
        {
            UI_ChooseYourName chooseName = FindObjectOfType<UI_ChooseYourName>();
            if (chooseName != null)
                chooseName.OnNameConfirmed();
        }

        if (_weapon != null && !_usePlayerInNoVR)
        {
            mousePos.z = 1000f;
            Ray ray = _cam.ScreenPointToRay(mousePos);
            Debug.DrawRay(ray.origin, ray.direction, Color.blue);
            _trWeapon.transform.LookAt(ray.GetPoint(1000f), Vector3.up);
            Debug.DrawLine(_trWeapon.position, _trWeapon.position + _trWeapon.forward * 1000f, Color.green);

            if (Input.GetMouseButtonDown(1))
            {
                _weapon.fireThatWeaponVR = true;
            }
        }

        if (_cam != null)
        {
            if (Input.GetMouseButton(0) && !_usePlayerInNoVR)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _startRotateMousePos = mousePos;
                    _startRotateCamRotation = _cam.transform.localRotation;
                }
                else
                {
                    Vector3 deltaMousePos = mousePos - _startRotateMousePos;
                    _cam.transform.localRotation = _startRotateCamRotation;
                    _cam.transform.Rotate(Vector3.up, deltaMousePos.x);
                }
            }
            if (Input.GetKey(KeyCode.H))
            {
                boat_followdummy boat = _cam.GetComponentInParent<boat_followdummy>();
                if (boat != null)
                {
                    Health boatHealth = boat.GetComponent<Health>();
                    boatHealth.ChangeHealth(100f);
                }
            }
        }

        if (_trToMove != null && !_usePlayerInNoVR)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                _trToMove.position -= _trToMove.right * Time.deltaTime * 5f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                _trToMove.position += _trToMove.right * Time.deltaTime * 5f;
            }
            if (Input.GetKey(KeyCode.Z))
            {
                _trToMove.position += _trToMove.forward * Time.deltaTime * 5f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                _trToMove.position -= _trToMove.forward * Time.deltaTime * 5f;
            }
            if (Input.GetKey(KeyCode.R))
            {
                _trToMove.position += _trToMove.up * Time.deltaTime * 5f;
            }
            if (Input.GetKey(KeyCode.F))
            {
                _trToMove.position -= _trToMove.up * Time.deltaTime * 5f;
            }
        }

        if (gameflowmultiplayer.myself != null && gameflowmultiplayer.myself.gameState == gameflowmultiplayer.GameState.LobbyLoaded)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                GameflowBase.SetMyTeam(0, true);
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                GameflowBase.SetMyTeam(1, true);
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                gameflowmultiplayer.SetMyValidated(true);
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                int id = Random.Range(0, 8);
                GameflowBase.piratenames[id] = "Tester_" + id;
                GameflowBase.pirateStatsNames[id] = "Tester_" + id;
                GameflowBase.SetTeamTables(true, 0, id);
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                int id = Random.Range(0, 8);
                GameflowBase.piratenames[id] = "Tester_" + id;
                GameflowBase.pirateStatsNames[id] = "Tester_" + id;
                GameflowBase.SetTeamTables(true, 1, id);
            }
        }
    }
}
