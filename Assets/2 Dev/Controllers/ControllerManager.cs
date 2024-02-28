using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    public enum Mode
    {
        HUMANvHUMAN = 0,
        HUMANvAI = 1,
        AIvAI = 2
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

    #endregion

    #region Core Behaviour

    private void Start()
    {
        if (isInGameScene) CreateControllers();
    }

    #endregion

    #region Controller Creation

    private void CreateControllers()
    {
        switch (mode)
        {
            case Mode.HUMANvHUMAN:
                CreateHumanControllers();
                return;
            case Mode.HUMANvAI:
                CreateHumanAndAIControllers();
                return;
            case Mode.AIvAI:
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

        PlayerManager.AssignPlayerControllers(human1, human2);
    }
    private void CreateHumanAndAIControllers()
    {
        HumanController human = InstantiateHumanController();
        AIController ai = InstantiateAIController();

        human.name = "Human Controller";
        ai.name = "AI Controller";

        PlayerManager.AssignPlayerControllers(human, ai);
    }
    private void CreateAIControllers()
    {
        AIController ai1 = InstantiateAIController();
        AIController ai2 = InstantiateAIController();

        ai1.name = "AI Controller 1";
        ai2.name = "AI Controller 2";

        PlayerManager.AssignPlayerControllers(ai1, ai2);
    }

    private HumanController InstantiateHumanController()
    {
        return Instantiate(humanControllerPrefab, controllerContainer).GetComponent<HumanController>();
    }
    private AIController InstantiateAIController()
    {
        return Instantiate(humanControllerPrefab, controllerContainer).GetComponent<AIController>();
    }

    #endregion

    #region Mode

    public int ControllerMode
    {
        set
        {
            mode = (Mode)value;
        }
    }

    #endregion
}
