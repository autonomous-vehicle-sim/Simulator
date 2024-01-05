using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [HideInInspector] public bool IsOpened = false;

    [SerializeField] private Image _blurImage;
    [SerializeField] private float _menuFadeSpeed = 4.0f;
    [SerializeField] private Color _fadeStartColor = new(1.0f, 1.0f, 1.0f, 0.0f);
    [SerializeField] private Color _fadeEndColor = new(0.6f, 0.6f, 0.6f, 1.0f);

    private CanvasGroup _menuGroup;
    private CanvasGroup _hudGroup;
    private bool _menuFadeIn = false;
    private bool _menuFadeOut = false;
    private float _currentFade = 0.0f;           // 0.0f - 1.0f, which corresponds to 0% - 100% fade

    [ContextMenu("Show Menu")]
    public void ShowSettingsMenu()
    {
        _menuFadeIn = true;
        IsOpened = true;
    }

    [ContextMenu("Hide Menu")]
    public void HideSettingsMenu()
    {
        _menuFadeOut = true;
        IsOpened = false;
    }

    private void Start()
    {
        _menuGroup = GetComponent<CanvasGroup>();
        _hudGroup = GameObject.Find("HUD").GetComponent<CanvasGroup>();
        _menuGroup.interactable = false;
        _menuGroup.alpha = 0.0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsOpened) HideSettingsMenu();
            else ShowSettingsMenu();
        }

        if (_menuFadeIn)
        {
            _menuFadeOut = false;
            if (_currentFade < 1.0f)
            {
                _currentFade += Time.deltaTime * _menuFadeSpeed;
                if (_blurImage != null) _blurImage.color = Color.Lerp(_fadeStartColor, _fadeEndColor, _currentFade);
            }
            else
            {
                _currentFade = 1.0f;
                _menuFadeIn = false;
                _menuGroup.interactable = true;
            }
        }

        if (_menuFadeOut)
        {
            _menuFadeIn = false;
            if (_currentFade > 0.0f)
            {
                _currentFade -= Time.deltaTime * _menuFadeSpeed;
                if (_blurImage != null) _blurImage.color = Color.Lerp(_fadeStartColor, _fadeEndColor, _currentFade);
            }
            else
            {
                _currentFade = 0.0f;
                _menuFadeOut = false;
                _menuGroup.interactable = false;
            }
        }

        _menuGroup.alpha = _currentFade;
        _hudGroup.alpha = 1.0f - _currentFade;
    }
}
