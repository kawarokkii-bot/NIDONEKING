using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Audio;

public class NidoneGameManager : MonoBehaviour
{
    [Header("---Base---")]
    public GameObject titlePanel;
    public GameObject GoodPanel;
    public GameObject BadPanel;
    public Button startButton;
    public Text statusText;
    public Button sleepButton;
    public Button wakeUpButton;
    public Button NextButton;   

    [Header("---event---")]
    public GameObject event1;
    public GameObject event2;
    public GameObject event3;
    public GameObject event4;    
    public GameObject event5;
    public GameObject event6;
    public GameObject event7;
    public GameObject event8;
    public GameObject event9;
    public GameObject event10;


    [Header("---pose---")]
    public GameObject logArea;       
    public Text logText;
    public Button logToggleButton;   
    public ScrollRect logScrollRect;
    public GameObject pausePanel;
    public GameObject rulesPanel;
    public Button rulesButton;
    public Button restartButton;

    [Header("---演出---")]
    public Text rouletteText;        
    public Text NidoneText;    
    public Text ScoreText;
    public Text EventText;
    public Text TimeText;
    public Text alarmText;

    [Header("--- 音 ---")]
    public AudioSource bgmSource; 
    public AudioSource seSource;  
    public AudioClip normalBGM;
    public AudioClip rouletteBGM; 
    public AudioClip badBGM;
    public AudioClip rouletteSE;
    public AudioClip goodSE;
    public AudioClip badSE;
    public AudioClip eventSE;
    public AudioClip nidoneSE;
    public AudioClip getSE;
    public AudioClip buttonSE;

    [Header("--- UIパネル設定 ---")]

    public AudioMixer mainMixer; // インスペクターでセットする
    public AudioMixer subMixer; // インスペクターでセットする
    


    [Header("---ランキング---")]
    public TMP_Text rankingText;

    private int totalScore = 0;
    private int day = 1;
    private int timeMin = 0;
    private int todayScore = 0;
    private int spinCount = 0;

    // 状態フラグ
    private float nextTimeMult = 1.0f;
    private float nextScoreMult = 1.0f;
    private int hyperSleepTurns = 0;
    private bool isHungry = false;
    private bool hasAlarm = false;
    private bool hasForcedWakeup = false;
    private bool isNextPressed = false;

    private string[] events = {
        "熟睡タイム", "寝不足タイム", "強制起床", "時計のずれ", 
        "ハイパー熟睡タイム", "空腹", "アラーム", "タイムスリップ", 
        "最高の夢", "最悪の夢"
    };

    void Start()
    {
        //以下、初期設定
        rankingText.text = $"1位:\n {PlayerPrefs.GetInt("Score1", 0)}点\n" +  
                           $"2位:\n {PlayerPrefs.GetInt("Score2", 0)}点\n" +
                           $"3位:\n {PlayerPrefs.GetInt("Score3", 0)}点";


        sleepButton.onClick.AddListener(OnSleepButtonClicked);
        wakeUpButton.onClick.AddListener(OnWakeUpButtonClicked);
        logToggleButton.onClick.AddListener(ToggleLogArea);
        startButton.onClick.AddListener(OnStartButtonClicked);

        GoodPanel.gameObject.SetActive(false); 
        BadPanel.gameObject.SetActive(false);
        ScoreText.gameObject.SetActive(false);
        TimeText.gameObject.SetActive(false);
        EventText.gameObject.SetActive(false);
        NidoneText.gameObject.SetActive(false);
        rouletteText.gameObject.SetActive(false);
        alarmText.gameObject.SetActive(false);
        logArea.SetActive(false); 
        event1.gameObject.SetActive(false);
        event2.gameObject.SetActive(false);
        event3.gameObject.SetActive(false);
        event4.gameObject.SetActive(false);
        event5.gameObject.SetActive(false);
        event6.gameObject.SetActive(false);
        event7.gameObject.SetActive(false);
        event8.gameObject.SetActive(false);
        event9.gameObject.SetActive(false); 
        event10.gameObject.SetActive(false);

        titlePanel.SetActive(true);
        sleepButton.interactable = false;
        wakeUpButton.interactable = false;
        
        statusText.text = "【待機中】「次へ」を押してゲーム開始";

        if (NextButton != null)
        {
            NextButton.onClick.AddListener(() => isNextPressed = true);
            NextButton.gameObject.SetActive(false); 
        }
        StartGame();
    }

