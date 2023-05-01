using System;
using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Behaviors;
using Characters;
using CleverCrow.Fluid.BTs.Trees;
using Gaslight.Characters.Descriptors;
using UnityEngine.Events;
using Gaslight.Characters.Logic;


[Serializable]
public enum EFaction
{
    Player,            //Hurts enemies and neutral, acts on Player Turn only
    Enemy,             //Hurts player and neutrals, acts on Enemy Turn only
    Neutral,           //Hurts  regardless of faction, acts every turn
    Chaos              //Also hurts regardless of faction, acts every turn, might phase out later
}
[RequireComponent(typeof(BoxCollider))]
public abstract class Character : MonoBehaviour
{
    public Vector3 _detectionRaycastOffset = default;
    public abstract MovementComponent movementComponent { get; }
    public List<Roles> roles= new List<Roles>();
    public Sprite icon;
    [Serializable]
    public struct NamedFloatTrait{
        public string name;
        public float value;
    }

    public BoxCollider _boundingBox;
    public abstract int GetMaxTraversibleTilesInOneTurn();

    public abstract (bool, float) GetPathfindingHeuristic(Node previous,int to);
    public abstract Task Attack(int location);
    public abstract Task<(bool oneGo, List<int>path)> GetPathTowards(int start, int end);

    public abstract Task<bool> MoveToTile(int index);
    //This one forces the character to move that path even if its speed does not allow it
    public abstract Task<bool> TraversePath(List<int> path);

    [Serializable]
    public struct NamedStringTrait
    {
        public string name;
        public string value;
    }
    public Level gameLevel;
    //This should be set by the level, as the character starts
    public Tile MyTile;
    public UnityEvent<(string key, float value)> FloatTraitChanged;
    public UnityEvent<(string key, string value)> StringTraitChanged;
    //From murdle, I have discovered that the performance benefit of dictionary
    //is not worth the headache of not being serialized
    //Since traits will usually just be 10/20 per object, searching a list
    //won't take long
    public List<NamedFloatTrait> floatTraits= new List<NamedFloatTrait>();
    public List<NamedStringTrait> stringTraits = new List<NamedStringTrait>();

    public UnityEvent<int> ActionPointsChanged;
    public UnityEvent<int> HealthChanged;
    public int actionPoints=0;

    public void SetActionPoints(int newValue)
    {
        int old = this.actionPoints;
        this.actionPoints = newValue;
        ActionPointsChanged.Invoke(old);
    }
    public int health=0;
    public void SetHealth(int newValue)
    {
        int old = this.health;
        this.health = newValue;
        HealthChanged.Invoke(old);
    }
    public EFaction faction = EFaction.Neutral; 
    protected void Awake()
    {
        if (gameLevel != null)
        {
            gameLevel.TileSelected.AddListener(OnTileSelected);
            gameLevel.TileDeselected.AddListener(OnTileDeselected);
            //Set my rotation to follow the direction I'm facing
            transform.rotation = Quaternion.Euler(0.0f,Level.instance.RotationBetween(_facingDirection),0.0f);
        }

        this._boundingBox = this.GetComponent<BoxCollider>();
        FloatTraitChanged.AddListener(OnTraitChanged);
        StringTraitChanged.AddListener(OnStringTraitChanged);
        this.actionPoints = (int)GetFloatTrait("max ap");
    }

    public abstract void OnTileChangeSelf();
    public abstract void OnCharacterTileChanged(int location, Character character);

    public void OnDestroy()
    {
        if (gameLevel != null)
        {
            gameLevel.TileDeselected.RemoveListener(OnTileDeselected);
            gameLevel.TileSelected.RemoveListener(OnTileSelected);
        }
    } 
    void ErrorOnDuplicateTraits()
    {
        if (floatTraits == null)
        {
            return;
        }

        //N^2 but will worry about optimization later
        var duplicates = floatTraits.GroupBy(p => p.name)
            .Where(p => p.Count() > 1)
            .Select(p=>p.Key).ToList();
        if (duplicates.Count > 0)
        {
            Debug.LogWarning("Found duplicates " + String.Join(", ",duplicates.ToArray()));
        }
    }

