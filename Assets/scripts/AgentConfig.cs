using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "AgentConfig", menuName = "ScriptableObjects/AgentConfig", order = 1)]
public class AgentConfig : ScriptableObject
{
    public int TeamId;
    public float MaxHealth;
    public float Defense;
    public float ActionsPerSecond;
    public List<ActionConfig> Actions;
}

public enum ActionType
{
    Damage,
    Buff_Attack,
    Buff_Defense,
    Buff_Speed,
}

[Serializable]
public class ActionConfig
{
    public ActionType Type;
    public float Amount;        // total amount of health/stat change applied 
    public float Time;          // total time of affect for DOT/buff.  0 for instant damage attack.
    public float Interval;      // DOT interval.  Amount*Interval/Time is applied every Interval seconds.


    public bool ShouldTargetFriendly()
    {
        // -damage is a heal for friendlies, +damage is a hurt for enemies
        if(Type == ActionType.Damage)
            return Amount < 0;
        // for buffs, +numbers for friendlies, -numbers for enemies
        return Amount > 0;
    }

    public override string ToString()
    {
        return ToString(0);
    }
    public string ToString(float amountBuff)
    {
        if (Type == ActionType.Damage)
        {
            var type = (Amount > 0) ? "D" : "H";
            var time = (Time == 0) ? string.Empty : "oT";
            var amount = (Amount > 0) ? Mathf.Max(0, Amount + amountBuff) : Mathf.Min(0, Amount + amountBuff);
            return type + time + " " + Math.Abs(amount);
        }

        // buffs:
        var buff = (Type == ActionType.Buff_Attack) ? "Atk" :
                (Type == ActionType.Buff_Defense) ? "Def" : "Spd";
        return buff + Amount.ToString("+0;-#");
    }
}