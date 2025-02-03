using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class GooglePayManager : MonoBehaviour, IDetailedStoreListener
{
    public static GooglePayManager instance;
    public IStoreController storeController = null;
    public Action OnPurchaseSuccessful = null;
    public bool DEBUG;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        SetupStoreBuilder();
    }

    public void SetupStoreBuilder()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.Configure<IGooglePlayConfiguration>().SetServiceDisconnectAtInitializeListener(() =>
       {
           Debug.Log("Unable to connect to the Google Play Billing service. " +
               "User may not have a Google account on their device.");
       });

        for (int p = 0; p < Variables.GOOGLE_PAY.PRODUCTS.Length; p++)
        {
            GOOGLE_PAY_PRODUCT_DATA productData = Variables.GOOGLE_PAY.PRODUCTS[p];
            builder.AddProduct(productData.ID, productData.TYPE);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        throw new System.NotImplementedException();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Log("Purchase failed! " + error.ToString());
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Log("Purchase failed! " + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var product = purchaseEvent.purchasedProduct;
       Debug.Log($"Purchase completed: {product.definition.id}");

        Variables.GOOGLE_PAY.PRODUCTS.First((p) => p.ID == product.definition.id).OnPurchase.Invoke();

        OnPurchaseSuccessful?.Invoke();
        OnPurchaseSuccessful = null;

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        throw new System.NotImplementedException();
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        Debug.Log("Store initialized!");
    }
}
