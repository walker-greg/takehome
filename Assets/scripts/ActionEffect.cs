using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionEffect
{
    public ActionConfig config;
    float attackBuff;
    Agent target;

    float timeSinceInterval;
    float timeSinceStart;
    float amountApplied;

    // take into consideration any attack buff for calculating amounts.  can't buff past 0
    private float Amount => (config.Amount > 0) ?
        Mathf.Max(0f, config.Amount + attackBuff) :
        Mathf.Min(0f, config.Amount + attackBuff);

    public ActionEffect(ActionConfig config, float attackBuff, Agent target)
    {
        this.config = config;
        this.attackBuff = attackBuff;
        this.target = target;
        // start with a full interval timer so we apply once immediately:
        this.timeSinceInterval = config.Interval;
    }

    public override string ToString()
    {
        var t = config.Time > 0 ? (" " + (config.Time - timeSinceStart).ToString("N1")) : string.Empty;
        return "[" + config.ToString(attackBuff) + t + "]";
    }

    // return true if still busy, false when done:
    public bool Apply(float dt)
    {
        timeSinceInterval += dt;
        timeSinceStart += dt;

        // done?
        if(timeSinceStart >= config.Time)
        {
            ApplyRemainingEffect();
            return false;   // effect done
        }

        // apply an interval?
        if (config.Type == ActionType.Damage)
        {
            while (timeSinceInterval >= config.Interval)
            {
                timeSinceInterval -= config.Interval;
                ApplyIntervalEffect();
            }
        }
        return true;    // effect still active
    }

    // apply one interval:
    private void ApplyIntervalEffect()
    {
        // buffs don't actively do anything:
        if (config.Type != ActionType.Damage)
            return;

        // interval damage:
        var amount = Amount * config.Interval / config.Time;

        // clamp to remaining:
        if (Mathf.Abs(amount) > Mathf.Abs(Amount - amountApplied))
            amount = Amount - amountApplied;

        // apply:
        target.health = Mathf.Clamp(target.health - amount, 0, target.config.MaxHealth);        
        amountApplied += amount;

        Debug.Log("Effect: Applied " + amount + " intervalDamage to " + target.name + ", health = " + target.health + ".  Effect time left: " + (config.Time - timeSinceStart) + ", amount left: " + (Amount - amountApplied));
    }

    // apply anything left:
    private void ApplyRemainingEffect()
    {
        // buffs don't actively do anything:
        if (config.Type != ActionType.Damage)
            return;

        // apply remaining interval damage:
        var amount = Amount - amountApplied;

        // if its instant damage, reduce amount by target defense
        if(config.Time == 0)
        {
            amount = Mathf.Max(0, amount - target.CurrentDefense);
        }


        target.health = Mathf.Clamp(target.health - amount, 0, target.config.MaxHealth);

        Debug.Log("Effect: Applied " + amount + " damage to " + target.name + ", health = " + target.health);

    }
}
