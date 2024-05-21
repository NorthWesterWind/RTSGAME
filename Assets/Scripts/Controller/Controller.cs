using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Controller : MonoBehaviour
{
    private LineRenderer lineRenderer;

    //��껭�ߵ��ĸ����λ��
    private Vector3 LeftupPoint;
    private Vector3 RightupPoint;
    private Vector3 LeftdownPoint;
    private Vector3 RightdownPoint;

    //��¼����Ƿ���
    private bool IsDown = false;
  
    //��¼ѡ�񵽵�ʿ��
    public List<SoldierObj> soldierList = new List<SoldierObj>();

    //��ʼλ�õ���������
    private Vector3 beginWorldPoint;
    //����λ�õ���������
    private Vector3 endWorldPoint;

    private RaycastHit hitInfo;

    //��һ������Ҽ������λ��
    private Vector3 frontpoint = Vector3.zero;

    //������ʿ���ľ���
    private int soldierGap = 3;
    void Start()
    {
        //�õ��������
        lineRenderer = this.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        SelectSoldier();
        ControlSoldiersMove();
    }

    //ѡ��ʿ���߼�
    public void SelectSoldier()
    {

        //������ ����̧����Ϊ����
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

            //ÿһ������ѡ��ʿ��ʱ  ���ϴ�����Ҽ������λ������
            frontpoint = Vector3.zero;

        }
        else if (Input.GetMouseButtonUp(0))
        {

            IsDown = false;
            lineRenderer.positionCount = 0;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000, 1 << LayerMask.NameToLayer("Ground")))
            {
                endWorldPoint = hitInfo.point;
                //������ײ�����ĵ�
                Vector3 Center = new Vector3((beginWorldPoint.x + endWorldPoint.x) / 2, 2, (beginWorldPoint.z + endWorldPoint.z) / 2);
                //������ײ������ߵ�һ��
                Vector3 halfValue = new Vector3(Mathf.Abs(endWorldPoint.x - beginWorldPoint.x) / 2, 1, Mathf.Abs(endWorldPoint.z - beginWorldPoint.z) / 2);
                //���ú�״��ײ��
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

                //���ݱ������ͽ�������
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

            //�����߶λ��ߵ���������ϵ�ĵ�
            lineRenderer.positionCount = 4;
            lineRenderer.SetPosition(0, Camera.main.ScreenToWorldPoint(LeftupPoint));
            lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(RightupPoint));
            lineRenderer.SetPosition(2, Camera.main.ScreenToWorldPoint(RightdownPoint));
            lineRenderer.SetPosition(3, Camera.main.ScreenToWorldPoint(LeftdownPoint));
        }
    }

    //���������ƶ��߼�
    public void ControlSoldiersMove()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (soldierList.Count == 0)
                return;

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition) , out hitInfo , 1000 , 1 << LayerMask.NameToLayer("Ground")))
            {
                //�ж϶���ת����� �� ����60�ȣ���ʿ����������
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

                //ͨ��Ŀ���  ���������������Ŀ���
                List<Vector3> TargetPos = GetTargetPoint(hitInfo.point);

                //��������Ŀ���
                for (int i = 0; i < soldierList.Count; i++)
                {
                    //ʹ��SoldierObj �з�װ�õ��Զ�Ѱ·����
                    soldierList[i].GetPoint(TargetPos[i]);
                }
              
            }
        }
       
    }
    /// <summary>
    /// ����Ŀ��㣬���ؼ���õ����͵�λ
    /// </summary>
    /// <param name="pos"> Ŀ���</param>
    /// <returns></returns>
    private List<Vector3> GetTargetPoint(Vector3 pos)
    {
        Vector3 nowforwardDir = Vector3.zero;
        Vector3 rightDir;
        List<Vector3> targetPoints = new List<Vector3>();

        if(frontpoint != Vector3.zero)
        {
            //˵��û������ѡ��ʿ��
            nowforwardDir = (pos - frontpoint).normalized;
        }
        else
        {
            if (soldierList.Count > 0)
              nowforwardDir = (pos - soldierList[0].transform.position).normalized;
        }
        rightDir = Quaternion.Euler(0, 90, 0) * nowforwardDir.normalized;

        //���з�����ж�
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
