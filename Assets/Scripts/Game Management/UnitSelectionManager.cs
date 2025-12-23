using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static UnityEngine.UI.CanvasScaler;

public class UnitSelectionManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public static UnitSelectionManager Instance { get; set; }
    public int ranks = 3;
    public GameObject file;
    public GameObject formation;

    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> Formations = new List<GameObject>();
    public List<GameObject> selectedUnitsList = new List<GameObject>();
    public List<GameObject> soldiersList = new List<GameObject>();

    Vector3 GeneralTarget = new Vector3(4f, 0f, 40f);

    List<Vector3> Waypoints = new List<Vector3>();

    LayerMask clickable;
    LayerMask ground;

    Vector3 Target = Vector3.zero;

    private Camera cam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    string CheckState(GameObject unit)
    {
        return unit.GetComponent<BaseUnitScript>().getState();
    }

    private void Start()
    {
        cam = Camera.main;
        clickable = LayerMask.GetMask("PlayerTeam");
        ground = LayerMask.GetMask("Floor");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray movePosition = cam.ScreenPointToRay(Input.mousePosition);
            // If we hit a clickable object
            if (Physics.Raycast(movePosition, out hit, Mathf.Infinity, clickable))
            {
                Target = hit.point;
                // Adds multiple selection
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    MultiSelect(hit.collider.gameObject);
                }
                else
                {
                    SelectByClicking(hit.collider.gameObject);
                }
            }
            else
            {
                if (!(Input.GetKey(KeyCode.LeftShift)))
                {
                    DeselectAll();
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray movePosition = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(movePosition, out hit, Mathf.Infinity, ground))
            {
                Target = hit.point;
                if (Formations.Count > 0)
                {
                    // Would be replaced with a request from the A* pathfinding algorithm but is this for now
                    Waypoints.Clear();
                    for (int i = 0; i < 10; i++)
                    {
                        Waypoints.Add(RandomVector(50f, 50f));
                    }

                    foreach (GameObject formation in Formations)
                    {
                        // Adds an extra point in front of the formation to keep the formation straight initally
                        Vector3[] fpath = new Vector3[Waypoints.Count + 1];
                        fpath[0] = formation.transform.position + formation.transform.forward * formation.GetComponent<Formation>().ZSpacing();
                        for (int i = 1; i <= Waypoints.Count; i++)
                        {
                            // Adds waypoints to the formations
                            fpath[i] = Waypoints[i - 1];
                        }
                        // Sets the path
                        formation.GetComponent<Formation>().setPath(fpath);
                    }
                }
                else
                {
                    foreach (GameObject unit in selectedUnitsList)
                    {
                        unit.GetComponent<BaseUnitScript>().setState("Moving");
                        unit.GetComponent<BaseUnitScript>().setTarget(hit.point);
                    }
                }
            }
        }
        else if (Input.GetKeyDown("f"))
        {
            // Probably check if any of the existing units are already in a formation
            // And if so, delete the formation that they're in
            bool noformations = true;
            // Clears all formations if any selected units are in a formation
            foreach (GameObject Unit in selectedUnitsList)
            {
                Debug.Log("Foreach loop reached");
                // Add or statements for other states the unit could be in in formation
                if (CheckState(Unit) == "At Ease" || CheckState(Unit) == "Forming Up" || CheckState(Unit) == "Marching")
                {
                    Debug.Log("Unit in Formation Detected");
                    foreach (GameObject F in Formations)
                    {
                        Destroy(F);
                        Debug.Log("Formation Destroyed (?)");
                    }
                    Debug.Log("Formation wil not be made (?)");
                    noformations = false;
                }
            }

            if (noformations)
            {
                // This is where the selection script stuff goes
                Vector3 GeneralDirection = GeneralTarget - COMofUnits();
                Quaternion FormationOrientation = Quaternion.Euler(0f, Bearing(GeneralDirection), 0f);

                // Creates the formation
                GameObject Formation = Instantiate(formation, COMofUnits(), FormationOrientation);
                // Make units that are brutes not get into formation

                foreach (GameObject unit in selectedUnitsList)
                {
                    if (unit.GetComponent<BaseUnitScript>().getType() == "Soldier")
                    {
                        soldiersList.Add(unit);
                    }
                }

                // Calculates the number of files to create
                int nfiles = soldiersList.Count / ranks + 1;
                // Calculates the spacing of each of the files
                Vector3 down = -(GeneralDirection).normalized * Formation.GetComponent<Formation>().ZSpacing();
                Vector3 across = Formation.transform.right * Formation.GetComponent<Formation>().XSpacing();
                Formation.GetComponent<Formation>().initialDirection = GeneralDirection.normalized;

                // Variables for inputting units into files
                // Counts the number of units inputted
                int k = 0;

                for (int i = 0; i < nfiles; i++)
                {
                    GameObject File = Instantiate(file, COMofUnits() + down * i, FormationOrientation);
                    Formation.GetComponent<Formation>().AddFile(File);
                    File.GetComponent<File>().HorizontalSpacing = Formation.GetComponent<Formation>().XSpacing();
                    File.GetComponent<File>().n_ranks = ranks;
                    for (int j = 0; j < ranks; j++)
                    {
                        if (k < soldiersList.Count)
                        {
                            // Gets the current unit that it's selecting
                            GameObject currentunit = soldiersList[k];
                            // Gets the current target
                            Vector3 currentTarget = COMofUnits() + down * i + across * (ranks / 2 - j);
                            // Adds unit to the file
                            File.GetComponent<File>().AddUnit(currentunit);
                            // Sets it's target
                            currentunit.GetComponent<BaseUnitScript>().setTarget(currentTarget);
                            // Set's its desired direction
                            currentunit.GetComponent<BaseUnitScript>().TargetForwardTransform = GeneralDirection;
                            // Tells it to go in into formation
                            currentunit.GetComponent<BaseUnitScript>().setState("Forming Up");
                        }
                        else
                        {
                            break;
                        }
                        k++;
                    }

                }
                soldiersList.Clear();
            }
            // Then when we tell them to move, they'll move with the formation
        }
    }

    private void MultiSelect(GameObject gameObject)
    {
        if (!(selectedUnitsList.Contains(gameObject)))
        {
            TriggerSelectionIndicator(gameObject, true);
            selectedUnitsList.Add(gameObject);
        }
        else
        {
            TriggerSelectionIndicator(gameObject, false);
            selectedUnitsList.Remove(gameObject);
        }
    }

    Vector3 RandomVector(float Xcomp, float Zcomp)
    {
        float X = UnityEngine.Random.Range(-Xcomp, Xcomp);
        float Z = UnityEngine.Random.Range(-Zcomp, Zcomp);
        Vector3 Out = new Vector3(X, 0.0f, Z);
        return Out;
    }
    float Bearing(Vector3 Vect2)
    {
        Vect2.y = 0;
        // finds the angle between the vector and forward
        float Angle = Vector3.Angle(Vector3.forward, Vect2);
        // Multiplies it by -1 if the angle is to the left of forward
        Angle *= Mathf.Sign(Vector3.Dot(Vect2, Vector3.right));
        return Angle;
    }

    public void DeselectAll()
    {
        foreach (GameObject unit in selectedUnitsList)
        {
            TriggerSelectionIndicator(unit, false);
        }
        selectedUnitsList.Clear();
    }

    private void SelectByClicking(GameObject unit)
    {


        DeselectAll();
        TriggerSelectionIndicator(unit, true);
        selectedUnitsList.Add(unit);

    }

    private void TriggerSelectionIndicator(GameObject unit, bool isVisible)
    {
        unit.transform.GetChild(0).gameObject.SetActive(isVisible);
    }

    Vector3 COMofUnits()
    {
        // Gets the average location of all selected units
        float n = 0;
        Vector3 COM = Vector3.zero;
        foreach (GameObject unit in selectedUnitsList)
        {
            // Adds up all their positions
            COM += unit.transform.position;
            n += 1;
        }
        // Divides by N to get the mean
        COM = COM / n;
        // Makes it flat
        COM.y = 0;
        return COM;
    }

    internal void DragSelect(GameObject unit)
    {
        if (selectedUnitsList.Contains(unit) == false)
        {
            selectedUnitsList.Add(unit);
            TriggerSelectionIndicator(unit, true);
        }
    }
}
