using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class AgentScript : Agent
{
    /// <summary>
    /// The moving speed 
    /// </summary>
    private float _moveSpeed = 4;
    
    /// <summary>
    /// Number of candies on board
    /// </summary>
    private int _numberOfCandies = 1;

    /// <summary>
    /// Penalty for being idle
    /// </summary>
    private float _idleTimePenalty = 0f;

    /// <summary>
    /// Callback to tell the parent that episode has started
    /// </summary>
    private Action _OnStartOfEpisode;
    
    /// <summary>
    /// Callback to tell the parent that agent hit the wall
    /// </summary>
    private Action _OnAgentHitWall;

    /// <summary>
    /// Callback to tell the parent that agent collected candy
    /// </summary>
    private Func <int, bool> _OnAgentCollectedReward;

    /// <summary>
    /// Callback to tell the parent about the first move (for the tutorial)
    /// </summary>
    private Action _OnPlayerFirstMove;
    
    /// <summary>
    /// Penalty for timeout
    /// </summary>
    private float _timeoutPenalty = -2;
    
    /// <summary>
    /// Penalty for going out of stage
    /// </summary>
    private float _offStagePenalty = -2;
    
    /// <summary>
    /// The candies elements on the board
    /// </summary>
    private RewardCandyScript[] _rewardCandies;
    
    /// <summary>
    /// 
    /// </summary>
    [SerializeField] private BehaviorParameters _behaviorParameters;

    /// <summary>
    /// Face / eyes / mouth
    /// </summary>
    [SerializeField] private GameObject[] _agentVisualElements;
    
    /// <summary>
    /// True if initialized
    /// </summary>
    private bool _initialized = false;

    /// <summary>
    /// Needed for the tutorial (to show the user the hint if not moving)
    /// </summary>
    private bool _firstMoveReported = false;

    /// <summary>
    /// We want a delay before the agent can move so we can show the agent and candies
    /// </summary>
    private bool _readyToMove = false;

    private bool _pengingEpisodeStart = false;
    
    public void Initialize(float moveSpeed, int numberOfCandies, float idleTimePenalty, float timeoutPenalty, float offStagePenalty, RewardCandyScript[] rewardCandies, Action OnStartOfEpisode, Action OnAgentHitWall, Func<int, bool> OnAgentCollectedReward, Action OnPlayerFirstMove)
    {
        _moveSpeed = moveSpeed;
        _numberOfCandies = numberOfCandies;
        _idleTimePenalty = idleTimePenalty;
        _timeoutPenalty = timeoutPenalty;
        _offStagePenalty = offStagePenalty;
        _rewardCandies = rewardCandies;
        _OnStartOfEpisode = OnStartOfEpisode;
        _OnAgentHitWall = OnAgentHitWall;
        _OnAgentCollectedReward = OnAgentCollectedReward;
        _OnPlayerFirstMove = OnPlayerFirstMove;

        _initialized = true;

        if (_pengingEpisodeStart)
        {
            _pengingEpisodeStart = false;
            _OnStartOfEpisode?.Invoke();
        }
    }
    
    public override void OnEpisodeBegin()
    {
        if (!_initialized)
        {
            _pengingEpisodeStart = true;
        }
        else
        {
            _OnStartOfEpisode?.Invoke();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (_initialized)
        {
            // Add the agent's local position as input
            sensor.AddObservation(transform.localPosition);

            for (int candyIndex = 0; candyIndex < _rewardCandies.Length; candyIndex++)
            {
                if (_rewardCandies[candyIndex].IsRewardCollected())
                {
                    // Add the candy position as input
                    sensor.AddObservation(Vector3.zero);

                    // Add the candy reward
                    sensor.AddObservation(-0.001f);
                }
                else
                {
                    // Add the candy position as input
                    sensor.AddObservation(_rewardCandies[candyIndex].transform.localPosition);

                    // Add the candy reward
                    sensor.AddObservation(_rewardCandies[candyIndex].GetRewardValue());
                }
            }

            for (int candyIndex = _rewardCandies.Length; candyIndex < AppConfiguration.NumberOfCandies; candyIndex++)
            {
                // Add the candy position as input
                sensor.AddObservation(Vector3.zero);

                // Add the candy reward
                sensor.AddObservation(0);
            }
        }

        base.CollectObservations(sensor);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (_initialized && _readyToMove)
        {
            float moveX = actions.ContinuousActions[0];
            float moveZ = actions.ContinuousActions[1];

            transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime * _moveSpeed;

            if (_idleTimePenalty != 0)
            {
                AddReward(_idleTimePenalty);
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (_initialized && _readyToMove)
        {
            ActionSegment<float> continuesActions = actionsOut.ContinuousActions;
            continuesActions[0] = Input.GetAxisRaw("Horizontal");
            continuesActions[1] = Input.GetAxisRaw("Vertical");

            // Report the first move of the player
            if (((continuesActions[0] != 0) || (continuesActions[1] != 0)) && !_firstMoveReported)
            {
                _firstMoveReported = true;

                _OnPlayerFirstMove?.Invoke();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("wall"))
        {
            _OnAgentHitWall?.Invoke();
            
            SetReward(_offStagePenalty);

            RequestToEndEpisode();
        }
        else if (other.tag.Equals("candy"))
        {
            RewardCandyScript rewardCandyScript = other.GetComponent<RewardCandyScript>();
            if (rewardCandyScript != null)
            {
                // Debug.Log("Add reward value of: " + rewardCandyScript.GetRewardValue() + " to agent");
                
                SetReward(rewardCandyScript.GetRewardValue());
                rewardCandyScript.RewardCollected();

                bool needToEndEpisode = _OnAgentCollectedReward((int)rewardCandyScript.GetRewardValue());

                if (needToEndEpisode)
                {
                    RequestToEndEpisode();
                }
            }
        }
    }

    /// <summary>
    /// Handle timeout of episode (when enabled)
    /// </summary>
    public void EpisodeTimeout()
    {
        SetReward(_timeoutPenalty);

        RequestToEndEpisode();
    }

    private void RequestToEndEpisode()
    {
        _readyToMove = false;
        
        // TODO: when in simulation we need to enable this
        // EndEpisode();
    }

    public void ShowAgent(bool b)
    {
        for (int i = 0; i < _agentVisualElements.Length; i++)
        {
            _agentVisualElements[i].SetActive(b);
        }
        
        GetComponent<MeshRenderer>().enabled = b;        
    }

    public void SetReadyToMove(bool isReady)
    {
        _readyToMove = isReady;
    }
}
