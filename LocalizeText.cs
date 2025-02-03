using Assets.SimpleLocalization.Scripts;
using UnityEngine;

public class LocalizeText : MonoBehaviour
{
    public string nameToLocalize;

    public void Start()
    {
        Localize();
        LocalizationManager.OnLocalizationChanged += Localize;
    }

    public void OnDestroy()
    {
        LocalizationManager.OnLocalizationChanged -= Localize;
    }

    public void Localize()
    {
        UI.TEXT.SET(gameObject, UI.Localize(nameToLocalize));
    }
}
