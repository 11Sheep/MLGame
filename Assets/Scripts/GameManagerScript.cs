using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

public class GameManagerScript : MonoBehaviour
{
    private const float MIN_DISTANCE_FROM_ELEMENTS = 2f;
    private const int NUMBER_OF_COMPETITION_ROUNDS = 10;
    private static Color AGENT_HUMAN_COLOR = new Color(0.49f, 0.38f, 0.68f);
    private static Color AGENT_COMPUTER_COLOR = new Color(0.14f, 0.52f, 0.83f);
    
    private enum WhoWonEnum { None, Human, Computer }

    private enum WinReasonEnum
    {
        CollectedAllCandies,
        Timeout
    };
    
    private const float STAGE_SIZE = 4;
    
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
        CompetitionPopup,
        CompetitionFinished,
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

    /// <summary>
    /// We need to show this button only when in tutorial mode
    /// </summary>
    [SerializeField] private Button _skipTutorialButton;
    
    /// <summary>
    /// The lower status bar. we only show it during competition mode
    /// </summary>
    [SerializeField] private CanvasGroup _lowerCanvasGroup;

    /// <summary>
    /// The before competition popup
    /// </summary>
    [SerializeField] private CanvasGroup _competitionPopupCanvasGroup;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField] private CanvasGroup _competitionResultPopupCanvasGroup;
    
    /// <summary>
    /// Start competition button. we need to enable/disable it
    /// </summary>
    [SerializeField] private Button _competitionStartButton;
    
    /// <summary>
    /// Result competition button. we need to enable/disable it
    /// </summary>
    [SerializeField] private Button _competitionResultButton;
    
    [SerializeField] private TMPro.TextMeshProUGUI _competitionResultText;
    
    [SerializeField] private GameObject _humanCompetitionResultImage;
    [SerializeField] private GameObject _computerCompetitionResultImage;

    [SerializeField] private TMPro.TextMeshProUGUI _timerText;
    
    private int _currentRound = 0;

    private int _humanWins = 0;
    private int _computerWins = 0;

    private int _HumanRoundPoints = 0;
    private int _ComputerRoundPoints = 0;
    
    /// <summary>
    /// Flag so we know if the hint is needed
    /// </summary>
    private bool _userPressedKeyboard = false;

    /// <summary>
    /// Count down for level completion
    /// </summary>
    private float _timerCounter = 0;

    /// <summary>
    /// Time for the current round
    /// </summary>
    private float _roundTime = 0;

    [SerializeField] private CircleProgressElement _HumanUIProgress;
    [SerializeField] private CircleProgressElement _ComputerUIProgress;
    
    /// <summary>
    /// The timer ui element
    /// </summary>
    [SerializeField] private Slider _timerSlider;
    
    private void Awake()
    {
        // Set up the audio
        AudioManager.Instance.Initialize();
        
        _helloText.color = GeneralUtils.MakeColorTransparent(_helloText.color);
        _keyboardHintImage.color = GeneralUtils.MakeColorTransparent(_keyboardHintImage.color);
        _HumanPlayerBoard.gameObject.SetActive(false);
        _HumanPlayerBoard.SetCallbacks(OnHumanRewardCollected);
        _ComputerPlayerBoard.gameObject.SetActive(false);
        _ComputerPlayerBoard.SetCallbacks(OnComputerRewardCollected);
        
        _humanPointsText.color = GeneralUtils.MakeColorTransparent(_humanPointsText.color);
        _computerPointsText.color = GeneralUtils.MakeColorTransparent(_computerPointsText.color);
        
        _HumanUIProgress.Initialize(AGENT_HUMAN_COLOR);
        _ComputerUIProgress.Initialize(AGENT_COMPUTER_COLOR);
        
        _lowerCanvasGroup.alpha = 0;
        
        _skipTutorialButton.GetComponent<CanvasGroup>().alpha = 0;

        _competitionPopupCanvasGroup.gameObject.SetActive(false);

        _competitionResultPopupCanvasGroup.gameObject.SetActive(false);
    }
    
    private void Start()
    {
        // TODO:
        SetState(GameStates.Hello);
        //SetState(GameStates.CompetitionPopup);
        
        // Add listeners
        EventManagerScript.Instance.StartListening(EventManagerScript.EVENT__PLAYER_FIRST_MOVE, (object obj) =>
        {
            _userPressedKeyboard = true;
            
            // Hide the keyboard hint is presented
            _keyboardHintImage.DOKill();
            _keyboardHintImage.DOFade(0, 0.5f);    
        });
    }

    private void Update()
    {
        if (_timerCounter > 0)
        {
            _timerCounter -= Time.deltaTime;
          
            _timerSlider.value = _timerCounter / _roundTime;             
            
            // Get the seconds
            float seconds = (int) _timerCounter;
            
            // Get the ms 
            float ms = (_timerCounter - Mathf.Floor(_timerCounter)) * 100;
                
            _timerText.text = seconds.ToString("00") + ":" + ms.ToString("00");
            
            if (_timerCounter <= 0)
            {
                _timerCounter = 0;
                
                // Jump to the next state
                if (_gameState == GameStates.TutorialStep5_WaitForRewardsCollection)
                {
                    SetState(GameStates.TutorialStep6_WaitForRewardsCollection);
                }
                else if (_gameState == GameStates.TutorialStep6_WaitForRewardsCollection)
                {
                    SetState(GameStates.CompetitionPopup);
                }
                else if (_gameState == GameStates.CompetitionMode)
                {
                    // See who is the winner
                    if (_HumanRoundPoints > _ComputerRoundPoints)
                    {
                        CompetitionRoundFinished(WhoWonEnum.Human, WinReasonEnum.Timeout);
                    }
                    else if (_HumanRoundPoints < _ComputerRoundPoints)
                    {
                        CompetitionRoundFinished(WhoWonEnum.Computer, WinReasonEnum.Timeout);
                    }
                    else
                    {
                        CompetitionRoundFinished(WhoWonEnum.None, WinReasonEnum.Timeout);
                    }
                }
            }
        }
    }

    private void CompetitionRoundFinished(WhoWonEnum whoWon, WinReasonEnum winReason)
    {
        float timeForRound = GetRoundTime(_currentRound);
        _timerCounter = 0;

        Debug.Log("CompetitionRoundFinished. winner: " + whoWon + ", reason: " + winReason);

        // Show the timer only if we have time usage in this round
        _timerSlider.gameObject.SetActive(timeForRound > 0);

        if (whoWon == WhoWonEnum.Human)
        {
            _ComputerPlayerBoard.OtherPlayerWon();
            _HumanPlayerBoard.YouWon();
            _humanWins++;
        }
        else if (whoWon == WhoWonEnum.Computer)
        {
            _HumanPlayerBoard.OtherPlayerWon();
            _ComputerPlayerBoard.YouWon();
            _computerWins++;
        }
        else
        {
            _HumanPlayerBoard.OtherPlayerWon();
            _ComputerPlayerBoard.OtherPlayerWon();
        }
        
        _currentRound++;
        
        // Give 2 seconds of wining
        StartCoroutine(GeneralUtils.WaitAndPerform(2, () =>
        {
            if ((_computerWins == NUMBER_OF_COMPETITION_ROUNDS) || (_humanWins == NUMBER_OF_COMPETITION_ROUNDS))
            {
                SetState(GameStates.CompetitionFinished);
            }
            else
            {
                StartAnotherRound(whoWon, timeForRound);
            }
        }));
    }

    private void StartAnotherRound(WhoWonEnum whoWon, float timeForRound)
    {
        _roundTime = timeForRound;
        
        Debug.Log("Starting round " + _currentRound + " with time " + _roundTime + " seconds");
        
        (Vector3 agentLocation, Vector3[] rewardLocations) = GetLocationsForAgentAndCandies(4, false);

        _HumanRoundPoints = 0;
        _ComputerRoundPoints = 0;
        
        _HumanPlayerBoard.SetLocations(agentLocation, rewardLocations);
        _ComputerPlayerBoard.SetLocations(agentLocation, rewardLocations);
                
        _HumanPlayerBoard.StartAnotherMatch();
        _ComputerPlayerBoard.StartAnotherMatch();

        if (whoWon == WhoWonEnum.Human)
        {
            _HumanUIProgress.AddProgress();
        }
        else if (whoWon == WhoWonEnum.Computer)
        {
            _ComputerUIProgress.AddProgress();
        }
        
        _timerText.text = _roundTime.ToString("00") + ":00";
        _timerSlider.value = 1;

        StartCoroutine(GeneralUtils.WaitAndPerform(2, () =>
        {
            _humanPointsText.text = "Human: 0";
            _computerPointsText.text = "Computer: 0";
                
            _timerCounter = _roundTime;
        }));
    }

    private void OnHumanRewardCollected(int obj, bool isEndOfRound)
    {
        AudioManager.Instance.PlayGeneralSound(AudioManager.Sound__collectCandy);
        
        int numOfPoints = (int) obj;
        
        _HumanRoundPoints += numOfPoints;
        // Show the points
        _humanPointsText.text = "Human: " + _HumanRoundPoints.ToString();
        _humanPointsText.DOFade(1, 0.5f);

        if (_gameState == GameStates.CompetitionMode)
        {
            if (isEndOfRound)
            {
                CompetitionRoundFinished(WhoWonEnum.Human, WinReasonEnum.CollectedAllCandies);
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
        
        if (isEndOfRound)
        {
            _ComputerRoundPoints = 0;
            _HumanRoundPoints = 0;
        }
    }

    private void OnComputerRewardCollected(int obj, bool isEndOfRound)
    {
        AudioManager.Instance.PlayGeneralSound(AudioManager.Sound__collectCandy);
        
        int numOfPoints = (int) obj;

        _ComputerRoundPoints += numOfPoints;
        // Show the points
        _computerPointsText.text = "Computer: " + _ComputerRoundPoints.ToString();
        _computerPointsText.DOFade(1, 0.5f);
        
        if (_gameState == GameStates.CompetitionMode)
        {
            if (isEndOfRound)
            {
                CompetitionRoundFinished(WhoWonEnum.Computer, WinReasonEnum.CollectedAllCandies);
            }
        }

        if (isEndOfRound)
        {
            _ComputerRoundPoints = 0;
            _HumanRoundPoints = 0;
        }
    }

    private void SetState(GameStates newState)
    {
        Vector3 agentLocation;
        Vector3[] rewardLocations;
        
        if (newState != _gameState)
        {
            GameStates nextState = GameStates.None;
            
            Debug.Log("Switching from " + _gameState + " to " + newState);
         
            switch (newState)
            {
                case GameStates.Hello:

                    StartCoroutine(GeneralUtils.WaitAndPerform(0.8f, () =>
                    {
                        AudioManager.Instance.PlayGeneralSound(AudioManager.Sound__hello);
                    }));
                    
                    _helloText.DOFade(1, 1).SetDelay(1).OnComplete(() =>
                    {
                        _helloText.DOFade(0, 1).SetDelay(2).OnComplete(() =>
                        {
                            SetState(GameStates.TutorialStep1_ShowPlayerEnv);
                        });
                    });
                    break;
                
                case GameStates.TutorialStep1_ShowPlayerEnv:
                    
                    _skipTutorialButton.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
                    
                    _HumanPlayerBoard.SetTutorialStep(1);
                    
                    // Random locations for candies
                    (agentLocation, rewardLocations) = GetLocationsForAgentAndCandies(4, false);
                    _HumanPlayerBoard.SetLocations(agentLocation, rewardLocations);
                    
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
                    // Random locations for candies
                    (agentLocation, rewardLocations) = GetLocationsForAgentAndCandies(4, false);
                    _HumanPlayerBoard.SetLocations(agentLocation, rewardLocations);
                    
                    _HumanPlayerBoard.SetTutorialStep(2);
                    break;
                
                case GameStates.TutorialStep4_WaitForRewardsCollection:
                    // Random locations for candies
                    (agentLocation, rewardLocations) = GetLocationsForAgentAndCandies(4, false);
                    _HumanPlayerBoard.SetLocations(agentLocation, rewardLocations);
                    
                    _HumanPlayerBoard.SetTutorialStep(3);
                    break;
                
                case GameStates.TutorialStep5_WaitForRewardsCollection:
                    // Random locations for candies
                    (agentLocation, rewardLocations) = GetLocationsForAgentAndCandies(4, false);
                    _HumanPlayerBoard.SetLocations(agentLocation, rewardLocations);
                    
                    _HumanPlayerBoard.SetTutorialStep(4);
                    
                    StartCoroutine(GeneralUtils.WaitAndPerform(2, () =>
                    {
                        _roundTime = 10f;
                        _timerCounter = 10f;
                    }));
                    break;
                
                case GameStates.TutorialStep6_WaitForRewardsCollection:
                    // Random locations for candies
                    (agentLocation, rewardLocations) = GetLocationsForAgentAndCandies(4, false);
                    _HumanPlayerBoard.SetLocations(agentLocation, rewardLocations);
                    
                    _HumanPlayerBoard.SetTutorialStep(5);
                    
                    StartCoroutine(GeneralUtils.WaitAndPerform(2, () =>
                    {
                        _roundTime = 5f;
                        _timerCounter = 5f;
                    }));
                    
                    break;
                
                case GameStates.CompetitionPopup:
                    _competitionPopupCanvasGroup.alpha = 0;
                    _competitionPopupCanvasGroup.gameObject.SetActive(true);
                    _competitionPopupCanvasGroup.DOFade(1, 0.3f);
                    _competitionStartButton.interactable = true;
                    break;
                
                case GameStates.CompetitionMode:
                    _HumanPlayerBoard.transform.localPosition = new Vector3(-6, 0, 0);
                    _ComputerPlayerBoard.transform.localPosition = new Vector3(6, 0, 0);
                    _HumanPlayerBoard.gameObject.SetActive(true);
                    _ComputerPlayerBoard.gameObject.SetActive(true);
                    _lowerCanvasGroup.DOFade(1, 0.5f);
                    
                    _currentRound = 0;
                    _computerWins = 0;
                    _humanWins = 0;

                    _HumanUIProgress.Reset();
                    _ComputerUIProgress.Reset();
            
                    StartAnotherRound(WhoWonEnum.None, 10);
                    break;
                
                case GameStates.CompetitionFinished:
                    _competitionResultButton.interactable = true;
                    _competitionResultPopupCanvasGroup.gameObject.SetActive(true);
                    _competitionResultPopupCanvasGroup.alpha = 0;
                    _competitionResultPopupCanvasGroup.DOFade(1, 0.5f);
                    if (_computerWins == NUMBER_OF_COMPETITION_ROUNDS)
                    {
                        _computerCompetitionResultImage.SetActive(true);
                        _humanCompetitionResultImage.SetActive(false);
                        _competitionResultText.text = "AI";
                    }
                    else
                    {
                        _computerCompetitionResultImage.SetActive(false);
                        _humanCompetitionResultImage.SetActive(true);
                        _competitionResultText.text = "Human";
                    }
                    break;
            }       
            
            _gameState = newState;
        }
    }
    
    private (Vector3,Vector3[]) GetLocationsForAgentAndCandies(int numOfCandies, bool isAgentLocationRandom)
    {
        Vector3 agentLocation = new Vector3(Random.Range(-STAGE_SIZE, STAGE_SIZE), 0, Random.Range(-STAGE_SIZE, STAGE_SIZE));
        Vector3[] candiesLocations = new Vector3[numOfCandies];
        
        if (isAgentLocationRandom)
        {
            // Put in random location
            agentLocation = new Vector3(Random.Range(-STAGE_SIZE, STAGE_SIZE), 0, Random.Range(-STAGE_SIZE, STAGE_SIZE));
        }
        else
        {
            // Put in the middle
            agentLocation = Vector3.zero;
        }
        
        bool tooCloseToAgent;
        bool tooCloseToAnotherCandy;
        Vector3 randomPosition;

        // Set all the candies on the board and make sure that they are not too close to the agent or to another candy
        for (int candyIndex = 0; candyIndex < candiesLocations.Length; candyIndex++)
        {
            do
            {
                // Decide on a random position
                randomPosition = new Vector3(Random.Range(-STAGE_SIZE, STAGE_SIZE), 0, Random.Range(-STAGE_SIZE, STAGE_SIZE));
                
                // Check if the position is too close to the agent
                tooCloseToAgent = Vector3.Distance(randomPosition, agentLocation) < MIN_DISTANCE_FROM_ELEMENTS;
                
                // Check if the position is too close to another candy
                tooCloseToAnotherCandy = false;
                
                for (int otherCandyIndex = 0; otherCandyIndex < candyIndex; otherCandyIndex++)
                {
                    if (Vector3.Distance(randomPosition, candiesLocations[otherCandyIndex]) < MIN_DISTANCE_FROM_ELEMENTS)
                    {
                        tooCloseToAnotherCandy = true;
                        break;
                    }
                }
            } while (tooCloseToAnotherCandy || tooCloseToAgent);
            
            candiesLocations[candyIndex] = randomPosition;
        }
        
        return (agentLocation, candiesLocations);
    }

    public float GetRoundTime(int roundIndex)
    {
        float roundTime = 0;
        
        if (roundIndex > 10)
        {
            roundTime = 4;
        }
        else
        {
            roundTime = (10 - roundIndex) + 2;
        }
        
        return roundTime;
    }

    #region UI

    public void OnUISkipTutorial()
    {
        SetState(GameStates.CompetitionPopup);
    }

    public void OnUIStartCompetition()
    {
        if (_gameState == GameStates.CompetitionPopup)
        {
            _competitionStartButton.interactable = false;
            
            _competitionPopupCanvasGroup.DOFade(0, 0.5f).OnComplete(() =>
            {
                _competitionPopupCanvasGroup.gameObject.SetActive(false);
                
                SetState(GameStates.CompetitionMode);
            });
            
        }
    }

    public void OnUIRestartCompetition()
    {
        _competitionResultButton.interactable = false;
        _competitionResultPopupCanvasGroup.DOFade(0, 0.5f).OnComplete(() =>
        {
            _competitionResultPopupCanvasGroup.gameObject.SetActive(false);
           
            SetState(GameStates.CompetitionPopup);
        });
    }

    #endregion
}
