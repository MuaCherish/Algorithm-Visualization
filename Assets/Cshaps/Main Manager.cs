//using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

//using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
//using static Unity.Burst.Intrinsics.X86.Avx;
//using static UnityEditor.Progress;

//枚举
public enum TYPE { air, wall, end, start }
public enum ClickMode { None, StartMode, EndMode, WallsMode, }

//目前只能通过编辑器来选择
public enum Algorithms { BFS, DFS, A_star, Dijkstra, Bellman_Ford, Floyd_Warshall, Bidirectional_Search, IDA, Jump_Point_Search, Theta_Star }

//Manager
public class MainManager : MonoBehaviour
{
    [Header("GameObject")]
    public GameObject CubesArea;
    public Sprite sprite;
    public TextMeshProUGUI tmp;
    public TextMeshProUGUI wintext;

    [Header("World")]
    [Range(0, 33)]
    public int NumberSize;    //数量
    private int previous_numberSize;

    //dynamic Cube
    private float CubeWidth;   //大小
    private float CubeOffset;  //间隔

    //Cubes Array
    private cube[,] Cubes;
    private Dictionary<GameObject, Vector2> CubesPositions;
    private ClickMode clickMode = ClickMode.None;

    //数据结构
    private bool hasExec_Start = true;
    private bool hasExec_End = true;
    private Vector2 startLocation;
    private Vector2 endLocation;
    private List<Vector2> wallsLocation;

    //算法类型
    [Header("algorithm")]
    private bool isFindEnd = false;
    public Algorithms algorithms = Algorithms.BFS;
    public float delay_time = 1f;
    public List<Vector2> bestWay;



    //////////////////////////////// 生命周期 ////////////////////////////////////////


    private void Start()
    {

        UpdateMap();
        previous_numberSize = NumberSize;

    }

    private void Update()
    {
        //如果更改size则重新渲染
        if (previous_numberSize != NumberSize)
        {
            UpdateMap();
            previous_numberSize = NumberSize;
        }

        //鼠标点击模式
        if (clickMode != ClickMode.None)
        {
            //StartMode
            if (clickMode == ClickMode.StartMode)
            {
                if (Input.GetMouseButton(0))
                {
                    AddToStart_List();
                }
            }

            //EndMode
            if (clickMode == ClickMode.EndMode)
            {
                if (Input.GetMouseButton(0))
                {
                    AddToEnd_List();
                }
            }

            //WallsMode
            if (clickMode == ClickMode.WallsMode)
            {
                if (Input.GetMouseButton(0))
                {
                    AddToWalls_List();
                }
            }






        }

    }


    //////////////////////////////////////////////////////////////////////////////////






    //////////////////////////////// 地图结构 ////////////////////////////////////////

