using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using JetBrains.Annotations;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; //플레이어 이동속도

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;

    float moveInput;
    public NPCManager npcManager;
    public GameStateManager gameStateManager;
    public DialogueUIController dialogueUIController;
    public IllustratorBox illustratorBox;
    public BackGroundBox backGroundBox;
    public InsertImageBox insertImageBox;
    public Button Choice1Button;
    public Button Choice2Button;

    public AudioSource audioSource; //AudioSource 컴포넌트 참조
    public AudioClip footstepClip;  //발걸음 소리 오디오 클립

    public AudioSource dialogueAudioSource; //대화 넘기기 사운드 AudioSource
    public AudioClip pageTurnClip; //책 넘기는 사운드 오디오 클립


    //private bool isDialogueActive = false;
    public float range = 2.0f;


    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.forward, range); // 빨강색 원으로 거리 확인
#endif
    }


    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        //대화 사운드를 위한 AudioSource 컴포넌트 추가
        dialogueAudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {

        moveInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == 1;
        }

        if (rigid.velocity.x == 0)
        {
            animator.SetBool("isWalking", false);
            StopFootstepSound(); //플레이어가 멈췄을 때 소리 멈추기
        }
        else
        {
            animator.SetBool("isWalking", true);
            PlayFootstepSound(); //플레이어가 이동할 때 발걸음 소리 재생
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            /*if (isDialogueActive)
            {
                HideDialogueUI();
            }
            else
            {
                InteractWithNPC();
            }*/
            InteractWithNPC();
            PlayPageTurnSound(); //대화가 진행될 때 책 넘기는 사운드 재생
        }



    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        Vector2 velocity = rigid.velocity;
        velocity.x = moveInput * moveSpeed;
        rigid.velocity = velocity;
    }

    void PlayFootstepSound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = footstepClip;
            audioSource.Play();
        }
    }

    void StopFootstepSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void PlayPageTurnSound()
    {
        if(pageTurnClip != null)
        {
            dialogueAudioSource.clip = pageTurnClip;
            dialogueAudioSource.Play();
        }
    }

    public void InteractWithNPC()
    {
        //오버랩된 NPC 오브젝트를 가져온다 (TAG 사용)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("NPC"))
            {
                //NPC 오브젝트에서 다이얼로그 데이터 가져오기

                Entity_Dialogue.Param npcParam =
                    npcManager.GetParamData(collider.GetComponent<NPCActor>().npcNumber, gameStateManager.gameState);
                if (npcParam != null)
                {
                    //대화실행
                    //Debug.Log($"Dialog : {npcParam.Dialog}");


                    dialogueUIController.npcName.text = npcParam.npcname;
                    dialogueUIController.dialogueText.text = npcParam.Dialog;
                    dialogueUIController.choice1Text.text = npcParam.choice1text;
                    dialogueUIController.choice2Text.text = npcParam.choice2text;

                    if (npcParam.background >= 0 && npcParam.background < backGroundBox.backGroundList.Length)
                    {
                        dialogueUIController.backGround.sprite = backGroundBox.backGroundList[npcParam.background];
                    }

                    // 왼쪽 캐릭터 일러스트 설정
                    if (npcParam.leftface > 0 && npcParam.leftface < illustratorBox.illustratorList.Length)
                    {
                        dialogueUIController.LeftFaceUi(true);
                        dialogueUIController.leftFace.sprite = illustratorBox.illustratorList[npcParam.leftface];
                    }
                    else
                    {
                        // 일러스트를 숨김
                        dialogueUIController.LeftFaceUi(false);
                    }

                    // 오른쪽 캐릭터 일러스트 설정
                    if (npcParam.rightface > 0 && npcParam.rightface < illustratorBox.illustratorList.Length)
                    {
                        dialogueUIController.RightFaceUi(true);
                        dialogueUIController.rightFace.sprite = illustratorBox.illustratorList[npcParam.rightface];
                    }
                    else
                    {
                        // 일러스트를 숨김
                        dialogueUIController.RightFaceUi(false);
                    }

                    // 중간 이미지 설정
                    if (npcParam.midimage > 0 && npcParam.midimage < insertImageBox.insertImageList.Length)
                    {
                        dialogueUIController.MiddleImageUi(true);
                        dialogueUIController.middleImage.sprite = insertImageBox.insertImageList[npcParam.midimage];
                    }
                    else
                    {
                        // 일러스트를 숨김
                        dialogueUIController.MiddleImageUi(false);
                    }


                    //작업수행
                    if (npcParam.changeState > 0)
                    {

                        gameStateManager.gameState = npcParam.changeState;
                    }

                    if (npcParam.choice1 > 0 || npcParam.choice2 > 0)
                    {
                        dialogueUIController.ChoiceUi(true);

                        // Choice1Button과 Choice2Button에 대한 클릭 이벤트 핸들러 설정
                        Choice1Button.onClick.AddListener(() =>
                        {
                            // 선택지 1에 대한 동작 구현
                            gameStateManager.gameState = npcParam.choice1;
                            InteractWithNPC();
                            // Choice1Button 클릭 이벤트 후에 ChoiceUi를 false로 변경
                            dialogueUIController.ChoiceUi(false);
                        });

                        Choice2Button.onClick.AddListener(() =>
                        {
                            // 선택지 2에 대한 동작 구현
                            gameStateManager.gameState = npcParam.choice2;
                            InteractWithNPC();
                            // Choice2Button 클릭 이벤트 후에 ChoiceUi를 false로 변경
                            dialogueUIController.ChoiceUi(false);
                        });
                    }
                    //대화UI 활성화
                    dialogueUIController.ActiveUI(true);
                    //isDialogueActive = true;

                    
                    

                }
                else
                {
                    Debug.LogWarning("해당하는 데이터가 없습니다. ");

                    //대화UI 비활성화
                    HideDialogueUI();
                }
            }
        }
    }

    private void HideDialogueUI()
    {
        dialogueUIController.ActiveUI(false);
        dialogueUIController.LeftFaceUi(false);
        dialogueUIController.RightFaceUi(false);        
        //isDialogueActive = false;
    }
}

