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
    public int NumberSize;    //数量
    private int previous_numberSize;

    //dynamic Cube
    private float CubeWidth;   //大小
    private float CubeOffset;  //间隔
     
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
    /// 地图结构
    /// </summary>

    void UpdateMap()
    {
        //设置numbersize 
        NumberSize = (NumberSize <= 0) ? 1: NumberSize;
        NumberSize = (NumberSize >= 34) ? 33 : NumberSize;

        //初始化数组
        Cubes = new GameObject[NumberSize, NumberSize];

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
        for (int y = 0; y < NumberSize; y ++)
        {
            for (int x = 0; x < NumberSize; x ++)
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

                //加入数组
                Cubes[x, y] = cube; ;

                //把第一个渲染成黄色
                if (x == 0 && y == 0)
                {
                    image.color = Color.yellow;
                }
            }
        }

        
    }
    


    /// <summary>
    /// 算法区域
    /// </summary>

    IEnumerator bfs_test()
    {
        yield return null;
    }


    /// <summary>
    /// 工具类
    /// </summary>

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

    void CheckWASD(int _x, int _y)
    {

    }
}
