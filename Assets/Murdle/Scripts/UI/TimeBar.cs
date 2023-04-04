using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeBar : MonoBehaviour
{
    private Image BarImage;
    void Awake()
    {
        BarImage = transform.Find("Bar").GetComponent<Image>();
        BarImage.fillAmount = .3f;
    }

    public void SetProgress(float fillAmount)
    {
        BarImage.fillAmount = fillAmount;
    }
}
