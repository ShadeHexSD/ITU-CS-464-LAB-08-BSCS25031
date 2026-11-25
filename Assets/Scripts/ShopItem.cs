using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put on a shop card that has a buy Button and a price label.
// NOTE: the class name must match this file name so Unity can attach it.
public class ShopItem : MonoBehaviour
{
    enum Reward { Coins, Gems, Skin }

    [SerializeField] Reward reward = Reward.Coins;
    [SerializeField] int amount = 500;      // how many coins or gems you get
    [SerializeField] int gemCost = 0;       // 0 = free
    [SerializeField] bool oneTime = true;   // can only be bought once
    [SerializeField] Button buyButton;
    [SerializeField] TMP_Text priceLabel;

    bool owned;
    string originalText;
    Coroutine flash;

    void Awake()
    {
        // If you forgot to drag them in, look for them inside this card.
        if (buyButton == null) buyButton = GetComponentInChildren<Button>(true);
        if (priceLabel == null && buyButton != null)
            priceLabel = buyButton.GetComponentInChildren<TMP_Text>(true);

        // Tell the developer (you) what is wrong instead of failing silently.
        if (buyButton == null) { Debug.LogError(name + ": no Button found inside this card.", this); return; }
        if (priceLabel == null) { Debug.LogError(name + ": no price label found inside the buy button.", this); return; }

        originalText = priceLabel.text;
        buyButton.onClick.AddListener(Buy);
    }

    void Buy()
    {
        if (owned) return;

        // Not enough gems: say so, take nothing, and play error feedback.
        if (gemCost > 0 && !Wallet.TrySpendGems(gemCost))
        {
            Toast.Instance?.Show("Not enough gems. Try the gem pack.", true);
            return;
        }

        switch (reward)
        {
            case Reward.Coins: Wallet.AddCoins(amount); break;
            case Reward.Gems: Wallet.AddGems(amount); break;
            case Reward.Skin: SkinStore.UnlockChrome(); break;
        }

        if (oneTime)
        {
            owned = true;
            SetLabel("Owned");
            buyButton.interactable = false;
            if (buyButton.image != null) buyButton.image.color = new Color(1f, 1f, 1f, 0.5f);  // dimmed = finished
        }
        else
        {
            // Repeatable item: flash what you got, then go back to the price.
            if (flash != null) StopCoroutine(flash);
            flash = StartCoroutine(FlashLabel("+" + amount));
        }

        Toast.Instance?.Show("Purchased!");
        StartCoroutine(Punch());
    }

    void SetLabel(string text) { priceLabel.text = text; }

    IEnumerator FlashLabel(string text)
    {
        SetLabel(text);
        yield return new WaitForSecondsRealtime(0.8f);
        SetLabel(originalText);
    }

    IEnumerator Punch()
    {
        // The label bounces so your eye goes to what changed.
        yield return Tween.Run(0.38f, Ease.OutBack, p =>
        {
            float s = Mathf.LerpUnclamped(1.3f, 1f, p);
            priceLabel.transform.localScale = new Vector3(s, s, 1f);
        });
    }
}
