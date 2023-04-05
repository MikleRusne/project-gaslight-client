using System;
using System.Collections.Generic;
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
        public VisualTreeAsset _moveDisplayPrefab;
        public DirectiveFactory _directiveFactory = new DirectiveFactory();
        public VisualTreeAsset _playerCharacterIconTemplate;
        public PlayerController(DirectiveManager manager,
            DirectiveIconManager directiveIcon,
            VisualElement playerControllerUI,
            VisualTreeAsset moveDisplayPrefab,
            VisualTreeAsset playerCharacterIconTemplate,
            SelectUnderMouse selector)
        {
            _directiveIcon = directiveIcon;
            _directiveManager = manager;
            _playerControllerUI = playerControllerUI;
            _moveDisplayPrefab = moveDisplayPrefab;
            _selector = selector;
            this._playerCharacterIconTemplate = playerCharacterIconTemplate;
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

        private bool isPlayerChanged = true;
        async Task PerformMove(SimpleCharacter character, String name)
        {
            Debug.Log("Performing " + name + " with " + character.name);
            //Instantiate the directive with that name
            var tempDirective = _directiveFactory.GetDirective(name);
            tempDirective.Invoker = character;
            //Ok so you can't await in a while loop. However, if you wrap it in an async, then you can await inside it
            //So we do that
            List<int> selectedTiles = new List<int>();
                // Level.instance.TurnOnAllDisplays();
            while (tempDirective.MoreConditions())
            {
                // Debug.Log("Waiting for tile selection");
                var selectedTile = await _selector.SelectTile((Tile tile) =>
                {
                    return tempDirective.IsLocationValid(tile.tileKey);
                });
                // Debug.Log("Tile selected");
                tempDirective.AddTarget(selectedTile.Value);
                tempDirective.StepCondition();
                selectedTiles.Add(selectedTile.Value);
                Level.instance.TurnOffAllDisplays();
            }
            // Debug.Log("Selected tile "+ selectedTile);
            // Debug.Log("Directive conditions fulfilled");
            character.passiveDirective = tempDirective;
            await character.passiveDirective.DoAction();
            Level.instance.ChangeTileDisplaySelectionState(selectedTiles.ToArray(), TileDisplay.State.Idle);
            Level.instance.ChangeTileDisplayActivationState(selectedTiles.ToArray(), false);
        }

        public int CalculateTurnAbleCharacters(List<SimpleCharacter> players)
        {
            return players
                .Count((player) => player.actionPoints > 0);
        }

        public void HandlePlayerCharacterChangeButtonState(VisualElement ChangeCharacterButton,int validCharacterCount)
        {
            if (validCharacterCount > 1)
            {
                //Remove the class that greys it out
                ChangeCharacterButton.RemoveFromClassList("no_other_characters");
            }
            else
            {
                if(! ChangeCharacterButton.ClassListContains("no_other_characters"))
                ChangeCharacterButton.AddToClassList("no_other_characters");
            }
        }
        int currentPlayerIndex = 0;

        void SwitchPlayerCharacter(List<SimpleCharacter> players)
        {
            if (CalculateTurnAbleCharacters(players) < 2)
            {
                Debug.Log("Not changing character because no other valid characters.");
                return;
            }

            int initial = currentPlayerIndex;
            //Switch to the next player character that has at least one action point left
            Debug.Log("Changing from "+ currentPlayerIndex + " to " + (currentPlayerIndex + 1) % players.Count);
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            while (players[currentPlayerIndex].actionPoints == 0 && currentPlayerIndex!=initial)
            {
                Debug.Log("Changing from "+ currentPlayerIndex + " to " + (currentPlayerIndex + 1) % players.Count);
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            }

            if (currentPlayerIndex == initial)
            {
                Debug.Log("New character same as old one");
                isPlayerChanged = false;
                return;
            }
            Debug.Log("Changing current player to " + players[currentPlayerIndex].name);
            isPlayerChanged = true;
        }
        public async Task Execute()
        {
            isPlayerChanged = true;
            var players = Level.instance.GetCharactersOfFaction(EFaction.Player);
            currentPlayerIndex = 0;
            //We have a list of players
            if (players.Count == 0)
            {
                return;
            }
            //Initialize their action points
            players.ForEach((player) =>
            {
                player.actionPoints = Mathf.RoundToInt(player.GetFloatTrait("max_action_points"));
            });

            VisualElement CharacterIconsContainer = _playerControllerUI.Q("PLAYER_CHARACTER_ICONS_CONTAINER");
            
            //Get the UI button that changes characters
            var ChangeCharacterButton = _playerControllerUI.Q("CHANGE_CHARACTER_BUTTON");
            ChangeCharacterButton.RegisterCallback<ClickEvent>((evt) =>
            {
                SwitchPlayerCharacter(players);
            });
            if (ChangeCharacterButton == null)
            {
                Debug.LogError("Couldn't find CHANGE_CHARACTER_BUTTON");
            }
            
            //Check how many characters have at least one action point
            var validCharacterCount = CalculateTurnAbleCharacters(players);
            HandlePlayerCharacterChangeButtonState(ChangeCharacterButton,validCharacterCount);
            
            //While at least one player has action points left, loop
            while ((validCharacterCount = CalculateTurnAbleCharacters(players))>0)
            {
                if (isPlayerChanged)
                {
                    var CharacterIconsList = players.Select((player, index) =>
                    {
                        var newIcon = _playerCharacterIconTemplate.Instantiate();
                        newIcon.Q("CHARACTER_ICON").style.backgroundImage = new StyleBackground(player.icon);
                        //Base class is already added
                        
                        //If character has some ap, add the available class
                        if (player.actionPoints != 0)
                        {
                            newIcon.AddToClassList("character_icon_available");
                        }
                        
                        //If it is the current character, add the final class
                        if (index == currentPlayerIndex)
                        {
                            newIcon.AddToClassList("character_icon_selected");
                        }
                        newIcon.RegisterCallback<ClickEvent>(evt => ChangeCharacterTo(index));
                        return newIcon;
                    }).ToList();
                    CharacterIconsContainer.Clear();
                    CharacterIconsList.ForEach((icon) =>
                    {
                        CharacterIconsContainer.Add(icon);
                    });
                    Debug.Log("Recreating UI");
                    //We have a player, initialize the UI
                    var currentPlayer = players[currentPlayerIndex];
                    var currentRoles = currentPlayer.roles;
                    var validMoves =currentRoles.Select((cr) => _directiveManager.GetValidDirectivesForRole(cr)).SelectMany(x=>x).ToList();
                    
                    //Get the icons for each valid move and add it to the visual element
                    var validMovesContainer = _playerControllerUI.Q<VisualElement>("VALID_MOVES");
                    var validMoveButtons = validMoves.Select((move) =>
                    {
                        var newButton = _moveDisplayPrefab.Instantiate();
                        var icon= newButton.Q("Icon");
                        
                        icon.style.backgroundImage = new StyleBackground(_directiveIcon.GetIcon(move));
                        newButton.RegisterCallback<ClickEvent>(async (evt) =>
                        {
                            //Debug log the name for now
                            await PerformMove(currentPlayer, move);
                        });
                        return newButton;
                    }).ToList();
                    validMovesContainer.Clear();
                    foreach (var validMoveButton in validMoveButtons)
                    {
                        validMovesContainer.Add(validMoveButton);
                    }
                    isPlayerChanged = false;
                }
                HandlePlayerCharacterChangeButtonState(ChangeCharacterButton,validCharacterCount);
                await Task.Yield();
            }
            Level.instance.TurnOffAllDisplays();
        }

        private void ChangeCharacterTo(int index)
        {
            if (currentPlayerIndex == index)
            {
                return;
            }

            currentPlayerIndex = index;
            isPlayerChanged = true;
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
    public VisualTreeAsset _playerCharacterIconTemplate = default;
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
        playerController = new PlayerController(_directiveManager, _directiveIcon, _playerControllerUI, validMoveTemplate, _playerCharacterIconTemplate,_selector);
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
