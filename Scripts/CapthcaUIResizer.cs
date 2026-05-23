using UnityEngine;
using Gree.UnityWebView;

public class CaptchaUIResizer : MonoBehaviour
{
    [Header("--- UI BOUNDS ---")]
    [SerializeField] private RectTransform captchaPlaceholder; // Canvas üzerindeki şeffaf Image alanı

    [Header("--- WEB SETTINGS ---")]
    [SerializeField] private string captchaUrl = "https://vanthlore.repandus.com/captcha-page.html";

    // 🚀 BÜYÜTME MOTORU: Buradan kutuyu istediğin kadar devasa yapabilirsin canım!
    // 1.0 = Normal boyutu | 1.5 = %150 büyüklük | 2.0 = Tam iki katı büyüklük
    [Header("--- ZOOM SETTINGS ---")]
    [Range(1f, 3f)] [SerializeField] private float contentZoom = 1.4f; 

    private WebViewObject _webViewObject;
    private string _retrievedCaptchaToken = "";

    public string RetrievedCaptchaToken => _retrievedCaptchaToken;

    private void OnEnable()
    {
        ResetCaptcha();
        StartCaptchaFlow();
    }

    public void ResetCaptcha()
    {
        _retrievedCaptchaToken = "";
        CleanUpWebView();
    }

    public void StartCaptchaFlow()
    {
        Canvas canvas = captchaPlaceholder.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera; 
        }

        _webViewObject = gameObject.AddComponent<WebViewObject>();

        _webViewObject.Init(
            cb: OnWebViewMessageReceived,
            transparent: true, 
            err: (msg) => Debug.LogError($"hCaptcha Web Hata: {msg}"),
            started: (msg) => Debug.Log($"hCaptcha Yükleme Başladı: {msg}"),
            ld: (msg) => {
                Debug.Log($"hCaptcha Sayfa Yüklendi: {msg}");
                
                // 🔥 SİHİRLİ JAVASCRIPT ENJEKSİYONU
                // Sayfa başarıyla yüklendiği an HTML gövdesine sızıp CSS ölçeklemesini tetikliyoruz canım!
                // İnternet standartlarına uygun (InvariantCulture) nokta biçimlendirmesi kullanıyoruz.
                string zoomString = contentZoom.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                string jsCommand = $"document.body.style.transform = 'scale({zoomString})'; document.body.style.transformOrigin = 'center';";
                
                _webViewObject.EvaluateJS(jsCommand);
            }
        );

        CalculateAndSetMargins(uiCamera);

        _webViewObject.LoadURL(captchaUrl);
        _webViewObject.SetVisibility(true);

        Debug.Log("<color=cyan>hCaptcha:</color> WebView sorunsuzca ayağa kaldırıldı canım.");
    }

    private void CalculateAndSetMargins(Camera uiCamera)
    {
        if (_webViewObject == null || captchaPlaceholder == null) return;

        Vector3[] corners = new Vector3[4];
        captchaPlaceholder.GetWorldCorners(corners);

        Vector2 screenSolAlt = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
        Vector2 screenSagUst = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);

        int leftMargin = (int)screenSolAlt.x;
        int topMargin = (int)(Screen.height - screenSagUst.y);
        int rightMargin = (int)(Screen.width - screenSagUst.x);
        int bottomMargin = (int)screenSolAlt.y;

        _webViewObject.SetMargins(leftMargin, topMargin, rightMargin, bottomMargin);
    }

    private void OnWebViewMessageReceived(string message)
    {
        if (!string.IsNullOrEmpty(message) && message.StartsWith("captcha-success:"))
        {
            _retrievedCaptchaToken = message.Replace("captcha-success:", "");
            Debug.Log("<color=green>hCaptcha Başarılı!</color> Jeton havada kapıldı, form gönderilmeye hazır canım.");
        }
    }

    public void CleanUpWebView()
    {
        if (_webViewObject != null)
        {
            _webViewObject.SetVisibility(false);
            Destroy(_webViewObject);
            _webViewObject = null;
        }
    }

    private void Update()
    {
        if (_webViewObject != null && Time.frameCount % 30 == 0)
        {
            Canvas canvas = captchaPlaceholder.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            CalculateAndSetMargins(uiCamera);
        }
    }

    private void OnDisable()
    {
        CleanUpWebView(); 
    }

    private void OnDestroy()
    {
        CleanUpWebView();
    }
}