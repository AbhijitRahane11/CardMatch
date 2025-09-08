using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Card : MonoBehaviour
{
    public int cardID;
    public Image frontImageUI;
    public Image backImageUI;
    public Sprite frontSprite;
    public Sprite backSprite;
    public float flipDuration = 0.3f;
    private bool isFlipped = false;
    private bool isMatched = false;

    public bool IsMatched => isMatched;

    public bool IsBusy =>
        DOTween.IsTweening(frontImageUI.transform) ||
        DOTween.IsTweening(backImageUI.transform);

    public void SetupCard(int id, Sprite front, Sprite back)
    {
        cardID = id;
        frontSprite = front;
        backSprite = back;
        frontImageUI.sprite = frontSprite;
        backImageUI.sprite = backSprite;
        // Initialize to face-down
        isFlipped = false;
        isMatched = false;
        frontImageUI.transform.eulerAngles = new Vector3(0, 90, 0);// Vector3.zero;
        backImageUI.transform.eulerAngles = Vector3.zero;//new Vector3(0, 90, 0);
    }

    public void OnCardClicked()
    {
        if (isMatched || IsBusy || !GameManager.Instance.CanInteract) return;
        FlipCard();
        GameManager.Instance.OnCardFlipped(this);
    }

    public void FlipCard()
    {
        isFlipped = !isFlipped;
        SoundManager.Instance.PlayFlipSound();

        if (isFlipped)
        {
            frontImageUI.transform.DORotate(new Vector3(0, 0, 0), flipDuration).From(new Vector3(0, 90, 0));
            backImageUI.transform.DORotate(new Vector3(0, 90, 0), flipDuration).From(Vector3.zero);
        }
        else
        {
            backImageUI.transform.DORotate(new Vector3(0, 0, 0), flipDuration).From(new Vector3(0, 90, 0));
            frontImageUI.transform.DORotate(new Vector3(0, 90, 0), flipDuration).From(Vector3.zero);
        }
    }

    public void FlipFront()
    {
        frontImageUI.transform.DORotate(new Vector3(0, 0, 0), flipDuration).From(new Vector3(0, 90, 0));
        backImageUI.transform.DORotate(new Vector3(0, 90, 0), flipDuration).From(Vector3.zero);

    }
    public void FlipBack()
    {
        backImageUI.transform.DORotate(new Vector3(0, 0, 0), flipDuration).From(new Vector3(0, 90, 0));
        frontImageUI.transform.DORotate(new Vector3(0, 90, 0), flipDuration).From(Vector3.zero);
    }

    public void SetMatched()
    {
        isMatched = true;
    }
}