    //更新地图
    void UpdateMap()
    {
        //设置numbersize 
        NumberSize = (NumberSize <= 0) ? 1 : NumberSize;
        NumberSize = (NumberSize >= 34) ? 33 : NumberSize;

        //初始化数组
        Cubes = new cube[NumberSize, NumberSize];
        CubesPositions = new Dictionary<GameObject, Vector2>();
        startLocation = new Vector2(-1f, -1f);
        endLocation = new Vector2(-1f, -1f);
        wallsLocation = new List<Vector2>();
        clickMode = ClickMode.None;
        isFindEnd = false;
        wintext.text = "";
        bestWay = new List<Vector2>();

        //初始化cube大小
        RectTransform CubesArea_rectTransform = CubesArea.GetComponent<RectTransform>();
        CubeOffset = 1f; // 或者任何你希望的固定值
        CubeWidth = (CubesArea_rectTransform.rect.width - (NumberSize + 1) * CubeOffset) / NumberSize;
        //CubeOffset = (NumberSize >= 10) ? 1 : (CubeWidth / (NumberSize * 5));


        // 删除父对象下的所有子对象 
        foreach (Transform child in CubesArea.transform)
        {
            Destroy(child.gameObject); // 或者使用 DestroyImmediate(child.gameObject)；
        }


        //设定Cubes
        for (int y = 0; y < NumberSize; y++)
        {
            for (int x = 0; x < NumberSize; x++)
            {
                //创建本体
                GameObject cube = new GameObject($"({x},{y})");
                RectTransform rectTransform = cube.AddComponent<RectTransform>();

                //设定纹理
                Image image = cube.AddComponent<Image>();
                image.sprite = sprite;

                // 设置锚点为左下角对齐
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);

                //自身模式
                cube.transform.SetParent(CubesArea.transform, false);

                //设置数值
                Vector2 position = new Vector2(x * CubeWidth + CubeWidth / 2, y * CubeWidth + CubeWidth / 2);
                Vector2 offset = new Vector2((x + 1) * CubeOffset, (y + 1) * CubeOffset);

                rectTransform.anchoredPosition = position + offset;
                rectTransform.sizeDelta = new Vector2(CubeWidth, CubeWidth);

                //设定Cubes
                Cubes[x, y] = new cube(cube, new Vector2(x, y), TYPE.air);
                //Cubes[x, y].waybackhome.Add(new Vector2(x, y));

                //设置positions
                CubesPositions.Add(cube, new Vector2(x, y));

            }
        }


    }

    //////////////////////////////////////////////////////////////////////////////////






    //////////////////////////////// 算法区域 ////////////////////////////////////////

    //IEnumerator BFS()
    //{
    //    //每轮搜索表
    //    List<Vector2> round = new List<Vector2>();
    //    List<Vector2> round_next = new List<Vector2>();

    //    //初始化
    //    round.Add(startLocation);
    //    Cubes[0, 0].isSearched = true;

    //    //如果搜索表不为空一直循环
    //    while (round.Count != 0)
    //    {
    //        if (isFindEnd)
    //        {
    //            break;
    //        }

    //        //遍历round
    //        foreach (Vector2 item in round)
    //        {
    //            if (isFindEnd)
    //            {
    //                break;
    //            }


    //            //搜索上下左右
    //            for (int d = 0; d < 4; d++)
    //            {
    //                Vector2 direct = Data.Direction[d];
    //                Vector2 target = item + direct;

    //                //如果出界 && isSearch 就跳过
    //                if (isOutOfBound((int)target.x, (int)target.y) || Cubes[(int)target.x, (int)target.y].isSearched)
    //                {
    //                    continue;
    //                }

    //                cube thisCube = Cubes[(int)item.x, (int)item.y];
    //                cube targetCube = Cubes[(int)target.x, (int)target.y];

    //                //进行搜索，并添加waybackhome
    //                if (targetCube.waybackhome.Count == 1)
    //                {
    //                    Cubes[(int)target.x, (int)target.y].waybackhome.InsertRange(0, thisCube.waybackhome);
    //                }

    //                //如果碰到终点则isFinish
    //                if (targetCube.type == TYPE.end)
    //                {
    //                    isFindEnd = true;
    //                    break;
    //                }

    //                //碰到空气就添加到下一次寻路队列
    //                if (targetCube.type == TYPE.air)
    //                {
    //                    round_next.Add(target);

    //                    //添加颜色
    //                    targetCube.cubeObject.GetComponent<Image>().color = Color.gray;
    //                }


    //            }

    //        }


    //        //一轮结束
    //        //添加isSearch
    //        foreach (Vector2 item in round)
    //        {
    //            Cubes[(int)item.x, (int)item.y].isSearched = true;
    //        }

    //        //round清空
    //        round.Clear();
    //        round = round_next.ToList();
    //        round_next.Clear();

    //        yield return new WaitForSeconds(delay_time);
    //    }

    //    //是否找到
    //    if (!isFindEnd)
    //    {
    //        wintext.text = " Not Find..";
    //        wintext.color = Color.red;
    //        bestWay = new List<Vector2>();
    //    }
    //    else
    //    {
    //        wintext.color = Color.green;
    //        bestWay = Cubes[NumberSize - 1, NumberSize - 1].waybackhome.ToList();

    //        //长度没算上首尾
    //        wintext.text = $"Find!\nlength:{bestWay.Count - 2}";

    //        //走一遍到终点
    //        foreach (Vector2 item in bestWay)
    //        {
    //            if (Cubes[(int)item.x, (int)item.y].type != TYPE.end && Cubes[(int)item.x, (int)item.y].type != TYPE.start)
    //            {
    //                Cubes[(int)item.x, (int)item.y].cubeObject.GetComponent<Image>().color = Color.blue;
    //                yield return new WaitForSeconds(delay_time);
    //            }


    //        }

    //    }


    //    yield return null;
    //}

    IEnumerator BFS()
    {
        //变量
        Queue<Vector2> round = new Queue<Vector2>();
        Queue<Vector2> round_next = new Queue<Vector2>();

        //初始化
        round.Enqueue(startLocation);
        //Cubes[0, 0].isSearched = true;

        //遍历
        while (true)
        {
            //退出端口
            if (isFindEnd)
            {
                break;
            }

            //探索队列未走完不许停
            while (round.Count != 0)
            {
                //从队列取出一个
                Vector2 item = round.Dequeue();

                //搜索四个方向
                for (int i = 0; i < 4; i++)
                {
                    //变量
                    Vector2 direct = Data.Direction[i];
                    Vector2 target = item + direct;

                    //如果出界 && 以搜索 && Wall 则跳过
                    if (isOutOfBound((int)target.x, (int)target.y) || Cubes[(int)target.x, (int)target.y].isSearched || Cubes[(int)target.x, (int)target.y].type == TYPE.wall)
                    {
                        continue;
                    }

                    cube targetCube = Cubes[(int)target.x, (int)target.y];

                    //如果终点则结束
                    if (targetCube.type == TYPE.end)
                    {
                        isFindEnd = true;
                        round_next.Enqueue(target);

                        //设置终点参数
                        targetCube.Source = item;
                        targetCube.isSearched = true;

                        break;
                    }

                    //如果是空气则加入队列，并改指针
                    if (targetCube.type == TYPE.air)
                    {
                        //入队
                        round_next.Enqueue(target);

                        //改成已搜索
                        targetCube.isSearched = true;

                        //改指针
                        targetCube.Source = item;

                        //添加颜色
                        targetCube.cubeObject.GetComponent<Image>().color = Color.gray;
                    }


                }
            }




            //////一轮结束/////

            //跳出条件
            if (round_next.Count == 0)  //说明没路可走
            {
                break;
            }

            //队列交换
            round = new Queue<Vector2>(round_next);
            round_next.Clear();

            yield return new WaitForSeconds(delay_time);
        }

        //是否找到
        if (!isFindEnd)
        {
            wintext.text = " Not Find..";
            wintext.color = Color.red;
            bestWay = new List<Vector2>();
        }
        else
        {
            //重走一遍还原最短路径
            Vector2 now = Cubes[(int)endLocation.x, (int)endLocation.y].Source;
            while (now != startLocation)
            {
                //添加到bestWay
                bestWay.Add(now);

                //更新now
                now = Cubes[(int)now.x, (int)now.y].Source;
            }

            //print
            wintext.color = Color.green;
            wintext.text = $"Find!\nlength:{bestWay.Count}";

            //画出最短路径
            bestWay.Reverse();
            foreach (Vector2 item in bestWay)
            {
                Cubes[(int)item.x, (int)item.y].cubeObject.GetComponent<Image>().color = Color.blue;

                yield return new WaitForSeconds(delay_time);
            }
        }

        yield return null;
    }
    //////////////////////////////////////////////////////////////////////////////////






    //////////////////////////////// 工具类 ////////////////////////////////////////

    bool isOutOfBound(int _x, int _y)
    {
        //x是否出界
        if (_x < 0 || _x >= NumberSize)
        {
            return true;
        }
        //y是否出界
        else if (_y < 0 || _y >= NumberSize)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    //射线检测点击UI
    public GameObject GetFirstPickGameObject(Vector2 position)
    {
        EventSystem eventSystem = EventSystem.current;
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = position;
        //射线检测ui
        List<RaycastResult> uiRaycastResultCache = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, uiRaycastResultCache);
        if (uiRaycastResultCache.Count > 0)
            return uiRaycastResultCache[0].gameObject;
        return null;
    }

    //////////////////////////////////////////////////////////////////////////////////






    //////////////////////////////// 按钮事件 ////////////////////////////////////////

    public void Click_StartGame()
    {
        //为Cubes赋值
        //Start
        Cubes[(int)startLocation.x, (int)startLocation.y].type = TYPE.start;
        //End
        Cubes[(int)endLocation.x, (int)endLocation.y].type = TYPE.end;
        //Walls
        foreach (Vector2 item in wallsLocation)
        {
            Cubes[(int)item.x, (int)item.y].type = TYPE.wall;
        }

        //选择并执行算法
        switch (algorithms)
        {
            case Algorithms.BFS: StartCoroutine(BFS()); break;
            case Algorithms.DFS: break;
            case Algorithms.A_star: break;
            case Algorithms.Dijkstra: break;
            case Algorithms.Theta_Star: break;
            case Algorithms.Bellman_Ford: break;
            case Algorithms.Bidirectional_Search: break;
            case Algorithms.IDA: break;
            case Algorithms.Jump_Point_Search: break;
            case Algorithms.Floyd_Warshall: break;
            default: break;
        }
    }

    public void Click_SetStart()
    {
        clickMode = ClickMode.StartMode;
        tmp.text = "Choose Start";
        hasExec_Start = true;
    }

    public void Click_SetEnd()
    {
        clickMode = ClickMode.EndMode;
        tmp.text = "Choose End";
        hasExec_End = true;
    }

    public void Click_Walls()
    {
        clickMode = ClickMode.WallsMode;
        tmp.text = "Choose Walls";
    }

    public void Click_Clear()
    {
        UpdateMap();
    }

    public void Click_FinishSelect()
    {
        clickMode = ClickMode.None;
    }

    private void AddToStart_List()
    {
        if (startLocation == new Vector2(-1f, -1f))
        {
            if (hasExec_Start)
            {
                //点击后
                GameObject obj = GetFirstPickGameObject(Input.mousePosition);
                //RectTransform rect = obj.GetComponent<RectTransform>();
                //int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
                //int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

                CubesPositions.TryGetValue(obj, out Vector2 pos);

                //筛选Cube
                if (obj.transform.parent == CubesArea.transform)
                {
                    //Debug.Log(obj.name);

                    obj.GetComponent<Image>().color = Color.yellow;

                    //赋值
                    startLocation = new Vector2(pos.x, pos.y);
                }

                hasExec_Start = false;
            }



        }
    }

    private void AddToEnd_List()
    {
        if (endLocation == new Vector2(-1f, -1f))
        {
            if (hasExec_End)
            {
                //点击后
                GameObject obj = GetFirstPickGameObject(Input.mousePosition);
                //RectTransform rect = obj.GetComponent<RectTransform>();
                //int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
                //int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

                CubesPositions.TryGetValue(obj, out Vector2 pos);

                //筛选Cube
                if (obj.transform.parent == CubesArea.transform)
                {
                    //Debug.Log(obj.name);

                    obj.GetComponent<Image>().color = Color.green;

                    //赋值
                    endLocation = new Vector2(pos.x, pos.y);
                }

                hasExec_End = false;
            }

        }
    }

    private void AddToWalls_List()
    {
        //点击后
        GameObject obj = GetFirstPickGameObject(Input.mousePosition);
        //RectTransform rect = obj.GetComponent<RectTransform>();
        //int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
        //int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

        CubesPositions.TryGetValue(obj, out Vector2 pos);

        //dic不包含 && 属于Cube Area && 不是Start,End
        if (!wallsLocation.Contains(new Vector2(pos.x, pos.y)) && obj.transform.parent == CubesArea.transform && new Vector2(pos.x, pos.y) != startLocation && new Vector2(pos.x, pos.y) != endLocation)
        {
            //Debug.Log($"position:{obj.transform.position},xy = {obj.name}");

            obj.GetComponent<Image>().color = Color.red;

            //添加到dictionary中
            wallsLocation.Add(new Vector2(pos.x, pos.y));
        }


    }

    //////////////////////////////////////////////////////////////////////////////////

}




/// <summary>
/// 数据结构
/// </summary>

public class cube
{
    //基本参数
    public GameObject cubeObject;
    public Vector2 position;
    public TYPE type;

    //BFS
    public Vector2 Source;
    public bool isSearched = false;

    public cube(GameObject obj, Vector2 pos, TYPE t)
    {
        cubeObject = obj;
        position = pos;
        type = t;

    }

}

