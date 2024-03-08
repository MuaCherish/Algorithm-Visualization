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

//ö��
public enum TYPE { air, wall, end, start }
public enum ClickMode { None, StartMode, EndMode, WallsMode, }

//Ŀǰֻ��ͨ���༭����ѡ��
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
    public int NumberSize;    //����
    private int previous_numberSize;

    //dynamic Cube
    private float CubeWidth;   //��С
    private float CubeOffset;  //���

    //Cubes Array
    private cube[,] Cubes;
    private Dictionary<GameObject, Vector2> CubesPositions;
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
    public List<Vector2> bestWay;



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
        CubesPositions = new Dictionary<GameObject, Vector2>();
        startLocation = new Vector2(-1f, -1f);
        endLocation = new Vector2(-1f, -1f);
        wallsLocation = new List<Vector2>();
        clickMode = ClickMode.None;
        isFindEnd = false;
        wintext.text = "";
        bestWay = new List<Vector2>();

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
                Cubes[x, y] = new cube(cube, new Vector2(x, y), TYPE.air);
                //Cubes[x, y].waybackhome.Add(new Vector2(x, y));

                //����positions
                CubesPositions.Add(cube, new Vector2(x, y));

            }
        }


    }

    //////////////////////////////////////////////////////////////////////////////////






    //////////////////////////////// �㷨���� ////////////////////////////////////////

    //IEnumerator BFS()
    //{
    //    //ÿ��������
    //    List<Vector2> round = new List<Vector2>();
    //    List<Vector2> round_next = new List<Vector2>();

    //    //��ʼ��
    //    round.Add(startLocation);
    //    Cubes[0, 0].isSearched = true;

    //    //���������Ϊ��һֱѭ��
    //    while (round.Count != 0)
    //    {
    //        if (isFindEnd)
    //        {
    //            break;
    //        }

    //        //����round
    //        foreach (Vector2 item in round)
    //        {
    //            if (isFindEnd)
    //            {
    //                break;
    //            }


    //            //������������
    //            for (int d = 0; d < 4; d++)
    //            {
    //                Vector2 direct = Data.Direction[d];
    //                Vector2 target = item + direct;

    //                //������� && isSearch ������
    //                if (isOutOfBound((int)target.x, (int)target.y) || Cubes[(int)target.x, (int)target.y].isSearched)
    //                {
    //                    continue;
    //                }

    //                cube thisCube = Cubes[(int)item.x, (int)item.y];
    //                cube targetCube = Cubes[(int)target.x, (int)target.y];

    //                //���������������waybackhome
    //                if (targetCube.waybackhome.Count == 1)
    //                {
    //                    Cubes[(int)target.x, (int)target.y].waybackhome.InsertRange(0, thisCube.waybackhome);
    //                }

    //                //��������յ���isFinish
    //                if (targetCube.type == TYPE.end)
    //                {
    //                    isFindEnd = true;
    //                    break;
    //                }

    //                //������������ӵ���һ��Ѱ·����
    //                if (targetCube.type == TYPE.air)
    //                {
    //                    round_next.Add(target);

    //                    //�����ɫ
    //                    targetCube.cubeObject.GetComponent<Image>().color = Color.gray;
    //                }


    //            }

    //        }


    //        //һ�ֽ���
    //        //���isSearch
    //        foreach (Vector2 item in round)
    //        {
    //            Cubes[(int)item.x, (int)item.y].isSearched = true;
    //        }

    //        //round���
    //        round.Clear();
    //        round = round_next.ToList();
    //        round_next.Clear();

    //        yield return new WaitForSeconds(delay_time);
    //    }

    //    //�Ƿ��ҵ�
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

    //        //����û������β
    //        wintext.text = $"Find!\nlength:{bestWay.Count - 2}";

    //        //��һ�鵽�յ�
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
        //����
        Queue<Vector2> round = new Queue<Vector2>();
        Queue<Vector2> round_next = new Queue<Vector2>();

        //��ʼ��
        round.Enqueue(startLocation);
        //Cubes[0, 0].isSearched = true;

        //����
        while (true)
        {
            //�˳��˿�
            if (isFindEnd)
            {
                break;
            }

            //̽������δ���겻��ͣ
            while (round.Count != 0)
            {
                //�Ӷ���ȡ��һ��
                Vector2 item = round.Dequeue();

                //�����ĸ�����
                for (int i = 0; i < 4; i++)
                {
                    //����
                    Vector2 direct = Data.Direction[i];
                    Vector2 target = item + direct;

                    //������� && ������ && Wall ������
                    if (isOutOfBound((int)target.x, (int)target.y) || Cubes[(int)target.x, (int)target.y].isSearched || Cubes[(int)target.x, (int)target.y].type == TYPE.wall)
                    {
                        continue;
                    }

                    cube targetCube = Cubes[(int)target.x, (int)target.y];

                    //����յ������
                    if (targetCube.type == TYPE.end)
                    {
                        isFindEnd = true;
                        round_next.Enqueue(target);

                        //�����յ����
                        targetCube.Source = item;
                        targetCube.isSearched = true;

                        break;
                    }

                    //����ǿ����������У�����ָ��
                    if (targetCube.type == TYPE.air)
                    {
                        //���
                        round_next.Enqueue(target);

                        //�ĳ�������
                        targetCube.isSearched = true;

                        //��ָ��
                        targetCube.Source = item;

                        //�����ɫ
                        targetCube.cubeObject.GetComponent<Image>().color = Color.gray;
                    }


                }
            }




            //////һ�ֽ���/////

            //��������
            if (round_next.Count == 0)  //˵��û·����
            {
                break;
            }

            //���н���
            round = new Queue<Vector2>(round_next);
            round_next.Clear();

            yield return new WaitForSeconds(delay_time);
        }

        //�Ƿ��ҵ�
        if (!isFindEnd)
        {
            wintext.text = " Not Find..";
            wintext.color = Color.red;
            bestWay = new List<Vector2>();
        }
        else
        {
            //����һ�黹ԭ���·��
            Vector2 now = Cubes[(int)endLocation.x, (int)endLocation.y].Source;
            while (now != startLocation)
            {
                //��ӵ�bestWay
                bestWay.Add(now);

                //����now
                now = Cubes[(int)now.x, (int)now.y].Source;
            }

            //print
            wintext.color = Color.green;
            wintext.text = $"Find!\nlength:{bestWay.Count}";

            //�������·��
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
                //�����
                GameObject obj = GetFirstPickGameObject(Input.mousePosition);
                //RectTransform rect = obj.GetComponent<RectTransform>();
                //int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
                //int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

                CubesPositions.TryGetValue(obj, out Vector2 pos);

                //ɸѡCube
                if (obj.transform.parent == CubesArea.transform)
                {
                    //Debug.Log(obj.name);

                    obj.GetComponent<Image>().color = Color.yellow;

                    //��ֵ
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
                //�����
                GameObject obj = GetFirstPickGameObject(Input.mousePosition);
                //RectTransform rect = obj.GetComponent<RectTransform>();
                //int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
                //int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

                CubesPositions.TryGetValue(obj, out Vector2 pos);

                //ɸѡCube
                if (obj.transform.parent == CubesArea.transform)
                {
                    //Debug.Log(obj.name);

                    obj.GetComponent<Image>().color = Color.green;

                    //��ֵ
                    endLocation = new Vector2(pos.x, pos.y);
                }

                hasExec_End = false;
            }

        }
    }

    private void AddToWalls_List()
    {
        //�����
        GameObject obj = GetFirstPickGameObject(Input.mousePosition);
        //RectTransform rect = obj.GetComponent<RectTransform>();
        //int x = (int)(rect.anchoredPosition.x / rect.sizeDelta.x);
        //int y = (int)(rect.anchoredPosition.y / rect.sizeDelta.y);

        CubesPositions.TryGetValue(obj, out Vector2 pos);

        //dic������ && ����Cube Area && ����Start,End
        if (!wallsLocation.Contains(new Vector2(pos.x, pos.y)) && obj.transform.parent == CubesArea.transform && new Vector2(pos.x, pos.y) != startLocation && new Vector2(pos.x, pos.y) != endLocation)
        {
            //Debug.Log($"position:{obj.transform.position},xy = {obj.name}");

            obj.GetComponent<Image>().color = Color.red;

            //��ӵ�dictionary��
            wallsLocation.Add(new Vector2(pos.x, pos.y));
        }


    }

    //////////////////////////////////////////////////////////////////////////////////

}




/// <summary>
/// ���ݽṹ
/// </summary>

public class cube
{
    //��������
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

