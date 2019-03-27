using UnityEngine;
using UnityEngine.UI;

public class View : MonoBehaviour
{
    public GameObject firstContainer;
    public GameObject clientFirstContainer;
    public GameObject clientSecondContainer;
    public GameObject serverFirstContainer;
    public GameObject serverSecondContainer;
    public UnityEngine.UI.InputField ipField;
    public UnityEngine.UI.Text yourIP;
    public static View instance = null;
    public Object sliderPrefab;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    private void disableAllContainers()
    {
        firstContainer.SetActive(false);
        clientFirstContainer.SetActive(false);
        clientSecondContainer.SetActive(false);
        serverFirstContainer.SetActive(false);
        serverSecondContainer.SetActive(false);
    }

    public void onServer()
    {
        Networking.instance.isServer = true;
        Networking.instance.Init();
        disableAllContainers();
        serverFirstContainer.SetActive(true);
        yourIP.text = Networking.GetLocalIPAddress();
    }

    public void onClient()
    {
        Networking.instance.Init();
        Networking.instance.isServer = false;
        disableAllContainers();
        clientFirstContainer.SetActive(true);
    }

    public void onClientConnect()
    {
        Networking.instance.connectTo(ipField.text, 1993);
    }

    public void onClientConnected()
    {
        disableAllContainers();
        clientSecondContainer.SetActive(true);

        Networking.instance.startSending();
    }

    public void onServerConnected()
    {
        disableAllContainers();
        serverSecondContainer.SetActive(true);
    }

    static float lastSliderHeight = 0;

    public Slider createSlider()
    {
        GameObject sliderGO = Instantiate(sliderPrefab, serverSecondContainer.transform) as GameObject;

        sliderGO.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        sliderGO.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        sliderGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, lastSliderHeight + 20);

        lastSliderHeight += 20;

        return sliderGO.GetComponent<UnityEngine.UI.Slider>();
    }
}
