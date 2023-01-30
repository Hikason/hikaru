using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageCtl : MonoBehaviour
{
    //[SerializeField, Header("StageTextをいれてね！")]
    //private TextAsset StageText;
    [SerializeField, Header("StageTextを入れてね")]
    private TextAsset[] StageText;

    public static int stageIndex = 0;  

    [SerializeField, Header("ClearのUIをいれてね")]
    private GameObject clearUI;

    [SerializeField, Header("NEXTのUIをいれてね")]
    private GameObject nextUI;

    [SerializeField, Header("TITLEのUIをいれてね")]
    private GameObject titleUI;

    [SerializeField, Header("EnterのUIをいれてね")]
    private GameObject EnterUI;

    public int rows;    //行
    public int columns; //列
    public enum StageTile
    {
        NONE, 　　　　　//[0]何もない
        GROUND,         //[1]地面
        GOAL,   　　　　//[2]ゴール地点
        PLAYER, 　　　　//[3]プレイヤー
        BLOCK,  　　　　//[4]ブロック

        PLAYER_ON_GOAL, //[5]プレイヤーがゴール地点にのってる
        BLOCK_ON_GOAL,  //[6]ブロックがゴール地点にのってる

        /*WALL,       */    //[7]壁
    }
    private StageTile[,] stageMap; //２次元配列（タイル情報の管理）

    private enum Action
    {
        UP,    //[0]上に移動
        RIGHT, //[1]右に移動
        DOWN,  //[2]下に移動
        LEFT,  //[3]左に移動
    }

    public float tileSize;     //タイルサイズ
    public Sprite groundSprite; //地面のスプライト
    public Sprite wallSprite;   //壁のスプライト
    public Sprite goalSplite;　 //ゴールのスプライト
    public Sprite playerSprite; //プレイヤーのスプライト
    public Sprite blockSprite;  //ブロックのスプライト

    private GameObject player;  //プレイヤーのゲームオブジェクト
    private Vector2 midd;       //中心
    public int blockCount;      //ブロックのカウント数

    public bool isClear;

    private Dictionary<GameObject, Vector2Int> gameObjectPosTable = new Dictionary<GameObject, Vector2Int>();

    private void Awake()
    {
        LoadStageText();//テキストのタイル情報の読み込み
        CreateStage();  //ステージ生成のメソッド呼び

        EnterUI.SetActive(true);
        clearUI.SetActive(false);
        nextUI.SetActive(false);
        titleUI.SetActive(false);
    }

    private void Update()
    {
        if (isClear) return;

        //上矢印を押したら
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //上に移動
            Debug.Log("おした");
            ActionMove(Action.UP);
        }
        //右矢印を押したら
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //右に移動
            Debug.Log("おした");
            ActionMove(Action.RIGHT);
        }
        //下矢印を押したら
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //右に移動
            Debug.Log("おした");
            ActionMove(Action.DOWN);
        }
        //左矢印を押したら
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //左に移動
            Debug.Log("おした");
            ActionMove(Action.LEFT);
        }

        if (Input.GetKey(KeyCode.Return))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }

    public void LoadStageText()
    {
        //Textの中身を一行ごとに分割　　　　　　　　　　　　　　　　余分な空白部分を詰めてくれるオプション
        string[] lines = StageText[stageIndex].text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        //タイルの列数を計算
        //一行取得し、カンマ区切りで分けた倍の長さを取得
        string[] nums = lines[0].Split(new[] { ',' });

        //タイルの列と行を保持
        rows = lines.Length;    //行
        columns = nums.Length;　//列

        //タイルの列数とint型の２次元配列で保持
        stageMap = new StageTile[columns, rows];

        //for文でstafe.textのデータを読み込んで「stageMap」に変換する
        for (int y = 0; y < rows; y++)
        {
            //一文字ずつ取得
            string st = lines[y];
            nums = st.Split(new[] { ',' });
            for (int x = 0; x < columns; x++)
            {
                //読み込んだ文字を数値に変換→保持
                stageMap[x, y] = (StageTile)int.Parse(nums[x]);
            }
        }

    }
    /// <summary>
    /// ステージの生成
    /// </summary>
    public void CreateStage()
    {
        //ステージの中心位置の計算
        midd.x = columns * tileSize * 0.5f - tileSize * 0.5f;
        midd.y = rows * tileSize * 0.5f - tileSize * 0.5f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                StageTile value = stageMap[x, y];

                //何もない場所はスルーする
                if (value == StageTile.NONE) continue;
                //タイルの名前に行番号と列番号を付与
                string name = "tile" + y + "_" + x;
                //タイルのゲームオブジェクトを作成
                GameObject tile = new GameObject(name);
                //タイルにスプライトレンダラーを取得
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                //タイルのスプライトを設定
                sr.sprite = groundSprite;
                //タイル位置の設定
                tile.transform.position = GetDisplayPosition(x, y);

                //壁の場合
                //if (value == StageTile.WALL)
                //{
                //    //壁のゲームオブジェクトの生成
                //    GameObject wall = new GameObject("wall");
                //    //タイルにスプライトレンダラーを取得
                //    sr = wall.AddComponent<SpriteRenderer>();
                //    //タイルのスプライトを設定
                //    sr.sprite = wallSprite;
                //    //目的地の描画順を手前にする
                //    sr.sortingOrder = 2;
                //    //壁位置の設定
                //    wall.transform.position = GetDisplayPosition(x, y);
                //}

                //ゴールの場合
                if (value == StageTile.GOAL)
                {
                    //目的地のゲームオブジェクトの生成
                    GameObject destination = new GameObject("destination");
                    //タイルにスプライトレンダラーを取得
                    sr = destination.AddComponent<SpriteRenderer>();
                    //タイルのスプライトを設定
                    sr.sprite = goalSplite;
                    //目的地の描画順を手前にする
                    sr.sortingOrder = 1;
                    //ゴール位置の設定
                    destination.transform.position = GetDisplayPosition(x, y);
                }

                //プレイヤーの場合
                if (value == StageTile.PLAYER)
                {
                    //プレイヤーのゲームオブジェクトの生成
                    player = new GameObject("player");
                    //プレイヤーにスプライトレンダラーを取得
                    sr = player.AddComponent<SpriteRenderer>();
                    //プレイヤーのスプライトを設定
                    sr.sprite = playerSprite;
                    //プレイヤーの描画順を手前にする
                    sr.sortingOrder = 2;
                    //プレイヤー位置の設定
                    player.transform.position = GetDisplayPosition(x, y);
                    //プレイヤーを連想配列に追加
                    gameObjectPosTable.Add(player, new Vector2Int(x, y));
                }

                //ブロックの場合
                else if (value == StageTile.BLOCK)
                {
                    //ブロックの数を増やす
                    blockCount++;
                    // ブロックのゲームオブジェクトを作成
                    GameObject block = new GameObject("block" + blockCount);
                    // ブロックにスプライトを描画する機能を追加
                    sr = block.AddComponent<SpriteRenderer>();
                    // ブロックのスプライトを設定
                    sr.sprite = blockSprite;
                    // ブロックの描画順を手前にする
                    sr.sortingOrder = 2;
                    // ブロックの位置を設定
                    block.transform.position = GetDisplayPosition(x, y);
                    // ブロックを連想配列に追加
                    gameObjectPosTable.Add(block, new Vector2Int(x, y));
                }

            }
        }
    }
    /// <summary>
    /// 指定された行番号と列番号からスプライトの表示位置を計算して返す
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Vector2 GetDisplayPosition(int x, int y)
    {
        return new Vector2(x * tileSize - midd.x, y * -tileSize + midd.y);
    }

    private bool IsValidPos(Vector2Int pos)
    {
        if (0 <= pos.x && pos.x < columns && 0 <= pos.y && pos.y < rows)
        {
            return stageMap[pos.x, pos.y] != StageTile.NONE;
        }
        return false;
    }
    private bool IsBlock(Vector2Int pos)
    {
        var cell = stageMap[pos.x, pos.y];
        return cell == StageTile.BLOCK || cell == StageTile.BLOCK_ON_GOAL;
    }

    private void ActionMove(Action action)
    {
        //プレイヤーの現在地を取得
        Vector2Int playerPos = gameObjectPosTable[player];

        //プレイヤーの移動先の位置を計算
        Vector2Int nextPlayerPos = GetNextPosAlong(playerPos, action);

        //プレイヤーの移動先がステージ内でない場合は無視
        if (!IsValidPos(nextPlayerPos)) return;

        //プレイヤーの移動先にブロックが存在する場合
        if (IsBlock(nextPlayerPos))
        {
            //ブロックの移動先の位置を計算
            Vector2Int nextBlockPos = GetNextPosAlong(nextPlayerPos, action);

            //ブロックの移動先がステージ内の場合かつ
            //ブロックの移動先にブロックが存在しない場合
            if (IsValidPos(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                //移動するブロックを取得
                GameObject block = GetGameObjectAtPos(nextPlayerPos);

                //プレイヤーの移動先のタイル情報の更新
                UpdateGameObjectPos(nextPlayerPos);

                //ブロックを移動
                block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y);

                //ブロックの位置を更新
                gameObjectPosTable[block] = nextBlockPos;

                //ブロックの移動先の番号を更新
                if (stageMap[nextBlockPos.x, nextBlockPos.y] == StageTile.GROUND)
                {
                    stageMap[nextBlockPos.x, nextBlockPos.y] = StageTile.BLOCK;
                }
                else if (stageMap[nextBlockPos.x, nextBlockPos.y] == StageTile.GOAL)
                {
                    stageMap[nextBlockPos.x, nextBlockPos.y] = StageTile.BLOCK_ON_GOAL;
                }


                //プレイヤーの現在地のタイル情報を更新
                UpdateGameObjectPos(playerPos);

                //プレイヤー移動
                player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

                //プレイヤー位置を更新
                gameObjectPosTable[player] = nextPlayerPos;

                //プレイヤーの移動先の番号を更新
                if (stageMap[nextPlayerPos.x, nextPlayerPos.y] == StageTile.GROUND)
                {
                    //移動先が地面ならプレイヤーの番号に更新
                    stageMap[nextPlayerPos.x, nextPlayerPos.y] = StageTile.PLAYER;
                }
                else if (stageMap[nextPlayerPos.x, nextPlayerPos.y] == StageTile.GOAL)
                {
                    //移動先がゴールならプレイヤー(ゴールの上)の番号(5)に更新
                    stageMap[nextPlayerPos.x, nextPlayerPos.y] = StageTile.PLAYER_ON_GOAL;
                }
            }
            //クリア確認
            //Clear();
        }
        //プレイヤーの移動先にブロックが存在しない場合
        else
        {
            //プレイヤーの現在地のタイルの情報を更新
            UpdateGameObjectPos(playerPos);

            //プレイヤーを移動
            player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

            //プレイヤーの位置更新
            gameObjectPosTable[player] = nextPlayerPos;

            //プレイヤーの移動先の番号を更新
            if (stageMap[nextPlayerPos.x, nextPlayerPos.y] == StageTile.GROUND)
            {
                // 移動先が地面ならプレイヤーの番号に更新
                stageMap[nextPlayerPos.x, nextPlayerPos.y] = StageTile.PLAYER;
            }
            else if (stageMap[nextPlayerPos.x, nextPlayerPos.y] == StageTile.GOAL)
            {
                stageMap[nextPlayerPos.x, nextPlayerPos.y] = StageTile.PLAYER_ON_GOAL;
            }
        }

        //クリア判定
        Clear();
    }
    private Vector2Int GetNextPosAlong(Vector2Int pos, Action action)
    {
        switch (action)
        {
            case Action.UP:
                pos.y -= 1;
                break;

            case Action.RIGHT:
                pos.x += 1;
                break;

            case Action.DOWN:
                pos.y += 1;
                break;

            case Action.LEFT:
                pos.x -= 1;
                break;
        }
        return pos;
    }
    private void UpdateGameObjectPos(Vector2Int pos)
    {
        var cell = stageMap[pos.x, pos.y];

        if (cell == StageTile.PLAYER || cell == StageTile.BLOCK)
        {
            stageMap[pos.x, pos.y] = StageTile.GROUND;
        }
        else if (cell == StageTile.PLAYER_ON_GOAL || cell == StageTile.BLOCK_ON_GOAL)
        {
            stageMap[pos.x, pos.y] = StageTile.GOAL;
        }
    }

    private GameObject GetGameObjectAtPos(Vector2Int pos)
    {
        foreach(var pair in gameObjectPosTable)
        {
            if (pair.Value == pos)
            {
                return pair.Key;
            }
        }
        return null;
    }
    //クリア判定
    private void Clear()
    {
        //ゴール上のブロック数をカウント
        int blockOnGoalCount = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if(stageMap[x, y] == StageTile.BLOCK_ON_GOAL)
                {
                    blockOnGoalCount++;
                }
            }
        }
        //全ブロックが乗ったらクリア
        if (blockOnGoalCount == blockCount)
        {
            MapSelect();
            //説明を消す
            EnterUI.SetActive(false);
            //次ステージUIの表示
            nextUI.SetActive(true);
            //タイトルに戻るUIの表示
            titleUI.SetActive(true);
            //クリアUIの表示
            clearUI.SetActive(true);
            //クリアフラグをONにする
            isClear = true;
        }
    }
    public void MapSelect()
    {
        switch (stageIndex)
        {
            case 0:
                stageIndex++;
                break;

            case 1:
                stageIndex++;
                break;

            case 2:
                stageIndex++;
                break;

            default:
                stageIndex = 1;
                break;
        }
    }
}
