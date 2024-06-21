using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgentUI : MonoBehaviour
{
    public Image BGImage;
    public TMP_Text Title;
    public TMP_Text HealthText;
    public TMP_Text LastAction;
    public TMP_Text Effects;
    public Image HealthBarBG;
    public Image FrontHealthBar;
    public Image BackHealthBar;

    private Agent agent;

    private Color healthBarColor;
    private Color frontBarStart;
    private Color frontBarEnd;
    private Color backBarStart;
    private Color backBarEnd;
    private float barFadeTime = float.MaxValue;
    const float maxBarFadeTime = 0.2f;
    private float lastHealth;

    public void Init(Agent agent)
    {
        this.agent = agent;
        this.name = agent.name;
        this.lastHealth = agent.health;
        this.healthBarColor = FrontHealthBar.color;
        this.frontBarEnd = healthBarColor;
        this.backBarEnd = healthBarColor;
        Title.text = agent.name;
    }

    public void SetLastAction(string lastAction)
    {
        LastAction.text = "Last Action: " + lastAction;
    }

    private void Update()
    {
        var dt = Time.unscaledDeltaTime;

        // update health:
        HealthText.text = agent.health.ToString("N1") + " / " + agent.config.MaxHealth;

        if(agent.health != lastHealth)
        {
            SetupHealthBarFade(lastHealth, agent.health, agent.config.MaxHealth);
            lastHealth = agent.health;
        }
        UpdateHealthBarFade(dt);

        // dead?
        if (agent.health <= 0)
            BGImage.color = Color.black;

        // update effects list:
        var s = new StringBuilder("Effects: ");
        for(int a=0; a<agent.activeEffects.Count; a++)
        {
            if (a != 0)
                s.Append(", ");
            s.Append(agent.activeEffects[a].ToString());
        }
        Effects.text = s.ToString();
    }

    private void SetupHealthBarFade(float oldHealth, float newHealth, float maxHealth)
    {
        var barWidth = HealthBarBG.GetComponent<RectTransform>().sizeDelta.x;

        // if health changed up, back bar is new value green fade to white, front bar is old value white
        if (newHealth > oldHealth)
        {
            backBarStart = Color.green;
            backBarEnd = healthBarColor;
            var backRT = BackHealthBar.GetComponent<RectTransform>();
            backRT.sizeDelta = new Vector2(barWidth * newHealth / maxHealth, backRT.sizeDelta.y);

            frontBarStart = healthBarColor;
            frontBarEnd = healthBarColor;
            var frontRT = FrontHealthBar.GetComponent<RectTransform>();
            frontRT.sizeDelta = new Vector2(barWidth * oldHealth / maxHealth, frontRT.sizeDelta.y);
        }

        // if health changed down, back bar is old value red fade to clear, front bar is new value white
        if (newHealth < oldHealth)
        {
            backBarStart = Color.red;
            backBarEnd = new Color(1, 0, 0, 0); // transparent red
            var backRT = BackHealthBar.GetComponent<RectTransform>();
            backRT.sizeDelta = new Vector2(barWidth * oldHealth / maxHealth, backRT.sizeDelta.y);

            frontBarStart = healthBarColor;
            frontBarEnd = healthBarColor;
            var frontRT = FrontHealthBar.GetComponent<RectTransform>();
            frontRT.sizeDelta = new Vector2(barWidth * newHealth / maxHealth, frontRT.sizeDelta.y);
        }

        barFadeTime = 0;
    }

    private void UpdateHealthBarFade(float dt)
    {
        barFadeTime += dt;
        var fadeI = barFadeTime / maxBarFadeTime;
        FrontHealthBar.color = Color.Lerp(frontBarStart, frontBarEnd, fadeI);
        BackHealthBar.color = Color.Lerp(backBarStart, backBarEnd, fadeI);
    }
}
