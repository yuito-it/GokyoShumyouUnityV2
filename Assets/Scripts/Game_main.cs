using System.Collections;
using System.Collections.Generic;
using UE = UnityEngine;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Threading;

public class Game_main : MonoBehaviour
{
    List<GameObject> WhiteStone = new List<GameObject>();
    List<GameObject> BlackStone = new List<GameObject>();
    char[] posX = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T' };

    List<GTP_Send_Param> procInput = new List<GTP_Send_Param>();
    List<GTP_Send_Param> outputWaitingTasks = new List<GTP_Send_Param>();

    [SerializeField]
    GameObject whitePrefab;
    [SerializeField]
    GameObject blackPrefab;

    SettingData settingData;

    // Start is called before the first frame update
    void Start()
    {
        GenStone();

        //TODO:getsettingメソッド作る。
        settingData = new SettingData();
        settingData.komi = 5;
        settingData.Black = true;
        settingData.GTP.Add(new GTP_Path_set()
        {
            url = "C:\\Users\\sigum\\Documents\\ray-windows\\ray64.exe"
        });
        //TODO:GTP対応
        //StartCoroutine(GTP_Process_Manager());

        //初期化GTPプロンプト
        //GTP_Set_Cmd();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            UE.Debug.Log("IF");
            GameObject gameObject = PosToObj(Camera.main.ScreenToWorldPoint(Input.mousePosition), settingData.Black);
            if (gameObject.activeSelf)
            {
                return;
            }
            else
            {
                gameObject.SetActive(true);
                UE.Debug.Log(gameObject.name);
            }
        }
    }

    /// <summary>
    /// GTPの通信管理コルーチン
    /// </summary>
    /// <returns></returns>
    IEnumerator GTP_Process_Manager()
    {
        UE.Debug.Log("Start:GTP_Ptocess_Manager");
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = settingData.GTP[0].url,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        };

        //意地でもusing使わないつもりの奴ww
        Process process = new Process();
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        process.EnableRaisingEvents = true;
        process.StartInfo = startInfo;

        //アウトプット受け取るイベントハンドラー
        process.OutputDataReceived += (sender, ev) =>
        {
            string output = ev.Data;
            UE.Debug.Log($"Process:output={output}");
            outputProcess(output);
        };

        //process終了した時の奴(そんなことないやろうけど)
        process.Exited += (sender, ev) =>
            {
                UE.Debug.Log($"Process:exited");
                cancellationToken.Cancel();
            };
        process.Start();
        try
        {
            while (true)
            {
                GTP_Send_Param param = procInput[0];
                UE.Debug.Log(param.cmd);
                process.StandardInput.WriteLine(param.cmd);
                outputWaitingTasks.Add(param);
            }
        }
        catch
        {

        }

        finally
        {
            process?.Dispose();
            cancellationToken?.Dispose();
        }
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));

        yield return "test";
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
                ins.SetActive(false);
                ins.name = "W - " + i.ToString() + " - " + b.ToString();
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
                GameObject ins = Instantiate(blackPrefab, pos, Quaternion.identity);
                ins.SetActive(false);
                ins.name = "B - " + i.ToString() + " - " + b.ToString();
                BlackStone.Add(ins);
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
        Raw.Remove(0, 0);
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

    /// <summary>
    /// アウトプットされたGTPのスクリプトを処理する。
    /// </summary>
    /// <param name="output">アウトプットされたデータを渡す。</param>
    async void outputProcess(string output)
    {
        //空出力を無視
        if (output.Length == 0 || output[0] == ' ') return;
        //残りのコマンドリストから今実行したものを削除
        outputWaitingTasks.RemoveAt(0);

        //GTPがErrorはいたらError出力
        if (output[0] == '?') UE.Debug.LogError("Error");
        if (output[0] == '=')
        {
            //=消す。
            output.Remove(0, 1);

            //残ってる出力ないか探す。
            while (output.Length > 0)
            {
                //posXに一文字目あれば、それは碁盤上の座標として処理。
                if (Array.Exists<char>(posX, item => item == output[0]))
                {
                    //y座標が何桁あるか
                    string param;
                    if (output[2] == ' ')
                    {
                        param = output.Substring(0, 1);
                        output.Remove(0, 1);
                    }
                    else
                    {
                        param = output.Substring(0, 2);
                        output.Remove(0, 2);
                    }
                    //戻り値のオブジェクトをアクティブ化
                    StringToObj(param, procInput[0].Black).SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// GTPのコミやその他もろもろの設定
    /// </summary>
    void GTP_Set_Cmd()
    {
        procInput.Add(new GTP_Send_Param()
        {
            cmd = "boardsize 19",
            putStoneCmd = false
        });
        procInput.Add(new GTP_Send_Param()
        {
            cmd = "place_free_handicap 5",
            putStoneCmd = true,
            Black = true
        });
    }
}



/// <summary>
/// setting.json読み込むときのクラス
/// </summary>
public class SettingData
{
    public List<GTP_Path_set> GTP = new List<GTP_Path_set>();
    public int komi;
    public bool Black;
}

/// <summary>
/// GTPのパスのやつ
/// </summary>
public class GTP_Path_set
{
    public string url;
}

/// <summary>
/// GTPに送るパラムのクラス
/// </summary>
public class GTP_Send_Param
{
    public string cmd;
    public bool putStoneCmd;
    public bool Black;
}