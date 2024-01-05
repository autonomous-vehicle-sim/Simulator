using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public bool isOpened = false;

    [SerializeField] private Image blurImage;
    [SerializeField] private float menuFadeSpeed = 4.0f;
    [SerializeField] private Color fadeStartColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    [SerializeField] private Color fadeEndColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);

    private CanvasGroup menuGroup;
    private CanvasGroup hudGroup;
    private bool menuFadeIn = false;
    private bool menuFadeOut = false;
    private float currentFade = 0.0f;           // 0.0f - 1.0f, which corresponds to 0% - 100% fade

    [ContextMenu("Show Menu")]
    public void ShowSettingsMenu()
    {
        menuFadeIn = true;
        isOpened = true;
    }

    [ContextMenu("Hide Menu")]
    public void HideSettingsMenu()
    {
        menuFadeOut = true;
        isOpened = false;
    }

    private void Start()
    {
        menuGroup = GetComponent<CanvasGroup>();
        hudGroup = GameObject.Find("HUD").GetComponent<CanvasGroup>();
        menuGroup.interactable = false;
        menuGroup.alpha = 0.0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpened) HideSettingsMenu();
            else ShowSettingsMenu();
        }

        if (menuFadeIn)
        {
            menuFadeOut = false;
            if (currentFade < 1.0f)
            {
                currentFade += Time.deltaTime * menuFadeSpeed;
                if (blurImage != null) blurImage.color = Color.Lerp(fadeStartColor, fadeEndColor, currentFade);
            }
            else
            {
                currentFade = 1.0f;
                menuFadeIn = false;
                menuGroup.interactable = true;
            }
        }

        if (menuFadeOut)
        {
            menuFadeIn = false;
            if (currentFade > 0.0f)
            {
                currentFade -= Time.deltaTime * menuFadeSpeed;
                if (blurImage != null) blurImage.color = Color.Lerp(fadeStartColor, fadeEndColor, currentFade);
            }
            else
            {
                currentFade = 0.0f;
                menuFadeOut = false;
                menuGroup.interactable = false;
            }
        }

        menuGroup.alpha = currentFade;
        hudGroup.alpha = 1.0f - currentFade;
    }
}
