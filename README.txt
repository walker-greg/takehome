readme.txt



Two hours didn't seem like a lot for this, so some starting assumptions to simplify/clarify:

- no smartness: targets are selected randomly, actions are selected randomly. (but from valid options).
- defense directly reduces any instant attack amount; it doesn't applied to DoT.
- buffs/debuffs can only add/subtract to the stats, not scale them.
- buffs/debuffs can infinitely stack.
- damage=heal and buff=debuff, just with the amount positive/negative.
	- this defines targeting, ie you can only damage an enemy, not a teammate.
- instant damage is just DoT with an actionTime of 0.  this way the implementations are the same.
	- it also means you can technically define an "instant buff", though this would do nothing.




Data Configs:

ScriptableObject AgentConfig:
	- int teamId
	- float health
	- float defense
	- float actionsPerSecond
	- list<ActionConfig> actions

struct ActionConfig:
	- Enum type (damage, buff atk, buff def, buff spd)
	- float amount
	- float interval (interval when each DoT fraction is applied)
	- float actionTime  (0=instant damage/heal, >0 = DoT, >0 for all buffs)
 
struct WorldConfig
	- list<AgentConfig> agents

Agent type definitions are done by scriptable objects, in "/Assets/data/".  These are then added to
the Game script (on the "game" object in the scene) to define the starting players in the game.  A
single agent config can be used to create multiple runtime agents.  The team is baked into the config,
so to make a similar agent on both teams you'd need two configs.  Nothing restricts this to two teams,
it should work with N teams.




Runtime:

Game:
- owns all the runtime agents
- updates the living ones each frame
- checks for victory when there's only one team left alive.

Agent:
- runtime instance of an AgentConfig.
- owns an AgentUI instance about itself.
- has a list of all ActionEffects currently applied to itself, updates them.
- runs a timer to select and start an action

ActionEffect:
- runtime instance of an ActionConfig.
- if its an instant effect (instant damage/heal), the agent creates and applies it immediately to the target.
- if its an effect over time (DoT/HoT/Buff/Debuff), the agent creates and stores it on the target agent.
- bakes in the current AttackBuff amount (if any) of the agent that created it.

AgentUI:
- runtime controller for the UI prefab that draws agent info




The game config is setup now with 4 on 4 with a couple different classes of agent.  Seems somewhat
trivially balanced, as in I've seen both teams win at some point.  You can define new classes with a
new AgentConfig scriptable object, then place that into the list in the Game script.

It definitely took more than 2 hours.  I did it broken up through the day, so I'm not sure how much
exactly, but I'd guess 3 maybe 4.  A lot of the time was spent testing - each component is fairly simple,
but they combine into a lot of possibilities.  There are a lot of options for an attack, and then all 
those with a buff applied, and then all those against a buffed target, etc.