    virtual public void OnValidate()
    {
        ErrorOnDuplicateTraits(); 
    }

    
    public void Start()
    {
        if (isTrait("max health"))
        {
            this.health = Mathf.RoundToInt(GetFloatTrait("max health"));
        }
        else
        {
            this.health = 5;
        }

        HealthChanged.Invoke(0);
        if (isTrait("max action points"))
        {
            this.actionPoints = Mathf.RoundToInt(GetFloatTrait("max action points"));
        }
        ActionPointsChanged.Invoke(0);
        Level.instance.CharacterChangedTile.AddListener(OnCharacterTileChanged);

    }
    abstract public void OnTileSelected();

    abstract public void OnTileDeselected(int index);

    //This is all they need
    virtual public float GetFloatTrait(string key){
        if(isTrait(key)){
            var RequiredTrait=floatTraits.First(t=>t.name==key);
            return RequiredTrait.value;
        }else{
            return 0.0f; 
        }
    }
    virtual public void SetFloatTrait(string key, float value)
    {
        float oldValue = 0.0f; 
        if(isTrait(key)){
            var RequiredTrait=floatTraits.First(t=>t.name==key);
            oldValue = RequiredTrait.value;
            RequiredTrait.value = value;
        }else{
            floatTraits.Add(new NamedFloatTrait{
                name= key,
                value= value
            });
        }

        FloatTraitChanged.Invoke((key, oldValue));
    }
    virtual public void SetStringTrait(string key, string value)
    {
        string oldValue = ""; 
        if(isTrait(key)){
            var RequiredTrait=stringTraits.First(t=>t.name==key);
            oldValue = RequiredTrait.value;
            RequiredTrait.value = value;
        }else{
            stringTraits.Add(new NamedStringTrait(){
                name= key,
                value= value
            });
        }

        StringTraitChanged.Invoke((key, oldValue));
    }
    public bool isTrait(string key){
        if (floatTraits.Any(t=>t.name == key)){
            return true;
        }else{
            return false;
        }
    }
    public bool isStringTrait(string key){
        if (stringTraits.Any(t=>t.name == key)){
            return true;
        }else{
            return false;
        }
    }

    public List<String> getValidTraits()
    {
        return floatTraits.Select((trait => trait.name)).ToList();
    }
    public virtual void OnTraitChanged( (string key, float oldValue) changed )
    {
        
    }

    public void FillFromArchetype(ArchetypeDescriptor inp)
    {
        FillFloatTraits(inp.floatTraits);
        FillStringTraits(inp.stringTraits);
        this.roles.AddRange(inp.roles);
    }

    public void FillFloatTraits(NamedFloatTrait[] inp)
    {
        foreach (var namedFloatTrait in inp)
        {
           this.SetFloatTrait(namedFloatTrait.name, namedFloatTrait.value); 
        }
        
    }

    public void FillStringTraits(NamedStringTrait[] inp)
    {
        foreach (var namedStringTrait in inp)
        {
           this.SetStringTrait(namedStringTrait.name, namedStringTrait.value); 
        }
        
    }
    
    public virtual void OnStringTraitChanged((string key, string value) changed)
    {
    
    }
    public virtual void Highlight(){}

    public virtual void Dehighlight()
    {
    }

    public bool isHighlighted;

    public virtual void Select()
    {
    }

    public virtual void Deselect()
    {
    }

    public bool isSelected = false;

    [SerializeField]
    public Directive passiveDirective;
    [SerializeField] public Behavior behavior;
    public BehaviorTree bt;
    public Level.ELevelDirection _facingDirection;
    public abstract void OnAttacked(Character other);
    public abstract void OnAttack(Character other);
    public abstract void Attack(Character target);
}
