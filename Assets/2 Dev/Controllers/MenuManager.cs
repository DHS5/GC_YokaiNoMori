using UnityEngine;
using UnityEngine.UI;
using Group15;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Toggle board4x3Toggle;
    [SerializeField] private GameObject dropdowns;

    private void Start()
    {
        SetHvH(true);
        OnAILevelChanged(0);
        OnPlayerTurnChanged(0);
    }

    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(board4x3Toggle.isOn ? 1 : 2);
    }

    public void SetHvH(bool isHvH)
    {
        if (isHvH)
        {
            ControllerManager.CurrentMode = ControllerManager.Mode.HUMAN_v_HUMAN;
            dropdowns.SetActive(false);
        }
        ControllerManager.ModeSetInMenu = true;
    }

    public void SetHvAI(bool isHvAI)
    {
        if (isHvAI)
        {
            ControllerManager.CurrentMode = ControllerManager.Mode.HUMAN_v_AI;
            dropdowns.SetActive(true);
        }
        ControllerManager.ModeSetInMenu = true;
    }

    public void SetAIvAI(bool isAIvAI)
    {
        if (isAIvAI)
        {
            ControllerManager.CurrentMode = ControllerManager.Mode.AI_v_AI;
            dropdowns.SetActive(false);
        }
        ControllerManager.ModeSetInMenu = true;
    }

    public void OnAILevelChanged(int level)
    {
        AIController.Level = (AILevel)level;
    }
    
    public void OnPlayerTurnChanged(int turn)
    {
        ControllerManager.HumanFirst = turn == 0;
    }
}