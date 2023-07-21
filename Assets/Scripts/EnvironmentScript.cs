using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public class EnvironmentScript : MonoBehaviour
{
    private static Color FLOOR_IDLE_COLOR = new Color(0.6f, 0.6f, 0.6f);
    private static Color FLOOR_HIT_WALL_COLOR = new Color(1f, 0.3f, 0.3f);
    private static Color FLOOR_CANDY_COLLECTED_COLOR = new Color(.4f, .7f, 0.4f);
    
    /// <summary>
    /// The scale of the stage (the stage is a square) 
    /// </summary>
    [SerializeField] [Range(1,2)] private float _stageScale = 1;
    
    /// <summary>
    /// Number of candies to generate on the board
    /// </summary>
    [SerializeField] [Range(1,5)] private int _numOfCandies = 1;

    /// <summary>
    /// Keep the stage transform so we can scale it
    /// </summary>
    [SerializeField] private Transform _stageT;

    /// <summary>
    /// The parent of all the candies in the hierarchy 
    /// </summary>
    [SerializeField] private Transform _candiesContainer;

    /// <summary>
    /// Our agent
    /// </summary>
    [SerializeField] private Transform _agentT;

    /// <summary>
    /// Choose if the agent should be randomly placed or not
    /// </summary>
    [SerializeField] private bool _isAgentLocationRandom = false;
    
    /// <summary>
    /// The candies elements on the board
    /// </summary>
    private RewardCandyScript[] _rewardCandies;
    
    /// <summary>
    /// The materials for the visual indication of the win/lose state
    /// </summary>
    private Material _floorMaterial;
    
    /// <summary>
    /// Floor material to change the color of the floor according to the win/lose state
    /// </summary>
    [SerializeField] private MeshRenderer floorMeshRenderer;
    
    /// <summary>
    /// The ML agent
    /// </summary>
    [SerializeField] private AgentScript _agentScript;

    /// <summary>
    /// Moving speed of the agent
    /// </summary>
    [SerializeField] [Range(1, 6)] private float _moveSpeed = 4f;
    
    /// <summary>
    /// The penalty for being idle
    /// </summary>
    [SerializeField] [Range(0, 0.1f)] private float _idleTimePenalty = 0;

    /// <summary>
    /// if 0 then there is no time limit, otherwise the episode will end after this time
    /// </summary>
    [SerializeField] [Range(0, 100)] private int _maxEpisodeTime = 0; 
    
    /// <summary>
    /// Penalty for timeout
    /// </summary>
    [SerializeField] [Range(-10, 0)] private int _timeoutPenalty = -2;
    
    /// <summary>
    /// Penalty for going out of stage
    /// </summary>
    [SerializeField] [Range(-10, 0)] private float _offStagePenalty = -2;
    
    /// <summary>
    /// Measure the episode time, so we can stop it if needed
    /// </summary>
    private float _episodeTimeStart = 0;
    
    /// <summary>
    /// Keep count of the number of candies so we can end the training
    /// </summary>
    private int _numberOfCandiesCollected = 0;
    
    /// <summary>
    /// Indicate if we are during an episode
    /// </summary>
    private bool _duringEpisode = false;

    /// <summary>
    /// During the tutorial the agent acts differently
    /// </summary>
    private bool _tutorialMode = false;

    /// <summary>
    /// Callback to tell the parent that a reward was collected
    /// </summary>
    private Action<int, bool> _OnRewardCollected;

    /// <summary>
    /// Agent location 
    /// </summary>
    private Vector3 _agentLocation;

    /// <summary>
    /// Candies locations
    /// </summary>
    /// <returns></returns>
    private Vector3[] _candiesLocation;
    
    private void Start()
    {
        // Scale the stage
        _stageT.localScale = new Vector3(_stageScale, 1, _stageScale);
        
        // Create materials
        _floorMaterial = new Material(Shader.Find("Standard"));
        _floorMaterial.color = FLOOR_IDLE_COLOR;
        floorMeshRenderer.material = _floorMaterial;

        if (!_tutorialMode)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        // Remove all children from the candies container
        foreach (Transform child in _candiesContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create the candies
        _rewardCandies = new RewardCandyScript[_numOfCandies];

        for (int candyIndex = 0; candyIndex < _numOfCandies; candyIndex++)
        {
            GameObject candy = Instantiate(Resources.Load("RewardCandy") as GameObject, _candiesContainer);
            _rewardCandies[candyIndex] = candy.GetComponent<RewardCandyScript>();
            _rewardCandies[candyIndex].gameObject.SetActive(false);
        }
        
        _agentScript.SetReadyToMove(false);
        _agentScript.Initialize(_moveSpeed, _numOfCandies, _idleTimePenalty, _timeoutPenalty, _offStagePenalty, _rewardCandies, OnStartOfEpisode, OnAgentHitWall, OnAgentCollectedReward, OnFirstMove);
        _agentScript.ShowAgent(false);
        
        if (_tutorialMode)
        {
            OnStartOfEpisode();
        } 
    }

    private void Update()   
    {
        if (_duringEpisode)
        {
            if (_maxEpisodeTime > 0)
            {
                if ((Time.time - _episodeTimeStart) > _maxEpisodeTime)
                {
                    Debug.Log("Ending episode due to time out");

                    _agentScript.EpisodeTimeout();
                }
            }
        }
    }

    private void OnStartOfEpisode()
    {
        _numberOfCandiesCollected = 0;
        
        _floorMaterial.DOColor(FLOOR_IDLE_COLOR, 0.3f).SetDelay(0.2f);

        StartCoroutine(GeneralUtils.WaitAndPerform(1, () =>
        {
            // Show agent
            _agentScript.ShowAgent(true);
            SetAgentOnBoard(_agentLocation);
            
            AudioManager.Instance.PlayGeneralSound(AudioManager.Sound__showPlayer);
            
            StartCoroutine(GeneralUtils.WaitAndPerform(1, () =>
            {
                SetCandiesOnBoard(_candiesLocation);
                SetCandiesReward();
                
                _agentScript.SetReadyToMove(true);
                
                // Keep the start time so we know to calculate the episode time
                _episodeTimeStart = Time.time;

                _duringEpisode = true;
            }));
        }));
    }

    private void OnAgentHitWall()
    {
        // fade out color in material tween
        _floorMaterial.DOColor(FLOOR_HIT_WALL_COLOR, 0.3f);
        
        Debug.Log("End episode because hit wall");
        
        _agentScript.ShowAgent(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>true if episode ended</returns>
    private bool OnAgentCollectedReward(int rewardSum)
    {
        bool needToEndEpisode = false;
        
        _numberOfCandiesCollected++;
        
        _OnRewardCollected?.Invoke(rewardSum, (_numberOfCandiesCollected == _rewardCandies.Length));

        _floorMaterial.DOKill();
        _floorMaterial.DOColor(FLOOR_CANDY_COLLECTED_COLOR, 0.05f).OnComplete(() =>
        {
            _floorMaterial.DOColor(FLOOR_IDLE_COLOR, 0.1f);
        });
                
        if (_numberOfCandiesCollected == _rewardCandies.Length)
        {
            Debug.Log("End episode because all rewards were collected");

            _agentScript.ShowAgent(false);
            
            needToEndEpisode = true;
        }

        return needToEndEpisode;
    }

    private void OnFirstMove()
    {
        if (_tutorialMode)
        {
            EventManagerScript.Instance.TriggerEvent(EventManagerScript.EVENT__PLAYER_FIRST_MOVE, null);
        }
    }

    private void SetAgentOnBoard(Vector3 agentLocation)
    {
        _agentT.localPosition = agentLocation;
    }

    private void SetCandiesOnBoard(Vector3[] candiesLocation)
    {
        // Set all the candies on the board and make sure that they are not too close to the agent or to another candy
        for (int candyIndex = 0; candyIndex < _rewardCandies.Length; candyIndex++)
        {
            // Put in random position
            _rewardCandies[candyIndex].gameObject.SetActive(false);
            _rewardCandies[candyIndex].transform.localPosition = candiesLocation[candyIndex];
        }
    }
    
    private void SetCandiesReward()
    {
        float totalReward = 0;
        
        for (int candyIndex = 0; candyIndex < _rewardCandies.Length; candyIndex++)
        {
            float reward = (candyIndex + 1) * 2;
            totalReward += reward;
            
            
            StartCoroutine(SetCandiesRewardHelper(0.05f * candyIndex, candyIndex, reward));
        }
        
        // Debug.Log("Total rewards on board: " + totalReward);
    }
    
        
    private IEnumerator SetCandiesRewardHelper(float showTime, int candyIndex, float reward)
    {
        yield return new WaitForSeconds(showTime);
        
        _rewardCandies[candyIndex].ResetCandy(reward);           
                
        AudioManager.Instance.PlayGeneralSound(AudioManager.Sound__showCandy);
    }

    public void EnablePlayer(bool bShow)
    {
        _agentScript.ShowAgent(bShow);
    }

    public void SetTutorialStep(int step)
    {
        if (step == 1)
        {
            _tutorialMode = true;
            _isAgentLocationRandom = false;
            _numOfCandies = 1;
            
            Initialize();

        }
        else if (step == 2)
        {
            _numOfCandies = 2;
            
            Initialize();
        }
        else if (step == 3)
        {
            _isAgentLocationRandom = true;
            _numOfCandies = 4;
            
            Initialize();
        }
        else if ((step == 4) || (step == 5)) 
        {
            _isAgentLocationRandom = true;
            _numOfCandies = 4;
            
            Initialize();
        }
    }

    public void SetCallbacks(Action<int, bool> onRewardCollected)
    {
        _OnRewardCollected = onRewardCollected;
    }

    public int GetTotalPoints()
    {
        int totalPoints = 0;
        
        for (int candyIndex = 0; candyIndex < _rewardCandies.Length; candyIndex++)
        {
            if (_rewardCandies[candyIndex].IsRewardCollected())
            {
                totalPoints += (int)_rewardCandies[candyIndex].GetRewardValue();
            }
        }

        return totalPoints;
    }

    public void OtherPlayerWon()
    {
        _agentScript.ShowAgent(false);
            
        _agentScript.SetReadyToMove(false);
    }

    public void StartAnotherMatch()
    {
        Initialize();
        
        OnStartOfEpisode();
    }

    public void SetLocations(Vector3 agentLocation, Vector3[] candiesLocation)
    {
        _agentLocation = agentLocation;
        _candiesLocation = candiesLocation;
    }
}
