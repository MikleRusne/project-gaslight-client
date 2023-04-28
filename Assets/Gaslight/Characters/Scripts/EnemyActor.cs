using Behaviors;

namespace Gaslight.Characters.Scripts
{
    public class EnemyActor : Actor
    {
        public DefaultEnemyBehavior _behavior;

        void Start()
        {
            base.Awake();
            // _behavior
            this.behavior = _behavior;
        }
    }
}