using UnityEngine;
using UnityEngine.UI;

namespace Thunder.UI
{
    public class SurvivalNoliUI : BaseUi
    {
        private string surviveTimePreText = "";
        private string scorePreText = "";

        private Text surviveTime;
        private Text score;

        protected override void Awake()
        {
            base.Awake();
            surviveTime = transform.Find("surviveTime").GetComponent<Text>();
            score = transform.Find("score").GetComponent<Text>();

            surviveTimePreText = surviveTime.text;
            scorePreText = score.text;
        }

        public void Refresh(float surviveTime, int score)
        {
            surviveTime = Mathf.Floor(surviveTime * 100) * 0.01f;
            this.surviveTime.text = surviveTimePreText + surviveTime.ToString("0.00");
            this.score.text = scorePreText + score;
        }
    }
}
