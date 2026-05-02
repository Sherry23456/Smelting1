using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class exam : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI tipText;
    public Button[] optionButtons;
    public Button nextBtn;
    public Button inter1;
    int currentQuestion = 0;
    int correctIndex;

    string[][] questions = new string[][]
    {
        new string[] { "我国古代最早大规模冶炼的金属是？", "铁", "铜", "银", "铝", "B" },
        new string[] { "西周颂壶属于哪类器物？", "玉器", "陶器", "青铜器", "铁器", "C" },
        new string[] { "古代青铜主要由哪两种金属合成？", "铜+锌", "铜+锡", "铁+碳", "铜+铝", "B" },
        new string[] { "下列农具与冶铁技术直接相关的是？", "陶罐", "石斧", "铁齿耙", "竹筐", "C" },
        new string[] { "记载古代农具技术的农书是？", "伤寒杂病论", "山海经", "齐民要术", "道德经", "C" },
    };

    void Start()
    {
        ShowQuestion(currentQuestion);
        inter1.onClick.AddListener(() => { SceneManager.LoadScene(2);   });
    }

    void ShowQuestion(int index)
    {
        nextBtn.gameObject.SetActive(false);
        tipText.text = "";

        questionText.text = questions[index][0];

        optionButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "A. " + questions[index][1];
        optionButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "B. " + questions[index][2];
        optionButtons[2].GetComponentInChildren<TextMeshProUGUI>().text = "C. " + questions[index][3];
        optionButtons[3].GetComponentInChildren<TextMeshProUGUI>().text = "D. " + questions[index][4];

        correctIndex = questions[index][5] switch
        {
            "A" => 0,
            "B" => 1,
            "C" => 2,
            "D" => 3,
            _ => 0
        };

        foreach (var btn in optionButtons)
        {
            btn.GetComponent<Image>().color =new  Color32(240,234,222,255);
            btn.interactable = true;
        }
    }

    public void OnSelectOption(int index)
    {
        if (index == correctIndex)
        {
            // 答对：变绿、显示下一题
            optionButtons[index].GetComponent<Image>().color = Color.green;
            tipText.text = "回答正确！";

            foreach (var btn in optionButtons)
                btn.interactable = false;

            nextBtn.gameObject.SetActive(true);
        }
        else
        {
            // 答错：只变红、只提示，不显示正确答案
            optionButtons[index].GetComponent<Image>().color = Color.red;
            tipText.text = "回答错误，请再试一次！";
        }
    }

    public void ToNextQuestion()
    {
        currentQuestion++;
        if (currentQuestion >= questions.Length)
        {
            questionText.text = "全部答题完成！";
            nextBtn.gameObject.SetActive(false);
            inter1.gameObject.SetActive(true);
            return;
        }
        ShowQuestion(currentQuestion);
    }
}