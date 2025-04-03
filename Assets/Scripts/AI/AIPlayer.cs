using RootMotion.FinalIK;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public static bool autoShoot = false;

    public Transform leftHand => _leftHand;
    public Transform rightHand => _rightHand;
    public Transform head => _head;

    private Transform _leftHand;
    private Transform _rightHand;
    private Transform _head;

    private ProjectileCannon _projectileCannonR = null;
    private ProjectileCannon _projectileCannonL = null;
    private attachobject _attachObjectR = null;
    private attachobject _attachObjectL = null;
    private SkinPlayer _skin = null;
    private VRIK _vrik = null;
    private Camera _cam = null;
    private Vector3 _startRotateMousePos;
    private Quaternion _startRotateCamRotation;
    private Transform _trToMove = null;
    private int _testLayer = 0;
    private float _autoShootDelay = 0f;


    // Start is called before the first frame update
    void Start()
    {
        _leftHand = gameObject.FindInChildren("LeftHand").transform;
        _rightHand = gameObject.FindInChildren("RightHand").transform;
        _head = gameObject.FindInChildren("VR_Camera").transform;
        _cam = _head.GetComponent<Camera>();
        _skin = gameObject.GetComponentInChildren<SkinPlayer>();
        _vrik = _skin.GetComponent<VRIK>();
        _projectileCannonR = _vrik.references.rightHand.GetComponentInChildren<ProjectileCannon>();
        _trToMove = transform;
        _testLayer = physicslayermask.MaskForLayer(LayerMask.NameToLayer("Projectiles"));
        ResetPositions();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.myplayer.isInPause)
            return;

        Vector3 mousePos = Input.mousePosition;
        bool shoot = Input.GetMouseButtonDown(1);
        if (autoShoot)
		{
            _autoShootDelay += Time.deltaTime;
            if (_autoShootDelay > 0.2f)
			{
                _autoShootDelay = 0f;
                shoot = true;
            }
        }
        if (shoot)
        {
            UpdateGunL();
            UpdateGunR();
            
            if (Input.GetKey(KeyCode.LeftControl))
            {
                _projectileCannonR?.gameObject.SetActive(false);
                _projectileCannonL?.gameObject.SetActive(false);
            }
            else if (Input.GetKey(KeyCode.LeftAlt))
            {
                foreach (ProjectileCannon cannon in ((Player.IPlayer)(Player.myplayer)).GetBoat().GetComponentsInChildren<ProjectileCannon>())
                {
                    if (Vector3.Distance(cannon.transform.position, Player.myplayer.GetFootPos()) < 3f)
                    {
                        if (poolhelper.myself.IsBoatBulletPool(cannon.poolname))
                            cannon.FireCannon();
                    }
                }
            }
            else
            {
                _projectileCannonR?.gameObject.SetActive(true);
                _projectileCannonR?.FireCannon();
                _projectileCannonL?.gameObject.SetActive(true);
                _projectileCannonL?.FireCannon();
            }
        }

        mousePos.z = 1000f;
        Ray ray = _cam.ScreenPointToRay(mousePos);
        ray.origin += ray.direction;
        Vector3 lookAt = ray.GetPoint(1000f);
        
        Debug.DrawLine(ray.origin, lookAt, Color.blue);
       
        _rightHand.transform.LookAt(lookAt, Vector3.up);
        _rightHand.localPosition = Vector3.up * 1f + Vector3.right * 0.3f + Vector3.forward * 0.3f;
        _rightHand.position += _rightHand.transform.forward * 0.3f;
        if (_projectileCannonR != null)
            _projectileCannonR.lookAt.LookAt(lookAt, Vector3.up);

        
        _leftHand.transform.LookAt(lookAt, Vector3.up);
        _leftHand.localPosition = Vector3.up * 1f + Vector3.left * 0.3f + Vector3.forward * 0.3f;
        _leftHand.position += _leftHand.transform.forward * 0.3f;
        if (_projectileCannonL != null)
            _projectileCannonL.lookAt.LookAt(lookAt, Vector3.up);

        if (Input.GetMouseButtonDown(2))
        {
            pointfromhand hand = _rightHand.GetComponentInChildren<pointfromhand>();
            if (hand != null)
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 1000f, _testLayer))
                {
                    lookAt = hitInfo.point;
                }
                hand.Teleport(lookAt);
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                _startRotateMousePos = mousePos;
                _startRotateCamRotation = _cam.transform.localRotation;
            }
            else
            {
                Vector3 deltaMousePos = mousePos - _startRotateMousePos;
                Vector3 euler = _startRotateCamRotation.eulerAngles;
                _cam.transform.localRotation = Quaternion.Euler(euler.x - deltaMousePos.y, euler.y + deltaMousePos.x, 0f);
            }
        }
        else
		{
            TurnPlayerToCameraDirection();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Player.myplayer.ChangeTypeOfWeaponData(0, Player.WeaponType.Musket);
            UpdateGunL();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Player.myplayer.ChangeTypeOfWeaponData(0, Player.WeaponType.TOG_Biggun);
            UpdateGunL();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Player.myplayer.ChangeTypeOfWeaponData(0, Player.WeaponType.Canon_Torche);
            UpdateGunL();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Player.myplayer.ChangeTypeOfWeaponData(1, Player.WeaponType.Musket);
            UpdateGunR();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Player.myplayer.ChangeTypeOfWeaponData(1, Player.WeaponType.TOG_Biggun);
            UpdateGunR();
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Player.myplayer.ChangeTypeOfWeaponData(1, Player.WeaponType.Canon_Torche);
            UpdateGunR();
        }

        if (_trToMove != null)
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
                _trToMove.position -= _trToMove.up* Time.deltaTime * 5f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                TurnPlayerToCameraDirection();
            }
        }
    }

    private void UpdateGunR()
    {
        Player.myplayer.ChangePlaceOfWeaponData(1, Player.WeaponPlace.RightHand);
        _attachObjectR = Player.myplayer.GetAttachObjectOfPlace(Player.WeaponPlace.RightHand);
        if (_attachObjectR != null)
            _projectileCannonR = _attachObjectR.gameObject.GetComponent<ProjectileCannon>();
    }

    private void UpdateGunL()
    {
        Player.myplayer.ChangePlaceOfWeaponData(0, Player.WeaponPlace.LeftHand);
        _attachObjectL = Player.myplayer.GetAttachObjectOfPlace(Player.WeaponPlace.LeftHand);
        if (_attachObjectL != null)
            _projectileCannonL = _attachObjectL.gameObject.GetComponent<ProjectileCannon>();
    }

    private void TurnPlayerToCameraDirection()
    {
        Vector3 dir = _cam.transform.forward;
        dir.y = 0f;
        _trToMove.LookAt(_trToMove.position + dir * 10f);
        _cam.transform.localRotation = Quaternion.identity;
    }

    public void ResetPositions()
    {
        _head.localPosition = Vector3.up * 1.5f;
        _leftHand.localPosition = Vector3.up * 1f + Vector3.left * 0.3f + Vector3.forward * 0.3f;
        _rightHand.localPosition = Vector3.up * 1f + Vector3.right * 0.3f + Vector3.forward * 0.3f;
    }
}
