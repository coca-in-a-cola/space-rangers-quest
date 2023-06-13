using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RichTextManager : MonoBehaviour
{
    public static RichTextManager instance = null;

    [SerializeField]
    private ThemeSettings _themeSettings;

    public ThemeSettings ThemeSettings { get { return _themeSettings; } }
    public Dictionary<string, string> replaceTags;

    // Метод, выполняемый при старте игры
    void Start() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    void Initialize() {
        var primaryHex = ColorUtility.ToHtmlStringRGBA(instance.ThemeSettings.primaryText);
        replaceTags = new Dictionary<string, string>() {
            {"<clr>", $"<color=#{primaryHex}>"},
            {"<clrEnd>", "</color>" },
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
