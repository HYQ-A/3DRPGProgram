using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MainMenue : MonoBehaviour
{
    private Button newGameBtn;
    private Button continueBtn;
    private Button quitBtn;
    private PlayableDirector director;

    private void Awake()
    {
        newGameBtn = this.transform.GetChild(1).GetComponent<Button>();
        continueBtn = this.transform.GetChild(2).GetComponent<Button>();
        quitBtn = this.transform.GetChild(3).GetComponent<Button>();

        newGameBtn.onClick.AddListener(PlayTimeline);
        continueBtn.onClick.AddListener(ContinueGame);
        quitBtn.onClick.AddListener(QuiteGame);

        director = FindObjectOfType<PlayableDirector>();
        director.stopped += NewGame;
    }

    private void PlayTimeline()
    {
        director.Play();
    }

    private void NewGame(PlayableDirector obj)
    {
        PlayerPrefs.DeleteAll();
        SceneController.Instance.TransitionToFirstLevel();
    }

    private void ContinueGame()
    {
        SceneController.Instance.TransitionToLoadGame();
    }

    private void QuiteGame()
    {
        Application.Quit();
        Debug.Log("ÍË³öÓÎÏ·");
    }
}
