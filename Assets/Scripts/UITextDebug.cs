using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITextDebug : MonoBehaviour
{
    public static UITextDebug Instance { get; private set; }

    private void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    [SerializeField] private TMP_Text m_debugText;

    public void Log(string text)
    {
        m_debugText.text = text;
    }
}
