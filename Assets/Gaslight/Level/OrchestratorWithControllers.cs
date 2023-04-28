using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gaslight.Characters.Logic;
using Tiles;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace GameManagement{
    //TODO: Add functionality to highlight the tile that the player is under
    [Serializable]
    public class PlayerController
    {
        public Color _currentPlayerTileColor;
        public SelectUnderMouse _selector;
        public DirectiveManager _directiveManager;
        public DirectiveIconManager _directiveIcon;
        public VisualElement _playerControllerUI;
        public VisualTreeAsset _moveDisplayPrefab;
        public DirectiveFactory _directiveFactory = new DirectiveFactory();
        public VisualTreeAsset _playerCharacterIconTemplate;
        public CancellationTokenSource directiveSelectCancel = new CancellationTokenSource();
        public CameraController _cam;
        public PlayerController(DirectiveManager manager,
            DirectiveIconManager directiveIcon,
            CameraController cam,
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
            _playerCharacterIconTemplate = playerCharacterIconTemplate;
            _cam = cam;
        }

        public void CancelDirective()
        {
            Debug.Log("Trying to cancel directive");
            directiveSelectCancel.Cancel();
        }
        public bool acceptInput = false;
        public bool block = false; 
        [ContextMenu("Accept")]
        public void AcceptInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed&& acceptInput)
            {
                block = false;
            }
        }

        private bool isPlayerChanged = true;
        private bool requestPlayerChange = false;
        public string currentlyExecutingMove = "";
        private bool isMoveRequested = false;
        private bool lockMoveSelection = false;
        [FormerlySerializedAs("players")] public List<SimpleCharacter> _players;
        async Task PerformMove(SimpleCharacter character, String name, CancellationToken ct)
        {
            CancellationTokenSource tileSelectionCTS = new CancellationTokenSource();
            Debug.Log("Starting performance of " + name + " with " + character.name);
            //Instantiate the directive with that name
            var tempDirective = _directiveFactory.GetDirective(name);
            
            tempDirective.Invoker = character;
            //Ok so you can't await in a while loop. However, if you wrap it in an async, then you can await inside it
            //So we do that
            List<int> selectedTiles = new List<int>();
            ct.Register(() =>
            {
                Debug.Log("Cancelling directive whereever");
                tileSelectionCTS.Cancel();
            });
            //Turn on the tile under the player and to selected to let them know
            var playerInitialTile = character.MyTile.tileKey;
            Level.instance.TurnOffAllDisplays();
            Level.instance.ChangeTileDisplayActivationState(new int[]{playerInitialTile}, true);
            while (tempDirective.MoreConditions())
            {
                tileSelectionCTS = new CancellationTokenSource();
                
                    Debug.Log("Waiting for tile selection");
                    int? selectedTile=null;
                    try
                    {
                        selectedTile =
                            await _selector.SelectTile(
                                (Tile tile) => { return tempDirective.IsLocationValid(tile.tileKey); },
                                ct);

                    }
                    catch (OperationCanceledException e)
                    {
                        Debug.Log("No longer awaiting tile selection for " + name);
                        Level.instance.TurnOffAllDisplays();
                        // Level.instance.ChangeTileDisplayActivationState(new int[]{playerInitialTile}, false);
                        // Level.instance.ResetTileDisplayColor(playerInitialTile);
                        // Level.instance.ChangeTileDisplaySelectionState(selectedTiles.ToArray(), TileDisplay.State.Idle);
                        // Level.instance.ChangeTileDisplayActivationState(selectedTiles.ToArray(), false);
                        ct.ThrowIfCancellationRequested();
                        // Debug.Log("After exception");
                    }

                    Debug.Log("Tile selected");
                    if (selectedTile == null)
                    {
                        Debug.Log("Selected tile null for " + name);
                    }
                    tempDirective.AddTarget(selectedTile.Value);
                    tempDirective.StepCondition();
                    selectedTiles.Add(selectedTile.Value);
                    Level.instance.TurnOffAllDisplays();
                
                
            }
            character.passiveDirective = tempDirective;
            // Level.instance.ChangeTileDisplayActivationState(new int[]{playerInitialTile}, false);
            // Level.instance.ChangeTileDisplayColor(new int[]{playerInitialTile}, TileDisplay.State.Idle);
            Level.instance.ResetTileDisplayColor(playerInitialTile);
            lockMoveSelection = true;
            Debug.Log("Performing " + name + " with " + character.name);
            await character.passiveDirective.DoAction();
            Level.instance.ResetTileDisplayColor(selectedTiles.ToArray());
            Level.instance.ChangeTileDisplayActivationState(selectedTiles.ToArray(), false);
            

            character.actionPoints -= tempDirective.actionPointCost;
            if (character.actionPoints <= 0)
            {
                requestPlayerChange = true;
            }

            await Task.Delay(millisecondsDelay:1000);
            lockMoveSelection = false;
        }

        public int CalculateTurnAbleCharacters(List<SimpleCharacter> players)
        {
            int counter = 0;
            foreach (var player in players)
            {
                // Debug.Log($"{player.name} has {player.actionPoints} points.");
                if (player.actionPoints != 0)
                { 
                    counter += player.actionPoints;
                }
            }

            return counter;
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
            // Debug.Log("Changing from "+ currentPlayerIndex + " to " + (currentPlayerIndex + 1) % players.Count);
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            while (players[currentPlayerIndex].actionPoints == 0 && currentPlayerIndex!=initial)
            {
                // Debug.Log("Changing from "+ currentPlayerIndex + " to " + (currentPlayerIndex + 1) % players.Count);
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            }

            if (currentPlayerIndex == initial)
            {
                // Debug.Log("New character same as old one");
                isPlayerChanged = false;
                return;
            }
            // Debug.Log("Changing current player to " + players[currentPlayerIndex].name);
            isPlayerChanged = true;
            Level.instance.TurnOffAllDisplays();
            directiveSelectCancel.Cancel();
        }
        public async Task Execute()
        {
            isPlayerChanged = true;
            var players = Level.instance.GetCharactersOfFaction(EFaction.Player);
            this._players = this._players;

            currentPlayerIndex = 0;
            //We have a list of players
            if (players.Count == 0)
            {
                // Debug.Log("0 players");
                await Task.Delay(millisecondsDelay: 600);
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
            
            //Button to give up the rest of your turn
            var foregoButton = _playerControllerUI.Q("FOREGO_TURN");
            foregoButton.RegisterCallback<ClickEvent>((evt) =>
            {
                players.ForEach((player) => player.actionPoints = 0);
                isPlayerChanged = true;
            });
            
            //Check how many characters have at least one action point
            var validCharacterCount = CalculateTurnAbleCharacters(players);
            HandlePlayerCharacterChangeButtonState(ChangeCharacterButton,validCharacterCount);
            //While at least one player has action points left, loop
            while ((validCharacterCount = CalculateTurnAbleCharacters(players))>0)
            {
                if (requestPlayerChange)
                {
                    SwitchPlayerCharacter(players);
                    requestPlayerChange = false;
                }
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
                        newIcon.RegisterCallback<ClickEvent>((evt) =>
                        {
                            if(player.actionPoints!=0)
                                ChangeCharacterTo(index);
                        });
                        return newIcon;
                    }).ToList();
                    CharacterIconsContainer.Clear();
                    CharacterIconsList.ForEach((icon) =>
                    {
                        CharacterIconsContainer.Add(icon);
                    });
                    // Debug.Log("Recreating UI");
                    //We have a player, initialize the UI
                    var currentPlayer = players[currentPlayerIndex];
                    await _cam.LerpToTarget(currentPlayer.transform.position);
                    _cam.SetFocusTarget(currentPlayer.transform);

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
                            CancelDirective();
                            // directiveSelectCancel.Dispose();
                            directiveSelectCancel = new CancellationTokenSource();
                            Debug.Log("Trying to perform "+ move + ". State of token: " + directiveSelectCancel.IsCancellationRequested);
                            try
                            {
                                await Task.Delay(millisecondsDelay: 100);
                                await PerformMove(currentPlayer, move, directiveSelectCancel.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                Debug.Log("Cancelled "+move);
                            }
                        
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
            _cam.Unfollow();
        }

        private void ChangeCharacterTo(int index)
        {
            if (currentPlayerIndex == index)
            {
                return;
            }
            directiveSelectCancel.Cancel();
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
            cam._follow = false;
            CurrentTurnObjectPrefab = inp;
            CurrentTurnObject = GameObject.Instantiate(CurrentTurnObjectPrefab, Vector3.zero,
                Quaternion.Euler(-90.0f, 0.0f, 0.0f)).GetComponent<CurrentTurnFollower>();
            CurrentTurnObject.gameObject.SetActive(false);
        }
        public async Task ExecuteBehaviors(){
            var enemies = Level.instance.GetCharactersOfFaction(EFaction.Enemy);
             StringBuilder sb = new StringBuilder();
             foreach (var enemy in enemies)
             {
                 sb.Append(enemy.name + ",");
             }
             // Debug.Log("Got enemies: "+ sb.ToString() );
             foreach (var enemy in enemies)
             {
                 cam._follow = true;
                 await cam.Follow(enemy.transform);
                 CurrentTurnObject.gameObject.SetActive(true);
                 // CurrentTurnObject.enabled = true;
                 CurrentTurnObject.target = enemy.transform;
                 enemy.behavior.Tick();
                 Debug.Log("Making "+ enemy.name + " perform passive action.");
                 await enemy.passiveDirective.DoAction();
                 // Debug.Log("Moving on from action");
                 CurrentTurnObject.gameObject.SetActive(false);
                 // CurrentTurnObject.enabled = false;
                 cam._follow = false;
                 await Task.Delay(millisecondsDelay: 100);
             }
             // Debug.Log("All directives complete");
             return;
         }


        public async Task Execute()
        {
             // Debug.Log("Starting directives");
             await ExecuteBehaviors();
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
    public CameraController _cam = default;
    public SelectUnderMouse _selector;
    public PlayerController playerController = default;
    public EnemyController enemyController = default;
    public VisualTreeAsset validMoveTemplate = default;
    public VisualTreeAsset _playerCharacterIconTemplate = default;
    public DirectiveManager _directiveManager;
    public DirectiveIconManager _directiveIcon;
    public bool _skipPlayerTurn = false;
    public void OnValidate()
    {
        CurrentTurnFollower.offset = new Vector3(0.0f, CurrentTurnFollowerYOffset, 0.0f);
    }
    // public Dictionary<EFaction, List<GameAction>> QueuedActions = new Dictionary<EFaction, List<GameAction>>();
    public void Awake()
    {
        _uiDocument = this.GetComponent<UIDocument>();
        if (_uiDocument == null)
        {
            Debug.LogError("_uiDocument null");
        }
        _enemyMovesUI = _uiDocument.rootVisualElement.Q("EnemyMovesContainer");
        _playerMovesUI = _uiDocument.rootVisualElement.Q("PlayerMovesContainer");
        if (_playerMovesUI == null)
        {
            Debug.LogError("_playerMovesUI null");
        }
        _playerControllerUI = _uiDocument.rootVisualElement.Q("PLAYER_CONTROLLER");
        playerController = new PlayerController(
            _directiveManager, _directiveIcon,
            _cam, _playerControllerUI, validMoveTemplate, _playerCharacterIconTemplate,_selector);
        enemyController = new EnemyController(currentTurnFollowerPrefab, _cam);
        HideEnemyTurnUI();
        HidePlayerTurnUI();
    }

    public bool startGame = false;
    public void OnLevelGenerated()
    {
        if (startGame)
        {
            Level.instance.TurnOffAllDisplays();
            TurnLoop();
            
        }
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

    [ContextMenu("cancel directive")]
    public void Cancel()
    {
        playerController.directiveSelectCancel.Cancel();
    }
    public void CancelDirective(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("Trying to cancel directive");
            playerController.directiveSelectCancel.Cancel();
        }
    }
}

}
