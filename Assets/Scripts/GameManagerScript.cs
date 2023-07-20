using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class GameManagerScript : MonoBehaviour
{
    private enum GameStates
    {
        None,                          // 
        Hello,                         // Say hello to the player
        TutorialStep1_ShowPlayerEnv,   // Show the character
        TutorialStep2_WaitForRewardsCollection,
        TutorialStep3_WaitForRewardsCollection,
        TutorialStep4_WaitForRewardsCollection,
        TutorialStep5_WaitForRewardsCollection
    }
    
    private GameStates _gameState = GameStates.None;

    /// <summary>
    /// The human environment
    /// </summary>
    [SerializeField] private EnvironmentScript _HumanPlayerBoard;
    
    /// <summary>
    /// The computer environment
    /// </summary>
    [SerializeField] private EnvironmentScript _ComputerPlayerBoard;

    /// <summary>
    /// Say hello at the beggining
    /// </summary>
    [SerializeField] private TMPro.TextMeshProUGUI _helloText;

    /// <summary>
    /// Hit for the user to use the keyboard
    /// </summary>
    [SerializeField] private Image _keyboardHintImage;

    [SerializeField] private TMPro.TextMeshProUGUI _humanPointsText;
    [SerializeField] private TMPro.TextMeshProUGUI _computerPointsText;
    [SerializeField] private TMPro.TextMeshProUGUI _timeText;
    [SerializeField] private TMPro.TextMeshProUGUI _roundText;
    [SerializeField] private TMPro.TextMeshProUGUI _separatorText;

    private int _HumanPoints = 0;
    private int _ComputerPoints = 0;
    
    /// <summary>
    /// Flag so we know if the hint is needed
    /// </summary>
    private bool _userPressedKeyboard = false;
    
    private void Awake()
    {
        _helloText.color = GeneralUtils.MakeColorTransparent(_helloText.color);
        _keyboardHintImage.color = GeneralUtils.MakeColorTransparent(_keyboardHintImage.color);
        _HumanPlayerBoard.gameObject.SetActive(false);
        _HumanPlayerBoard.SetCallbacks(OnHumanRewardCollected);
        _ComputerPlayerBoard.gameObject.SetActive(false);
        
        _humanPointsText.color = GeneralUtils.MakeColorTransparent(_humanPointsText.color);
        _computerPointsText.color = GeneralUtils.MakeColorTransparent(_computerPointsText.color);
        _roundText.color = GeneralUtils.MakeColorTransparent(_roundText.color);
        _timeText.color = GeneralUtils.MakeColorTransparent(_timeText.color);
        _separatorText.color = GeneralUtils.MakeColorTransparent(_separatorText.color);
    }

    private void OnHumanRewardCollected(int obj)
    {
        int numOfPoints = (int) obj;
        
        _HumanPoints += numOfPoints;
        // Show the points
        _humanPointsText.text = "Human: " + _HumanPoints.ToString();
        _humanPointsText.DOFade(1, 0.5f);
        
        if (_gameState == GameStates.TutorialStep2_WaitForRewardsCollection)
        {
            if (_HumanPoints >= 3)
            {
                SetState(GameStates.TutorialStep3_WaitForRewardsCollection);
            }
        }       
        else if (_gameState == GameStates.TutorialStep3_WaitForRewardsCollection)
        {
            if (_HumanPoints >= 13)
            {
                SetState(GameStates.TutorialStep4_WaitForRewardsCollection);
            }
        }   
        
        else if (_gameState == GameStates.TutorialStep4_WaitForRewardsCollection)
        {
            if (_HumanPoints >= 50)
            {
                SetState(GameStates.TutorialStep5_WaitForRewardsCollection);
            }
        }    
    }

    private void Start()
    {
        SetState(GameStates.Hello);
        
        // Add listeners
        EventManagerScript.Instance.StartListening(EventManagerScript.EVENT__PLAYER_FIRST_MOVE, (object obj) =>
        {
            _userPressedKeyboard = true;
            
            // Hide the keyboard hint is presented
            _keyboardHintImage.DOKill();
            _keyboardHintImage.DOFade(0, 0.5f);    
        });
    }

    private void SetState(GameStates newState)
    {
        if (newState != _gameState)
        {
            GameStates nextState = GameStates.None;
            
            Debug.Log("Switching from " + _gameState + " to " + newState);
         
            switch (newState)
            {
                case GameStates.Hello:
                    _helloText.DOFade(1, 1).SetDelay(2).OnComplete(() =>
                    {
                        _helloText.DOFade(0, 1).SetDelay(2).OnComplete(() =>
                        {
                            SetState(GameStates.TutorialStep1_ShowPlayerEnv);
                        });
                    });
                    break;
                
                case GameStates.TutorialStep1_ShowPlayerEnv:
                    _HumanPlayerBoard.SetTutorialStep(1);
                    _HumanPlayerBoard.gameObject.SetActive(true);
                    _HumanPlayerBoard.transform.localPosition = new Vector3(0, 0, 0);

                    StartCoroutine(GeneralUtils.WaitAndPerform(1, () =>
                    {
                        _HumanPlayerBoard.EnablePlayer(true);
                        
                        StartCoroutine(GeneralUtils.WaitAndPerform(1, () =>
                        {
                            // Show the reward
                            SetState(GameStates.TutorialStep2_WaitForRewardsCollection);
                            
                            // Set timer for the keyboard hint
                            StartCoroutine(GeneralUtils.WaitAndPerform(5, () =>
                            {
                                if (!_userPressedKeyboard)
                                {
                                    _keyboardHintImage.DOFade(1, 0.5f);
                                }
                            }));
                        }));
                    }));
                    break;
                
                case GameStates.TutorialStep2_WaitForRewardsCollection:
                    
                    break;
                
                case GameStates.TutorialStep3_WaitForRewardsCollection:
                    _HumanPlayerBoard.SetTutorialStep(2);
                    break;
                
                case GameStates.TutorialStep4_WaitForRewardsCollection:
                    _HumanPlayerBoard.SetTutorialStep(3);
                    break;
                
                case GameStates.TutorialStep5_WaitForRewardsCollection:
                    break;
            }       
            
            _gameState = newState;
        }
    }

    #region UI

    private void OnUISkipTutorial()
    {
        
    }

    #endregion
}
