using UnityEngine;

public class CardAnimationHandler : MonoBehaviour
{
    public void OnCardAnimationEnd()
    {
        Debug.Log("Card animation ended.");
        LevelManager.Instance.ShowLevelUpPopup();
    }
}
