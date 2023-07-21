using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RewardCandyScript : MonoBehaviour
{
    [SerializeField] private float _rewardValue = 1;

    [SerializeField] private TextMeshPro _rewardText;

    /// <summary>
    /// Moves the candy just for fun
    /// </summary>
    private float _moveEffectTimer = 0;
    
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

        _rewardText.text = rewardValue.ToString();
    }

    private void Update()
    {
        _moveEffectTimer -= Time.deltaTime;

        if (_moveEffectTimer <= 0)
        {
            _moveEffectTimer = Random.Range(0.5f, 1.5f);
            
            float moveEffectHeight = Random.Range(0f, 0.15f);

            transform.DOMoveY(moveEffectHeight, _moveEffectTimer);
        }
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
