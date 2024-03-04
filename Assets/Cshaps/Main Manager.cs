using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEditor.Progress;

//枚举
public enum TYPE { air, wall, end , start , isSearched }
public enum ClickMode { None, StartMode, EndMode, WallsMode, }

//目前只能通过编辑器来选择
public enum Algorithms { BFS , DFS, A_star, Dijkstra , Bellman_Ford , Floyd_Warshall , Bidirectional_Search , IDA , Jump_Point_Search , Theta_Star}

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
        startLocation = new Vector2(-1f,-1f);
        endLocation = new Vector2(-1f, -1f);
        wallsLocation = new List<Vector2>();
        clickMode = ClickMode.None;
        isFindEnd = false;
        wintext.text = "";

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
                Cubes[x, y] = new cube(cube, new Vector2(x,y), TYPE.air);

            }
        }


    }

    //////////////////////////////////////////////////////////////////////////////////






    //////////////////////////////// 算法区域 ////////////////////////////////////////

    IEnumerator Algorithm_BFS()
    {
        //
        int live = NumberSize * NumberSize;

        HashSet<Vector2> waitToCount_previous = new HashSet<Vector2>();
        HashSet<Vector2> waitToCount_new = new HashSet<Vector2>();

        //Init
        waitToCount_previous.Add(startLocation);

        //不停遍历waitToCount直到找到End
        while (live > 0 || waitToCount_previous.Count != 0)
        {
            if (isFindEnd)
            {
                break;
            }

            foreach (Vector2 item in waitToCount_previous)
            {
                //如果找到终点，则退出
                if (isFindEnd)
                {
                    break;
                }

                //上下左右
                for (int d = 0; d < 4; d++)
                {
                    Vector2 direction = CheckWASD((int)item.x, (int)item.y, d);

                    if (direction == new Vector2(-2f, -2f))
                    {
                        isFindEnd = true;
                        wintext.text = "Find!";
                        wintext.color = Color.green;
                        break;
                    }

                    if (direction != new Vector2(-1, -1))
                    {
                        //添加到数据结构
                        waitToCount_new.Add(item + direction);

                        //更新Cubes
                        Cubes[(int)(item + direction).x, (int)(item + direction).y].type = TYPE.isSearched;
                        Cubes[(int)(item + direction).x, (int)(item + direction).y].cubeObject.GetComponent<Image>().color = Color.gray;
                    }
                }
            }

            //一轮结束
            waitToCount_previous.Clear();
            waitToCount_previous.UnionWith(waitToCount_new);
            waitToCount_new.Clear();
            live--;

            //delay
            yield return new WaitForSeconds(delay_time);
        }

        //是否找到
        if (!isFindEnd)
        {
            wintext.text = " Not Find..";
            wintext.color = Color.red;
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

    //检查对应方向是否是空气
    //是空气,返回方向
    //不是空气，返回(-1,-1)
    Vector2 CheckWASD(int _x, int _y,int _d)
    {
        Vector2 direct = Data.Direction[_d];
        Vector2 target = new Vector2(_x, _y) + direct;

        //如果没有出界
        if (!isOutOfBound((int)target.x, (int)target.y))
        {
            //判断是否是空气
            if (Cubes[(int)target.x, (int)target.y].type == TYPE.end)
            {
                return new Vector2(-2f,-2f);
            }
            else if (Cubes[(int)target.x, (int)target.y].type == TYPE.air)
            {
                return direct;
            }
            else
            {
                return new Vector2(-1, -1);
            }
        }
        else
        {
            return new Vector2(-1, -1);
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
            case Algorithms.BFS:StartCoroutine(Algorithm_BFS()); break;
            case Algorithms.DFS: break;
            case Algorithms.A_star: break;
            case Algorithms.Dijkstra: break;
            case Algorithms.Theta_Star: break;
            case Algorithms.Bellman_Ford: break;
            case Algorithms.Bidirectional_Search: break;
            case Algorithms.IDA: break;
            case Algorithms.Jump_Point_Search: break;
            case Algorithms.Floyd_Warshall: break;
            default:break;
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
                RectTransform rect = obj.GetComponent<RectTransform>();
                int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
                int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

                //筛选Cube
                if (obj.transform.parent == CubesArea.transform)
                {
                    //Debug.Log(obj.name);

                    obj.GetComponent<Image>().color = Color.yellow;

                    //赋值
                    startLocation = new Vector2(x,y);
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
                RectTransform rect = obj.GetComponent<RectTransform>();
                int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
                int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

                //筛选Cube
                if (obj.transform.parent == CubesArea.transform)
                {
                    //Debug.Log(obj.name);

                    obj.GetComponent<Image>().color = Color.green;

                    //赋值
                    endLocation = new Vector2(x,y);
                }

                hasExec_End = false;
            }

        }
    }

    private void AddToWalls_List()
    {
        //点击后
        GameObject obj = GetFirstPickGameObject(Input.mousePosition);
        RectTransform rect = obj.GetComponent<RectTransform>();
        int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
        int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

        //dic不包含 && 属于Cube Area && 不是Start,End
        if (!wallsLocation.Contains(new Vector2(x,y)) && obj.transform.parent == CubesArea.transform && new Vector2(x,y) != startLocation && new Vector2(x, y) != endLocation)
        {
            //Debug.Log($"position:{obj.transform.position},xy = {obj.name}");

            obj.GetComponent<Image>().color = Color.red;

            //添加到dictionary中
            wallsLocation.Add(new Vector2(x,y));
        }

        
    }

    //////////////////////////////////////////////////////////////////////////////////

}




/// <summary>
/// 数据结构
/// </summary>

public class cube
{
    public GameObject cubeObject;
    public Vector2 position;
    public TYPE type;

    public cube(GameObject obj, Vector2 pos, TYPE t)
    {
        cubeObject = obj;
        position = pos;
        type = t;
    }
}

