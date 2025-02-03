using System;
using System.Collections.Generic;
using System.Linq;
using Assets.SimpleLocalization.Scripts;
using UnityEngine;

public class LanguageManager : Monobehaviour
{
    public static LanguageManager instance;
    public string[] Languages;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void Initialize()
    {
        LocalizationManager.Read();

        Languages = LocalizationManager.Languages;

        // User language may be any language in the world that Unity added
        string detectedLanguage = Application.systemLanguage.ToString();

        // So, if the user language is in the localized language by us, then use it, or use the default language
        bool isLanguageIncluded = LocalizationManager.Languages.Contains(detectedLanguage);

        if (isLanguageIncluded)
            LocalizationManager.Language = detectedLanguage;
        else
            LocalizationManager.Language = "English";
    }

    public void SetUserLanguage()
    {
        string selectedLanguage = GameManager.instance.PLAYER_SETTINGS.LANGUAGE;

        bool isThereSavedLanguage = Languages.Contains(selectedLanguage);

        if (!isThereSavedLanguage)
        {
            GameManager.instance.PLAYER_SETTINGS.LANGUAGE = LocalizationManager.Language;
            return;
        }

        if (LocalizationManager.Language != selectedLanguage)
            ChangeLanguage(selectedLanguage);
    }

    public void ChangeLanguage(string lang)
    {
        LocalizationManager.Language = lang;
        GameManager.instance.PLAYER_SETTINGS.LANGUAGE = lang;

        OnLanguageChanged();
    }

    void OnLanguageChanged()
    {
        List<LocalizeText> localizeTexts = new(FindObjectsByType<LocalizeText>(FindObjectsSortMode.None));

        foreach (LocalizeText localizeText in localizeTexts)
            localizeText.Localize();
    }
}
