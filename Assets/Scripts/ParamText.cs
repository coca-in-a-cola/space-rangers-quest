using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ParamText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text textMeshPro;

    private string paramText;

    void Start() {
        textMeshPro = GetComponentInChildren<TMP_Text>();
        textMeshPro.text = paramText;
    }

    public void Init(string paramText) {
        this.paramText = paramText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
