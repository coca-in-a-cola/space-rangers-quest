using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class RichQuestText : MonoBehaviour
{
    private TMP_Text tmp;

    private void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
    }

    private void OnDisable() {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
    }

    private void Start() {
        tmp = GetComponent<TMP_Text>();
    }

    void ON_TEXT_CHANGED(Object obj) {
        if (tmp != null && obj == tmp) {
            StartCoroutine(OnTextChanged());
        }
    }

    IEnumerator OnTextChanged() {
        yield return new WaitUntil(() => RichTextManager.instance != null);
        yield return new WaitUntil(() => tmp != null);

        Debug.Log("TEXT CHANGED!!!");

        var replace = RichTextManager.instance.replaceTags;
        string newText = tmp.text;
        foreach (KeyValuePair<string, string> entry in replace) {
            newText = newText.Replace(entry.Key, entry.Value);
        }
        yield return new WaitForEndOfFrame();
        tmp.text = newText;
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
