using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("-----------------------------[ Core ]")]
    public bool isOver;
    public int score;
    public int maxLevel;


    [Header("-----------------------------[ Object Pooling ]")]
    public GameObject FruitPrefab;
    public Transform FruitGroup;
    public List<Fruit> fruitPool;


    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Fruit LastFruit;

    [Header("-----------------------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor;

    [Header("-----------------------------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;

    [Header("-----------------------------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;

    void Awake()
    {
        Application.targetFrameRate = 60;    

        fruitPool = new List<Fruit>();
        effectPool = new List<ParticleSystem>();
        for(int index = 0; index < poolSize; index++)
        {
            MakeFruit();
        }
        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    public void GameStart()
    {
        // 오브젝트 활성화
        line.SetActive(true); 
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        // 사운드 플레이
        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        // 게임 시작 (과일 생성)
        Invoke("NextFruit", 1.5f);
    }

    Fruit MakeFruit()
    {
        // 이펙트 생성

        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 동글 생성
        GameObject instantFruitObj = Instantiate(FruitPrefab, FruitGroup);
        instantFruitObj.name = "Fruit " + fruitPool.Count;
        Fruit instantFruit = instantFruitObj.GetComponent<Fruit>();
        instantFruit.manager = this;
        instantFruit.effect = instantEffect;

        instantFruit.transform.localScale = new Vector3(100f, 100f, 1f);

        fruitPool.Add(instantFruit);

        return instantFruit;
    }

    Fruit GetFruit()
    {
        for(int index = 0; index < fruitPool.Count; index++)
        {
            poolCursor = (poolCursor + 1) % fruitPool.Count;
            if (!fruitPool[poolCursor].gameObject.activeSelf)
            {
                return fruitPool[poolCursor];
            }
        }
        return MakeFruit();    
    }
    void NextFruit()
    {
        if(isOver)
        {
            return;
        }

        LastFruit = GetFruit();
        LastFruit.level = Random.Range(0, maxLevel);
        LastFruit.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }


    IEnumerator WaitNext()
    {
        while (LastFruit != null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2.5f);

        NextFruit();
    }
    public void TouchDown()
    {
        if (LastFruit == null)
            return;
        LastFruit.Drag();
    }
    public void TouchUp()
    {
        if (LastFruit == null)
            return;
        LastFruit.Drop();
        LastFruit = null;
    }

    public void GameOver()
    {
        if (isOver)
        {
            return;
        }
        isOver = true;

        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine()
    {
        // 1. 장면 안에 활성화 되어있는 모든 과일 가져오기
        Fruit[] fruits = FindObjectsOfType<Fruit>();
        // 2. 지우기 전에 모든 과일의 물리효과 비활성화
        for (int index = 0; index < fruits.Length; index++)
        {
            fruits[index].rigid.simulated = false;
        }
        // 3. 1번의 목록을 하나씩 접근해서 지우기
        for (int index = 0; index < fruits.Length; index++)
        {
            fruits[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);
        // 최고 점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        WebClient11.Instance.Send(maxScore);
        PlayerPrefs.SetInt("MaxScore", maxScore);

        // 게임오버 UI 표시
        subScoreText.text = "점수 : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine("ResetCoroutine");
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }
    public void SfxPlay(Sfx type)
    {
        switch (type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    // 모바일에서 나가는 함수
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }    
    }
    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
