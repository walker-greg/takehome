using System.Collections.Generic;
using UnityEngine;

public class Agent
{
    public AgentConfig config;
    public string name;

    public float health;
    public bool IsAlive => health > 0;

    private Game game;
    private AgentUI ui;

    private float timeSinceAction;

    public List<ActionEffect> activeEffects = new List<ActionEffect>();

    public Agent(AgentConfig config, int instanceNum, Game game)
    {
        this.config = config;
        this.game = game;
        this.name = instanceNum + ":" + config.name;
        this.health = config.MaxHealth;
        this.timeSinceAction = Random.Range(0f, 1f / config.ActionsPerSecond);   // randomize starting position within action interval

        // instantiate a UI for it:
        this.ui = GameObject.Instantiate(game.AgentUIPrefab, game.UIRoot.transform).GetComponent<AgentUI>();
        this.ui.Init(this);

        // place in the UI according to team:
        int teamId = config.TeamId;
        int teamMembers = game.GetOtherAliveAgentsOnTeam(this).Count;
        
        var rectTransform  = this.ui.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector3(50 + teamId * 400, -50 - teamMembers * 120, 0);
    }

    public void Update(float dt)
    {
        // update effects on me:
        for (int a = activeEffects.Count - 1; a >= 0; a--)
        {
            var effect = activeEffects[a];
            if(effect.Apply(dt) == false)
            {
                activeEffects.RemoveAt(a);
            }
        }

        // time for an action?
        var actionsPerSecond = config.ActionsPerSecond + GetBuffEffect(ActionType.Buff_Speed);
        if (actionsPerSecond <= 0)
        {
            // if you got buffed to nothing, stop any more actions:
            timeSinceAction = 0;
        }
        else
        {
            var actionTime = 1.0f / actionsPerSecond;

            timeSinceAction += dt;
            while (timeSinceAction > actionTime)
            {
                timeSinceAction -= actionTime;
                DoAction();
            }
        }
    }

    public float GetBuffEffect(ActionType type)
    {
        // find all the buffs of this type, add up their amounts:
        var buffEffect = 0f;
        foreach (var effect in activeEffects)
            if(effect.config.Type == type)
                buffEffect += effect.config.Amount;
        return buffEffect;
    }

    public float CurrentDefense => Mathf.Max(0, config.Defense + GetBuffEffect(ActionType.Buff_Defense));

    public void DoAction()
    {
        // deep copy the action list:
        var actions = new List<ActionConfig>(config.Actions);

        while (actions.Count > 0)
        {
            // pick an action randomly:
            var action = config.Actions[Random.Range(0, config.Actions.Count)];

            // pick a target randomly:
            var targets = action.ShouldTargetFriendly() ? game.GetOtherAliveAgentsOnTeam(this) : game.GetAliveAgentsNotOnTeam(config.TeamId);
            if (targets.Count == 0)
            {
                // if there are no appropriate targets for this action, remove it from consideration and try again:
                actions.Remove(action);
                continue;
            }
            var target = targets[Random.Range(0, targets.Count)];

            // apply the action:
            float attackBuff = 0;
            if(action.Type == ActionType.Damage)
                attackBuff = GetBuffEffect(ActionType.Buff_Attack);

            var effect = new ActionEffect(action, attackBuff, target);

            Debug.Log("Action: " + name + " " + effect.ToString() + " to " + target.name);
            ui.SetLastAction(effect.ToString() + " to " + target.name);

            // apply it asap for instant damage, if its still active (DOT or buff), store it on the target:
            if (effect.Apply(0))
                target.activeEffects.Add(effect);

            break;
        }
    }

}
