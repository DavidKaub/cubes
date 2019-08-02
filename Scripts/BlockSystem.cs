using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockSystem : MonoBehaviour
{

    [SerializeField]
    private BlockType[] allBlockTypes;

    [HideInInspector]
    public Dictionary<int, Block> allBlocks = new Dictionary<int, Block>();

    private void Awake()
    {
        for (int i = 0; i < allBlockTypes.Length; i++)
        {
            BlockType newBlockType = allBlockTypes[i];
            Block newBlock = new Block(i, newBlockType.blockName, newBlockType.blockMat, newBlockType.icon);
            allBlocks[i] = newBlock;
            //Debug.Log("Block added to dictionary " + allBlocks[i].name);
        }
    }
}

public class Block : Item
{
    public Material blockMaterial;

    public Block(int id, string name, Material mat, Sprite icon)
    {
        this.itemId = id;
        this.name = name;
        blockMaterial = mat;
        this.icon = icon;
    }
}

[Serializable]
public struct BlockType
{
    public string blockName;
    public Material blockMat;
    public Sprite icon;
}


