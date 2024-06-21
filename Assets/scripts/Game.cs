using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public List<AgentConfig> agentConfigs = new List<AgentConfig>();

    public GameObject UIRoot;
    public TMPro.TMP_Text VictoryText;
    public GameObject AgentUIPrefab;

    private List<Agent> agents = new List<Agent>();

    void Start()
    {
        // create all agents:
        for (int a = 0; a < agentConfigs.Count; a++)
        {
            agents.Add(new Agent(agentConfigs[a], a, this));
        }
    }


    void Update()
    {
        HashSet<int> aliveTeams = new HashSet<int>();
        var dt = Time.deltaTime;
        foreach (var agent in agents)
            if (agent.IsAlive)
            {
                agent.Update(dt);
                aliveTeams.Add(agent.config.TeamId);
            }

        // check for victory:
        if(aliveTeams.Count == 1)
        {
            // get the int.  Linq.Single():
            int teamId = 0;
            foreach (var team in aliveTeams) {
                teamId = team;
                break;
            }
            // show it:
            VictoryText.text = "Team " + teamId + " Wins!";
            VictoryText.gameObject.SetActive(true);
            this.enabled = false;
        }        
    }


    public List<Agent> GetAgents(Predicate<Agent> filter)
    {
        return agents.FindAll(filter);
    }

    // get all the other agents on my team but not me
    public List<Agent> GetOtherAliveAgentsOnTeam(Agent self)
    {
        return GetAgents(agent =>
            agent.IsAlive &&
            agent != self &&
            agent.config.TeamId == self.config.TeamId);
    }

    // get all the agents not on this team:
    public List<Agent> GetAliveAgentsNotOnTeam(int teamId)
    {
        return GetAgents(agent =>
            agent.IsAlive &&
            agent.config.TeamId != teamId);
    }
}
