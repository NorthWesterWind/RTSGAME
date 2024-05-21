using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum SoldierType 
{
    Hero,//英雄
    Warrior,//战士
    Archer,//猎人
    Magician,//魔法师
    Loong,//龙
}

//士兵类脚本
public class SoldierObj : MonoBehaviour
{
    //动画的切换
    private Animator animator;
    //移动方法
    private NavMeshAgent agent;
    //选择特效的显隐
    private GameObject footEffect;

    //声明士兵枚举类型变量
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
        //将人物速度是否大于0作为动画切换条件
        animator.SetBool("IsMove" , agent.velocity.magnitude > 0);
    }

    //设置寻路目标点
    public void GetPoint (Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    //设置人物选中状态时脚下特效的显示
    public void SetEffect(bool isOK)
    {
        footEffect.SetActive(isOK);
    }
}
