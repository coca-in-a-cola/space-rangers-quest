using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SRQ;
using UnityEngine.Events;
using TMPro;
using System;

public class ChoiceButton : MonoBehaviour
{
    [SerializeField]
    private PlayerChoice choice;

    [SerializeField]
    private Button button;

    [SerializeField]
    private TMP_Text textMeshPro;

    private UnityAction<PlayerChoice> OnChoiceSelect;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        textMeshPro = GetComponentInChildren<TMP_Text>();
        textMeshPro.text = choice.Text;

        button.onClick.AddListener(() => OnChoiceSelect(choice));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(PlayerChoice choice, UnityAction<PlayerChoice> OnChoiceSelect) {
        this.choice = choice;
        this.OnChoiceSelect = OnChoiceSelect;
    }
}
