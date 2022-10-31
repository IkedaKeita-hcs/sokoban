using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sokoban : MonoBehaviour
{
    private void Start()
    {
        LoadTileData(); // タイルの情報を読み込む
        CreateStage(); // ステージを作成
    }

    // 指定された行番号と列番号からスプライトの表示位置を計算して返す
    private Vector2 GetDisplayPosition(int x, int y)
    {
        return new Vector2
        (
            x * _tileSize - middleOffset.x,
            y * -_tileSize + middleOffset.y
        );
    }
    private enum TileType
    {
        NONE, // 何も無い
        GROUND,// 地面
        TARGET,// 目的地
        PLAYER,// プレイヤー
        BLOCK,// ブロック

        PLAYER_ON_TARGET,// プレイヤー（目的地の上）
        BLOCK_ON_TARGET// ブロック（目的地の上）
    }

    public TextAsset _stageFile;//ステージ構造を二次元配列で記述したテキストファイル

    private int _cols;//行
    private int _rows;//列
    private TileType[,] _tileList;//タイル管理用二次元配列

    private bool isClear;

    [SerializeField] float _tileSize; // タイルのサイズ

    [SerializeField] Sprite _groundSprite; // 地面のスプライト
    [SerializeField] Sprite _targetSprite; // 目的地のスプライト
    [SerializeField] Sprite _playerSprite; // プレイヤーのスプライト
    [SerializeField] Sprite _blockSprite; // ブロックのスプライト

    private GameObject player; // プレイヤーのゲームオブジェクト
    private Vector2 middleOffset; // 中心位置
    private int blockCount; // ブロックの数


    //タイルの情報を読み込む
    private void LoadTileData()
    {
        //タイルの情報を１行ごとに分割
        string[] lines = _stageFile.text.Split
        (
            new[] { '\r', '\n' },
            System.StringSplitOptions.RemoveEmptyEntries
        );

        //タイルの列数を計算
        string[] nums = lines[0].Split(new[] { ',' });

        //タイルの列数と行数を保持
        _rows = lines.Length;//行
        _cols = nums.Length;//列
        
        //タイルの情報をint型の２次元配列で保持
        _tileList = new TileType[_cols, _rows];
        for (int y = 0; y < _rows; y++)
        {
            //一文字ずつ取得
            string st = lines[y];
            nums = st.Split(new[] { ',' });
            for (int x = 0; x < _cols; x++)
            {
                //読み込んだ文字を数値に変換して保持
                _tileList[x, y] = (TileType)int.Parse(nums[x]);
            }
        }
    }
   

    // 各位置に存在するゲームオブジェクトを管理する連想配列
    private Dictionary<GameObject, Vector2Int> gameObjectPosTable = new Dictionary<GameObject, Vector2Int>();

    private void CreateStage()
    {
        middleOffset.x = _cols * _tileSize * 0.5f - _tileSize * 0.5f;
        middleOffset.y = _rows * _tileSize * 0.5f - _tileSize * 0.5f;

        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _cols; x++)
            {
                TileType val = _tileList[x, y];

                // 何も無い場所は無視
                if (val == TileType.NONE) continue;

                // タイルの名前に行番号と列番号を付与
                string name = "tile" + y + "_" + x;

                // タイルのゲームオブジェクトを作成
                GameObject tile = new GameObject(name);

                // タイルにスプライトを描画する機能を追加
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

                // タイルのスプライトを設定
                sr.sprite = _groundSprite;

                // タイルの位置を設定
                tile.transform.position = GetDisplayPosition(x, y);
                // 目的地の場合
                if (val == TileType.TARGET)
                {
                    // 目的地のゲームオブジェクトを作成
                    GameObject destination = new GameObject("destination");

                    // 目的地にスプライトを描画する機能を追加
                    sr = destination.AddComponent<SpriteRenderer>();

                    // 目的地のスプライトを設定
                    sr.sprite = _targetSprite;

                    // 目的地の描画順を手前にする
                    sr.sortingOrder = 1;

                    // 目的地の位置を設定
                    destination.transform.position = GetDisplayPosition(x, y);
                }
                // プレイヤーの場合
                if (val == TileType.PLAYER)
                {
                    // プレイヤーのゲームオブジェクトを作成
                    player = new GameObject("player");

                    // プレイヤーにスプライトを描画する機能を追加
                    sr = player.AddComponent<SpriteRenderer>();

                    // プレイヤーのスプライトを設定
                    sr.sprite = _playerSprite;

                    // プレイヤーの描画順を手前にする
                    sr.sortingOrder = 2;

                    // プレイヤーの位置を設定
                    player.transform.position = GetDisplayPosition(x, y);

                    // プレイヤーを連想配列に追加
                    gameObjectPosTable.Add(player, new Vector2Int(x, y));
                }
                // ブロックの場合
                else if (val == TileType.BLOCK)
                {
                    // ブロックの数を増やす
                    blockCount++;

                    // ブロックのゲームオブジェクトを作成
                    GameObject block = new GameObject("block" + blockCount);

                    // ブロックにスプライトを描画する機能を追加
                    sr = block.AddComponent<SpriteRenderer>();

                    // ブロックのスプライトを設定
                    sr.sprite = _blockSprite;

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

    //以下判定用のチェック関数
    private GameObject GetGameObjectAtPosition(Vector2Int pos)
    {
        foreach (KeyValuePair<GameObject, Vector2Int> pair in gameObjectPosTable)
        {
            //指定された位置が見つかったとき
            if (pair.Value == pos)
            {
                //その位置にあるオブジェクトを返す
                return pair.Key;
            }

        }
        return null;
    }

    //指定された位置がステージ内ならtrueを返す
    private bool IsValidPosition(Vector2Int pos)
    {
        if (0 <= pos.x && pos.x < _cols && 0 <= pos.y && pos.y < _rows)
        {
            return _tileList[pos.x, pos.y] != TileType.NONE;
        }
        return false;
    }

    //指定された位置のタイルがブロックならtrueを返す
    private bool IsBlock(Vector2Int pos)
    {
        TileType cell = _tileList[pos.x, pos.y];
        return cell == TileType.BLOCK || cell == TileType.BLOCK_ON_TARGET;
    }
    
    private void Update()
    {
        if (isClear) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TryMovePlayer(DirectionType.UP);
        }

        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TryMovePlayer(DirectionType.RIGHT);
        }

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            TryMovePlayer(DirectionType.DOWN);
        }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TryMovePlayer(DirectionType.LEFT);
        }
    }


    private enum DirectionType
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }
    private void TryMovePlayer(DirectionType direction)
    {
        Vector2Int currentPlayerPos = gameObjectPosTable[player];
        Vector2Int nextPlayerPos = GetNextPositionAlong(currentPlayerPos, direction);

        if (!IsValidPosition(nextPlayerPos)) return;

        if (IsBlock(nextPlayerPos))
        {
            Vector2Int nextBlockPos = GetNextPositionAlong(nextPlayerPos, direction);
            if (IsValidPosition(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                // 移動するブロックを取得
                GameObject block = GetGameObjectAtPosition(nextPlayerPos);

                // プレイヤーの移動先のタイルの情報を更新
                UpdateGameObjectPosition(nextPlayerPos);

                // ブロックを移動
                block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y);

                // ブロックの位置を更新
                gameObjectPosTable[block] = nextBlockPos;

                // ブロックの移動先の番号を更新
                if (_tileList[nextBlockPos.x, nextBlockPos.y] == TileType.GROUND)
                {
                    // 移動先が地面ならブロックの番号に更新
                    _tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK;
                }
                else if (_tileList[nextBlockPos.x, nextBlockPos.y] == TileType.TARGET)
                {
                    // 移動先が目的地ならブロック（目的地の上）の番号に更新
                    _tileList[nextBlockPos.x, nextBlockPos.y] = TileType.BLOCK_ON_TARGET;
                }

                // プレイヤーの現在地のタイルの情報を更新
                UpdateGameObjectPosition(currentPlayerPos);

                // プレイヤーを移動
                player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

                // プレイヤーの位置を更新
                gameObjectPosTable[player] = nextPlayerPos;

                // プレイヤーの移動先の番号を更新
                if (_tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.GROUND)
                {
                    // 移動先が地面ならプレイヤーの番号に更新
                    _tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER;
                }
                else if (_tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.TARGET)
                {
                    // 移動先が目的地ならプレイヤー（目的地の上）の番号に更新
                    _tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER_ON_TARGET;
                }
            }
        }
        // プレイヤーの移動先にブロックが存在しない場合
        else
        {
            // プレイヤーの現在地のタイルの情報を更新
            UpdateGameObjectPosition(currentPlayerPos);

            // プレイヤーを移動
            player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

            // プレイヤーの位置を更新
            gameObjectPosTable[player] = nextPlayerPos;

            // プレイヤーの移動先の番号を更新
            if (_tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.GROUND)
            {
                // 移動先が地面ならプレイヤーの番号に更新
                _tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER;
            }
            else if (_tileList[nextPlayerPos.x, nextPlayerPos.y] == TileType.TARGET)
            {
                // 移動先が目的地ならプレイヤー（目的地の上）の番号に更新
                _tileList[nextPlayerPos.x, nextPlayerPos.y] = TileType.PLAYER_ON_TARGET;
            }
        }

        // ゲームをクリアしたかどうか確認
        CheckCompletion();
    }
   
