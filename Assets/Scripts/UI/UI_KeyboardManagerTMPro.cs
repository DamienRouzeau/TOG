using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using VRUiKits.Utils;
using TMPro;
public class UI_KeyboardManagerTMPro : KeyboardManager
{
    #region Delegates

    public delegate void OnValueChangedDelegate(string text);

    #endregion

    #region Properties

    public OnValueChangedDelegate onValueChangedCbk = null;

    [Header("Target TMP_InputField")]
    [SerializeField]
    private TMP_InputField _targetTMPro = null;
    
    #endregion

    protected override string Input
    {
        get
        {
            if (null == _targetTMPro)
            {
                return "";
            }

            return _targetTMPro.text;
        }
        set
        {
            if (null == _targetTMPro)
            {
                return;
            }

            _targetTMPro.text = value;
            // Force target input field activated if losing selection
            _targetTMPro.ActivateInputField();
            _targetTMPro.MoveTextEnd(false);

            if (onValueChangedCbk != null)
                onValueChangedCbk(value);
        }
    }
}