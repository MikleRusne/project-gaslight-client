using System.Threading.Tasks;
using Characters;
using UnityEngine;

namespace GameActions
{
    public  class GameAction
    {
        public Character Invoker;
        public EFaction InvokingFaction;

        public float cost = 0;
        public virtual bool isViable()
        {
            return false;
        }
        public virtual Task Execute()
        {
            return null;
        }

        public virtual bool isComplete()
        {
            return false;
        }

        public GameAction(Character Invoker, EFaction invokingFaction, float cost = 0)
        {
            this.Invoker = Invoker;
            this.InvokingFaction = invokingFaction;
            this.cost = cost;
        }

        public new virtual string ToString()
        {
            return "Default game action";
        }
    }
}