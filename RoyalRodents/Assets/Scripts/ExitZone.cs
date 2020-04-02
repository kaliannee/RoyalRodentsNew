﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public GameObject _locRight;
    public GameObject _locLeft;

    public bool _playerzone;
    public bool _neutralzone;
    public bool _enemyzone;

    private Teleporter _right;
    private Teleporter _left;
    private Teleporter _Active;

    private List<BuildableObject> _outposts = new List<BuildableObject>();

    private void Start()
    {

        if( (_playerzone && (_enemyzone || _neutralzone)) || (_neutralzone && (_enemyzone || _playerzone)))
        {
            Debug.LogError(this.transform.gameObject + "  is set to multiple zones");
        }
        else
        {
            if (_playerzone)
                GameManager.Instance.setExitZone("playerzone", this);
            if (_neutralzone)
                GameManager.Instance.setExitZone("neutralzone", this);
            if (_enemyzone)
                GameManager.Instance.setExitZone("enemyzone", this);
        }

        if (_locRight)
            _right = _locRight.GetComponent<Teleporter>();
        if(_locLeft)
            _left = _locLeft.GetComponent<Teleporter>();
    }


    public void SetOutpost(BuildableObject outpost)
    {
        if (!_outposts.Contains(outpost))
            _outposts.Add(outpost);
    }
    public void RemoveOutpost(BuildableObject outpost)
    {
        if (_outposts.Contains(outpost))
            _outposts.Remove(outpost);
    }

    public List<BuildableObject> getOutposts()
    {
        return _outposts;
    }

    public void childClicked(Teleporter t)
    {
        if (t == _right || t == _left)
            _Active = t;

        selectOutpostMode();

    }

    private void selectOutpostMode()
    {
        int totalTroops =0;
        foreach( var b in _outposts)
        {
            b.setOutlineAvailable();
           totalTroops+= b.getEmployeeCount();
        }

        //Show top screen selection text
        UITroopSelection.Instance.ShowSelection(true, totalTroops, this);
    }
    public List<GameObject> findSelected()
    {
        List<GameObject> chosen = new List<GameObject>();
        foreach (var b in _outposts)
        {
            if (b.checkSelected())
            {
                List<GameObject> recieved = b.getEmployees();
                foreach (var e in recieved)
                {
                    chosen.Add(e);
                    //Do we have to remove them from the outpost? what happens if they die
                    // in combat? do they unassign then?
                    var ss= e.GetComponent<SubjectScript>();
                    if(ss)
                    {
                        ss.setRoyalGuard();
                    }
                }
            }
        }
        //Dont forget the player
        chosen.Add(GameObject.FindGameObjectWithTag("Player"));

        return chosen;
    }
    public void confirmed()
    {
        _Active.Teleport(findSelected(), new Vector3(160, -189.67f, 0));
    }

}
