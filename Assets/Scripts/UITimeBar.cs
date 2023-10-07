using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimeBar : MonoBehaviour {
    public RectTransform fillRect;
    public Image fillColor;
    public Gradient gradient;

    private void Update() {
        float factor = GameManager.instance.currentTimeToMatch / GameManager.instance.timeToMatch;
        factor = Mathf.Clamp(factor, 0f, 1f);
        factor = 1 - factor;
        fillRect.localScale = new Vector3(factor, 1, 1);
        fillColor.color = gradient.Evaluate(factor);
    }

}
