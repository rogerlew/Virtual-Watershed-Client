using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class mouseray : MonoBehaviour
{
    public mouselistener ml;
    Camera camera;
    public GameObject cursor;

    public GameObject marker1, marker2;
    public List<GameObject> markers = new List<GameObject>();
    Vector3 curpos;
    float timecount;

    // Use this for initialization
    void Start()
    {

    }
    bool previous;
    void activateCursor(bool val)
    {
        if (val != previous)
        {
            cursor.SetActive(val);
            previous = val;
        }
    }
    void setCursor(Vector3 worldp)
    {
        cursor.transform.position = worldp;
        //cursor.transform.eulerAngles = Vector3.zero;
        //Vector3 normal = tm.samepleInterpolatedNormal(worldp);
        //float normAngle = Vector3.Angle (new Vector3(0,1,0),normal);
        //Vector3 rotAxis = Vector3.Cross (new Vector3(0,1,0), normal);
        //cursor.transform.Rotate(rotAxis,normAngle);
        //cursor.transform.RotateAround(worldp,rotAxis,normAngle);
    }
    public static Vector3 raycastHit(Vector3 position)
    {
        Camera camera = Camera.main;
        // Raycast check
        Ray ray = camera.ScreenPointToRay(position);
        RaycastHit[] raycasts = Physics.RaycastAll(ray);

        RaycastHit closestObject = new RaycastHit();
        float range = float.MaxValue;
        bool found = false;
        foreach (RaycastHit i in raycasts)
        {
            //print(i.distance);
            if (i.distance < range)
            {
                range = i.distance;
                closestObject = i;
                found = true;
            }
        }
        if (!found)
        {
            return position;
        }
        //Debug.LogError(closestObject.point + " " + position);

        return closestObject.point;
    }

    public static Vector3 raycastHitFurtherest(Vector3 position, Vector3 direction, float ypoint = -1000)
    {
        //Camera camera = Camera.main;
        // Raycast check
        //Ray ray = camera.ScreenPointToRay(position);
        float temp = position.y;
        position.y = ypoint;

        RaycastHit[] raycasts = Physics.RaycastAll(position, direction);

        RaycastHit closestObject = new RaycastHit();
        float range = float.MinValue;
        bool found = false;
        foreach (RaycastHit i in raycasts)
        {
            //print(i.distance);
            if (i.distance > range && i.transform.gameObject.GetComponent<Terrain>() != null)
            {
                range = i.distance;
                //Debug.LogError(i.transform.gameObject.name);
                closestObject = i;
                found = true;
            }
        }
        if (!found)
        {
            position.y = temp;
            return position;
        }
        //Debug.LogError(closestObject.point + " " + position);

        return closestObject.point;
    }

    float _modf(float k, float bound)
    {
        float r = k / bound;
        return (r - Mathf.Floor(r)) * bound;
    }

    // This function will rotate a 2D vector.
    Vector2 rotate(Vector2 vec, float radians)
    {
        return new Vector2(Mathf.Cos(radians) * vec.x - vec.y * Mathf.Sin(radians), Mathf.Sin(radians) * vec.x + vec.y * Mathf.Cos(radians));
    }
    float invertangle(Vector3 a, Vector3 b, float angle)
    {
        var cross = Vector3.Cross(a, b);
        if (cross.y < 0)
            angle = -angle;
        return angle;
    }
    bool checkLineDistanceFromPoint(Vector3 a, Vector3 b, Vector3 point, float distance = 10.0f)
    {
        a.y = b.y = point.y = 0;
        Vector3 sidea = a - b;
        Vector3 sideb = point - a;
        Vector3 sidec = point - b;
        float ang = Vector3.Angle(-sidea, sideb);
        float ang2 = Vector3.Angle(-sideb, -sidec);
        float ang3 = Vector3.Angle(sidea, sidec);
        //Debug.LogError(sideb + " " + sidec);
        //Debug.LogError((ang + ang2 + ang3));
        ang = invertangle(-sidea, sideb, ang);
        //ang2 = invertangle(-sideb,-sidec,ang2);
        //ang3 = invertangle(sidea,sidec,ang3);

        float bang = ang;
        ang *= (Mathf.PI / 180.0f);
        //Debug.LogError(bang + " " + ang2 + " " + ang3 + " " + Mathf.Abs(sideb.magnitude * Mathf.Sin(ang)));
        //Debug.LogError(sideb.magnitude * Mathf.Sin(ang));

        if (Mathf.Abs(sideb.magnitude * Mathf.Sin(ang)) >= distance || Mathf.Abs(bang) > 120.0f || ang3 > 120.0f)
            return false;

        return true;
    }
    Vector3 mark1posflat
    {
        get
        {
            return new Vector3(marker1.transform.position.x, 0, marker1.transform.position.z);
        }
    }
    Vector3 mark2posflat
    {
        get
        {
            return new Vector3(marker2.transform.position.x, 0, marker2.transform.position.z);
        }
    }
    Vector3 curposflat
    {
        get
        {
            return new Vector3(cursor.transform.position.x, 0, cursor.transform.position.z);
        }
    }
    bool mark1highlighted = false;
    bool mark2highlighted = false;
    // Update is called once per frame
    void Update()
    {

        if (marker1 != null && marker2 != null)
        {
            if (mark1highlighted)
            {
                marker1.GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 0f, 0f, 1.0f));
            }
            if (mark2highlighted)
            {
                marker2.GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 0f, 0f, 1.0f));
            }
            if ((curposflat - mark1posflat).magnitude <= 5.0f)
            {
                marker1.GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 0f, 1f, 1.0f));
                if (Input.GetMouseButtonDown(1))
                {
                    marker1.GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 0f, 0f, 1.0f));
                    mark1highlighted = !mark1highlighted;
                }
            }
            else if (!mark1highlighted)
            {
                marker1.GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1.0f));
            }
            if ((curposflat - mark2posflat).magnitude <= 5.0f)
            {
                marker2.GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 0f, 1f, 1.0f));
                if (Input.GetMouseButtonDown(1))
                {
                    marker2.GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 0f, 0f, 1.0f));
                    mark2highlighted = !mark2highlighted;
                }
            }
            else if (!mark2highlighted)
            {
                marker2.GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1.0f));
            }
            //Debug.LogError(checkLineDistanceFromPoint(marker1.transform.position, marker2.transform.position, curpos));
            timecount += Time.deltaTime;

            if (timecount > 3.0f)

                timecount = _modf(timecount, 3.0f);



            float start = (timecount / 3.0f) * 15.0f;
            Vector3 point_vect = marker2.transform.position - marker1.transform.position;
            float pv_mag = point_vect.magnitude;
            point_vect.Normalize();
            // set sphere positions

            foreach (GameObject sp in markers)
            {


                Vector3 dif_vect = point_vect * (start + 7.5f);

                //sp.transform.position = marker1.transform.position + dif_vect;

                float ypos = raycastHitFurtherest(sp.transform.position, Vector3.up).y;//t.SampleHeight(sp.transform.position);

                Vector3 v = sp.transform.position;

                sp.transform.position = new Vector3(v.x, ypos + 5f, v.z);
                //Debug.LogError(sp.collider.name);
                start += 15.0f;

            }
        }
        // First check if the current state of the mouse is terrain
        if (ml.cursorState == true)
        {
            activateCursor(true);

            /*camera = Camera.main;
            // Raycast check
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] raycasts = Physics.RaycastAll(ray);
			
            RaycastHit closestObject = new RaycastHit();
            float range = float.MaxValue;
            foreach(RaycastHit i in raycasts)
            {
                //print(i.distance);
                if(i.distance < range)
                {
                    range = i.distance;
                    closestObject = i;
                }
            }*/
            // Set Cursor based on check

            //print(closestObject.point);
            curpos = raycastHit(Input.mousePosition);//closestObject.point;

            if (Input.GetMouseButtonDown(0))
            {
                // Place Markers
                if (marker1 == null)
                {
                    // set first marker
                    marker1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    marker1.transform.localScale += new Vector3(10f, 10f, 10f);
                    marker1.transform.position = new Vector3(curpos.x, curpos.y + 7.5f, curpos.z);
                    marker1.GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1.0f));

                }
                else if (marker2 == null)
                {

                    // set first marker
                    marker2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    marker2.transform.localScale += new Vector3(10f, 10f, 10f);
                    marker2.transform.position = new Vector3(curpos.x, curpos.y + 7.5f, curpos.z);
                    marker2.GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1.0f));

                    // setup spheres
                    Vector3 point_vect = marker2.transform.position - marker1.transform.position;
                    float start = 0.0f;
                    float pv_mag = point_vect.magnitude;
                    point_vect.Normalize();
                    while (start < pv_mag - 15.0f)
                    {

                        GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        Vector3 dif_vect = point_vect * (start + 7.5f);

                        sp.transform.localScale += new Vector3(2f, 2f, 2f);

                        sp.transform.position = marker1.transform.position + dif_vect;
                        //sp.transform.position.y -= 10;
                        sp.GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1.0f));

                        markers.Add(sp);

                        start += 15.0f;

                    }
                    timecount = 0.0f;
                }
                else
                {
                    DestroyObject(marker1);
                    DestroyObject(marker2);
                    marker1 = null;
                    marker2 = null;
                    mark1highlighted = false;
                    mark2highlighted = false;
                    // clear sub-spheres
                    foreach (GameObject sp in markers)
                    {
                        DestroyObject(sp);
                    }
                    markers.Clear();
                }
            }

            curpos.y += 10;
            setCursor(curpos);
            coordsystem.transformToUnity(curpos);
            if (Input.GetMouseButton(1))
            {
                if (mark1highlighted && !mark2highlighted)
                {
                    //cursor.active = false;
                    marker1.transform.position = curpos;
                    foreach (GameObject sp in markers)
                    {
                        DestroyObject(sp);
                    }
                    markers.Clear();
                    // setup spheres
                    Vector3 point_vect = marker2.transform.position - marker1.transform.position;
                    float start = 0.0f;
                    float pv_mag = point_vect.magnitude;
                    point_vect.Normalize();
                    while (start < pv_mag - 15.0f)
                    {

                        GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        Vector3 dif_vect = point_vect * (start + 7.5f);

                        sp.transform.localScale += new Vector3(2f, 2f, 2f);

                        sp.transform.position = marker1.transform.position + dif_vect;
                        //sp.transform.position.y -= 10;
                        sp.GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1.0f));

                        markers.Add(sp);

                        start += 15.0f;

                    }
                    timecount = 0.0f;
                    activateCursor(false);
                    return;
                }
                else if (mark2highlighted && !mark1highlighted)
                {
                    //cursor.active = false;
                    marker2.transform.position = curpos;
                    foreach (GameObject sp in markers)
                    {
                        DestroyObject(sp);
                    }
                    markers.Clear();
                    // setup spheres
                    Vector3 point_vect = marker2.transform.position - marker1.transform.position;
                    float start = 0.0f;
                    float pv_mag = point_vect.magnitude;
                    point_vect.Normalize();
                    while (start < pv_mag - 15.0f)
                    {

                        GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        Vector3 dif_vect = point_vect * (start + 7.5f);

                        sp.transform.localScale += new Vector3(2f, 2f, 2f);

                        sp.transform.position = marker1.transform.position + dif_vect;
                        //sp.transform.position.y -= 10;
                        sp.GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1.0f));

                        markers.Add(sp);

                        start += 15.0f;

                    }
                    timecount = 0.0f;
                    activateCursor(false);
                    return;
                }
                else if (mark1highlighted && mark2highlighted)
                {
                    //cursor.active = false;
                    activateCursor(false);
                    return;
                }
            }
            // Check if mouse was clicked
            // set/clear spheres
            // Check if Cursor is between the spheres
            // Draw Arrow -- move cursor and spheres if any movement
        }
        else
        {
            activateCursor(false);
        }
        curpos.y = -10000;
        //print ("HEIGHT: " + tm.sampleHeight(curpos) + " " + curpos);
    }
    void OnGUI()
    {
        /*Vector2 posi = tm.ds.UnityToLatLong (new Vector2 (curpos.x, curpos.z));
        string str = "Height: " + tm.sampleHeight (curpos)+"\n LAT_LONG: " + posi.x + ", " + posi.y;
        Vector2 posoffset = GUI.skin.box.CalcSize(new GUIContent(str));
        GUI.Box(new Rect(Screen.width-posoffset.x,Screen.height-posoffset.y,posoffset.x,posoffset.y),str);*/
    }
    void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(curpos,10);
    }

    void getPointInformation()
    {
        // Query the data of the dataset class
        // One function needed in the dataset class
    }

    void setPoints()
    {
        // Here is where the cross section information goes...
    }


}
