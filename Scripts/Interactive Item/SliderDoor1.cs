using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DoorState1
{
    Open,
    Animating,
    Closed
}

public class SliderDoor1 : InteractiveItem
{
    public float SliderDistance = 4.0f;
    public float Duration = 1.5f;
    public AnimationCurve JumpCurve = new AnimationCurve();
    private Transform _transform = null;
    private Vector3 _openPos = Vector3.zero;
    private Vector3 _closedPos = Vector3.zero;
    private DoorState1 _doorState = DoorState1.Closed;
    public AudioSource audios;
    public BoxCollider _door;
    public Material _material;
    public IEnumerator cour;

    public GameObject eventZombies;


    protected override void Start ()
    {
        base.Start();
        _material.SetFloat("_OutlineWidth", 1.00f);
        _transform = transform;
        _closedPos = _transform.position;
        _openPos = _closedPos + (_transform.right * SliderDistance);	
	}

    IEnumerator AnimateDoor(DoorState1 newState)
    {
        _doorState = DoorState1.Animating;
        float time = 0.0f;
        Vector3 startPos = (newState == DoorState1.Open) ? _closedPos : _openPos;
        Vector3 endPos = (newState == DoorState1.Open) ? _openPos : _closedPos;
        while(time <= Duration)
        {
            float t = time / Duration;
            _transform.position = Vector3.Lerp(startPos, endPos, JumpCurve.Evaluate(t));
            time += Time.deltaTime;
            yield return null;
        }
        _transform.position = endPos;
        _doorState = newState;
    }

    public override string GetText()
    {
        return "按E開門";
    }

    public override void Activate(CharacterManager characterManager)
    {
        if(_doorState != DoorState1.Animating)
        {
            if (_doorState == DoorState1.Open)
            {
                cour = AnimateDoor(DoorState1.Closed);
                photonView.RPC("ani", PhotonTargets.All);
            }
            else
            {
                cour = AnimateDoor(DoorState1.Open);
                photonView.RPC("ani", PhotonTargets.All);
            }
            //  StartCoroutine(AnimateDoor((_doorState == DoorState1.Open) ? DoorState1.Closed : DoorState1.Open));
            audios.Play();
        }                                    
    }

    [PunRPC]
    void ani()
    {
        if (cour == null)
            return;
        StartCoroutine(cour);
    }

    public void ZombiesEvent()
    {
            StartCoroutine(AnimateDoor((_doorState == DoorState1.Open) ? DoorState1.Closed : DoorState1.Open));
            audios.Play();
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.03f);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.00f);
    }
}
