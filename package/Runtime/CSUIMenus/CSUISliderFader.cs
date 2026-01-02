using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// CSUISliderFader
// handles a triple slider arrangement, to show slider growth/shrinkage over time with changes
// created 14/3/24

public class CSUISliderFader : MonoBehaviour
{
    [SerializeField] private Slider slider; // front "main" colour bar
    [SerializeField] private Slider sliderFade; // middle "hit" colour bar
    [SerializeField] private Slider sliderGrow; // back "restore" colour bar
    [SerializeField] private float slidePerTime = 0.5f; // amount slider can change per unit time
    private float barValueActual;
    private float barValueActualMax;
    private float barValueActualMin;
    private float barValueGradual = -1f;
    private float barValue = -1f;
    private Coroutine coroutineBarGradual;


    // shows the amount of the bar that will be consumed (for showing ammo consumption)
    // call with spend = 0 to just reset the bar
    // returns true if the current bar value is not enough for the spend
    public bool ShowBarCost(float spend)
    {
        if (barValueActualMax <= 0) return true;

        gameObject.SetActive(true);

        float valueLeft = barValueActual - spend;
        float valueTemp;
        bool notEnough = false;

        if (valueLeft < 0)
        {
            // show some sort of alert indicator or X indicator
            valueLeft = 0;
            notEnough = true;
        }
        valueTemp = BarValue(valueLeft);

        slider.value = valueTemp;
        sliderFade.value = barValue;
        sliderGrow.value = barValue;

        if (coroutineBarGradual != null)
            StopCoroutine(coroutineBarGradual);

        return notEnough;
    }

    // takes the maximum and current values of health and updates the bar and counter
    // gradualDurationOverride is when we want to make it seem like the resource is changing gradually (e.g. ammo running down over time while firing)
    public void UpdateBar(float value, float valueMax, float valueMin = -1f)
    {
        bool changed = false;

        gameObject.SetActive(true);

        if (value != barValue || valueMax != barValueActualMax || valueMin != barValueActualMin)
        {
            changed = true;
            barValueActual = value;
            barValueActualMax = valueMax;
            barValueActualMin = valueMin;
        }

        float valueNew = BarValue(barValueActual);

        if (barValue != valueNew)
            changed = true;

        if (changed)
        {
            barValue = valueNew;

            if (coroutineBarGradual != null)
                StopCoroutine(coroutineBarGradual);

            if (barValueGradual < 0)
            {
                // do it all right now - either forced, or because this is the first time the bar is being set
                barValueGradual = valueNew;
                slider.value = valueNew;
                sliderFade.value = valueNew;
                sliderGrow.value = valueNew;
                return;
            }
            else
                coroutineBarGradual = StartCoroutine(UpdateBarGradual());
        }
    }
    // simple version for just setting the bar fill value directly
    public void Updatebar(float value)
    {
        UpdateBar(value, 1f);
    }
    // force the bar to value immediately
    public void SetBar(float value)
    {
        barValueActual = value;
        barValueActualMax = 1f;
        barValueActualMin = -1f;
        barValue = value;
        barValueGradual = value;
        slider.value = value;
        sliderFade.value = value;
        sliderGrow.value = value;
    }

    private IEnumerator UpdateBarGradual()
    {
        if (barValueGradual > barValue) // show bar reducing
        {
            slider.value = barValue;
            sliderFade.value = barValueGradual;
            sliderGrow.value = barValue; // doesn't matter, just needs to be out of sight
        }
        else // show bar increasing
        {
            slider.value = barValueGradual;
            sliderFade.value = barValueGradual; // doesn't matter, just needs to be out of sight
            sliderGrow.value = barValue;
        }

        while (barValue != barValueGradual)
        {
            float frameTime = Time.deltaTime;
            float frameAmount = frameTime * slidePerTime;
            if (barValueGradual > barValue)
            {
                barValueGradual = Mathf.Max(barValueGradual - frameAmount, barValue);
                sliderFade.value = barValueGradual;
            }
            else if (barValueGradual < barValue)
            {
                barValueGradual = Mathf.Min(barValueGradual + frameAmount, barValue);
                slider.value = barValueGradual;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    // calculate the correct bar fill for the provided value, given the min and max values
    private float BarValue(float value)
    {
        if (barValueActualMax <= 0)
            return 1f;
        if (barValueActualMin >= 0)
            return Mathf.Clamp((value - barValueActualMin) / (barValueActualMax - barValueActualMin), 0f, 1f);

        return Mathf.Clamp((value / barValueActualMax), 0f, 1f);
    }
}
