using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class TrackInfo  //儲存混音器群組訊息
{
    public string Name = string.Empty;  //軌道明子
    public AudioMixerGroup Group = null;  //混音器群組
    public IEnumerator TrackFader = null;  //淡入淡出的協成
}

public class AudioPoolItem  //音樂物件池
{
    public GameObject GameObject = null;
    public Transform Transform = null;
    public AudioSource AudioSource = null;
    public float Unimportance = float.MaxValue;  //不重要的聲音
    public bool Playing = false;
    public IEnumerator Coroutine = null;
    public ulong ID = 0;  //請求
}

public class AudioManager : MonoBehaviour
{
    
    private static AudioManager _instance = null;  
    public static AudioManager instance  //讓其他腳本可以引用
    {
        get
        {
            if(_instance == null)
            {
                _instance = (AudioManager)FindObjectOfType(typeof(AudioManager));
               // Debug.Log(PhotonNetwork.player.ID + _instance.ToString());
            }
            return _instance;
        }
    }

    [SerializeField]
    AudioMixer _mixer = null;  //混音器
    [SerializeField]
    int _maxSounds = 10;  //最大音樂數

    Dictionary<string, TrackInfo> _tracks = new Dictionary<string, TrackInfo>();  //儲存訊息
    List<AudioPoolItem> _pool = new List<AudioPoolItem>();  //音樂池
    Dictionary<ulong, AudioPoolItem> _activePool = new Dictionary<ulong, AudioPoolItem>();  //撥放中的音樂
    List<LayeredAudioSource> _layeredAudio = new List<LayeredAudioSource>();
    ulong _idGiver = 0;
    Transform _listenerPos = null;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);  //讀取新場景時保存此項
        _instance = (AudioManager)FindObjectOfType(typeof(AudioManager));
        if (!_mixer)  //如果沒有
        {
            return;  //返回
        }

        AudioMixerGroup[] groups = _mixer.FindMatchingGroups(string.Empty);  //在混音器裡面找到跟名子一樣的群組

        foreach(AudioMixerGroup group in groups)  //找到所有群組 並依據名子設置軌道
        {
            TrackInfo trackInfo = new TrackInfo();
            trackInfo.Name = group.name;  
            trackInfo.Group = group;
            trackInfo.TrackFader = null;
            _tracks[group.name] = trackInfo;
        }

        for(int i = 0; i < _maxSounds; i++)
        {
            GameObject go = new GameObject("Pool Item");  //創造物件池物件
            AudioSource audioSource = go.AddComponent<AudioSource>();  //增加音樂撥放器
            go.transform.parent = transform;  //把AudioManager設為父親位置
            
            AudioPoolItem poolItem = new AudioPoolItem();  //實體化物件池類別
            poolItem.GameObject = go;  //設置物件池
            poolItem.AudioSource = audioSource;
            poolItem.Transform = go.transform;
            poolItem.Playing = false;
            go.SetActive(false);
            _pool.Add(poolItem);
        }
        /*if (!photonView.isMine)
        {
            enabled = false;
        }*/
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _listenerPos = FindObjectOfType<AudioListener>().transform;  //轉換場景時 取得AudioListener
    }

    void Update()
    {
        foreach(LayeredAudioSource las in _layeredAudio)
        {
            if(las != null)
            {
                las.Update();
            }
        }
    }

    public float GetTrackVolume(string track)  //取得軌道音量
    {
        TrackInfo trackInfo;
        if (_tracks.TryGetValue(track, out trackInfo))
        {
            float volume;
            _mixer.GetFloat(track, out volume);
            return volume;
        }

        return float.MinValue;  //如果沒有回傳最小值
    }

    public AudioMixerGroup GetAudioGroupFromTrackName(string name)  //從群組裡面找到軌道明子
    {
        TrackInfo ti;
        if(_tracks.TryGetValue(name, out ti))
        {
            return ti.Group;
        }
        return null;
    }

    public void SetTrackVolume(string track, float volume, float fadeTime = 0.0f)  //設置音量
    {
        if (!_mixer)
        {
            return;
        }
        TrackInfo trackInfo;
        if(_tracks.TryGetValue(track, out trackInfo))
        {
            if(trackInfo.TrackFader != null)
            {
                StopCoroutine(trackInfo.TrackFader);  //停止協成
            }

            if(fadeTime == 0.0f)
            {
                _mixer.SetFloat(track, volume);  //設置軌道 音量
            }
            else
            {
                trackInfo.TrackFader = SetTrackVolumeInternal(track, volume, fadeTime);
                StartCoroutine(trackInfo.TrackFader);  //啟動淡入淡出協成
            }
        }
    }

    protected IEnumerator SetTrackVolumeInternal(string track, float volume, float fadeTime)  //在一段時間內 音量淡入淡出 
    {
        float startVolume = 0.0f;  //剛開始音量
        float timer = 0.0f;  //協成經過的時間
        _mixer.GetFloat(track, out startVolume);  //設置音軌 音量

        while(timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;  //不受 Time.timeScale 影響的 Time.deltaTime
            _mixer.SetFloat(track, Mathf.Lerp(startVolume, volume, timer / fadeTime));  
            yield return null;
        }

        _mixer.SetFloat(track, volume);
    }

    protected ulong ConfigurePoolObject(int poolIndex, string track, AudioClip clip, Vector3 position, float volume, float spatiaBlend, float unimportance)  //設置物件池的音樂 
    {
        if(poolIndex < 0 || poolIndex >= _pool.Count)  //檢查是否超出範圍
        {
            return 0;  //回傳ID 0 代表請求失敗
        }

        AudioPoolItem poolItem = _pool[poolIndex];  //從物件池裡拿出物件
        _idGiver++;  //ID +1
        AudioSource source = poolItem.AudioSource;  //取得AudioSource
        source.clip = clip;  //設置clip
        source.volume = volume;  //設置音量
        source.spatialBlend = spatiaBlend;  //3D或是2D音樂

        source.outputAudioMixerGroup = _tracks[track].Group;  //設置軌道.群組 給輸出
        source.transform.position = position;  //設置發出請求的音樂位置
        poolItem.Playing = true;  
        poolItem.Unimportance = unimportance;  //設為不重要的聲音
        poolItem.ID = _idGiver;  //設置ID給物件池
        poolItem.GameObject.SetActive(true);  //開啟物件
        source.Play();  //撥放聲音
        poolItem.Coroutine = StopSoundDelayed(_idGiver, source.clip.length);
        StartCoroutine(poolItem.Coroutine);

        _activePool[_idGiver] = poolItem;  //使用這個ID把聲音加進活動物件池字典中
        return _idGiver;  //回傳ID
    }

    protected IEnumerator StopSoundDelayed(ulong id, float duration)  //在幾秒後停止撥放聲音
    {
        yield return new WaitForSeconds(duration);

        AudioPoolItem activeSound;  //有效的聲音
        if(_activePool.TryGetValue(id, out activeSound))  //如果這個存在於活動物件池中
        {
            activeSound.AudioSource.Stop();  //停止撥放
            activeSound.AudioSource.clip = null;  //清除音樂
            activeSound.GameObject.SetActive(false);  //關閉物件
            _activePool.Remove(id);  //從活動的物件池中移除

            activeSound.Playing = false;
        }
    }

    public void StopOneShotSound(ulong id)  //關閉聲音
    {
        AudioPoolItem activeSound;  //有效的聲音

        if(_activePool.TryGetValue(id, out activeSound))  //如果存在於活動物件池中
        {
            StopCoroutine(activeSound.Coroutine);  //停止可能正在等待關閉這個聲音的協成

            activeSound.AudioSource.Stop();  //停止撥放
            activeSound.AudioSource.clip = null;  //清除音樂
            activeSound.GameObject.SetActive(false);  //關閉物件

            _activePool.Remove(id);  //從活動的物件池中移除
            activeSound.Playing = false;
        }
    }
    
    public ulong PlayOneShotSound(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, int priority = 128)  //判定聲音的優先順序 並尋找未使用的物件池音樂使用
    {
        if(!_tracks.ContainsKey(track) || clip == null || volume.Equals(0.0f))  //如果軌道不存在 clip為空值 聲音=0 
        {
            return 0;
        }

        float unimportance = (_listenerPos.position - position).sqrMagnitude / Mathf.Max(1, priority);  //計算重要性

        int leastImportantIndex = -1;  //不重要的索引整數
        float leastImportanceValue = float.MaxValue;  //不重要的值

        for(int i = 0; i < _pool.Count; i++)  //尋找可以用的audio source
        {
            AudioPoolItem poolItem = _pool[i];  //目前的物件池項目

            if (!poolItem.Playing)  //找到第一個沒有再撥放的物件池項目
            {
                return ConfigurePoolObject(i, track, clip, position, volume, spatialBlend, unimportance);  //設置物件池音樂
            }
            else if(poolItem.Unimportance > leastImportanceValue)  //如果有一個比我們目前撥放的音樂更重要
            {
                    leastImportanceValue = poolItem.Unimportance;  //紀錄目前為止最不重要的聲音 以替換新聲音的請求
                    leastImportantIndex = i;  //紀錄不重要的聲音池索引整數
            }                       
        }
        //如果所有聲音都被使用 目前正在撥放最不重要的聲音 沒有比聲音請求重要 替換掉聲音
        if (leastImportanceValue > unimportance)
        {
            return ConfigurePoolObject(leastImportantIndex, track, clip, position, volume, spatialBlend, unimportance);
        }

        return 0;  //沒有聲音可以撥放
    }

    public IEnumerator PlayOneShotSoundDelayed(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float duration, int priority = 128) //一段時間後撥放音樂
    {
        yield return new WaitForSeconds(duration);

        PlayOneShotSound(track, clip, position, volume, spatialBlend, priority);
    }
    
    public ILayeredAudioSource RegisterLayeredAudioSource(AudioSource source, int layers)
    {
        if(source != null && layers > 0)
        {
            for(int i = 0; i < _layeredAudio.Count; i++)
            {
                LayeredAudioSource item = _layeredAudio[i];
                if(item != null)
                {
                    if(item.audioSource == source)
                    {
                        return item;
                    }
                }
            }
            LayeredAudioSource newLayeredAudio = new LayeredAudioSource(source, layers);
            _layeredAudio.Add(newLayeredAudio);

            return newLayeredAudio;
        }
        return null;
    }

    public void UnregisterLayeredAudioSource(ILayeredAudioSource source)
    {
        _layeredAudio.Remove((LayeredAudioSource)source);
    }

    public void UnregisterLayeredAudioSource(AudioSource source)
    {
        for(int i = 0; i < _layeredAudio.Count; i++)
        {
            LayeredAudioSource item = _layeredAudio[i];
            if(item != null)
            {
                if(item.audioSource == source)
                {
                    _layeredAudio.Remove(item);
                    return;
                }
            }
        }
    }
}
