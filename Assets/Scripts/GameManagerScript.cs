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
        TutorialStep5_WaitForRewardsCollection,
        TutorialStep6_WaitForRewardsCollection,
        CompetitionMode,
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

    /// <summary>
    /// Count down for level completion
    /// </summary>
    private float _timerCounter = 0;
    
    private void Awake()
    {
        _helloText.color = GeneralUtils.MakeColorTransparent(_helloText.color);
        _keyboardHintImage.color = GeneralUtils.MakeColorTransparent(_keyboardHintImage.color);
        _HumanPlayerBoard.gameObject.SetActive(false);
        _HumanPlayerBoard.SetCallbacks(OnHumanRewardCollected, OnHumanFailed);
        _ComputerPlayerBoard.gameObject.SetActive(false);
        _ComputerPlayerBoard.SetCallbacks(OnComputerRewardCollected, OnComputerFailed);
        
        _humanPointsText.color = GeneralUtils.MakeColorTransparent(_humanPointsText.color);
        _computerPointsText.color = GeneralUtils.MakeColorTransparent(_computerPointsText.color);
        _roundText.color = GeneralUtils.MakeColorTransparent(_roundText.color);
        _timeText.color = GeneralUtils.MakeColorTransparent(_timeText.color);
        _separatorText.color = GeneralUtils.MakeColorTransparent(_separatorText.color);
    }

    private void Update()
    {
        if (_timerCounter > 0)
        {
            _timerCounter -= Time.deltaTime;
          
            // Convert the float value to a TimeSpan
            TimeSpan timeSpan = TimeSpan.FromSeconds(_timerCounter);
            string formattedTime = $"{timeSpan.Seconds}:{timeSpan.Milliseconds:D1}";
            _timeText.text = "Timer: " + formattedTime;
            
            if (_timerCounter <= 0)
            {
                _timerCounter = 0;
                
                // Jump to the next state
                if (_gameState == GameStates.TutorialStep5_WaitForRewardsCollection)
                {
                    SetState(GameStates.TutorialStep6_WaitForRewardsCollection);
                }
            }
        }
    }

    private void OnHumanRewardCollected(int obj, bool isEndOfRound)
    {
        int numOfPoints = (int) obj;
        
        _HumanPoints += numOfPoints;
        // Show the points
        _humanPointsText.text = "Human: " + _HumanPoints.ToString();
        _humanPointsText.DOFade(1, 0.5f);

        if (_gameState == GameStates.CompetitionMode)
        {
            if (isEndOfRound)
            {
                _ComputerPlayerBoard.OtherPlayerWon();
            }
        }
        else if (_gameState == GameStates.TutorialStep2_WaitForRewardsCollection)
        {
            if (isEndOfRound)
            {
                SetState(GameStates.TutorialStep3_WaitForRewardsCollection);
            }
        }       
        else if (_gameState == GameStates.TutorialStep3_WaitForRewardsCollection)
        {
            if (isEndOfRound)
            {
                SetState(GameStates.TutorialStep4_WaitForRewardsCollection);
            }
        }   
        
        else if (_gameState == GameStates.TutorialStep4_WaitForRewardsCollection)
        {
            if (isEndOfRound)
            {
                SetState(GameStates.TutorialStep5_WaitForRewardsCollection);
            }
        }    
        else if (_gameState == GameStates.TutorialStep5_WaitForRewardsCollection)
        {
            if (isEndOfRound)
            {
                _timerCounter = 0;
                SetState(GameStates.TutorialStep6_WaitForRewardsCollection);
            }
        }    
        else if (_gameState == GameStates.TutorialStep6_WaitForRewardsCollection)
        {
            if (isEndOfRound)
            {
                _timerCounter = 0;
            }
        }    
    }

    private void OnHumanFailed()
    {
        // TODO: make that it is impossible to fall and fail (only by time)
        
        if (_gameState == GameStates.TutorialStep5_WaitForRewardsCollection)
        {
            SetState(GameStates.TutorialStep6_WaitForRewardsCollection);
        }    
        else if (_gameState == GameStates.TutorialStep6_WaitForRewardsCollection)
        {
          
        }    
    }

    private void OnComputerRewardCollected(int obj, bool isEndOfRound)
    {
        int numOfPoints = (int) obj;

        _ComputerPoints += numOfPoints;
        // Show the points
        _computerPointsText.text = "Computer: " + _ComputerPoints.ToString();
        _computerPointsText.DOFade(1, 0.5f);
        
        if (_gameState == GameStates.CompetitionMode)
        {
            if (isEndOfRound)
            {
                _HumanPlayerBoard.OtherPlayerWon();

                _HumanPlayerBoard.StartAnotherMatch();
                _ComputerPlayerBoard.StartAnotherMatch();
            }
        }
    }
    
    private void OnComputerFailed()
    {
    }

    private void Start()
    {
        // TODO:
        //SetState(GameStates.Hello);
        SetState(GameStates.CompetitionMode);
        
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
                    _HumanPlayerBoard.SetTutorialStep(4);
                    _timeText.DOFade(1, 0.3f);
                    _timeText.text = "Timer: 10:0";
                    
                    StartCoroutine(GeneralUtils.WaitAndPerform(2, () =>
                    {
                        _timeText.DOFade(1, 0.3f);
                        _timerCounter = 10f;
                    }));
                    break;
                
                case GameStates.TutorialStep6_WaitForRewardsCollection:
                    _HumanPlayerBoard.SetTutorialStep(5);
                    _timeText.text = "Timer: 5:0";
                    
                    StartCoroutine(GeneralUtils.WaitAndPerform(2, () =>
                    {
                        _timerCounter = 5f;
                    }));
                    
                    break;
                
                case GameStates.CompetitionMode:
                    _HumanPlayerBoard.transform.localPosition = new Vector3(-6, 0, 0);
                    _ComputerPlayerBoard.transform.localPosition = new Vector3(6, 0, 0);
                    _HumanPlayerBoard.gameObject.SetActive(true);
                    _ComputerPlayerBoard.gameObject.SetActive(true);
                    break;
            }       
            
            _gameState = newState;
        }
    }

    #region UI

    public void OnUISkipTutorial()
    {
        SetState(GameStates.CompetitionMode);
    }

    #endregion
}
