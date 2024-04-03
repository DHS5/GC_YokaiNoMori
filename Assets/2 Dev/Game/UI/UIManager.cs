using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    #region Global Members

    [Header("UI Manager")]
    [SerializeField] private GameObject aiComputePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI playerText;

    #endregion

    #region Core Behaviour

    private void OnEnable()
    {
        GameManager.OnSetTurn += OnSetTurn;
        AIController.OnAITurn += AICompute;
    }
    private void OnDisable()
    {
        GameManager.OnSetTurn -= OnSetTurn;
        AIController.OnAITurn -= AICompute;
    }

    private void OnSetTurn(int playerIndex)
    {
        AIEndCompute();
        if (playerIndex > 0)
        {
            playerText.text = "Player : " + playerIndex;
        }
    }

    #endregion

    #region Game Over Behaviour

    public void SetWinner(int winner)
    {
        DOVirtual.DelayedCall(1f, () => gameOverPanel.SetActive(true));
        
        if (winner == 0)
        {
            winnerText.text = "It's a draw !";
        }
        else
        {
            winnerText.text = "Player " + winner + " wins !";
        }
    }

    #endregion

    #region Back To Menu

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    #endregion

    private void AICompute()
    {
        aiComputePanel.SetActive(true);
    }
    private void AIEndCompute()
    {
        aiComputePanel.SetActive(false);
    }
}
