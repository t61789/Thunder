using UnityEngine;
using UnityEngine.UI;

public class SurvivalLiUI : BaseUI
{
    private string surviveTimePreText = "";
    private string scorePreText = "";

    private Text surviveTime;
    private Text score;

    private float maxTime;

    public void Init(float maxTime)
    {
        this.maxTime = maxTime;
    }

    public override void Awake()
    {
        base.Awake();
        surviveTime = transform.Find("surviveTime").GetComponent<Text>();
        score = transform.Find("score").GetComponent<Text>();

        surviveTimePreText = surviveTime.text;
        scorePreText = score.text;
    }

    public void Refresh(float surviveTime, int score)
    {
        surviveTime = Mathf.Floor(surviveTime*100)*0.01f;
        this.surviveTime.text = surviveTimePreText+surviveTime.ToString("0.00")+"/"+maxTime;
        this.score.text = scorePreText + score;
    }
}
