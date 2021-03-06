﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public GameObject _locRight;
    public GameObject _locLeft;

    //public bool _playerzone;
    //public bool _neutralzone;
    //public bool _enemyzone;
    // vvv
    public int _zone;
    public bool _isRightZone;

    [SerializeField]
    private GameObject _TeleportDummy;

    private Teleporter _right;
    private Teleporter _left;
    private Teleporter _Active;

    public bool test=false;
    private bool _shutDown = false;

    private List<BuildableObject> _outposts = new List<BuildableObject>();
    private Dictionary<Rodent, BuildableObject> _TroopLocations = new Dictionary<Rodent, BuildableObject>();

    private void Start()
    {

        //if( (_playerzone && (_enemyzone || _neutralzone)) || (_neutralzone && (_enemyzone || _playerzone)))
        //{
            //Debug.LogError(this.transform.gameObject + "  is set to multiple zones");
        //}
        //else
        //{
            /*
            if (_zone == 1)
                GameManager.Instance.setExitZone("playerzone", this);
            if (_zone == 0)
                GameManager.Instance.setExitZone("neutralzone", this);
            if (_zone = 2)
                GameManager.Instance.setExitZone("enemyzone", this);
            */
            GameManager.Instance.setExitZone(_zone, _isRightZone, this);
        //}

        if (_locRight)
            _right = _locRight.GetComponent<Teleporter>();
        if(_locLeft)
            _left = _locLeft.GetComponent<Teleporter>();

        if(_TeleportDummy==null)
            _TeleportDummy = GameObject.FindGameObjectWithTag("TeleportedRodents");

        if(_zone==2)
        {
            //Tell the event system were important 
            if (_isRightZone)
                EventSystem.Instance.ZoneR += ShutDown;
            else 
                EventSystem.Instance.ZoneL += ShutDown;
        }
    }

    private void Update()
    {
        if (test)
        {
            ShutDown();
            test = false;
        }
    }
    public BuildableObject getRodentOutpost(Rodent r)
    {
       // print("looking for " + r.getName());
        return _TroopLocations[r];
    }
    public void RemoveDeadRodent(Rodent r)
    {
        if (_TroopLocations.ContainsKey(r))
            _TroopLocations.Remove(r);
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
       // print("CLICKED CHILD" + t.gameObject);
        if (t == _right || t == _left)
            _Active = t;

        if (_zone == 1)
            selectOutpostMode();
        else 
            selectDeployedTroops();

    }
    private void selectDeployedTroops()
    {
        //Tell UITroopSelection to show only confirm/cancel
        UITroopSelection.Instance.ShowSelection(true, this);
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
        if (_zone == 1)
            return findfromOutposts();
        else
            return findfromCached();

    }
    /**Find rodents whove been teleported */
    private List<GameObject> findfromCached()
    {

       // Debug.Log("Finding from Cached");
        List<GameObject> chosen = new List<GameObject>();

        //Get the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        chosen.Add(player);
      
        //get the royal guard
        PlayerStats ps = player.GetComponent<PlayerStats>();
        if (ps)
        {
            foreach (var e in ps.getEmployees())
            {
                chosen.Add(e);
                //everytime we teleport clear previous targets and go back to royal guarding
                Rodent r = e.GetComponent<Rodent>();
                if (r)
                {
                    var ss = e.GetComponent<SubjectScript>();
                    if (ss)
                    {
                        ss.setRoyalGuard();

                    }
                }
            }
        }

        //get the previously teleported troops
        foreach (Rodent child in _TeleportDummy.GetComponentsInChildren<Rodent>())
        {
            chosen.Add(child.gameObject);
            //put them back in heirarchy
            child.transform.SetParent(GameObject.FindGameObjectWithTag("PlayerRodents").transform);
        }

        //If were coming back from the neutral zone to player zone
        if ((_zone == 2 && !_isRightZone) || (_zone == 3 && _isRightZone))
        {
            //Reset the rodents to go back to the outpost
            foreach (BuildableObject b in _outposts)
            {
                List<GameObject> workers = b.getEmployees();
                foreach (var go in workers)
                {
                    //b.AssignWorker(go.GetComponent<Rodent>());
                    Rodent r = go.GetComponent<Rodent>();
                    if (r)
                        r.setTarget(b.gameObject); // setting target will reset them back to outpost duties
                }
            }
        }
        _TroopLocations.Clear();
        return chosen;
    }

    /** Find rodents from the players outpost */
    private List<GameObject> findfromOutposts()
    {
        List<GameObject> chosen = new List<GameObject>();

        //Get the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        chosen.Add(player);

        //get the royal guard
        PlayerStats ps = player.GetComponent<PlayerStats>();
        if (ps)
        {
            foreach (var e in ps.getEmployees())
            {
                chosen.Add(e);
            }
        }
        // print("called find selected()   size= " + _outposts.Count);

        foreach (var b in _outposts)
        {
            // print("checking building:" + b.name);
            if (b.checkSelected())
            {
                //print(b.name + "is true");
                List<GameObject> recieved = b.getEmployees();
                foreach (var e in recieved)
                {
                    //print("we found employee " + e);
                    chosen.Add(e);
                    Rodent r = e.GetComponent<Rodent>();
                    if (r)
                    {
                        if (!_TroopLocations.ContainsKey(r))
                            _TroopLocations.Add(r, b);
                        //Do we have to remove them from the outpost? what happens if they die
                        // in combat? do they unassign then?
                        var ss = e.GetComponent<SubjectScript>();
                        if (ss)
                        {
                            ss.setRoyalGuard();

                        }
                        //Set them to a new item in the heirarchy to teleport back later
                        e.transform.SetParent(_TeleportDummy.transform);
                    }
                }
            }
        }
        return chosen;

    }

    public void ShutDown()
    {
        if (_Active == null)
            _Active = _right.isActiveAndEnabled ? _right : _left ; //if rights gameobject is active, chose right, else do left. If left is inactive, were fucked

        if (_Active == null)
        {
            Debug.LogError("Active is null return");
            return;
        }
        _shutDown = true;
        confirmed();
    }
    //Need this to subscribe to the event system for unique Locs
    public void confirmed()
    {
        _Active.Teleport(findSelected(), _shutDown);
    }

}
