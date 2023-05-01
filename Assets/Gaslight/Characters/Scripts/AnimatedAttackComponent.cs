using System.Threading.Tasks;
using UnityEngine;

namespace Gaslight
{
    public class AnimatedAttackComponent: MonoBehaviour
    {
        public Character Invoker;
        public GameObject Weapon;
        public Material WeaponMaterial;

        public void Start()
        {
            if (Weapon != null)
                WeaponMaterial = Weapon.GetComponent<Renderer>().material;
            else WeaponMaterial = null;
            Invoker = this.GetComponent<Character>();
        }

        public float dissolve;
        public async Task MaterializeWeapon(Material _targetMaterial, float start, float end)
        {
            dissolve = start;
            float timer = 0;
            while (dissolve > 0)
            {
                dissolve = Mathf.Lerp(start, end, timer);
                if (_targetMaterial.HasProperty("_Dissolve"))
                {
                _targetMaterial.SetFloat("_Dissolve", dissolve);
                    
                }
                else
                {
                    Debug.LogError("Material property does not exist");
                }
                timer += Time.deltaTime;
                await Task.Yield();
            }
            _targetMaterial.SetFloat("_Dissolve", end);
        }
        public async Task Attack(int otherlocation)
        {
            //Rotate to the target, copy this from MovementComponent
            
            if (WeaponMaterial != null)
            {
                await MaterializeWeapon(WeaponMaterial, 1, 0);
            }
            var _animator = Invoker.GetComponent<Animator>();
            if (_animator != null)
            {
                _animator.SetTrigger("Attacking");
                
            }
            var _other_character = Level.instance.GetCharacterOnTile(otherlocation);
            if (_other_character != null)
            {
                var _other_animator = _other_character.GetComponent<Animator>();
                if (_other_animator != null)
                {
                    _other_animator.SetTrigger("Being attacked");
                }
            }
            
        }
        
    }
}