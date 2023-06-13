using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SRQ;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private QMPlayer qmPlayer;
    private QM quest;
    private PlayerState currentState;

    [SerializeField]
    private TMP_Text mainText;

    [SerializeField]
    private RectTransform choicesList;

    [SerializeField]
    private GameObject choiceObject;


    [SerializeField]
    private RectTransform paramsList;

    [SerializeField]
    private GameObject paramsObject;


    // Start is called before the first frame update
    void Start()
    {
        // TODO: запускать файл на выбор
        var srcQmFile = Application.dataPath + "/Borrowed/qm/KPHD/Easywork.qmm";
        quest = QM.Parse(File.ReadAllBytes(srcQmFile));

        StartQuest();
    }

    void StartQuest() {
        qmPlayer = new QMPlayer(quest, "rus");
        qmPlayer.Start();
        StartCoroutine(OnRefreshView());
    }

    IEnumerator OnRefreshView() {
        currentState = qmPlayer.GetState();
        yield return new WaitForEndOfFrame();

        mainText.text = currentState.Text;

        var choices = currentState.Choices;

        // Clear Choices
        foreach (RectTransform child in choicesList) {
            Destroy(child.gameObject);
        }

        foreach (var choice in choices) {
            var choiceObj = Instantiate(choiceObject, choicesList);
            var choiceBehaviour = choiceObj.AddComponent<ChoiceButton>();

            choiceObj.SetActive(choice.Active);
            choiceBehaviour.Init(choice, OnChoiceSelect);
        }

        // Clear Params
        foreach (RectTransform child in paramsList) {
            Destroy(child.gameObject);
        }

        foreach (var param in currentState.ParamsState) {
            var paramsObj = Instantiate(paramsObject, paramsList);
            paramsObj.AddComponent<ParamText>().Init(param); 
        }

        yield return null;
    }

    void OnChoiceSelect (PlayerChoice choice) {
        qmPlayer.PerformJump(choice.JumpId);
        StartCoroutine(OnRefreshView());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            StartQuest();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }
}
