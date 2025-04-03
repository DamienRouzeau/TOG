// Copyright 2019 Rerolled

using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Manages the language available in a game
/// Load and parse text files to register all keys and their localized text for each language.
/// /!\ Text files must be retrieved from resource paths.
/// /!\ Text files are csv formated list where the first column is the localization key and the second column is :
/// 	- the localized text for texts (the key must start with 'str_')
/// 	- the name of asset array and the path of the asset in the asset array separated by a '|' (the key must not start with 'str_')
/// </summary>

namespace RRLib
{
    public class RRLanguageManager : RRSingleton<RRLanguageManager>
    {
        public delegate void OnLanguageChanged();
        public OnLanguageChanged m_onLanguageChangedEvent;

        public enum TimeUnitTextId
        {
            Days,
            Day,
            Hours,
            Hour,
            Minutes,
            Minute,
            Seconds,
            Second,
        }
        public static readonly int timeUnitTextIdCount = System.Enum.GetValues(typeof(TimeUnitTextId)).Length;

        /// <summary>
        /// Gets the index of the current language among all available languages.
        /// </summary>
        /// <value>The index of the current language.</value>
        public int nCurrentLanguageIndex
        {
            get
            {
                Debug.Assert(m_nLanguageIndex >= 0 && m_nLanguageIndex < m_availableLanguages.Length);
                return m_nLanguageIndex;
            }
        }

        /// <summary>
        /// Gets the current language.
        /// </summary>
        /// <value>The current language.</value>
        public RRCountry currentLanguage
        {
            get
            {
                Debug.Assert(m_nLanguageIndex >= 0 && m_nLanguageIndex < m_availableLanguages.Length);
                return m_availableLanguages[m_nLanguageIndex];
            }
        }

        /// <summary>
        /// Gets the language at specified index.
        /// </summary>
        /// <param name="nLanguageIndex">Index.</param>
        public RRCountry this[int nLanguageIndex]
        {
            get
            {
                Debug.Assert(nLanguageIndex >= 0 && nLanguageIndex < m_availableLanguages.Length);
                return m_availableLanguages[nLanguageIndex];
            }
        }

