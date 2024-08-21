using UnityEngine;
using UnityEngine.UI;
using TMPro; // Include this if using TextMeshPro

public class WristUIController : MonoBehaviour
{
    public GameObject prefab;
    public int offspringSize;
    private GameObject[] menuInstances;
    private AppConfig config;

    void Start()
    {
        config = ConfigManager.LoadConfig("Assets/revolve2/vr/db/config.json");
        offspringSize = config.OFFSPRING_SIZE;
        menuInstances = new GameObject[offspringSize];

        for (int i = 0; i < offspringSize; i++)
        {
            GameObject instance = Instantiate(prefab, this.transform, false);
            instance.name = "Menu " + (i + 1);
            menuInstances[i] = instance;

            // Set up button listeners
            Button nextButton = instance.transform.Find("NextButton").GetComponent<Button>();
            Button prevButton = instance.transform.Find("PreviousButton").GetComponent<Button>();

            TMP_Text header = instance.transform.Find("Header").GetComponent<TMP_Text>();
            //TMP_Text header = instance.GetComponentInChildren<TMP_Text>();
            header.text = $"Select parents of child {i+1}/{offspringSize}";

            if (i == 0)
            {
                prevButton.interactable = false; // Disable previous button on the first prefab
            }
            if (i == offspringSize - 1)
            {
                nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "Submit"; // Change Next to Submit on the last prefab
            }

            nextButton.onClick.AddListener(() => ShowNextMenu(instance));
            prevButton.onClick.AddListener(() => ShowPreviousMenu(instance));

            // Initially set all inactive except the first one
            if (i > 0) instance.SetActive(false);
        }
    }

    private void ShowNextMenu(GameObject currentMenu)
    {
        TMP_Dropdown selectParent1 = currentMenu.transform.Find("SelectParent1").GetComponent<TMP_Dropdown>();
        string parent1Value = selectParent1.options[selectParent1.value].text;
        TMP_Dropdown selectParent2 = currentMenu.transform.Find("SelectParent2").GetComponent<TMP_Dropdown>();
        string parent2Value = selectParent2.options[selectParent2.value].text;
        if(parent1Value == "Select parent..." || parent2Value == "Select parent...") {
            TMP_Text selectParentError = currentMenu.transform.Find("SelectParentError").GetComponent<TMP_Text>();
            selectParentError.gameObject.SetActive(true);
            return;
        }
        int currentIndex = System.Array.IndexOf(menuInstances, currentMenu);
        if (currentIndex < offspringSize - 1)
        {
            menuInstances[currentIndex + 1].SetActive(true);
            currentMenu.SetActive(false);
        }
        else
        {
            // Handle the 'Submit' action here
            Debug.Log("Submit all selections");
        }
    }

    private void ShowPreviousMenu(GameObject currentMenu)
    {
        int currentIndex = System.Array.IndexOf(menuInstances, currentMenu);
        if (currentIndex > 0)
        {
            menuInstances[currentIndex - 1].SetActive(true);
            currentMenu.SetActive(false);
        }
    }
}
