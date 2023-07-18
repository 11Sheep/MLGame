using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
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
    private Func <bool> _OnAgentCollectedReward;
    
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
    
    public void Initialize(float moveSpeed, int numberOfCandies, float idleTimePenalty, float timeoutPenalty, float offStagePenalty, RewardCandyScript[] rewardCandies, Action OnStartOfEpisode, Action OnAgentHitWall, Func<bool> OnAgentCollectedReward)
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
    }
    
    public override void OnEpisodeBegin()
    {
        _OnStartOfEpisode?.Invoke();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);

        for (int candyIndex = 0; candyIndex < _rewardCandies.Length; candyIndex++)
        {
            if (_rewardCandies[candyIndex].IsRewardCollected())
            {
                sensor.AddObservation(Vector3.zero);
                sensor.AddObservation(0);
            }
            else
            {
                sensor.AddObservation(_rewardCandies[candyIndex].transform.localPosition);
                sensor.AddObservation(_rewardCandies[candyIndex].GetRewardValue());
            }
        }

        base.CollectObservations(sensor);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        
        transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime * _moveSpeed;

        if (_idleTimePenalty != 0)
        {
            AddReward(_idleTimePenalty);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuesActions = actionsOut.ContinuousActions;
        continuesActions[0] = Input.GetAxisRaw("Horizontal");
        continuesActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("wall"))
        {
            _OnAgentHitWall?.Invoke();
            
            SetReward(_offStagePenalty);
            
            EndEpisode();
        }
        else if (other.tag.Equals("candy"))
        {
            RewardCandyScript rewardCandyScript = other.GetComponent<RewardCandyScript>();
            if (rewardCandyScript != null)
            {
                Debug.Log("Add reward value of: " + rewardCandyScript.GetRewardValue() + " to agent");
                
                SetReward(rewardCandyScript.GetRewardValue());
                rewardCandyScript.RewardCollected();

                bool needToEndEpisode = _OnAgentCollectedReward();

                if (needToEndEpisode)
                {
                    EndEpisode();
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
        
        EndEpisode();
    }
}
