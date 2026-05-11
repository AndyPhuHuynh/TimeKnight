using System;
using System.Collections;
using TimeKnight.Core.TimePower;
using TimeKnight.Utils;
using UnityEngine;

public class TimeWipe : MonoBehaviour
{
    private RectTransform rectTransform = null!;
    [SerializeField] private float scaleRate = 1f;
    [SerializeField] private float maxScale = 20f;
    private CoWrapper timeWipeWrapper = null!;

    private void Awake()
    {
        timeWipeWrapper = new CoWrapper(this);
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        TimeManager.OnTimeSlowActivate += OnTimeSlowActivate;
        TimeManager.OnTimeSlowDeactivate += OnTimeSlowDeactivate;

    }

    private void OnDisable()
    {
        TimeManager.OnTimeSlowActivate -= OnTimeSlowActivate;
        TimeManager.OnTimeSlowDeactivate -= OnTimeSlowDeactivate;
    }

    private void OnTimeSlowActivate()
    {
        timeWipeWrapper.Start(ExpandTimeWipe());
    }

    private void OnTimeSlowDeactivate()
    {
        timeWipeWrapper.Start(ShrinkTimeWipe());
    }

    private IEnumerator ExpandTimeWipe()
    {
        while (rectTransform.localScale.x < maxScale)
        {

            float newScale = rectTransform.localScale.x + Time.deltaTime * scaleRate;

            rectTransform.localScale = new Vector3(newScale, newScale, newScale);
            yield return null;
        }

    }

    private IEnumerator ShrinkTimeWipe()
    {
        while (rectTransform.localScale.x > 0)
        {

            float newScale = rectTransform.localScale.x - Time.deltaTime * scaleRate;

            rectTransform.localScale = new Vector3(newScale, newScale, newScale);
            yield return null;
        }

    }
}
