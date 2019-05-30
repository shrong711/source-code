using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveItem : Photon.PunBehaviour
{
    [SerializeField]
    protected int _priority = 0;  //優先順序

    protected GameSceneManager _gameSceneManager = null;  //引用場景管理
    protected Collider _collider = null;  //碰撞器
    

    public int priority { get { return _priority; } }

    public virtual string GetText()  //顯示被十字準心瞄準的東西文字
    {
        return null;
    }

    public virtual void Activate (CharacterManager characterManager)  //互動  (開門 拿武器 )
    {

    }

    protected virtual void Start()
    {
        _gameSceneManager = GameSceneManager.instance;
        _collider = GetComponent<Collider>();

        if (_gameSceneManager != null && _collider != null)
        {
            _gameSceneManager.RegisterInteractiveItem(_collider.GetInstanceID(), this);  //註冊互動項目在遊戲場景
        }
    }

    /*protected virtual void OnTriggerEnter()
    {    
        _material.SetFloat("_OutlineWidth", 1.03f);
    }   */

   /* protected virtual void OnTriggerExit()
    {
        _material.SetFloat("_OutlineWidth", 1.00f); 
    }*/
}
