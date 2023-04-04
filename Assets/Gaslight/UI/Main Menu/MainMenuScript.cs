using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuScript : MonoBehaviour
{
    #region MainMenu
    public enum MainMenuState
    {
        Main,
        NewGame,
        LoadGame,
        Options
    }
    [SerializeField] public VisualTreeAsset mainMenu;
    [SerializeField] public VisualTreeAsset optionsMenu =default;
    [SerializeField] public VisualTreeAsset newGameMenu =default;
    [SerializeField] public VisualTreeAsset newGameCommunityLevelLabel = default;
    [SerializeField] public MainMenuState startState=default;
    //Setup container
    private TemplateContainer mainMenuContainer = default;
    //Declare the buttons needed
    private Button mainMenuOptionsButton = default;
    private Button mainMenuNGButton = default;
    private TemplateContainer optionsMenuContainer = default;
    private TemplateContainer newGameMenuContainer = default;
    private Button newGameMenuCommunityButton { get; set; }
    
    #endregion
    #region New Game menu
    
    #endregion
    public UIDocument _document = default;
    void Awake()
    {
        _document = GetComponent<UIDocument>();
        mainMenuContainer = InstantiateContainer(mainMenu);
        mainMenuOptionsButton = mainMenuContainer.Q("OptionsButton") as Button;
        mainMenuOptionsButton.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            startState = MainMenuState.Options;
            HandleState();
        });
        mainMenuNGButton = mainMenuContainer.Q("NewGameButton") as Button;
        mainMenuOptionsButton.RegisterCallback<ClickEvent>((ClickEvent evt) =>
        {
            startState = MainMenuState.NewGame;
            HandleState();
        });
        newGameMenuContainer = InstantiateContainer(newGameMenu);
        //For now, load all levels on startup
        var levels = newGameMenuContainer.Q("levels") as VisualElement;
        // var levelsUrl = "127.0.0.1/levels";
        var levelUrl = "http://127.0.0.1:3001/levels";
        StartCoroutine(GetLevels());
        optionsMenuContainer = InstantiateContainer(optionsMenu);
        _document.rootVisualElement.style.width = Screen.safeArea.width;
        _document.rootVisualElement.style.height = Screen.safeArea.height;
    }

    [Serializable]
    public struct LevelsSchema
    {
        public LevelsPrev[] root;
    }

    [Serializable]
    public struct LevelsPrev
    {
        public int id;
    }

    public string levelsURL;
    public LevelsSchema curLevels;
    private IEnumerator GetLevels()
    {
        using (var webRequest = UnityWebRequest.Get(levelsURL))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log("Error:" + webRequest.error);
            }
            else
            {
                var text = webRequest.downloadHandler.text;
                Debug.Log("received" + text);
                var LevelsContainer = newGameMenuContainer.Q("Levels");
                // text = text.Substring(1, text.Length-2);
                // Debug.Log("After trim" + webRequest.downloadHandler.text);
                curLevels= JsonUtility.FromJson<LevelsSchema>(text);
                //We've got the level IDs. Construct some labels and put them in the newGameMenuContainer
                foreach (var levelsPrev in curLevels.root)
                {
                    var newLabel = newGameCommunityLevelLabel.Instantiate();
                    var idLabel = newLabel.Q("LevelID") as Label;
                    idLabel.text = levelsPrev.id.ToString();
                    LevelsContainer.Add(newLabel);
                    Debug.Log("Created "+ levelsPrev.id);
                }
            }
        };     
    }

    
    TemplateContainer InstantiateContainer(VisualTreeAsset visualTreeAsset)
    {
        var newContainer = visualTreeAsset.Instantiate();
        newContainer.style.width = Screen.safeArea.width;
        newContainer.style.height = Screen.safeArea.height;
        return newContainer;
    }

    void SetDocument(TemplateContainer container)
    {
        _document.rootVisualElement.Clear();
        _document.rootVisualElement.Add(container);
    }

    void HandleState()
    {
        switch (startState)
        {
            case MainMenuState.Main:
                SetDocument(mainMenuContainer);
                break;
            case MainMenuState.Options:
                SetDocument(optionsMenuContainer);
                break;
            case MainMenuState.NewGame:
                SetDocument(newGameMenuContainer);
                break;
            default:
                Debug.Log("start state invalid");
                break;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        HandleState();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
