using System.Collections.Generic;
using UnityEngine.Purchasing;
using System.Collections;
using UnityEngine;
using System;
using TMPro;

public class IAPManager : MonoBehaviour
{
    [SerializeField] private GameObject LoadingPanel;
    [SerializeField] private bool DisableOnPurchase = false;
    [SerializeField] private bool ShowPriceFromStore = false;

    private CodelessIAPButton codelessIAPButton;

    private Dictionary<string, Action> productRewards;

    private Dictionary<string, Func<bool>> IAPProductPurchased = new()
    {
        { "removeads", () => PlayerPrefs.GetInt("RemoveAds", 0) == 1},
        { "megabundle", () => PlayerPrefs.GetInt("MegaBundle", 0) == 1},
        { "tubes", () => PlayerPrefs.GetInt("Tubes", 0) == 1},
        { "backgrounds", () => PlayerPrefs.GetInt("BackGrounds", 0) == 1},
    };

    private void OnEnable()
    {
        StartCoroutine(DisableObject());
    }

    private void Awake()
    {

        if (TryGetComponent(out codelessIAPButton))
        {
            if (codelessIAPButton.buttonType == CodelessButtonType.Purchase)
            {
                if (ShowPriceFromStore) GetComponentInChildren<TextMeshProUGUI>().text = "???";
                
                codelessIAPButton.onProductFetched.RemoveAllListeners();
                codelessIAPButton.onProductFetched.AddListener(OnProductDataFetched);

                codelessIAPButton.onProductFetchFailed.RemoveAllListeners();
                codelessIAPButton.onProductFetchFailed.AddListener(OnProductDataFetchedFailed);

                codelessIAPButton.onPurchaseFetched.RemoveAllListeners();
                codelessIAPButton.onPurchaseFetched.AddListener(OnPurchaseFetched);

                codelessIAPButton.onOrderPending.RemoveAllListeners();
                codelessIAPButton.onOrderPending.AddListener(OnOrderPending);

                codelessIAPButton.onOrderConfirmed.RemoveAllListeners();
                codelessIAPButton.onOrderConfirmed.AddListener(OnOrderConfirmed);

                codelessIAPButton.onPurchaseFailed.RemoveAllListeners();
                codelessIAPButton.onPurchaseFailed.AddListener(OnPurchaseFailed);

                codelessIAPButton.onOrderDeferred.RemoveAllListeners();
                codelessIAPButton.onOrderDeferred.AddListener(OnOrderDeferred);
            }
            else if (codelessIAPButton.buttonType == CodelessButtonType.Restore)
            {
                codelessIAPButton.onTransactionsRestored.RemoveAllListeners();
                codelessIAPButton.onTransactionsRestored.AddListener(OnTransactionRestored);
            }
        }
        else
        {
            Debug.LogError("No Codeless IAP m_Button found");
        }
    }

    private IEnumerator Start()
    {
        InitializeProductRewards();

        yield return new WaitUntil(() => codelessIAPButton != null && codelessIAPButton.button != null);

        if (codelessIAPButton.button)
            codelessIAPButton.button.onClick.AddListener(() =>
            {
                if (LoadingPanel) LoadingPanel.GetComponent<AnimationBase>().Show();
            });

        yield return null;
    }


    private IEnumerator DisableObject()
    {
        string lowName = gameObject.name.ToLower();
        if (DisableOnPurchase && IAPProductPurchased.TryGetValue(lowName, out var action))
        {
            bool check = (bool)action?.Invoke();
            Debug.Log($"[IAP] Disabling object based on purchase Product '{lowName}' purchased: {check}", gameObject);
            if (DisableOnPurchase && check)
            {
                yield return new WaitUntil(() => codelessIAPButton != null && codelessIAPButton.button != null);
                codelessIAPButton.button.interactable = false;
                yield return new WaitForEndOfFrame();
                gameObject.SetActive(false);
            }
           
        }

    }

    void OnProductDataFetched(Product product)
    {
        if (product == null) return;

        Debug.Log("Product data fetched: " + product);

        if (ShowPriceFromStore)
            GetComponentInChildren<TextMeshProUGUI>().text = product.metadata.localizedPriceString;
    }

    void OnProductDataFetchedFailed(ProductDefinition definition, string message)
    {
        if (ShowPriceFromStore)
            GetComponentInChildren<TextMeshProUGUI>().text = "???";

        codelessIAPButton.button.interactable = false;
    }

    void OnPurchaseFetched(Order order)
    {
        if (order == null)
        {
            Debug.LogWarning("[IAP] OnPurchaseFetched called with null Order.");
            return;
        }

        foreach (var item in order.CartOrdered.Items())
        {
            GrantProduct(item.Product);
        }
    }

