using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum SoldierType 
{
    Hero,//Ӣ��
    Warrior,//սʿ
    Archer,//����
    Magician,//ħ��ʦ
    Loong,//��
}

//ʿ����ű�
public class SoldierObj : MonoBehaviour
{
    //�������л�
    private Animator animator;
    //�ƶ�����
    private NavMeshAgent agent;
    //ѡ����Ч������
    private GameObject footEffect;

    //����ʿ��ö�����ͱ���
   public SoldierType type;
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponentInChildren<Animator>();
        agent = this.GetComponent<NavMeshAgent>();
        footEffect = this.transform.Find("FootEffect").gameObject;
        SetEffect(false);
    }

    // Update is called once per frame
    void Update()
    {
        //�������ٶ��Ƿ����0��Ϊ�����л�����
        animator.SetBool("IsMove" , agent.velocity.magnitude > 0);
    }

    //����Ѱ·Ŀ���
    public void GetPoint (Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    //��������ѡ��״̬ʱ������Ч����ʾ
    public void SetEffect(bool isOK)
    {
        footEffect.SetActive(isOK);
    }
}
