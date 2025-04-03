using UnityEngine;
using UnityEngine.UI;

public class UI_SetSprite : MonoBehaviour
{
    public enum SpritesType
    {
        PlayerSkin
    }

    [SerializeField]
    private Image _imgToUpdate = null;
    [SerializeField]
    private Sprite[] _sprites = null;
    [SerializeField]
    private SpritesType _spritesType = SpritesType.PlayerSkin;

    // Start is called before the first frame update
    void Start()
    {
        if (_imgToUpdate == null)
            _imgToUpdate = gameObject.GetComponent<Image>();
        if (_imgToUpdate != null)
        {
            switch (_spritesType)
            {
                case SpritesType.PlayerSkin:
                    int idx = gamesettings_player.myself.GetSkinIndexFromName(gameflowmultiplayer.GetMySkinName());
                    _imgToUpdate.sprite = _sprites[idx];
                    break;
            }
        }
    }
}