    private void OnOrderPending(PendingOrder pendingOrder)
    {
        if (pendingOrder == null)
        {
            Debug.LogWarning("[IAP] OnOrderPending called with null PendingOrder.");
            return;
        }

        foreach (var item in pendingOrder.CartOrdered.Items())
        {
            Debug.Log($"[IAP] cart item: {item.Product}");
            GrantProduct(item.Product);
        }
        if (LoadingPanel) LoadingPanel.GetComponent<AnimationBase>().Hide();
    }

    private void OnOrderConfirmed(ConfirmedOrder confirmedOrder)
    {
        if (confirmedOrder == null)
        {
            Debug.LogWarning("[IAP] OnOrderConfirmed called with null ConfirmedOrder.");
            return;
        }

        Debug.Log("[IAP] Order confirmed.");
        StartCoroutine(DisableObject());
        // Don’t grant here (granting should be done in OnOrderPending).
        // Use for: analytics, UI success popup, hide spinner, etc.
    }

    private void OnOrderDeferred(DeferredOrder deferredOrder)
    {
        if (deferredOrder == null)
        {
            Debug.LogWarning("[IAP] OnOrderDeferred called with null DeferredOrder.");
            return;
        }

        Debug.Log("[IAP] Order deferred (e.g., Ask to Buy). Show waiting UI.");
    }


    private void OnPurchaseFailed(FailedOrder failedOrder)
    {
        if (failedOrder == null)
        {
            Debug.LogWarning("[IAP] OnPurchaseFailed called with null FailedOrder.");
            return;
        }

        // FailedOrder usually contains details; depending on exact IAP build,
        // you may have accessors for reason/message.
        Debug.LogWarning("[IAP] Purchase failed (FailedOrder).");
        if (LoadingPanel) LoadingPanel.GetComponent<AnimationBase>().Hide();
        // Example: show "Cancelled" / "Failed" UI and re-enable button
    }

    void OnTransactionRestored(bool success, string error)
    {
        Debug.Log("Transactions restored: " + success);
        if (LoadingPanel) LoadingPanel.GetComponent<AnimationBase>().Hide();
    }

    /// <summary>
    /// Grants the purchased or restored product entitlements.
    /// </summary>
    /// <param name="product">The product to grant</param>
    private void GrantProduct(Product product)
    {
        string id = product.definition.id.ToLower();

        if (PlayerPrefs.GetInt($"IAP_GRANTED_{id}", 0) == 1)
            return;
        
        if (productRewards.TryGetValue(id, out var rewardAction))
        {
            rewardAction.Invoke();
            PlayerPrefs.SetInt($"IAP_GRANTED_{id}", 1);
            Debug.Log($"[IAP] Granted product: {id}");
        }
        else
        {
            Debug.LogWarning($"[IAP] Unhandled product ID: {id}");
        }

      
    }


    private void InitializeProductRewards()
    {
        productRewards = new Dictionary<string, Action>
        {
            {
                IAPProductIDs.RemoveAds, () =>
                {
                    PlayerPrefs.SetInt("RemoveAds", 1);
                    ApplovinManager.Instance.DestroyBanner();
                    ApplovinManager.Instance.DestroyBigBanner();
                }
            },
            {
                IAPProductIDs.Coin1, () =>
                {
                    PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 1000);
                }
            },
            {
                IAPProductIDs.Coin2, () =>
                {
                    PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 3000);
                }
            },
            {
                IAPProductIDs.Tubes, () =>
                {
                    UnlockAllTubes();
                    PlayerPrefs.SetInt("Tubes", 1);
                }
            },
            {
                IAPProductIDs.Background, () =>
                {
                    UnlockAllBG();
                    PlayerPrefs.SetInt("BackGrounds", 1);
                }
            } ,
            {
                IAPProductIDs.MegaBundle, () =>
                {
                    MegaBundle();
                    PlayerPrefs.SetInt("MegaBundle", 1);
                }
            }

            // Add more products here as needed
        };
    }


    void UnlockAllTubes()
    {
        Store_Handler.instance.PurchaseBottles(5);
    }

    void UnlockAllBG()
    {
        Store_Handler.instance.PurchaseBGs(10);
    }

    void MegaBundle()
    {
        PlayerPrefs.SetInt("RemoveAds", 1);
        ApplovinManager.Instance.DestroyBanner();
        ApplovinManager.Instance.DestroyBigBanner();

        PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 1500);

        Store_Handler.instance.PurchaseBottles(3);
        Store_Handler.instance.PurchaseBGs(5);
    }
}

public static class IAPProductIDs
{
    public static readonly string Coin1 = "ios.hadeel.watersort.coins1";
    public static readonly string Coin2 = "ios.hadeel.watersort.coins2";
    public static readonly string RemoveAds = "ios.hadeel.watersort.removeads";
    public static readonly string Tubes = "ios.hadeel.watersort.tubes";
    public static readonly string Background = "ios.hadeel.watersort.bg";
    public static readonly string MegaBundle = "ios.hadeel.watersort.megabundle";
}