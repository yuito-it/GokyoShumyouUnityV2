using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Game_main : MonoBehaviour
{
    List<GameObject> WhiteStone = new List<GameObject>();
    List<GameObject> BlackStone = new List<GameObject>();
    char[] posX = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T' };


    [SerializeField]
    GameObject whitePrefab;
    [SerializeField]
    GameObject blackPrefab;

    // Start is called before the first frame update
    void Start()
    {
        GenStone();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 石生み出してBlackStoneとWhiteStoneというListに入れておく。
    /// 初期化に使う。
    /// </summary>
    void GenStone()
    {
        for (int i = 1; i < 20; i++)
        {
            float x = i * 0.48f - 10;
            for (int b = 1; b < 20; b++)
            {
                float y = b * 0.48f - 10;
                Vector3 pos = new Vector3(x, y, 0);
                GameObject ins = Instantiate(whitePrefab, pos, Quaternion.identity) as GameObject;
                WhiteStone.Add(ins);
            }
        }
        for (int i = 1; i < 20; i++)
        {
            float x = i * 0.48f - 10;
            for (int b = 1; b < 20; b++)
            {
                float y = b * 0.48f - 10;
                Vector3 pos = new Vector3(x, y, 0);
                BlackStone.Add(Instantiate(blackPrefab, pos, Quaternion.identity));
            }
        }
    }

    /// <summary>
    /// Vector2からobjを取る。
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="Black">黒ならtrue.</param>
    /// <returns>その位置の黒、もしくは白のGameObject</returns>
    GameObject PosToObj(Vector2 pos, bool Black)
    {
        float x = pos.x / 0.48f + 10;
        float y = pos.y / 0.48f + 10;
        if (Black)
        {
            return BlackStone[NoToListNo(x, y)];
        }
        else
        {
            return WhiteStone[NoToListNo(x, y)];
        }
    }

    /// <summary>
    /// GTPが返した文字列からObjを取る。
    /// </summary>
    /// <param name="Raw">生のデータを入れる。</param>
    /// <param name="Black">黒ならtrue.</param>
    /// <returns>その位置の黒、もしくは白のGameObject</returns>
    GameObject StringToObj(string Raw, bool Black)
    {
        GameObject res = new GameObject();
        int posx;
        posx = Array.IndexOf(posX, Raw[0]);
        Raw = Raw.Remove(0, 0);
        int posy;
        posy = int.Parse(Raw);
        if (Black)
        {
            res = BlackStone[posy * 19 + posx];
        }
        else
        {
            res = WhiteStone[posy * 19 + posx];
        }
        return res;
    }

    /// <summary>
    /// A-Sのx番号とyの何列目かを与えて、リスト内でのオブジェクトの番号を取得。
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>リスト内でのオブジェクトの番号。</returns>
    int NoToListNo(float x, float y)
    {
        return (int)x + 19 * (int)y;
    }
}
