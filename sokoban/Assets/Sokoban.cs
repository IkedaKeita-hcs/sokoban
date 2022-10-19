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

    public TextAsset _stageFile;

    private int _cols;//行
    private int _rows;//列
    private TileType[,] _tileList;//タイル管理用二次元配列



    private void LoadTileData()
    {
        string[] lines = _stageFile.text.Split
        (
            new[] { '\r', '\n' },
            System.StringSplitOptions.RemoveEmptyEntries
        );

        string[] nums = lines[0].Split(new[] { ',' });

        _rows = lines.Length;
        _cols = nums.Length;

        _tileList = new TileType[_cols, _rows];
        for (int y = 0; y < _rows; y++)
        {
            string st = lines[ y ];
            nums = st.Split(new[] { ',' });
            for (int x = 0; x < _cols; x++) 
            {
                _tileList[x, y] = (TileType)int.Parse(nums[x]);
            }
        } 
    }
   [SerializeField] float _tileSize; // タイルのサイズ

    [SerializeField] Sprite _groundSprite; // 地面のスプライト
    [SerializeField] Sprite _targetSprite; // 目的地のスプライト
    [SerializeField] Sprite _playerSprite; // プレイヤーのスプライト
    [SerializeField] Sprite _blockSprite; // ブロックのスプライト

    private GameObject player; // プレイヤーのゲームオブジェクト
    private Vector2 middleOffset; // 中心位置
    private int blockCount; // ブロックの数

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
    

}