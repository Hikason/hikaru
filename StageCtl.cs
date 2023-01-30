using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageCtl : MonoBehaviour
{
    //[SerializeField, Header("StageText������ĂˁI")]
    //private TextAsset StageText;
    [SerializeField, Header("StageText�����Ă�")]
    private TextAsset[] StageText;

    public static int stageIndex = 0;  

    [SerializeField, Header("Clear��UI������Ă�")]
    private GameObject clearUI;

    [SerializeField, Header("NEXT��UI������Ă�")]
    private GameObject nextUI;

    [SerializeField, Header("TITLE��UI������Ă�")]
    private GameObject titleUI;

    [SerializeField, Header("Enter��UI������Ă�")]
    private GameObject EnterUI;

    public int rows;    //�s
    public int columns; //��
    public enum StageTile
    {
        NONE, �@�@�@�@�@//[0]�����Ȃ�
        GROUND,         //[1]�n��
        GOAL,   �@�@�@�@//[2]�S�[���n�_
        PLAYER, �@�@�@�@//[3]�v���C���[
        BLOCK,  �@�@�@�@//[4]�u���b�N

        PLAYER_ON_GOAL, //[5]�v���C���[���S�[���n�_�ɂ̂��Ă�
        BLOCK_ON_GOAL,  //[6]�u���b�N���S�[���n�_�ɂ̂��Ă�

        /*WALL,       */    //[7]��
    }
    private StageTile[,] stageMap; //�Q�����z��i�^�C�����̊Ǘ��j

    private enum Action
    {
        UP,    //[0]��Ɉړ�
        RIGHT, //[1]�E�Ɉړ�
        DOWN,  //[2]���Ɉړ�
        LEFT,  //[3]���Ɉړ�
    }

    public float tileSize;     //�^�C���T�C�Y
    public Sprite groundSprite; //�n�ʂ̃X�v���C�g
    public Sprite wallSprite;   //�ǂ̃X�v���C�g
    public Sprite goalSplite;�@ //�S�[���̃X�v���C�g
    public Sprite playerSprite; //�v���C���[�̃X�v���C�g
    public Sprite blockSprite;  //�u���b�N�̃X�v���C�g

    private GameObject player;  //�v���C���[�̃Q�[���I�u�W�F�N�g
    private Vector2 midd;       //���S
    public int blockCount;      //�u���b�N�̃J�E���g��

    public bool isClear;

    private Dictionary<GameObject, Vector2Int> gameObjectPosTable = new Dictionary<GameObject, Vector2Int>();

    private void Awake()
    {
        LoadStageText();//�e�L�X�g�̃^�C�����̓ǂݍ���
        CreateStage();  //�X�e�[�W�����̃��\�b�h�Ă�

        EnterUI.SetActive(true);
        clearUI.SetActive(false);
        nextUI.SetActive(false);
        titleUI.SetActive(false);
    }

    private void Update()
    {
        if (isClear) return;

        //�������������
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //��Ɉړ�
            Debug.Log("������");
            ActionMove(Action.UP);
        }
        //�E������������
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //�E�Ɉړ�
            Debug.Log("������");
            ActionMove(Action.RIGHT);
        }
        //��������������
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //�E�Ɉړ�
            Debug.Log("������");
            ActionMove(Action.DOWN);
        }
        //��������������
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //���Ɉړ�
            Debug.Log("������");
            ActionMove(Action.LEFT);
        }

        if (Input.GetKey(KeyCode.Return))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }

    public void LoadStageText()
    {
        //Text�̒��g����s���Ƃɕ����@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�@�]���ȋ󔒕������l�߂Ă����I�v�V����
        string[] lines = StageText[stageIndex].text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        //�^�C���̗񐔂��v�Z
        //��s�擾���A�J���}��؂�ŕ������{�̒������擾
        string[] nums = lines[0].Split(new[] { ',' });

        //�^�C���̗�ƍs��ێ�
        rows = lines.Length;    //�s
        columns = nums.Length;�@//��

        //�^�C���̗񐔂�int�^�̂Q�����z��ŕێ�
        stageMap = new StageTile[columns, rows];

        //for����stafe.text�̃f�[�^��ǂݍ���ŁustageMap�v�ɕϊ�����
        for (int y = 0; y < rows; y++)
        {
            //�ꕶ�����擾
            string st = lines[y];
            nums = st.Split(new[] { ',' });
            for (int x = 0; x < columns; x++)
            {
                //�ǂݍ��񂾕����𐔒l�ɕϊ����ێ�
                stageMap[x, y] = (StageTile)int.Parse(nums[x]);
            }
        }

    }
    /// <summary>
    /// �X�e�[�W�̐���
    /// </summary>
    public void CreateStage()
    {
        //�X�e�[�W�̒��S�ʒu�̌v�Z
        midd.x = columns * tileSize * 0.5f - tileSize * 0.5f;
        midd.y = rows * tileSize * 0.5f - tileSize * 0.5f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                StageTile value = stageMap[x, y];

                //�����Ȃ��ꏊ�̓X���[����
                if (value == StageTile.NONE) continue;
                //�^�C���̖��O�ɍs�ԍ��Ɨ�ԍ���t�^
                string name = "tile" + y + "_" + x;
                //�^�C���̃Q�[���I�u�W�F�N�g���쐬
                GameObject tile = new GameObject(name);
                //�^�C���ɃX�v���C�g�����_���[���擾
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                //�^�C���̃X�v���C�g��ݒ�
                sr.sprite = groundSprite;
                //�^�C���ʒu�̐ݒ�
                tile.transform.position = GetDisplayPosition(x, y);

                //�ǂ̏ꍇ
                //if (value == StageTile.WALL)
                //{
                //    //�ǂ̃Q�[���I�u�W�F�N�g�̐���
                //    GameObject wall = new GameObject("wall");
                //    //�^�C���ɃX�v���C�g�����_���[���擾
                //    sr = wall.AddComponent<SpriteRenderer>();
                //    //�^�C���̃X�v���C�g��ݒ�
                //    sr.sprite = wallSprite;
                //    //�ړI�n�̕`�揇����O�ɂ���
                //    sr.sortingOrder = 2;
                //    //�ǈʒu�̐ݒ�
                //    wall.transform.position = GetDisplayPosition(x, y);
                //}

                //�S�[���̏ꍇ
                if (value == StageTile.GOAL)
                {
                    //�ړI�n�̃Q�[���I�u�W�F�N�g�̐���
                    GameObject destination = new GameObject("destination");
                    //�^�C���ɃX�v���C�g�����_���[���擾
                    sr = destination.AddComponent<SpriteRenderer>();
                    //�^�C���̃X�v���C�g��ݒ�
                    sr.sprite = goalSplite;
                    //�ړI�n�̕`�揇����O�ɂ���
                    sr.sortingOrder = 1;
                    //�S�[���ʒu�̐ݒ�
                    destination.transform.position = GetDisplayPosition(x, y);
                }

                //�v���C���[�̏ꍇ
                if (value == StageTile.PLAYER)
                {
                    //�v���C���[�̃Q�[���I�u�W�F�N�g�̐���
                    player = new GameObject("player");
                    //�v���C���[�ɃX�v���C�g�����_���[���擾
                    sr = player.AddComponent<SpriteRenderer>();
                    //�v���C���[�̃X�v���C�g��ݒ�
                    sr.sprite = playerSprite;
                    //�v���C���[�̕`�揇����O�ɂ���
                    sr.sortingOrder = 2;
                    //�v���C���[�ʒu�̐ݒ�
                    player.transform.position = GetDisplayPosition(x, y);
                    //�v���C���[��A�z�z��ɒǉ�
                    gameObjectPosTable.Add(player, new Vector2Int(x, y));
                }

                //�u���b�N�̏ꍇ
                else if (value == StageTile.BLOCK)
                {
                    //�u���b�N�̐��𑝂₷
                    blockCount++;
                    // �u���b�N�̃Q�[���I�u�W�F�N�g���쐬
                    GameObject block = new GameObject("block" + blockCount);
                    // �u���b�N�ɃX�v���C�g��`�悷��@�\��ǉ�
                    sr = block.AddComponent<SpriteRenderer>();
                    // �u���b�N�̃X�v���C�g��ݒ�
                    sr.sprite = blockSprite;
                    // �u���b�N�̕`�揇����O�ɂ���
                    sr.sortingOrder = 2;
                    // �u���b�N�̈ʒu��ݒ�
                    block.transform.position = GetDisplayPosition(x, y);
                    // �u���b�N��A�z�z��ɒǉ�
                    gameObjectPosTable.Add(block, new Vector2Int(x, y));
                }

            }
        }
    }
    /// <summary>
    /// �w�肳�ꂽ�s�ԍ��Ɨ�ԍ�����X�v���C�g�̕\���ʒu���v�Z���ĕԂ�
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
        //�v���C���[�̌��ݒn���擾
        Vector2Int playerPos = gameObjectPosTable[player];

        //�v���C���[�̈ړ���̈ʒu���v�Z
        Vector2Int nextPlayerPos = GetNextPosAlong(playerPos, action);

        //�v���C���[�̈ړ��悪�X�e�[�W���łȂ��ꍇ�͖���
        if (!IsValidPos(nextPlayerPos)) return;

        //�v���C���[�̈ړ���Ƀu���b�N�����݂���ꍇ
        if (IsBlock(nextPlayerPos))
        {
            //�u���b�N�̈ړ���̈ʒu���v�Z
            Vector2Int nextBlockPos = GetNextPosAlong(nextPlayerPos, action);

            //�u���b�N�̈ړ��悪�X�e�[�W���̏ꍇ����
            //�u���b�N�̈ړ���Ƀu���b�N�����݂��Ȃ��ꍇ
            if (IsValidPos(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                //�ړ�����u���b�N���擾
                GameObject block = GetGameObjectAtPos(nextPlayerPos);

                //�v���C���[�̈ړ���̃^�C�����̍X�V
                UpdateGameObjectPos(nextPlayerPos);

                //�u���b�N���ړ�
                block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y);

                //�u���b�N�̈ʒu���X�V
                gameObjectPosTable[block] = nextBlockPos;

                //�u���b�N�̈ړ���̔ԍ����X�V
                if (stageMap[nextBlockPos.x, nextBlockPos.y] == StageTile.GROUND)
                {
                    stageMap[nextBlockPos.x, nextBlockPos.y] = StageTile.BLOCK;
                }
                else if (stageMap[nextBlockPos.x, nextBlockPos.y] == StageTile.GOAL)
                {
                    stageMap[nextBlockPos.x, nextBlockPos.y] = StageTile.BLOCK_ON_GOAL;
                }


                //�v���C���[�̌��ݒn�̃^�C�������X�V
                UpdateGameObjectPos(playerPos);

                //�v���C���[�ړ�
                player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

                //�v���C���[�ʒu���X�V
                gameObjectPosTable[player] = nextPlayerPos;

                //�v���C���[�̈ړ���̔ԍ����X�V
                if (stageMap[nextPlayerPos.x, nextPlayerPos.y] == StageTile.GROUND)
                {
                    //�ړ��悪�n�ʂȂ�v���C���[�̔ԍ��ɍX�V
                    stageMap[nextPlayerPos.x, nextPlayerPos.y] = StageTile.PLAYER;
                }
                else if (stageMap[nextPlayerPos.x, nextPlayerPos.y] == StageTile.GOAL)
                {
                    //�ړ��悪�S�[���Ȃ�v���C���[(�S�[���̏�)�̔ԍ�(5)�ɍX�V
                    stageMap[nextPlayerPos.x, nextPlayerPos.y] = StageTile.PLAYER_ON_GOAL;
                }
            }
            //�N���A�m�F
            //Clear();
        }
        //�v���C���[�̈ړ���Ƀu���b�N�����݂��Ȃ��ꍇ
        else
        {
            //�v���C���[�̌��ݒn�̃^�C���̏����X�V
            UpdateGameObjectPos(playerPos);

            //�v���C���[���ړ�
            player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

            //�v���C���[�̈ʒu�X�V
            gameObjectPosTable[player] = nextPlayerPos;

            //�v���C���[�̈ړ���̔ԍ����X�V
            if (stageMap[nextPlayerPos.x, nextPlayerPos.y] == StageTile.GROUND)
            {
                // �ړ��悪�n�ʂȂ�v���C���[�̔ԍ��ɍX�V
                stageMap[nextPlayerPos.x, nextPlayerPos.y] = StageTile.PLAYER;
            }
            else if (stageMap[nextPlayerPos.x, nextPlayerPos.y] == StageTile.GOAL)
            {
                stageMap[nextPlayerPos.x, nextPlayerPos.y] = StageTile.PLAYER_ON_GOAL;
            }
        }

        //�N���A����
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
    //�N���A����
    private void Clear()
    {
        //�S�[����̃u���b�N�����J�E���g
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
        //�S�u���b�N���������N���A
        if (blockOnGoalCount == blockCount)
        {
            MapSelect();
            //����������
            EnterUI.SetActive(false);
            //���X�e�[�WUI�̕\��
            nextUI.SetActive(true);
            //�^�C�g���ɖ߂�UI�̕\��
            titleUI.SetActive(true);
            //�N���AUI�̕\��
            clearUI.SetActive(true);
            //�N���A�t���O��ON�ɂ���
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
