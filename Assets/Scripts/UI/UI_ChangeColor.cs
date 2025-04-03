using UnityEngine;
using UnityEngine.UI;

public class UI_ChangeColor : MonoBehaviour
{
    #region Properties

    [SerializeField]
    private boat_followdummy _boatRef = null;
    [SerializeField]
    private ColorSettings.ColorType _colorType = ColorSettings.ColorType.HealthGauge;
    [SerializeField]
    private ColorSettings _colorSettings = null;
    [SerializeField]
    private Image _image = null;
    [SerializeField]
    private boat_followdummy.TeamColor _defaultTeam = boat_followdummy.TeamColor.Blue;

    private boat_followdummy.TeamColor _teamColor = boat_followdummy.TeamColor.Blue;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (_boatRef == null)
            _teamColor = GetComponentInParent<boat_followdummy>()?.teamColor ?? _defaultTeam;
        else
            _teamColor = _boatRef.teamColor;

        if (_colorSettings == null)
            _colorSettings = gamesettings_ui.myself?.colorSettings;

        if (_image == null)
            _image = gameObject.GetComponentInChildren<Image>();

        InitColor(_teamColor);
    }

    public void InitColor(boat_followdummy.TeamColor color)
	{
        _teamColor = color;

        if (_image != null)
            _image.color = GetColor();
    }

    public Color GetColor()
    {
        if (_colorSettings != null)
        {
            return _colorSettings.colors[_colorType][_teamColor];
        }
        return Color.blue;
    }
}