        /// <summary>
        /// Gets the amount of available languages
        /// </summary>
        /// <value>The language count.</value>
        public int nLanguageCount
        {
            get { return m_availableLanguages.Length; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="lwLanguageManager"/> class.
        /// </summary>
        public RRLanguageManager()
        {
            m_availableLanguages = new RRCountry[0];
            m_sTextFilesPerLanguage = new HashSet<AssetHolderKey>[0];

            TextAsset languageListAsset = Resources.Load<TextAsset>("Texts/Langs");
            if (languageListAsset == null)
            {
                Debug.LogError("Language Manager : the file 'Langs' has not been found in any 'Resources/Texts/' folder !");
            }
            else
            {
                string[] sLanguageCodes = languageListAsset.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                List<RRCountry> availableLanguages = new List<RRCountry>();
                for (int nLanguageCodeIndex = 0; nLanguageCodeIndex < sLanguageCodes.Length; ++nLanguageCodeIndex)
                {
                    RRCountry countryStructure = RRCountryCode.GetRRCountry(sLanguageCodes[nLanguageCodeIndex].Replace("\r", ""));
                    if (countryStructure != null)
                    {
                        availableLanguages.Add(countryStructure);
                    }
                }

                m_availableLanguages = availableLanguages.ToArray();
                m_sTextFilesPerLanguage = new HashSet<AssetHolderKey>[m_availableLanguages.Length];
                for (int nLanguageIndex = 0; nLanguageIndex < m_sTextFilesPerLanguage.Length; ++nLanguageIndex)
                {
                    m_sTextFilesPerLanguage[nLanguageIndex] = new HashSet<AssetHolderKey>();
                }
            }

            m_localizedTexts = new Dictionary<string, string>();
            m_localizedResources = new Dictionary<string, AssetHolderKey>();
        }

        /// <summary>
        /// Adds a text file to a language.
        /// </summary>
        /// <param name="sLanguageCode">Language code.</param>
        /// <param name="sAssetArrayName">Name of the asset array in the AssetHolder.</param>
        /// <param name="sFileResourcePath">Resource path of the file.</param>
        public void AddTextFile(string sLanguageCode, string sAssetArrayName, string sFileResourcePath = "")
        {
            Debug.Assert(String.IsNullOrEmpty(sLanguageCode) == false);
            Debug.Assert(String.IsNullOrEmpty(sAssetArrayName) == false);

            int nLanguageIndex = System.Array.FindIndex(m_availableLanguages, item => item.m_sLanguageCulture == sLanguageCode);
            if (nLanguageIndex >= 0 && nLanguageIndex < m_availableLanguages.Length)
            {
                AssetHolderKey assetHolderKey = new AssetHolderKey(sAssetArrayName, sFileResourcePath);
                if (!m_sTextFilesPerLanguage[nLanguageIndex].Contains(assetHolderKey))
                {
                    m_sTextFilesPerLanguage[nLanguageIndex].Add(assetHolderKey);

                    // if the language is the current one, load the file immediately
                    if (nLanguageIndex == m_nLanguageIndex)
                    {
                        List<string> sFilesContent = new List<string>();
                        FillCsvContents(ref sFilesContent, assetHolderKey);

                        for (int nFileContentIndex = 0; nFileContentIndex < sFilesContent.Count; ++nFileContentIndex)
                        {
                            FillDictionaryWithCsvContent(sFilesContent[nFileContentIndex]);
                        }
                    }
                }
            }
            else
            {
                Debug.LogErrorFormat("Language Manager : language code '{0}' is not an available language code.\nThis error happened while registering file at path '{1}'.", sLanguageCode, sFileResourcePath);
            }
        }

        /// <summary>
        /// Sets the default language.
        /// </summary>
        public void SetDefaultLanguage()
        {
            Debug.Assert(m_availableLanguages.Length > 0, "Language manager : no available language registered.");

            // Look for system language and if the language is not available, switch to the first english language.
            // If there is no english language, switch to the first available language.

            int nSystemLanguageIndex = -1;
            int nEnglishLanguageIndex = -1;

            int nLanguageIndex = 0;
            while (nLanguageIndex < m_availableLanguages.Length && nSystemLanguageIndex < 0)
            {
                if (m_availableLanguages[nLanguageIndex].m_eLang == Application.systemLanguage)
                {
                    nSystemLanguageIndex = nLanguageIndex;
                }
                else
                {
                    if (m_availableLanguages[nLanguageIndex].m_eLang == SystemLanguage.English && nEnglishLanguageIndex < 0)
                    {
                        nEnglishLanguageIndex = nLanguageIndex;
                    }
                    ++nLanguageIndex;
                }
            }

            if (nSystemLanguageIndex >= 0)
            {
                SetLanguage(nSystemLanguageIndex);
            }
            else if (nEnglishLanguageIndex >= 0)
            {
                SetLanguage(nEnglishLanguageIndex);
            }
            else
            {
                SetLanguage(0);
            }
        }

        /// <summary>
        /// Sets the language.
        /// </summary>
        /// <param name="sLanguageCode">Code of the language to set.</param>
        public void SetLanguage(string sLanguageCode)
        {
            int nLanguageIndex = System.Array.FindIndex(m_availableLanguages, item => item.m_sLanguageCulture == sLanguageCode);
            Debug.AssertFormat(nLanguageIndex >= 0 && nLanguageIndex < m_availableLanguages.Length, "Language '{0}' not found in available languages.", sLanguageCode);
            SetAndLoadLanguage(nLanguageIndex);
        }

        /// <summary>
        /// Sets the language.
        /// </summary>
        /// <param name="systemLanguage">System language.</param>
        public void SetLanguage(SystemLanguage systemLanguage)
        {
            int nLanguageIndex = System.Array.FindIndex(m_availableLanguages, item => item.m_eLang == systemLanguage);
            Debug.AssertFormat(nLanguageIndex >= 0 && nLanguageIndex < m_availableLanguages.Length, "Language '{0}' not found in available languages.", systemLanguage);
            SetAndLoadLanguage(nLanguageIndex);
        }

        /// <summary>
        /// Sets the language.
        /// </summary>
        /// <param name="nLanguageIndex">Index of the language to set among all available languages.</param>
        public void SetLanguage(int nLanguageIndex)
        {
            if (nLanguageIndex == -1)
            {
                SetDefaultLanguage();
            }
            else
            {
                SetAndLoadLanguage(nLanguageIndex);
            }
        }

        /// <summary>
        /// Sets the language to next available.
        /// </summary>
        public void SetLanguageToNextAvailable()
        {
            Debug.Assert(m_nLanguageIndex >= 0 && m_nLanguageIndex < m_availableLanguages.Length, "Language manager does not have a current language.");
            int nLanguageIndex = (m_nLanguageIndex + 1) % m_availableLanguages.Length;
            SetAndLoadLanguage(nLanguageIndex);
        }

        /// <summary>
        /// Sets the language to previous available.
        /// </summary>
        public void SetLanguageToPreviousAvailable()
        {
            Debug.Assert(m_nLanguageIndex >= 0 && m_nLanguageIndex < m_availableLanguages.Length, "Language manager does not have a current language.");
            int nLanguageIndex = (m_nLanguageIndex + m_availableLanguages.Length - 1) % m_availableLanguages.Length;
            SetAndLoadLanguage(nLanguageIndex);
        }

        /// <summary>
        /// Determines whether this instance has the specified key.
        /// </summary>
        /// <param name="sKey">Key.</param>
        public bool HasStringKey(string sKey)
        {
            Debug.Assert(m_nLanguageIndex >= 0 && m_nLanguageIndex < m_availableLanguages.Length, "Language manager does not have a current language.");
            return m_localizedTexts.ContainsKey(sKey);
        }

        /// <summary>
        /// Gets the localized text associated with the key given.
        /// </summary>
        /// <param name="sKey">Localization key.</param>
        public string GetString(string sKey)
        {
            Debug.Assert(m_nLanguageIndex >= 0 && m_nLanguageIndex < m_availableLanguages.Length, "Language manager does not have a current language.");

            string sValue;
            if (m_localizedTexts.TryGetValue(sKey, out sValue))
            {
                return sValue;
            }
            else
            {
                Debug.LogWarningFormat("Language manager : key '{0}' not found for language '{1}'.", sKey, m_availableLanguages[m_nLanguageIndex].m_sLanguageCulture);
                return string.Empty;
            }
        }

        /// <summary>
        /// Determines whether this instance has the specified sKey.
        /// </summary>
        /// <param name="sKey">Key.</param>
        public bool HasResourceKey(string sKey)
        {
            Debug.Assert(m_nLanguageIndex >= 0 && m_nLanguageIndex < m_availableLanguages.Length, "Language manager does not have a current language.");
            return m_localizedResources.ContainsKey(sKey);
        }


        

        /// <summary>
        /// Reloads the texts from the resource text files.
        /// </summary>
        public void ReloadTexts()
        {
            Debug.Assert(m_nLanguageIndex >= 0 && m_nLanguageIndex < m_availableLanguages.Length, "Language manager does not have a current language.");

            LoadCurrentLanguage();

            if (m_onLanguageChangedEvent != null)
            {
                m_onLanguageChangedEvent();
            }
        }

        #region Private
        #region Declarations
        private struct AssetHolderKey
        {
            public readonly string m_sAssetArrayName;
            public readonly string m_sAssetName;

            public AssetHolderKey(string sAssetArrayName, string sAssetName)
            {
                Debug.Assert(string.IsNullOrEmpty(sAssetArrayName) == false);
                m_sAssetArrayName = sAssetArrayName;
                m_sAssetName = sAssetName;
            }

            public AssetHolderKey(AssetHolderKey source)
            {
                Debug.Assert(string.IsNullOrEmpty(source.m_sAssetArrayName) == false);
                m_sAssetArrayName = source.m_sAssetArrayName;
                m_sAssetName = source.m_sAssetName;
            }
        }
        #endregion

        #region Methods
        private void SetAndLoadLanguage(int nLanguageIndex)
        {
            Debug.AssertFormat(nLanguageIndex >= 0 && nLanguageIndex < m_availableLanguages.Length, "Invalid language index : {0}.", nLanguageIndex);
            if (m_nLanguageIndex != nLanguageIndex)
            {
                m_nLanguageIndex = nLanguageIndex;

                LoadCurrentLanguage();

                if (m_onLanguageChangedEvent != null)
                {
                    m_onLanguageChangedEvent();
                }
            }
        }

        private void LoadCurrentLanguage()
        {
            m_localizedTexts.Clear();
            m_localizedResources.Clear();

            List<string> sFilesContent = new List<string>();
            HashSet<AssetHolderKey>.Enumerator filePathEnumerator = m_sTextFilesPerLanguage[m_nLanguageIndex].GetEnumerator();
            while (filePathEnumerator.MoveNext())
            {
                FillCsvContents(ref sFilesContent, filePathEnumerator.Current);
            }

            for (int nFileContentIndex = 0; nFileContentIndex < sFilesContent.Count; ++nFileContentIndex)
            {
                FillDictionaryWithCsvContent(sFilesContent[nFileContentIndex]);
            }
        }

        private void FillCsvContents(ref List<string> sCsvContents, AssetHolderKey sAssetHolderKey)
        {
            /*		if( lwAssetHolder.IsArrayAssetExist( sAssetHolderKey.m_sAssetArrayName ) )
                    {
                        if( String.IsNullOrEmpty( sAssetHolderKey.m_sAssetName ) )
                        {
                            int nAssetCount = lwAssetHolder.GetAssetArrayCount( sAssetHolderKey.m_sAssetArrayName );
                            for( int nAssetIndex = 0; nAssetIndex<nAssetCount; ++nAssetIndex )
                            {
                                TextAsset textAsset = lwAssetHolder.GetArrayAssetElement<TextAsset>( sAssetHolderKey.m_sAssetArrayName, nAssetIndex );
                                if( textAsset==null )
                                {
                                    Debug.LogErrorFormat( "Language Manager : Resource at path '{0}' in the asset array '{1}' is not a TextAsset.", sAssetHolderKey.m_sAssetName, sAssetHolderKey.m_sAssetArrayName );
                                }
                                else
                                {
                                    sCsvContents.Add( textAsset.text );
                                }
                            }
                        }
                        else
                        {
                            TextAsset textAsset = lwAssetHolder.GetArrayAssetElement<TextAsset>( sAssetHolderKey.m_sAssetArrayName, sAssetHolderKey.m_sAssetName );
                            if( textAsset==null )
                            {
                                Debug.LogErrorFormat( "Language Manager : Resource not found at path '{0}' in the asset array '{1}'.", sAssetHolderKey.m_sAssetName, sAssetHolderKey.m_sAssetArrayName );
                            }
                            else
                            {
                                sCsvContents.Add( textAsset.text );
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat( "Language manager : asset array '{0}' does not exist in the asset holder.", sAssetHolderKey.m_sAssetArrayName );
                    }*/


            TextAsset textAsset = Resources.Load<TextAsset>("Texts/" + sAssetHolderKey.m_sAssetArrayName + "/" + sAssetHolderKey.m_sAssetName);
            if (textAsset == null)
            {
                Debug.LogErrorFormat("Language Manager : Resource not found at path '{0}' in the asset array '{1}'.", sAssetHolderKey.m_sAssetName, sAssetHolderKey.m_sAssetArrayName);
            }
            else
            {
                sCsvContents.Add(textAsset.text);
            }
        }

        private void FillDictionaryWithCsvContent(string sCsvContent)
        {
            string[] sLines = sCsvContent.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            // omit first line (header)
            for (int nLineIndex = 1; nLineIndex < sLines.Length; ++nLineIndex)
            {
                if (string.IsNullOrEmpty(sLines[nLineIndex]) == false)
                {
                    string[] sKeyValuePair = sLines[nLineIndex].Split(new char[] { ';' });
                    if (sKeyValuePair.Length == 2)
                    {
                        if (string.IsNullOrEmpty(sKeyValuePair[0]))
                        {
                            Debug.LogErrorFormat("Language Manager : empty localization key found while parsing '{0}'.", sLines[nLineIndex]);
                        }
                        else
                        {
                            // localized text
                            if (sKeyValuePair[0].StartsWith("str_", StringComparison.Ordinal))
                            {
                                if (m_localizedTexts.ContainsKey(sKeyValuePair[0]))
                                {
                                    Debug.LogErrorFormat("Language manager : localization key '{0}' already registered.", sKeyValuePair[0]);
                                }
                                else
                                {
                                    m_localizedTexts.Add(sKeyValuePair[0], sKeyValuePair[1].Replace("\\n", "\n").Replace("\r", ""));
                                }
                            }
                            // localized resource
                            else
                            {
                                Debug.LogWarning("Not integrate yet");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("Language Manager : parsing a CSV file resulted in a parsing error with line '{0}'.", sLines[nLineIndex]);
                    }
                }
            }
        }
        #endregion

        #region Attributes
        private int m_nLanguageIndex;
        private readonly RRCountry[] m_availableLanguages;
        private readonly HashSet<AssetHolderKey>[] m_sTextFilesPerLanguage;

        private readonly Dictionary<string, string> m_localizedTexts;
        private readonly Dictionary<string, AssetHolderKey> m_localizedResources;
        #endregion
        #endregion
    }
}