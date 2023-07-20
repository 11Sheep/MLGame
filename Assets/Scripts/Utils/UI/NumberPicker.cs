using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NumberPicker : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    private const float SNAP_START_DETECTION_DISTANCE = 5f;
    private const float YEAR_ELEMENT_HEIGHT = 150f;

    [SerializeField] private RectTransform _Content;

    private bool _WaitForWheelToStop = false;
    private float _WheelPositionToCheck = 0;
    private float _WheelPositionTimer = 0;
    private float _SnapCounter = 0;
    private float _SnapPoint = 0;

    [SerializeField] private int _StartNumber = 1934;
    [SerializeField] private int _NumOfIndexes = 69;

    // Start is called before the first frame update
    void Start()
    {
        for (int index = 0; index < _NumOfIndexes; index++)
        {
            GameObject go = Instantiate(Resources.Load("NumberPickerElement")) as GameObject;
            go.GetComponent<NumberPickerElement>().SetYear(_StartNumber + (_NumOfIndexes - index - 1));
            go.transform.SetParent(_Content, false);
            (go.transform as RectTransform).anchoredPosition = new Vector2(0, YEAR_ELEMENT_HEIGHT * index - (YEAR_ELEMENT_HEIGHT * (_NumOfIndexes - 1) / 2f));
        }

        _Content.sizeDelta = new Vector2(_Content.sizeDelta.x, YEAR_ELEMENT_HEIGHT * (_NumOfIndexes + 4));
    }

    public int GetSelectedValue()
    {
        int closestIndex = Mathf.RoundToInt(_Content.anchoredPosition.y / 150f);

        return (_StartNumber + closestIndex + (_NumOfIndexes / 2));
    }

    private void Update()
    {
        if (_WaitForWheelToStop)
        {
            _WheelPositionTimer -= Time.deltaTime;

            if (_WheelPositionTimer <= 0)
            {
                if (Mathf.Abs(_WheelPositionToCheck - _Content.anchoredPosition.y) < SNAP_START_DETECTION_DISTANCE)
                {
                    // We are ready to snap
                    //Debug.Log("Ready to snap");

                    _WaitForWheelToStop = false;

                    _SnapPoint = Mathf.RoundToInt(_Content.anchoredPosition.y / 150f) * 150f;
                    float distanceToPoint = _Content.anchoredPosition.y - _SnapPoint;

                    _SnapCounter = distanceToPoint / 100f;
                }
                else
                {
                    _WheelPositionTimer = 0.1f;
                    _WheelPositionToCheck = _Content.anchoredPosition.y;
                }
            }
        }

        if (_SnapCounter != 0)
        {
            if (_SnapPoint > _Content.anchoredPosition.y)
            {
                _Content.anchoredPosition += new Vector2(0, Time.deltaTime * 100);
            }
            else
            {
                _Content.anchoredPosition -= new Vector2(0, Time.deltaTime * 100);
            }

            if (Mathf.Abs(_SnapPoint - _Content.anchoredPosition.y) < 1f)
            {
                _Content.anchoredPosition = new Vector2(0, _SnapPoint);

                _SnapCounter = 0;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _WheelPositionTimer = 0.1f;
        _WheelPositionToCheck = _Content.anchoredPosition.y;
        _WaitForWheelToStop = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _WaitForWheelToStop = false;
        _SnapCounter = 0;
    }
}
