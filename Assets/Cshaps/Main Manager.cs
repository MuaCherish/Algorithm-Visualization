using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEditor.Progress;

//ö��
public enum TYPE { air, wall, end , start , isSearched }
public enum ClickMode { None, StartMode, EndMode, WallsMode, }

//Ŀǰֻ��ͨ���༭����ѡ��
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
    public int NumberSize;    //����
    private int previous_numberSize;

    //dynamic Cube
    private float CubeWidth;   //��С
    private float CubeOffset;  //���

    //Cubes Array
    private cube[,] Cubes;
    private ClickMode clickMode = ClickMode.None;

    //���ݽṹ
    private bool hasExec_Start = true;
    private bool hasExec_End = true;
    private Vector2 startLocation;
    private Vector2 endLocation;
    private List<Vector2> wallsLocation;

    //�㷨����
    [Header("algorithm")]
    private bool isFindEnd = false;
    public Algorithms algorithms = Algorithms.BFS;
    public float delay_time = 1f;


    //////////////////////////////// �������� ////////////////////////////////////////


    private void Start()
    {

        UpdateMap();
        previous_numberSize = NumberSize;

    }

    private void Update()
    {
        //�������size��������Ⱦ
        if (previous_numberSize != NumberSize)
        {
            UpdateMap();
            previous_numberSize = NumberSize;
        }

        //�����ģʽ
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






    //////////////////////////////// ��ͼ�ṹ ////////////////////////////////////////

    //���µ�ͼ
    void UpdateMap()
    {
        //����numbersize 
        NumberSize = (NumberSize <= 0) ? 1 : NumberSize;
        NumberSize = (NumberSize >= 34) ? 33 : NumberSize;

        //��ʼ������
        Cubes = new cube[NumberSize, NumberSize];
        startLocation = new Vector2(-1f,-1f);
        endLocation = new Vector2(-1f, -1f);
        wallsLocation = new List<Vector2>();
        clickMode = ClickMode.None;
        isFindEnd = false;
        wintext.text = "";

        //��ʼ��cube��С
        RectTransform CubesArea_rectTransform = CubesArea.GetComponent<RectTransform>();
        CubeOffset = 1f; // �����κ���ϣ���Ĺ̶�ֵ
        CubeWidth = (CubesArea_rectTransform.rect.width - (NumberSize + 1) * CubeOffset) / NumberSize;
        //CubeOffset = (NumberSize >= 10) ? 1 : (CubeWidth / (NumberSize * 5));


        // ɾ���������µ������Ӷ��� 
        foreach (Transform child in CubesArea.transform)
        {
            Destroy(child.gameObject); // ����ʹ�� DestroyImmediate(child.gameObject)��
        }


        //�趨Cubes
        for (int y = 0; y < NumberSize; y++)
        {
            for (int x = 0; x < NumberSize; x++)
            {
                //��������
                GameObject cube = new GameObject($"({x},{y})");
                RectTransform rectTransform = cube.AddComponent<RectTransform>();

                //�趨����
                Image image = cube.AddComponent<Image>();
                image.sprite = sprite;

                // ����ê��Ϊ���½Ƕ���
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);

                //����ģʽ
                cube.transform.SetParent(CubesArea.transform, false);

                //������ֵ
                Vector2 position = new Vector2(x * CubeWidth + CubeWidth / 2, y * CubeWidth + CubeWidth / 2);
                Vector2 offset = new Vector2((x + 1) * CubeOffset, (y + 1) * CubeOffset);

                rectTransform.anchoredPosition = position + offset;
                rectTransform.sizeDelta = new Vector2(CubeWidth, CubeWidth);

                //�趨Cubes
                Cubes[x, y] = new cube(cube, new Vector2(x,y), TYPE.air);

            }
        }


    }

    //////////////////////////////////////////////////////////////////////////////////






    //////////////////////////////// �㷨���� ////////////////////////////////////////

    IEnumerator Algorithm_BFS()
    {
        //
        int live = NumberSize * NumberSize;

        HashSet<Vector2> waitToCount_previous = new HashSet<Vector2>();
        HashSet<Vector2> waitToCount_new = new HashSet<Vector2>();

        //Init
        waitToCount_previous.Add(startLocation);

        //��ͣ����waitToCountֱ���ҵ�End
        while (live > 0 || waitToCount_previous.Count != 0)
        {
            if (isFindEnd)
            {
                break;
            }

            foreach (Vector2 item in waitToCount_previous)
            {
                //����ҵ��յ㣬���˳�
                if (isFindEnd)
                {
                    break;
                }

                //��������
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
                        //��ӵ����ݽṹ
                        waitToCount_new.Add(item + direction);

                        //����Cubes
                        Cubes[(int)(item + direction).x, (int)(item + direction).y].type = TYPE.isSearched;
                        Cubes[(int)(item + direction).x, (int)(item + direction).y].cubeObject.GetComponent<Image>().color = Color.gray;
                    }
                }
            }

            //һ�ֽ���
            waitToCount_previous.Clear();
            waitToCount_previous.UnionWith(waitToCount_new);
            waitToCount_new.Clear();
            live--;

            //delay
            yield return new WaitForSeconds(delay_time);
        }

        //�Ƿ��ҵ�
        if (!isFindEnd)
        {
            wintext.text = " Not Find..";
            wintext.color = Color.red;
        }

        yield return null;
    }

    //////////////////////////////////////////////////////////////////////////////////






    //////////////////////////////// ������ ////////////////////////////////////////

    bool isOutOfBound(int _x, int _y)
    {
        //x�Ƿ����
        if (_x < 0 || _x >= NumberSize)
        {
            return true;
        }
        //y�Ƿ����
        else if (_y < 0 || _y >= NumberSize)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //����Ӧ�����Ƿ��ǿ���
    //�ǿ���,���ط���
    //���ǿ���������(-1,-1)
    Vector2 CheckWASD(int _x, int _y,int _d)
    {
        Vector2 direct = Data.Direction[_d];
        Vector2 target = new Vector2(_x, _y) + direct;

        //���û�г���
        if (!isOutOfBound((int)target.x, (int)target.y))
        {
            //�ж��Ƿ��ǿ���
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

    //���߼����UI
    public GameObject GetFirstPickGameObject(Vector2 position)
    {
        EventSystem eventSystem = EventSystem.current;
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = position;
        //���߼��ui
        List<RaycastResult> uiRaycastResultCache = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, uiRaycastResultCache);
        if (uiRaycastResultCache.Count > 0)
            return uiRaycastResultCache[0].gameObject;
        return null;
    }

    //////////////////////////////////////////////////////////////////////////////////






    //////////////////////////////// ��ť�¼� ////////////////////////////////////////

    public void Click_StartGame()
    {
        //ΪCubes��ֵ
        //Start
        Cubes[(int)startLocation.x, (int)startLocation.y].type = TYPE.start;
        //End
        Cubes[(int)endLocation.x, (int)endLocation.y].type = TYPE.end;
        //Walls
        foreach (Vector2 item in wallsLocation)
        {
            Cubes[(int)item.x, (int)item.y].type = TYPE.wall;  
        }

        //ѡ��ִ���㷨
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
                //�����
                GameObject obj = GetFirstPickGameObject(Input.mousePosition);
                RectTransform rect = obj.GetComponent<RectTransform>();
                int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
                int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

                //ɸѡCube
                if (obj.transform.parent == CubesArea.transform)
                {
                    //Debug.Log(obj.name);

                    obj.GetComponent<Image>().color = Color.yellow;

                    //��ֵ
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
                //�����
                GameObject obj = GetFirstPickGameObject(Input.mousePosition);
                RectTransform rect = obj.GetComponent<RectTransform>();
                int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
                int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

                //ɸѡCube
                if (obj.transform.parent == CubesArea.transform)
                {
                    //Debug.Log(obj.name);

                    obj.GetComponent<Image>().color = Color.green;

                    //��ֵ
                    endLocation = new Vector2(x,y);
                }

                hasExec_End = false;
            }

        }
    }

    private void AddToWalls_List()
    {
        //�����
        GameObject obj = GetFirstPickGameObject(Input.mousePosition);
        RectTransform rect = obj.GetComponent<RectTransform>();
        int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
        int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

        //dic������ && ����Cube Area && ����Start,End
        if (!wallsLocation.Contains(new Vector2(x,y)) && obj.transform.parent == CubesArea.transform && new Vector2(x,y) != startLocation && new Vector2(x, y) != endLocation)
        {
            //Debug.Log($"position:{obj.transform.position},xy = {obj.name}");

            obj.GetComponent<Image>().color = Color.red;

            //��ӵ�dictionary��
            wallsLocation.Add(new Vector2(x,y));
        }

        
    }

    //////////////////////////////////////////////////////////////////////////////////

}




/// <summary>
/// ���ݽṹ
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

