﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bHouse :MonoBehaviour
{
    private static Sprite _builtSpriteLevel1;
	private static Sprite _builtSpriteLevel2;
	private static Sprite _builtSpriteLevel3;
	private static int maxLevel = 3;

	private float _hitpoints = 50;
    private float _hitPointGrowth = 10;

	//create structure costs (costLevel1 is used to BUILD TO level 1, not ON level 1)
	public static Dictionary<ResourceManagerScript.ResourceType, int> _costLevel1 = new Dictionary<ResourceManagerScript.ResourceType, int>();
	public static Dictionary<ResourceManagerScript.ResourceType, int> _costLevel2 = new Dictionary<ResourceManagerScript.ResourceType, int>();
	public static Dictionary<ResourceManagerScript.ResourceType, int> _costLevel3 = new Dictionary<ResourceManagerScript.ResourceType, int>();

	//create Housing Capacity amounts per level
	private static int[] capacityIncrementAmount = new int[4];

	private static bool _isSet;

	void Start()
    {
        SetUpComponent();
        StartCoroutine(EnemyCheckDelay());
    }

	private static void SetUpComponent()
	{
		if (!_isSet)
		//Set Upgrade/Build Costs (1-3 levels)
		{
			_costLevel1.Add(ResourceManagerScript.ResourceType.Trash, 4);

			_costLevel2.Add(ResourceManagerScript.ResourceType.Trash, 6);
			_costLevel2.Add(ResourceManagerScript.ResourceType.Wood, 4);

			_costLevel3.Add(ResourceManagerScript.ResourceType.Trash, 9);
			_costLevel3.Add(ResourceManagerScript.ResourceType.Wood, 6);
			_costLevel3.Add(ResourceManagerScript.ResourceType.Stone, 4);

            _builtSpriteLevel1 = Resources.Load<Sprite>("Buildings/House/trash_house");
            _builtSpriteLevel2 = Resources.Load<Sprite>("Buildings/House/wood_house");
            _builtSpriteLevel3 = Resources.Load<Sprite>("Buildings/House/stone_house");

            //how much total each level contributes to the population capacity
            capacityIncrementAmount[0] = 0;
            capacityIncrementAmount[1] = 2;
            capacityIncrementAmount[2] = 5;
            capacityIncrementAmount[3] = 8;

            _isSet = true;
		}
	}

    IEnumerator EnemyCheckDelay()
    {
        yield return new WaitForSeconds(1);
        CheckEnemy();
    }

    public void CheckEnemy()
    {
        BuildableObject b = this.GetComponent<BuildableObject>();
        if (b)
        {
            if (b.getTeam() == 2)
            {
                b.SetType("House");
                b.SetLevel(1);
                b.BuildComplete();
            }
        }
    }
    public float BuildingComplete(int level)
    {
        if (!_isSet)
            SetUpComponent();

        //Set new structure sprite
        if (level == 1)
		{
			this.transform.GetComponent<SpriteRenderer>().sprite = _builtSpriteLevel1;
		}
        else if (level == 2)
		{
			this.transform.GetComponent<SpriteRenderer>().sprite = _builtSpriteLevel2;
		}
	    else if (level == 3)
		{
			this.transform.GetComponent<SpriteRenderer>().sprite = _builtSpriteLevel3;
		}
		//increment Population Capacity
		//Note: its okay that this is called again when Loading Save Data, becuz the save data gets called after and overwrites it thankfully
		BuildableObject scr = GetComponent<BuildableObject>();
		if (scr)
		{
			if (scr.getTeam() == 1)
				ResourceManagerScript.Instance.incrementPopulationCapacity(capacityIncrementAmount[level] - capacityIncrementAmount[level - 1]);
		}
			

		return (_hitpoints + (_hitPointGrowth*level));
    }

	public void DemolishAction(int level)
	{
		//decrement Population Capacity
		BuildableObject scr = GetComponent<BuildableObject>();
		//get if on player team
		if (scr)
		{
			if (scr.getTeam() == 1)
				ResourceManagerScript.Instance.incrementPopulationCapacity(-capacityIncrementAmount[level]);
		}	
	}
   
	public static Dictionary<ResourceManagerScript.ResourceType, int> getCost(int level)
	{

		if(_costLevel1.Count == 0)
		{
			SetUpComponent();
		}

		if (level == 1)
		{
			return _costLevel1;
		}
		else if (level == 2)
		{
			return _costLevel2;
		}
		else if (level == 3)
		{
			return _costLevel3;
		}
		else
			return null;
	}

	public static int getMaxLevel()
	{
		return maxLevel;
	}
}
