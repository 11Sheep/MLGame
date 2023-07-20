using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberPickerElement : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI _YearText; 

    public void SetYear(int year)
    {
        _YearText.text = year.ToString();
    }
}
