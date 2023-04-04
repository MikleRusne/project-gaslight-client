using System;
using System.Collections.Generic;
using GameActions;
using UnityEngine;
using UnityEngine.Events;

public class Orchestrator : MonoBehaviour
{
    public static Orchestrator inst;

    // public Dictionary<EFaction, List<GameAction>> QueuedActions = new Dictionary<EFaction, List<GameAction>>();
    private void Awake()
    {
        if (inst != null)
        {
            Destroy(gameObject);
        }
        else
        {
            inst = this;
        }
    }

    public List<GameAction> PendingActions = new List<GameAction>();
    
    private float time = 0;
    public bool shouldTick = true;
    public float TickTime = 3.0f;

    public UnityEvent OrchTick;

    public GameObject UICanvas;
    //Acts as a way to distinguish between ticks, for continuity purposes
    private int TotalTicks;
    public TimeBar timeBar;
    void Start()
    {
        foreach (EFaction Faction in Enum.GetValues(typeof(EFaction)))
        {
            // QueuedActions[Faction] = new List<GameAction>();
        }

        // timeBar= UICanvas.transform.Find("TimeBar").GetComponent<TimeBar>();
        PendingActions = new List<GameAction>();
    }

    public bool PlayerTurn;
    
    public UnityEvent<bool, int> TurnMade;
    // Update is called once per frame
    public void EnqueueAction(GameAction action)
    {
        PendingActions.Add(action);
    }

    public bool Verbose;

    void DebugActions()
    
    {
        if (Verbose)
        {
            Debug.Log("Current actions are:");
            foreach (var pendingAction in PendingActions)
            {
                Debug.Log(pendingAction.ToString());
            }
        }
    }
    public async void TickFunction()
    {
        shouldTick = false;
        DebugActions();
        if (PendingActions.Count > 0)
        {
            //The action on the tippy top should be the one to execute

            foreach (var pendingAction in PendingActions)
            {
                await pendingAction.Execute();
            }

            PendingActions = new List<GameAction>();

        }
        TurnMade.Invoke(PlayerTurn, ++TotalTicks);
        PlayerTurn = !PlayerTurn;
        shouldTick = true;
    }
    
    
    void Update()
    {
        if (shouldTick)
        {
            time += Time.deltaTime;
            // timeBar.SetProgress(time/TickTime);
            if (time >= TickTime)
            {
                // Debug.Log("Ticking");
                TickFunction();
                time -= TickTime;
            }
        }
        
    }

    public string[] DisplayActions()
    {
        if (PendingActions.Count == 0)
        {
            return new string[1] { "No pending actions." };
        }
        string[] temp = new string[PendingActions.Count];
        for (int i =0; i<PendingActions.Count; i++)
        {
            temp[i] = PendingActions[i].Invoker.name + ": " + PendingActions[i].ToString();
        }

        return temp;
    }
    
    
}
