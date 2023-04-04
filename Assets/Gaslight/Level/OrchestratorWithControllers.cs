using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gaslight.Characters.Logic;
using Tiles;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace GameManagement{
    public class PlayerController
    {
        public SelectUnderMouse _selector;
        public DirectiveManager _directiveManager;
        public DirectiveIconManager _directiveIcon;
        public VisualElement _playerControllerUI;
        public VisualElement _validMoves;
        public VisualTreeAsset _moveDisplayPrefab;
        public PlayerController(DirectiveManager manager,
            DirectiveIconManager directiveIcon,
            VisualElement playerControllerUI,
            VisualTreeAsset moveDisplayPrefab,
            SelectUnderMouse selector)
        {
            _directiveIcon = directiveIcon;
            _directiveManager = manager;
            _playerControllerUI = playerControllerUI;
            _moveDisplayPrefab = moveDisplayPrefab;
            _selector = selector;
        }

        public bool acceptInput = false;
        public bool block = false; 
        public void AcceptInput()
        {
            if (acceptInput)
            {
                block = false;
            }
        }

        void PerformMove(SimpleCharacter character, String name)
        {
            Debug.Log("Performing " + name + " with " + character.name);
        }
        public async Task Execute()
        {
            var players = Level.instance.GetCharactersOfFaction(EFaction.Player);
            Level.instance.TurnOnAllDisplays();
            var selectedTile = await _selector.SelectTile((Tile tile) =>
            {
                return true;
            });
            Debug.Log("Selected tile "+ selectedTile);
            var currentPlayer = players[0];
            var currentRoles = currentPlayer.roles;
            var validMoves =currentRoles.Select((cr) => _directiveManager.GetValidDirectivesForRole(cr)).SelectMany(x=>x).ToList();
            foreach (var validMove in validMoves)
            {
                Debug.Log(validMove);
            }

            validMoves.Add("test");
            //Get the icons for each valid move and add it to the visual element
            var validMovesContainer = _playerControllerUI.Q<VisualElement>("VALID_MOVES");
            var validMoveButtons = validMoves.Select((move) =>
            {
                var newButton = _moveDisplayPrefab.Instantiate();
                var icon= newButton.Q("Icon");
                icon.style.backgroundImage = new StyleBackground(_directiveIcon.GetIcon(move));
         
                return newButton;
            }).ToList();
            validMovesContainer.Clear();
            foreach (var validMoveButton in validMoveButtons)
            {
                validMovesContainer.Add(validMoveButton);
            }
            acceptInput = true;
            block = true;
            while (block)
            {
                await Task.Yield();
            }
            Level.instance.TurnOffAllDisplays();
            await (Task.Delay(2000));
        }
    }

    public class EnemyController
    {
        public CameraController cam;
        public GameObject CurrentTurnObjectPrefab;
        public CurrentTurnFollower CurrentTurnObject;
        public EnemyController(GameObject inp, CameraController cam)
        {
            this.cam = cam;
            cam.follow = false;
            CurrentTurnObjectPrefab = inp;
            CurrentTurnObject = GameObject.Instantiate(CurrentTurnObjectPrefab, Vector3.zero,
                Quaternion.Euler(-90.0f, 0.0f, 0.0f)).GetComponent<CurrentTurnFollower>();
            CurrentTurnObject.gameObject.SetActive(false);
        }
        public async Task PerformDirectives(){
            var enemies = Level.instance.GetCharactersOfFaction(EFaction.Enemy);
             StringBuilder sb = new StringBuilder();
             foreach (var enemy in enemies)
             {
                 sb.Append(enemy.name + ",");
             }
             // Debug.Log("Got enemies: "+ sb.ToString() );
             foreach (var enemy in enemies)
             {
                 cam.follow = true;
                 await cam.Follow(enemy.transform);
                 CurrentTurnObject.gameObject.SetActive(true);
                 // CurrentTurnObject.enabled = true;
                 CurrentTurnObject.target = enemy.transform;
                 enemy.behavior.Tick();
                 // Debug.Log("Making "+ enemy.name + " perform passive action.");
                 await enemy.passiveDirective.DoAction();
                CurrentTurnObject.gameObject.SetActive(false);
                 // CurrentTurnObject.enabled = false;
                 cam.follow = false;
                 await Task.Delay(millisecondsDelay: 100);
             }
             // Debug.Log("All directives complete");
             return;
         }


        public async Task Execute()
        {
             // Debug.Log("Starting directives");
             await PerformDirectives();
             // await (Task.Delay(2000));
             // Debug.Log("Moving on");
        }
    }
public class OrchestratorWithControllers : MonoBehaviour
{
    private UIDocument _uiDocument;
    [SerializeField] public GameObject currentTurnFollowerPrefab;
    public float CurrentTurnFollowerYOffset = default;
    public VisualElement _enemyMovesUI;
    public VisualElement _playerMovesUI;
    public VisualElement _playerControllerUI;
    public CameraController cam = default;
    public SelectUnderMouse _selector;
    public PlayerController playerController = default;
    public EnemyController enemyController = default;

    public VisualTreeAsset validMoveTemplate = default;
    public DirectiveManager _directiveManager;
    public DirectiveIconManager _directiveIcon;

    public void OnValidate()
    {
        CurrentTurnFollower.offset = new Vector3(0.0f, CurrentTurnFollowerYOffset, 0.0f);
    }
    [ContextMenu("Accept")]
    public void PlayerControllerInput()
    {
        playerController.AcceptInput();
    }
    // public Dictionary<EFaction, List<GameAction>> QueuedActions = new Dictionary<EFaction, List<GameAction>>();
    public void Awake()
    {
        _uiDocument = this.GetComponent<UIDocument>();
        _enemyMovesUI = _uiDocument.rootVisualElement.Q("EnemyMovesContainer");
        _playerMovesUI = _uiDocument.rootVisualElement.Q("PlayerMovesContainer");
        _playerControllerUI = _uiDocument.rootVisualElement.Q("PLAYER_CONTROLLER");
        playerController = new PlayerController(_directiveManager, _directiveIcon, _playerControllerUI, validMoveTemplate, _selector);
        enemyController = new EnemyController(currentTurnFollowerPrefab, cam);
        HideEnemyTurnUI();
        HidePlayerTurnUI();
    }
    
    public void OnLevelGenerated()
    {
        Level.instance.TurnOffAllDisplays();
        TurnLoop();
    }
    public bool playerTurn = default;
    
    //Add level ending functionality somewhere
    
    public UnityEvent LevelEnded;
    

   
    public async void TurnLoop()
    {
        if (playerTurn)
        {
            await PlayerTurn();
            playerTurn = false;
        }
        else
        {
            await EnemyTurn();
            playerTurn = true;
        }
        TurnLoop();
    }

    void ShowEnemyTurnUI()
    {
        // Debug.Log("Showing Enemy turn");
        _enemyMovesUI.style.display = DisplayStyle.Flex;
    }

    void HideEnemyTurnUI()
    {
        // Debug.Log("Hiding Enemy turn");
        _enemyMovesUI.style.display = DisplayStyle.None;
    }
    void ShowPlayerTurnUI()
    {
        _playerMovesUI.style.display = DisplayStyle.Flex;
        _playerControllerUI.style.display = DisplayStyle.Flex;
    }

    void HidePlayerTurnUI()
    {
        _playerMovesUI.style.display = DisplayStyle.None;
        _playerControllerUI.style.display = DisplayStyle.None;
        
    }
    

    async Task PlayerTurn()
    {
        playerTurn = true;
        ShowPlayerTurnUI();
        await playerController.Execute();
        HidePlayerTurnUI();
    }
    

    public async Task EnemyTurn()
    {
        playerTurn = false;
        ShowEnemyTurnUI();
        await enemyController.Execute();
        HideEnemyTurnUI();
    }
}

}
