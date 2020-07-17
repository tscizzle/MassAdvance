using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PostTrialScreen : MonoBehaviour
{
    private Text postTrialText;

    void Start()
    {
        postTrialText = GameObject.Find("PostTrialText").GetComponent<Text>();

        hide();
    }

    void Update()
    {
        if (TrialLogic.T.isTrialLoss)
        {
            postTrialText.text = "The Mass advanced.";
        } else if (TrialLogic.T.isTrialWin)
        {
            postTrialText.text = "You stopped the Mass from advancing.";
        } else
        {
            postTrialText.text = null;
        }
    }

    /* PUBLIC API */

    public void clickRestartTrial()
    /* Click handler for the button that shows after a trial, prompting the user to play again. */
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void hide()
    /* Hide the post-trial screen. */
    {
        transform.localScale = Vector3.zero;
    }

    public void show()
    /* Show the post-trial screen. */
    {
        transform.localScale = new Vector3(1, 1, 1);
    }
}
