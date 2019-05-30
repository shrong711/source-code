using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLayer
{
    public AudioClip Clip = null;
    public AudioCollection Collection = null;
    public int Bank = 0;
    public bool Looping = true;
    public float Time = 0.0f;
    public float Duration = 0.0f;
    public bool Muted = false;
}

public interface ILayeredAudioSource
{
    bool Play(AudioCollection pool, int bank, int layer, bool looping = true);
    void Stop(int layerIndex);
    void Mute(int layerIndex, bool mute);
    void Mute(bool mute);
}

public class LayeredAudioSource : ILayeredAudioSource
{
    AudioSource _audioSource = null;
    List<AudioLayer> _audioLayers = new List<AudioLayer>();
    int _activeLayer = -1;

    public AudioSource audioSource { get { return _audioSource; } }

    public LayeredAudioSource(AudioSource source, int layers)
    {
        if(source != null && layers > 0)
        {
            _audioSource = source;

            for (int i = 0; i < layers; i++)  //依照傳進來的圖層新增
            {  //新增音樂圖層
                AudioLayer newLayer = new AudioLayer();
                newLayer.Collection = null;
                newLayer.Duration = 0.0f;
                newLayer.Time = 0.0f;
                newLayer.Looping = false;
                newLayer.Bank = 0;
                newLayer.Muted = false;
                newLayer.Clip = null;
                _audioLayers.Add(newLayer);
            }
        }
    }

    public bool Play(AudioCollection collection, int bank, int layer, bool looping = true)
    {
        if(layer >= _audioLayers.Count)  //檢查是否在範圍內
        {
            return false;
        }

        AudioLayer audioLayer = _audioLayers[layer];  //找到要設置的Layer

        if(audioLayer.Collection == collection && audioLayer.Looping == looping && bank == audioLayer.Bank)  //如果都符合 就不用設置
        {
            return true;
        }

        audioLayer.Collection = collection;
        audioLayer.Bank = bank;
        audioLayer.Looping = looping;
        audioLayer.Time = 0.0f;
        audioLayer.Duration = 0.0f;
        audioLayer.Muted = false;
        audioLayer.Clip = null;

        return true;
    }

    public void Stop(int layerIndex)
    {
        if(layerIndex >= _audioLayers.Count)
        {
            return;
        }

        AudioLayer layer = _audioLayers[layerIndex];
        if(layer != null)
        {
            layer.Looping = false;
            layer.Time = layer.Duration;
        }
    }

    public void Mute(int layerIndex, bool mute)
    {
        if (layerIndex >= _audioLayers.Count)
        {
            return;
        }

        AudioLayer layer = _audioLayers[layerIndex];
        if (layer != null)
        {
            layer.Muted = mute;
        }
    }

    public void Mute(bool mute)
    {
        for(int i = 0; i < _audioLayers.Count; i++)
        {
            Mute(i, mute);
        }
    }

    public void Update()
    {
        int newActiveLayer = -1;
        bool refreshAudioSource = false;

        for(int i = _audioLayers.Count -1; i >= 0; i--)  //找到最高層級的layer (從List最後面開始)
        {
            AudioLayer layer = _audioLayers[i];

            if(layer.Collection == null)
            {
                continue;  //如果沒有繼續下一個循環
            }
            layer.Time += Time.deltaTime;

            if(layer.Time > layer.Duration)
            {
                if(layer.Looping || layer.Clip == null)
                {
                    AudioClip clip = layer.Collection[layer.Bank];  //從音樂池中分配新的音樂
                    if(clip == layer.Clip)  //如果是相同的音樂
                    {
                        layer.Time = layer.Time % layer.Clip.length;  //根據layer的時間和播放的音樂長度 計算位置達到無縫循環
                    }
                    else
                    {
                        layer.Time = 0.0f;
                    }
                    layer.Duration = clip.length;
                    layer.Clip = clip;

                    if(newActiveLayer < i)  //如果發現的新活動layer 小於索引
                    {
                        newActiveLayer = i;  //設置活動layer索引
                        refreshAudioSource = true;
                    }
                }
                else
                {  //如果不是循環 音樂播放玩 重製
                    layer.Clip = null;
                    layer.Collection = null;
                    layer.Duration = 0.0f;
                    layer.Bank = 0;
                    layer.Looping = false;
                }
            }
            else
            {
                if(newActiveLayer < i)
                {
                    newActiveLayer = i;
                }
            }
        }
        if(newActiveLayer != _activeLayer || refreshAudioSource)
        {
            if(newActiveLayer == -1)
            {
                _audioSource.Stop();
                _audioSource.clip = null;
            }
            else
            {
                AudioLayer layer = _audioLayers[newActiveLayer];
                _audioSource.clip = layer.Clip;
                _audioSource.volume = layer.Muted ? 0.0f : layer.Collection.volume;
                _audioSource.spatialBlend = layer.Collection.spatialBlend;
                _audioSource.time = layer.Time;
                _audioSource.loop = false;
                _audioSource.outputAudioMixerGroup = AudioManager.instance.GetAudioGroupFromTrackName(layer.Collection.audioGroup);
                _audioSource.Play();
            }
        }
        _activeLayer = newActiveLayer;

        if(_activeLayer != -1 && _audioSource)
        {
            AudioLayer audioLayer = _audioLayers[_activeLayer];
            if (audioLayer.Muted)
            {
                _audioSource.volume = 0.0f;
            }
            else
            {
                _audioSource.volume = audioLayer.Collection.volume;
            }
        }
    }
}
