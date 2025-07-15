using UnityEngine;
using UnityEngine.UI;

public class PhoneSystem : MonoBehaviour
{
    public GameObject phoneScreen; // Ë¹éÒ¨Íâ·ÃÈÑ¾·ì
    public GameObject[] appCanvases; // Canvas ¢Í§áÍ»·Ñé§ËÁ´

    private bool isPhoneOpen = false;

    public float slideSpeed = 10f;
    private Vector3 phoneOffScreenPos;
    private Vector3 phoneOnScreenPos;

    void Start()
    {
        // àÃÔèÁµé¹»Ô´â·ÃÈÑ¾·ìáÅÐ·Ø¡áÍ»
        phoneScreen.SetActive(false);
        CloseAllApps();
        phoneOnScreenPos = phoneScreen.transform.position;
        phoneOffScreenPos = phoneOnScreenPos - new Vector3(0, Screen.height, 0);
        phoneScreen.transform.position = phoneOffScreenPos;
    }

    void Update()
    {
        // µÃÇ¨ÊÍº¡ÒÃ¡´»ØèÁ M (ËÃ×Í»ØèÁÍ×è¹µÒÁ·Õè¡ÓË¹´)
        if (Input.GetKeyDown(KeyCode.M))
        {
            TogglePhone();
        }
        Vector3 targetPos = isPhoneOpen ? phoneOnScreenPos : phoneOffScreenPos;
        phoneScreen.transform.position = Vector3.Lerp(phoneScreen.transform.position, targetPos, slideSpeed * Time.deltaTime);
    }

    public void TogglePhone()
    {
        isPhoneOpen = !isPhoneOpen;
        phoneScreen.SetActive(isPhoneOpen);

        // ¶éÒ»Ô´â·ÃÈÑ¾·ì ãËé»Ô´·Ø¡áÍ»´éÇÂ
        if (!isPhoneOpen)
        {
            CloseAllApps();
        }
    }

    public void OpenApp(int appIndex)
    {
        // »Ô´·Ø¡áÍ»¡èÍ¹à»Ô´áÍ»ãËÁè
        CloseAllApps();

        // µÃÇ¨ÊÍºÇèÒ index ÍÂÙèã¹ªèÇ§·Õè¶Ù¡µéÍ§
        if (appIndex >= 0 && appIndex < appCanvases.Length)
        {
            appCanvases[appIndex].SetActive(true);
        }

        // »Ô´â·ÃÈÑ¾·ìàÁ×èÍà»Ô´áÍ» (optional)
        phoneScreen.SetActive(false);
        isPhoneOpen = false;
    }

    private void CloseAllApps()
    {
        foreach (GameObject appCanvas in appCanvases)
        {
            appCanvas.SetActive(false);
        }
    }
}