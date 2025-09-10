using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotAnimation : MonoBehaviour
{
    public GameObject AllSlots, slot1, slot2, slot3;
    public Button Play;
    private void Awake()
    {
        Play.onClick.AddListener(delegate { ScaleSlot(); });
    }

    void ScaleSlot()
    {
        AllSlots.transform.DOScale(new Vector3(2, 2, 2), 2).OnComplete(AnimPlay);

    }
    public void AnimPlay()
    {
     
        slot1.transform.localPosition = new Vector3(-132,0,0);
        slot2.transform.localPosition = new Vector3(0, 0, 0);
        slot3.transform.localPosition = new Vector3(132, 0, 0);


        int a = Random.Range(0, 9);
        int b = Random.Range(0, 9);
        int c = Random.Range(0, 9);
        print(a + " " + b + " " + c);
        int count = slot1.transform.childCount;

        for (int i = count; i > 0; i--)
        {

            slot1.transform.GetChild(i - 1).GetChild(0).GetComponent<TMP_Text>().text = (a + i) % 10 + "";
            slot2.transform.GetChild(i - 1).GetChild(0).GetComponent<TMP_Text>().text = (b + i) % 10 + "";
            slot3.transform.GetChild(i - 1).GetChild(0).GetComponent<TMP_Text>().text = (c + i) % 10 + "";
        }
       
        float childHeight = slot1.transform.GetChild(0).GetComponent<RectTransform>().rect.height;
        float targetY = (count - 1) * childHeight;


        slot1.GetComponent<RectTransform>().DOAnchorPosY(targetY, 2).SetEase(Ease.OutQuad);
        slot2.GetComponent<RectTransform>().DOAnchorPosY(targetY, 2.5f).SetEase(Ease.OutQuad);
        slot3.GetComponent<RectTransform>().DOAnchorPosY(targetY, 3).SetEase(Ease.OutQuad).OnComplete(AnimComplete);
    }



    void AnimComplete()
    {
        AllSlots.transform.DOScale(new Vector3(1, 1, 1), 2);
    }
}
