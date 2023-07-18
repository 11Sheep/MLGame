using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardCandyScript : MonoBehaviour
{
    [SerializeField] private float _rewardValue = 1;

    /// <summary>
    /// True if the reward was colelcted
    /// </summary>
    private bool _rewardCollected = false;
    
    public float GetRewardValue()
    {
        return _rewardValue;
    }
    
    public void ResetCandy(float rewardValue)
    {
        _rewardValue = rewardValue;
        gameObject.SetActive(true);
        _rewardCollected = false;
        
        // Create a material for the sphere
        var material = new Material(Shader.Find("Standard"));
        float rewardRedColor = 1 - (rewardValue / 10);
        material.color = new Color(1, rewardRedColor, rewardRedColor);
        GetComponent<MeshRenderer>().material = material;
    }
    
    public void RewardCollected()
    {
        _rewardCollected = true;
        gameObject.SetActive(false);
    }

    public bool IsRewardCollected()
    {
        return _rewardCollected;
    }
}
