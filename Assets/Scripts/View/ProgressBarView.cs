using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarView : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;

    public void UpdateBar(float progress)
    {
        progressBar.value = progress;
        progressText.text = "Loading... " + Mathf.Round(progress * 100f) + " %";
    }
}
