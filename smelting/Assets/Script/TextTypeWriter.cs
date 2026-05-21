using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextTypewriter : MonoBehaviour
{
    private TMP_Text _textComponent;

    private void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
    }

    public void SetText(string text)
    {
        _textComponent.text = text;
    }

    public void ShowText(string text, float charDelay = 0.05f)
    {
        _textComponent.text = "";
        StartCoroutine(TypeCoroutine(text, charDelay));
    }

    private IEnumerator TypeCoroutine(string text, float delay)
    {
        for (int i = 0; i < text.Length; i++)
        {
            _textComponent.text += text[i];
            yield return new WaitForSeconds(delay);
        }
    }
}