// 指定された方向の位置を返す
private Vector2Int GetNextPositionAlong(Vector2Int pos, DirectionType direction)
    {
        switch (direction)
        {
            // 上
            case DirectionType.UP:
                pos.y -= 1;
                break;

            // 右
            case DirectionType.RIGHT:
                pos.x += 1;
                break;

            // 下
            case DirectionType.DOWN:
                pos.y += 1;
                break;

            // 左
            case DirectionType.LEFT:
                pos.x -= 1;
                break;
        }
        return pos;
    }

    private void UpdateGameObjectPosition(Vector2Int pos)
    {
        // 指定された位置のタイルの番号を取得
        TileType cell = _tileList[pos.x, pos.y];

        // プレイヤーもしくはブロックの場合
        if (cell == TileType.PLAYER || cell == TileType.BLOCK)
        {
            // 地面に変更
            _tileList[pos.x, pos.y] = TileType.GROUND;
        }
        // 目的地に乗っているプレイヤーもしくはブロックの場合
        else if (cell == TileType.PLAYER_ON_TARGET || cell == TileType.BLOCK_ON_TARGET)
        {
            // 目的地に変更
            _tileList[pos.x, pos.y] = TileType.TARGET;
        }
    }

    private void CheckCompletion()
    {
        // 目的地に乗っているブロックの数を計算
        int blockOnTargetCount = 0;

        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _cols; x++)
            {
                if (_tileList[x, y] == TileType.BLOCK_ON_TARGET)
                {
                    blockOnTargetCount++;
                }
            }
        }

        // すべてのブロックが目的地の上に乗っている場合
        if (blockOnTargetCount == blockCount)
        {
            // ゲームクリア
            isClear = true;
        }
    }
}