    void OnStartButtonClicked() //スタートボタン処理
    {
        PlaySE(buttonSE);
        if (titlePanel != null) titlePanel.SetActive(false);

        StartGame();
    }

    void ToggleLogArea()//ポーズ画面
    {
        PlaySE(buttonSE);
        if (logArea != null)
        {
            logArea.SetActive(!logArea.activeSelf);
        }
    }

     public void UpdateRanking(int currentScore)//ランキング
    {
        // 保存されているスコアを読み込む（なければ0）
        int high1 = PlayerPrefs.GetInt("Score1", 0);
        int high2 = PlayerPrefs.GetInt("Score2", 0);
        int high3 = PlayerPrefs.GetInt("Score3", 0);

        // 新記録なら入れ替える（単純な並び替え）
        if (currentScore > high1) {
            PlayerPrefs.SetInt("Score3", high2);
            PlayerPrefs.SetInt("Score2", high1);
            PlayerPrefs.SetInt("Score1", currentScore);
        } else if (currentScore > high2) {
            PlayerPrefs.SetInt("Score3", high2);
            PlayerPrefs.SetInt("Score2", currentScore);
        } else if (currentScore > high3) {
            PlayerPrefs.SetInt("Score3", currentScore);
        }
        PlayerPrefs.Save(); // データを保存確定

        // 表示を更新
        rankingText.text = $"1位:\n {PlayerPrefs.GetInt("Score1")}点\n" +
                        $"2位:\n {PlayerPrefs.GetInt("Score2")}点\n" +
                        $"3位:\n {PlayerPrefs.GetInt("Score3")}点";
    }


    void StartGame()
    {

        totalScore = 0;
        day = 1;
        StartNextDay();
    }

    void StartNextDay()
    {
        if (day > 2)PlaySE(buttonSE);
        ChangeBGM(normalBGM);
        if (day > 5)
        {
            logText.text += $"\n==================================\nゲーム終了！\n5日間の合計スコア: {totalScore}\n==================================";
            statusText.text = $"【最終結果】 合計スコア: {totalScore}";
            sleepButton.interactable = false;
            wakeUpButton.interactable = false;
            
            // ゲーム終了時はログを強制表示
            if (logArea != null) logArea.SetActive(true);
            UpdateRanking(totalScore);
            return;
        }

        timeMin = 0;
        todayScore = 0;
        spinCount = 0;
        
        nextTimeMult = 1.0f;
        nextScoreMult = 1.0f;
        hyperSleepTurns = 0;
        isHungry = false;
        hasAlarm = false;
        hasForcedWakeup = false;
        ChangeBGM(normalBGM);

        logText.text += $"▼ 【{day}日目】開始！ 現在時刻は 06:00 です。\n";
        UpdateUI();
        
        sleepButton.interactable = true;
        wakeUpButton.interactable = false;
    }

    void UpdateUI()
    {
        int totalMinutes = 6 * 60 + timeMin;
        int h = (totalMinutes / 60) % 24;
        int m = totalMinutes % 60;
        
        statusText.text = $"【{day}日目】 時刻: {h:D2}:{m:D2}\n本日のスコア: {todayScore} | 合計スコア: {totalScore}";
    }

