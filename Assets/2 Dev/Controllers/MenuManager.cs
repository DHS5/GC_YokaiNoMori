using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Toggle board4x3Toggle;

    private void Start()
    {
        SetHvH(true);
    }

    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(board4x3Toggle.isOn ? 1 : 2);
    }

    public void SetHvH(bool isHvH)
    {
        if (isHvH) ControllerManager.CurrentMode = ControllerManager.Mode.HUMAN_v_HUMAN;
        ControllerManager.ModeSetInMenu = true;
    }

    public void SetHvAI(bool isHvAI)
    {
        if (isHvAI) ControllerManager.CurrentMode = ControllerManager.Mode.HUMAN_v_AI;
        ControllerManager.ModeSetInMenu = true;
    }

    public void SetAIvAI(bool isAIvAI)
    {
        if (isAIvAI) ControllerManager.CurrentMode = ControllerManager.Mode.AI_v_AI;
        ControllerManager.ModeSetInMenu = true;
    }
}