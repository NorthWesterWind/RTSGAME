using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Controller : MonoBehaviour
{
    private LineRenderer lineRenderer;

    //鼠标画线的四个点的位置
    private Vector3 LeftupPoint;
    private Vector3 RightupPoint;
    private Vector3 LeftdownPoint;
    private Vector3 RightdownPoint;

    //记录鼠标是否按下
    private bool IsDown = false;
  
    //记录选择到的士兵
    public List<SoldierObj> soldierList = new List<SoldierObj>();

    //开始位置的世界坐标
    private Vector3 beginWorldPoint;
    //结束位置的世界坐标
    private Vector3 endWorldPoint;

    private RaycastHit hitInfo;

    //上一次鼠标右键点击的位置
    private Vector3 frontpoint = Vector3.zero;

    //阵型中士兵的距离
    private int soldierGap = 3;
    void Start()
    {
        //得到画线组件
        lineRenderer = this.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        SelectSoldier();
        ControlSoldiersMove();
    }

    //选择士兵逻辑
    public void SelectSoldier()
    {

        //鼠标左键 按下抬起作为控制
        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < soldierList.Count; i++)
            {
                soldierList[i].SetEffect(false);
            }
            soldierList.Clear();

            LeftupPoint = Input.mousePosition;
            LeftupPoint.z = 5;
            IsDown = true;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000, 1 << LayerMask.NameToLayer("Ground")))
            {
                beginWorldPoint = hitInfo.point;
            }

            //每一次重新选择士兵时  将上次鼠标右键点击的位置重置
            frontpoint = Vector3.zero;

        }
        else if (Input.GetMouseButtonUp(0))
        {

            IsDown = false;
            lineRenderer.positionCount = 0;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000, 1 << LayerMask.NameToLayer("Ground")))
            {
                endWorldPoint = hitInfo.point;
                //设置碰撞器中心点
                Vector3 Center = new Vector3((beginWorldPoint.x + endWorldPoint.x) / 2, 2, (beginWorldPoint.z + endWorldPoint.z) / 2);
                //设置碰撞器长宽高的一半
                Vector3 halfValue = new Vector3(Mathf.Abs(endWorldPoint.x - beginWorldPoint.x) / 2, 1, Mathf.Abs(endWorldPoint.z - beginWorldPoint.z) / 2);
                //设置盒状碰撞器
                Collider[] soldierItem = Physics.OverlapBox(Center, halfValue);

                foreach (Collider item in soldierItem)
                {
                    SoldierObj sobj = item.GetComponent<SoldierObj>();
                    if (sobj != null)
                    {
                        sobj.SetEffect(true);
                        soldierList.Add(sobj);
                    }
                }

                //根据兵种类型进行排序
                soldierList.Sort((a, b) =>
                {
                    if (a.type < b.type)
                        return -1;
                    else if (a.type == b.type)
                        return 0;
                    else
                        return 1;
                });
            }
        }

        if (IsDown)
        {

            RightdownPoint = Input.mousePosition;
            RightdownPoint.z = 5;

            RightupPoint.x = RightdownPoint.x;
            RightupPoint.y = LeftupPoint.y;
            RightupPoint.z = 5;

            LeftdownPoint.x = LeftupPoint.x;
            LeftdownPoint.y = RightdownPoint.y;
            LeftdownPoint.z = 5;

            //设置线段画线的世界坐标系的点
            lineRenderer.positionCount = 4;
            lineRenderer.SetPosition(0, Camera.main.ScreenToWorldPoint(LeftupPoint));
            lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(RightupPoint));
            lineRenderer.SetPosition(2, Camera.main.ScreenToWorldPoint(RightdownPoint));
            lineRenderer.SetPosition(3, Camera.main.ScreenToWorldPoint(LeftdownPoint));
        }
    }

    //保持阵型移动逻辑
    public void ControlSoldiersMove()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (soldierList.Count == 0)
                return;

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition) , out hitInfo , 1000 , 1 << LayerMask.NameToLayer("Ground")))
            {
                //判断队伍转向幅度 ， 大于60度，将士兵重新排序
                if (Vector3.Angle(hitInfo.point - soldierList[0].transform.position , soldierList[0].transform.position) > 60)
                {
                    soldierList.Sort((a, b) =>
                    {
                        if (a.type < b.type)
                            return -1;
                        else if (a.type == b.type)
                        {
                            if(Vector3.Distance(a.transform.position , hitInfo.point) <= Vector3.Distance(b.transform.position , hitInfo.point))
                            {
                                return -1;
                            }
                            return 0;
                        }
                           
                        else
                            return 1;
                    });
                }

                //通过目标点  计算出真正的阵型目标点
                List<Vector3> TargetPos = GetTargetPoint(hitInfo.point);

                //计算阵型目标点
                for (int i = 0; i < soldierList.Count; i++)
                {
                    //使用SoldierObj 中封装好的自动寻路方法
                    soldierList[i].GetPoint(TargetPos[i]);
                }
              
            }
        }
       
    }
    /// <summary>
    /// 传入目标点，返回计算好的阵型点位
    /// </summary>
    /// <param name="pos"> 目标点</param>
    /// <returns></returns>
    private List<Vector3> GetTargetPoint(Vector3 pos)
    {
        Vector3 nowforwardDir = Vector3.zero;
        Vector3 rightDir;
        List<Vector3> targetPoints = new List<Vector3>();

        if(frontpoint != Vector3.zero)
        {
            //说明没有重新选择士兵
            nowforwardDir = (pos - frontpoint).normalized;
        }
        else
        {
            if (soldierList.Count > 0)
              nowforwardDir = (pos - soldierList[0].transform.position).normalized;
        }
        rightDir = Quaternion.Euler(0, 90, 0) * nowforwardDir.normalized;

        //进行分情况判断
        switch (soldierList.Count)
        {
            case 1:
                targetPoints.Add(pos );
                break;
            case 2:
                targetPoints.Add(pos - rightDir * soldierGap / 2);
                targetPoints.Add(pos + rightDir * soldierGap / 2);
                break;
            case 3:
                targetPoints.Add(pos);
                targetPoints.Add(pos - rightDir * soldierGap);
                targetPoints.Add(pos + rightDir * soldierGap);
                break;
            case 4:
                targetPoints.Add(pos + nowforwardDir * soldierGap / 2 - rightDir * soldierGap / 2);
                targetPoints.Add(pos + nowforwardDir * soldierGap / 2 + rightDir * soldierGap / 2);
                targetPoints.Add(pos - nowforwardDir * soldierGap / 2 - rightDir * soldierGap / 2);
                targetPoints.Add(pos - nowforwardDir * soldierGap / 2 + rightDir * soldierGap / 2);
                break;
            case 5:
                targetPoints.Add(pos + nowforwardDir * soldierGap / 2);
                targetPoints.Add(pos + nowforwardDir * soldierGap / 2 - rightDir * soldierGap);
                targetPoints.Add(pos + nowforwardDir * soldierGap / 2 + rightDir * soldierGap);
                targetPoints.Add(pos - nowforwardDir * soldierGap / 2 - rightDir * soldierGap);
                targetPoints.Add(pos - nowforwardDir * soldierGap / 2 + rightDir * soldierGap);
                break;
        }
        frontpoint = pos;
        return targetPoints;
    }
}
