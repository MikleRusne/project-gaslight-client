using System.Text;
using Characters;
using UnityEngine.Events;

namespace GameActions
{
    public enum EDirectiveType
    {
        Movement,
        Other
    }
    public class Directive
    {
        public EDirectiveType DirectiveType;
        public GameAction[] Actions;
        public int ToBePerformed;
        public bool isComplete
        {
            get { return ToBePerformed >= Actions.Length; }
        }

        public UnityEvent DirectiveCompleted;
        public Character Invoker;
        public Directive NextDirective;
        public bool Chains;
        
        //If something has changed, the directive is marked dirty
        //It must find a way to return to a stable state
        public bool Dirty;

        public  void MarkDirty()
        {
            Dirty = true;
        }
        public virtual Directive CreateReverse()
        {
            return null;
        }

        public virtual void Completed()
        {
            
        }
        public virtual void FixDirty()
        {
            
        }
        public Directive(Character Invoker, EDirectiveType DirectiveType, Directive NextDirective = null)
        {
            ToBePerformed = 0;
            this.Invoker = Invoker;
            this.DirectiveType = DirectiveType;
            if (NextDirective == null)
            {
                Chains = false;
            }
            else
            {
                Chains = true;
                this.NextDirective = NextDirective;
            }
        
        }

        public GameAction GetNextAction()
        {
            return Actions[ToBePerformed];
        }

        public override string ToString()
        {
            if (Actions.Length == 0)
            {
                return "No actions";
            }
            
            StringBuilder sb = new StringBuilder();
            if (Dirty)
            {
                sb.AppendLine("Dirty");
            };
            for (int i =0; i<Actions.Length;++i)
            {
                if (i == ToBePerformed)
                {
                    sb.Append("(Current) ");
                }
                sb.AppendLine(Actions[i].ToString());
            }
            return sb.ToString();
        }
        public void IncrementAction()
        {
            ToBePerformed++;
        }
    }

}