   void Log(string msg)//ログの処理
    {
        logText.text += msg + "\n";

        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;
        yield return null; 
        
        if (logScrollRect != null)
        {
            logScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    void OnSleepButtonClicked()
    {
        PlaySE(buttonSE);
        StartCoroutine(SleepRoutine());
    }

    IEnumerator SleepRoutine()//もう一度寝た時の処理
    {
        sleepButton.interactable = false;
        wakeUpButton.interactable = false;
        
        spinCount++;
        Log($"\n--- {spinCount + 1}度寝 ---");//ｎ度寝の表示
        
        PlaySE(nidoneSE);
        NidoneText.color = Color.red;
        NidoneText.text = $"{spinCount + 1}度寝！！";
        NidoneText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.0f); 
        NidoneText.gameObject.SetActive(false);

        float baseScore = Mathf.Floor(Mathf.Pow(spinCount, 1.3f) * 100);//スコア計算
        float currentScoreGain = baseScore * nextScoreMult;
        if (isHungry) currentScoreGain *= 0.5f;
        if (hyperSleepTurns > 0) currentScoreGain *= 3f;
            
        int finalGain = (int)Mathf.Floor(currentScoreGain);//スコア表示
        todayScore += finalGain;
        PlaySE(getSE);
        Log($"スコアを {finalGain} 獲得！");
        UpdateUI();

        ScoreText.color = Color.yellow; 
        ScoreText.text = $"+ {finalGain} pt !";
        ScoreText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.0f); 
        ScoreText.gameObject.SetActive(false);
        
        nextScoreMult = 1.0f;

        ChangeBGM(rouletteBGM);//ルーレット表示
        rouletteText.gameObject.SetActive(true);
        int[] fakeChoices = { 120, 60, 30, 15, 10, 5, 3, 1, 0 };
        for (int i = 0; i < 15; i++)
        {
            int randomNum = fakeChoices[Random.Range(0, fakeChoices.Length)];
            rouletteText.text = $"時間抽選中...\n[ {randomNum} 分 ]";
            yield return new WaitForSeconds(0.05f);
        }
        rouletteText.gameObject.SetActive(false);

        int passed = GetRouletteResult();

        if (passed > 0)
        {
            float actualPassed = passed * nextTimeMult;
            if (isHungry && (passed == 120 || passed == 60 || passed == 30))
            {
                actualPassed = 15;
            }
            timeMin += (int)actualPassed;
            Log($"ルーレットの結果: {(int)actualPassed}分 経過。");
            nextTimeMult = 1.0f;

            // 時間経過をポップアップ
            PlaySE(rouletteSE);
            TimeText.color = Color.cyan; // 時間は水色に
            TimeText.text = $"+ {(int)actualPassed} 分 経過";
            TimeText.gameObject.SetActive(true);
            bgmSource.Stop();
            yield return new WaitForSeconds(1.2f);
            TimeText.gameObject.SetActive(false);
        }
        //以下、イベント処理
        else
        {

            Log("ルーレットの結果: 0分！ イベント発生...");
            TimeText.color = Color.cyan; // 時間は水色に
            TimeText.text = $"+ 0 分 経過";
            bgmSource.Stop();
            PlaySE(rouletteSE);
            TimeText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2.0f);
            TimeText.gameObject.SetActive(false);
            EventText.gameObject.SetActive(true);
            EventText.color = new Color(1f, 0.5f, 0f);
            PlaySE(rouletteSE);
            EventText.text = $"イベント発生！！";

            yield return new WaitForSeconds(2.0f);
            string ev = events[Random.Range(0, events.Length)];
            
            switch (ev)//イベントイラスト表示
            {
            case "熟睡タイム": event1.gameObject.SetActive(true); break;
            case "寝不足タイム": event2.gameObject.SetActive(true); break;
            case "強制起床": event3.gameObject.SetActive(true); break;
            case "時計のずれ": event4.gameObject.SetActive(true); break;
            case "ハイパー熟睡タイム": event5.gameObject.SetActive(true); break;
            case "空腹": event6.gameObject.SetActive(true); break;
            case "アラーム": event7.gameObject.SetActive(true); break;
            case "タイムスリップ": event8.gameObject.SetActive(true); break;
            case "最高の夢": event9.gameObject.SetActive(true); break;
            case "最悪の夢": event10.gameObject.SetActive(true); break;
            }
            PlaySE(eventSE);
            isNextPressed = false;

            if (NextButton != null) NextButton.gameObject.SetActive(true); 
            yield return new WaitUntil(() => isNextPressed);                      
            if (NextButton != null) NextButton.gameObject.SetActive(false);
            EventText.gameObject.SetActive(false);
            PlaySE(buttonSE);

            TriggerEvent(ev);//イベント処理
        }

