using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CleverCrow.Fluid.BTs.Trees;
using TaskStatus = CleverCrow.Fluid.BTs.Tasks.TaskStatus;

public class cubeai : MonoBehaviour
{
    [SerializeField]
    private BehaviorTree _tree;
    [SerializeField]
    private bool flag = false;

    private int counter = 0;
    private int max = 60;
    async Task ChangeAfterDuration()
    {
        await Task.Delay(5000);
        
        flag = true;
    }
    void Awake()
    {
        _tree = new BehaviorTreeBuilder(gameObject)
            .Sequence()
            // .Selector()
            .Condition("This should be in a different color", () =>
            {
                return false;
            })
            .Do("My Action", () => {
                transform.Translate(0.1f,0.0f,0.1f);
                return TaskStatus.Success;
            })
            .End()
            .Build();   
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ChangeAfterDuration();
        if(counter++<max)
        _tree.Tick();
    }
}
