using System.Collections;
using UnityEngine;


class FCToolsPanelFunctionButtonBase : MonoBehaviour
{
    private Vector3 _originalPosition;

    public Transform destButtonTransform;

    void Start()
    {
        _originalPosition = gameObject.transform.localPosition;
    }

    void Update()
    {

    }

    public void StepToMoveAndScale(bool visible, float percent)
    {
        gameObject.SetActive(true);
        Vector3 begin = visible ? destButtonTransform.localPosition + new Vector3(-40, 40, 0) : _originalPosition;
        Vector3 end = visible ? _originalPosition : destButtonTransform.localPosition + new Vector3(-40, 40, 0);

        float OffsetX = end.x - begin.x;
        float OffsetY = end.y - begin.y;
        if (percent < 1)
        {
            float newScale = visible ? percent : 1 - percent;

            if (!visible)//tween to hide
            { 
                gameObject.transform.localScale = new Vector3(newScale, newScale, 1);
                gameObject.transform.localPosition = new Vector3(begin.x + OffsetX * percent, begin.y + OffsetY * percent, 0);
            }
            else
            {
                gameObject.transform.localScale = Vector3.one;
                gameObject.transform.localPosition = new Vector3(iTween.easeOutElastic(begin.x, end.x, percent),
                iTween.easeOutElastic(begin.y, end.y, percent), 0);
            }
            //no tween way.
            //gameObject.transform.localPosition = new Vector3(begin.x + OffsetX * percent, begin.y + OffsetY * percent, 0);
        }
        else
        {
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.localPosition = end;
            gameObject.SetActive(visible);
        }
    }
}

