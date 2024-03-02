using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    public enum Mode
    {
        HUMAN_v_HUMAN = 0,
        HUMAN_v_AI = 1,
        AI_v_AI = 2
    }

    #region DDOL Singleton

    private static ControllerManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Instance.isInGameScene = isInGameScene;
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Global Members

    [Header("Controller Manager")]
    [SerializeField] private bool isInGameScene;
    [SerializeField] private Mode mode;

    [Header("References")]
    [SerializeField] private Transform controllerContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject humanControllerPrefab;
    [SerializeField] private GameObject aiControllerPrefab;

    public static Mode CurrentMode => Instance.mode;

    #endregion

    #region Core Behaviour

    private void OnEnable()
    {
        if (Instance == this)
            GameManager.OnGameStart += OnGameStart;
    }
    private void OnDisable()
    {
        if (Instance == this)
            GameManager.OnGameStart -= OnGameStart;
    }

    private void OnGameStart()
    {
        if (isInGameScene) Instance.CreateControllers();
    }

    #endregion

    #region Controller Creation

    private void CreateControllers()
    {
        switch (mode)
        {
            case Mode.HUMAN_v_HUMAN:
                CreateHumanControllers();
                return;
            case Mode.HUMAN_v_AI:
                CreateHumanAndAIControllers();
                return;
            case Mode.AI_v_AI:
                CreateAIControllers();
                return;
        }
    }

    private void CreateHumanControllers()
    {
        HumanController human1 = InstantiateHumanController();
        HumanController human2 = InstantiateHumanController();

        human1.name = "Human Controller 1";
        human2.name = "Human Controller 2";

        Controller1 = human1;
        Controller2 = human2;

        PlayerManager.AssignPlayerControllers(human1, human2);
    }
    private void CreateHumanAndAIControllers()
    {
        HumanController human = InstantiateHumanController();
        AIController ai = InstantiateAIController();

        human.name = "Human Controller";
        ai.name = "AI Controller";

        bool humanFirst = Random.value > 0.5f;
        Controller1 = humanFirst ? human : ai;
        Controller2 = humanFirst ? ai : human;

        PlayerManager.AssignPlayerControllers(human, ai);
    }
    private void CreateAIControllers()
    {
        AIController ai1 = InstantiateAIController();
        AIController ai2 = InstantiateAIController();

        ai1.name = "AI Controller 1";
        ai2.name = "AI Controller 2";

        Controller1 = ai1;
        Controller2 = ai2;

        PlayerManager.AssignPlayerControllers(ai1, ai2);
    }

    private HumanController InstantiateHumanController()
    {
        return Instantiate(humanControllerPrefab, controllerContainer).GetComponent<HumanController>();
    }
    private AIController InstantiateAIController()
    {
        return Instantiate(aiControllerPrefab, controllerContainer).GetComponent<AIController>();
    }

    #endregion

    #region Controllers

    public static Controller Controller1 { get; private set; }
    public static Controller Controller2 { get; private set; }
           
    public static Controller CurrentController
    {
        get
        {
            int currentPlayer = GameManager.CurrentPlayer;
            if (currentPlayer == 0)
            {
                Debug.LogWarning("Current Player is 0");
                return null;
            }
            return currentPlayer == 1 ? Controller1 : Controller2;
        }
    }

    #endregion

    #region Mode

    public void SetHvH(bool isHvH)
    {
        if (isHvH) mode = Mode.HUMAN_v_HUMAN;
    }
    
    public void SetHvAI(bool isHvAI)
    {
        if (isHvAI) mode = Mode.HUMAN_v_AI;
    }
    
    public void SetAIvAI(bool isAIvAI)
    {
        if (isAIvAI) mode = Mode.AI_v_AI;
    }

    #endregion
}
