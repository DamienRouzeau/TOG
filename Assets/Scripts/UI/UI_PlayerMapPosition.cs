using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerMapPosition : MonoBehaviour
{
    [SerializeField]
    private RectTransform _arrowPlayer = null;
    [SerializeField]
    private Rect _mapRect = Rect.zero;
    [SerializeField]
    private Rect _worldRect = Rect.zero;
    [SerializeField]
    private Vector2 _mapHeightRange = Vector2.zero;
    [SerializeField]
    private Vector2 _worldHeightRange = Vector2.zero;
    [SerializeField]
    private GameObject _arrowAvatarPrefab = null;
    [SerializeField]
    private Transform _arrowAvatarRoot = null;
    [SerializeField]
    private Transform _scaleRoot = null;

    private Transform _playerCamTr = null;
    private Dictionary<Transform, GameObject> _targetDic = null;
    private Dictionary<Player_avatar, RectTransform> _arrowAvatars = null;

    public void AddTarget(Transform trWorld, GameObject miniMapTargetPrefab)
    {
        if (_targetDic == null)
            _targetDic = new Dictionary<Transform, GameObject>();
        GameObject targetGo = Instantiate(miniMapTargetPrefab, _arrowPlayer.parent);
        RectTransform rt = targetGo.GetAddComponent<RectTransform>();
        rt.anchoredPosition = ConvertWorldToMap(trWorld.position, out float z);
        Vector3 pos = rt.localPosition;
        pos.z = z;
        rt.localPosition = pos;
        _targetDic.Add(trWorld, targetGo);

    }

    public void RemoveTarget(Transform trWorld)
    {
        if (_targetDic != null && trWorld != null && _targetDic.ContainsKey(trWorld))
        {
            GameObject goTarget = _targetDic[trWorld];
            if (goTarget != null)
                GameObject.Destroy(goTarget);
            _targetDic.Remove(trWorld);
        }
    }

    public void SetScale(float scale)
	{
        _scaleRoot.localScale = Vector3.one * scale;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Player.myplayer != null)
            _playerCamTr = Player.myplayer.cam.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.myplayer != null)
        {
            UpdateArrowPlayer();
            UpdateArrowAvatars();
        }
    }

    private void UpdateArrowPlayer()
	{
        if (_playerCamTr != null)
        {
            _arrowPlayer.anchoredPosition = ConvertWorldToMap(_playerCamTr.position, out float z);
            float angleZ = _playerCamTr.rotation.eulerAngles.y;
            _arrowPlayer.localRotation = Quaternion.Euler(0f, 0f, -angleZ);
            Vector3 pos = _arrowPlayer.localPosition;
            pos.z = z;
            _arrowPlayer.localPosition = pos;
        }
        else
        {
            _playerCamTr = Player.myplayer.cam.transform;
        }
    }

    private void UpdateArrowAvatars()
	{
        foreach (Player_avatar avatar in Player.myplayer.avatars)
        {
            if (avatar.actornumber >= 0)
            {
                if (_arrowAvatars == null)
                    _arrowAvatars = new Dictionary<Player_avatar, RectTransform>();

                if (!_arrowAvatars.ContainsKey(avatar))
                {
                    GameObject arrow = GameObject.Instantiate(_arrowAvatarPrefab, _arrowAvatarRoot);
                    _arrowAvatars.Add(avatar, arrow.GetComponent<RectTransform>());
                }

                RectTransform rt = _arrowAvatars[avatar];
                rt.anchoredPosition = ConvertWorldToMap(avatar.transform.position, out float z);
                float angleZ = avatar.vrik.references.head.transform.eulerAngles.y;
                rt.localRotation = Quaternion.Euler(0f, 0f, -angleZ);
                Vector3 pos = rt.localPosition;
                pos.z = z;
                rt.localPosition = pos;
            }
            else
            {
                if (_arrowAvatars != null && _arrowAvatars.ContainsKey(avatar))
                {
                    RectTransform rt = _arrowAvatars[avatar];
                    _arrowAvatars.Remove(avatar);
                    GameObject.Destroy(rt.gameObject);
                }
            }
        }
    }

    private Vector2 ConvertWorldToMap(Vector3 pos, out float mapPosZ)
	{
        float ratioPosX = Mathf.InverseLerp(_worldRect.min.x, _worldRect.max.x, pos.x);
        float ratioPosY = Mathf.InverseLerp(_worldRect.min.y, _worldRect.max.y, pos.z);
        float ratioPosZ = Mathf.InverseLerp(_worldHeightRange.x, _worldHeightRange.y, pos.y);
        float mapPosX = Mathf.Lerp(_mapRect.min.x, _mapRect.max.x, ratioPosX);
        float mapPosY = Mathf.Lerp(_mapRect.min.y, _mapRect.max.y, ratioPosY);
        mapPosZ = Mathf.Lerp(_mapHeightRange.x, _mapHeightRange.y, ratioPosZ);
        return new Vector2(mapPosX, mapPosY);
    }
}
