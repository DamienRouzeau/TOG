using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Localization component used with TextMesh Pro text component
/// </summary>
/// 
namespace RRLib
{
//    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("Rerolled/Lang/LocalizedTextMP")]
    public sealed class RRLocalizedTextMP : RRLocalizedTextBase
    {
        #region Unity callbacks
        private void Awake()
        {
            m_textComponent_tmp = GetComponent<TMP_Text>();
            m_textComponent_tm = GetComponent<TextMesh>();
            m_textComponent_t = GetComponent<Text>();
//            Debug.AssertFormat(m_textComponent_tmp != null, "There is a localization component on object '{0}' but no TextMesh Pro text component attached.", name);

            //m_sMaterialName = m_textComponent.fontSharedMaterial.name;
            if (m_textComponent_tmp != null)            base.Init(m_textComponent_tmp.text);
            if (m_textComponent_tm != null) base.Init(m_textComponent_tm.text);
            if (m_textComponent_t != null) base.Init(m_textComponent_t.text);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_textComponent_tmp = null;
            m_textComponent_tm = null;
            m_textComponent_t = null;
        }
        #endregion

        #region Protected
        protected override void UpdateTextComponent()
        {
            //	lwCountry lang = lwLanguageManager.instance.currentLanguage;
            //	string sTextToSet;
            //	if( lang.m_bIsLatin || base.hasLocalizationKey==false )
            //	{
            //		sTextToSet = base.text;
            //	}
            //	else
            //	{
            //		if( string.IsNullOrEmpty( m_sMaterialName ) )
            //		{
            //			sTextToSet = string.Format( "<font=\"{0}\">{1}", lang.m_sLanguageCulture, base.text );
            //		}
            //		else
            //		{
            //			sTextToSet = string.Format( "<font=\"{0}\" material=\"{1}\">{2}", lang.m_sLanguageCulture, string.Format( "{0}_{1}", m_sMaterialName, lang.m_sLanguageCulture ), base.text );
            //		}
            //	}
            //  if( m_textComponent!=null ) m_textComponent.text = sTextToSet;
            if (m_textComponent_tmp != null) m_textComponent_tmp.text = base.text;
            if (m_textComponent_tm != null) m_textComponent_tm.text = base.text;
            if (m_textComponent_t != null) m_textComponent_t.text = base.text;
        }
        #endregion

        #region Private
        #region Attributes
        private TMP_Text m_textComponent_tmp = null;
        private TextMesh m_textComponent_tm = null;
        private Text m_textComponent_t = null;
        //private string m_sMaterialName;
        #endregion
        #endregion
    }
}
