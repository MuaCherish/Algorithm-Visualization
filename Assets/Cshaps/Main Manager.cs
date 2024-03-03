using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [Header("GameObject")]
    public GameObject CubesArea;
    public Sprite sprite;

    [Header("World")]
    public int NumberSize;    //����
    private int previous_numberSize;

    //dynamic Cube
    private float CubeWidth;   //��С
    private float CubeOffset;  //���
     
    //Cubes Array
    private GameObject[,] Cubes;

    public float Screen_Width;
    public float Screen_Height;


    private void Start()
    {
        UpdateMap();
        previous_numberSize = NumberSize;
    }

    private void Update()
    {
        Screen_Width = Screen.width; Screen_Height = Screen.height;

        if (previous_numberSize != NumberSize)
        {
            UpdateMap();
            previous_numberSize = NumberSize;
        }
    }

    /// <summary>
    /// ��ͼ�ṹ
    /// </summary>

    void UpdateMap()
    {
        //����numbersize 
        NumberSize = (NumberSize <= 0) ? 1: NumberSize;
        NumberSize = (NumberSize >= 34) ? 33 : NumberSize;

        //��ʼ������
        Cubes = new GameObject[NumberSize, NumberSize];

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
        for (int y = 0; y < NumberSize; y ++)
        {
            for (int x = 0; x < NumberSize; x ++)
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

                //��������
                Cubes[x, y] = cube; ;

                //�ѵ�һ����Ⱦ�ɻ�ɫ
                if (x == 0 && y == 0)
                {
                    image.color = Color.yellow;
                }
            }
        }

        
    }
    


    /// <summary>
    /// �㷨����
    /// </summary>

    IEnumerator bfs_test()
    {
        yield return null;
    }


    /// <summary>
    /// ������
    /// </summary>

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

    void CheckWASD(int _x, int _y)
    {

    }
}
