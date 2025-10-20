using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaternManager : MonoBehaviour
{

    [SerializeField] private Animator _lanternLeftAnimator;
    [SerializeField] private Animator _lanternRightAnimator;
    [SerializeField] private Button _windButton;

    void Start()
    {
        if (_windButton != null) _windButton.onClick.AddListener(OnWindButtonClicked);
    }

    private void OnWindButtonClicked()
    {
        _lanternLeftAnimator.SetTrigger("wind");
        _lanternRightAnimator.SetTrigger("wind");
    }

    private void OnDestroy()
    {
        _windButton.onClick.RemoveListener(OnWindButtonClicked);
    }
}