        CheckTimeAndState();
    }

    int GetRouletteResult()//経過時間計算
    {
        int[] rChoices = { 120, 60, 30, 15, 10, 5, 3, 1, 0 };
        int[] rWeights = { 2, 4, 6, 10, 20, 20, 5, 5, 28 };
        int total = 0;
        foreach (int w in rWeights) total += w;
        
        int r = Random.Range(0, total);
        for (int i = 0; i < rWeights.Length; i++)
        {
            if (r < rWeights[i]) return rChoices[i];
            r -= rWeights[i];
        }
        return 0;
    }

    void TriggerEvent(string ev)
    {
        Log($"イベントカード【{ev}】を引いた！");

        switch (ev)
        {
            case "熟睡タイム": nextTimeMult = 2.0f; nextScoreMult = 2.0f; Log("次回経過時間とスコアが2倍になります！"); break;
            case "寝不足タイム": nextTimeMult = 0.5f; nextScoreMult = 0.5f; Log("次回経過時間とスコアが半分になります！"); break;
            case "強制起床":
                if (!hasForcedWakeup) { hasForcedWakeup = true; Log("母親が起こしに来た！今日の睡眠はここまで！");GoodPanel.gameObject.SetActive(true); EndDay(); }
                else Log("（すでに引いていたので何も起きなかった）"); break;
            case "時計のずれ":
                if (Random.value < 0.5f) { timeMin += 20; Log("部屋の時計が進んでいた！時間を20分進めます。"); PlaySE(badSE);}
                else { timeMin -= 20; Log("部屋の時計が遅れていた！時間を20分戻します。"); PlaySE(goodSE);} break;
            case "ハイパー熟睡タイム": hyperSleepTurns += 3; Log("ここから3回、「しっかり起きる」が選択できません（その間スコア3倍！）"); break;
            case "空腹":
                if (!isHungry) { isHungry = true; Log("おなかがすいてしまった...（今後の長時間睡眠が15分になり、スコア半減）"); }
                else Log("（すでにおなかがすいているので何も起きなかった）"); break;
            case "アラーム":
                if (!hasAlarm) { hasAlarm = true; Log("8:00にアラームをセットした！（寝過ごしてもスコア半分を獲得）"); }
                else Log("（すでにアラームをセットしているので何も起きなかった）"); break;
            case "タイムスリップ": todayScore = 0; timeMin = 0; Log("なんとここまでは夢だった！時刻が6:00に戻りスコアがリセットされます。"); break;
            case "最高の夢": todayScore *= 2; Log("獲得スコアが2倍になった！"); break;
            case "最悪の夢": todayScore = (int)Mathf.Floor(todayScore * 0.5f); Log("獲得スコアが半分になった..."); break;
        }
        event1.gameObject.SetActive(false);
        event2.gameObject.SetActive(false);
        event3.gameObject.SetActive(false);
        event4.gameObject.SetActive(false);
        event5.gameObject.SetActive(false);
        event6.gameObject.SetActive(false);
        event7.gameObject.SetActive(false);
        event8.gameObject.SetActive(false);
        event9.gameObject.SetActive(false); 
        event10.gameObject.SetActive(false);
    }

    void CheckTimeAndState()//八時過ぎてるかの確認
    {
        UpdateUI();

        if (hasForcedWakeup) return;

        if (timeMin >= 121)
        {
            Log("\nあ！！！ 8:00を過ぎてしまった！！！");
            if (hasAlarm)
            {
                todayScore = (int)Mathf.Floor(todayScore / 2.0f);
                Log($"しかしアラームのおかげで、スコアを半分（{todayScore}）獲得して1日を終えました。");
                alarmText.gameObject.SetActive(true);
                alarmText.text = $"+ アラームがついていたので{todayScore}点 獲得";
            }
            else
            {
                todayScore = 0;
                Log("寝坊したため、今日のスコアは【 0 】になります...");
            }
            PlaySE(badSE);
            ChangeBGM(badBGM);
            BadPanel.gameObject.SetActive(true);
            EndDay();
        }
        else
        {
            if (hyperSleepTurns > 0)
            {
                hyperSleepTurns--;
                wakeUpButton.interactable = false;
                sleepButton.interactable = true;
                Log($"【ハイパー熟睡タイム中（残り強制睡眠: {hyperSleepTurns}回）】");
            }
            else
            {
                wakeUpButton.interactable = true;
                sleepButton.interactable = true;
            }
            ChangeBGM(normalBGM);
        }
    }

    void OnWakeUpButtonClicked()
    {
        PlaySE(buttonSE);
        Log("\nえらい！しっかり起きた！");
        PlaySE(goodSE);
        GoodPanel.gameObject.SetActive(true);
        EndDay();
    }

    void EndDay()
    {
        Log($"\n■ 【{day}日目 終了】 本日のスコア: {todayScore}");
        totalScore += todayScore;
        UpdateUI();
        
        sleepButton.interactable = false;
        wakeUpButton.interactable = false;
        
        day++;
        
        
        isNextPressed = false;
        if (NextButton != null) NextButton.gameObject.SetActive(true);
        StartCoroutine(WaitForNextDay());                                     
    }

    // BGMを切り替える関数
    public void ChangeBGM(AudioClip nextBGM)
    {
        if (bgmSource.clip != nextBGM)
        {
            bgmSource.clip = nextBGM;
            bgmSource.Play();
        }
    }

    // 効果音を鳴らす関数
    public void PlaySE(AudioClip se)
    {
        // PlayOneShotを使うと、音が重なっても途切れずに鳴ります！
        seSource.PlayOneShot(se);
    }
    
    IEnumerator WaitForNextDay()
    {
        yield return new WaitUntil(() => isNextPressed);                       
        if (NextButton != null) NextButton.gameObject.SetActive(false); 
        StartNextDay(); 
        GoodPanel.gameObject.SetActive(false);
        BadPanel.gameObject.SetActive(false);
        alarmText.gameObject.SetActive(false);
    }

        public void RestartGame()
    {
        PlaySE(buttonSE);
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToTitle()
    {
        Debug.Log("タイトルへ戻る（未実装の場合はここをシーン名に変える）");
    }

    public void TogglePause(bool isPaused)
    {
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ShowRules(bool isShow)
    {
        rulesPanel.SetActive(isShow);
    }

        public void ToggleRules()
    {
        PlaySE(buttonSE);
        bool currentState = rulesPanel.activeSelf;
        rulesPanel.SetActive(!currentState);
    }

    public void SetBGMVolume(float volume)
    {
        // 0.0001〜1の値を、-80dB〜0dBに変換
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        mainMixer.SetFloat("BGMVol", db); 
        Debug.Log($"BGM volume changed: {db} dB");
    }

    public void SetSEVolume(float volume)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        mainMixer.SetFloat("SEVol", db);
        Debug.Log($"SE volume changed: {db} dB");
    }

    public void ResetRanking()
    {
        PlaySE(buttonSE);
        PlayerPrefs.DeleteKey("Score1");
        PlayerPrefs.DeleteKey("Score2");
        PlayerPrefs.DeleteKey("Score3");
        
        PlayerPrefs.Save();

        rankingText.text = "1位:\n 0点\n2位:\n 0点\n3位:\n 0点";
        
        Debug.Log("ランキングをリセットしました！");
    }
}