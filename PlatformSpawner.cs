using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformSpawner : MonoBehaviour
{
    RaycastHit hit;
    [Tooltip("The distance objects can be targeted at")]
    public float range = 6.5f;
    [Tooltip("Delay between each platform spawn")]
    public float cooldown = 0.1f;
    float _cooldown;
    [Tooltip("Layers that can be targeted to spawn platforms from")]
    public LayerMask targetableLayers;

    [Tooltip("Object that can be spawned")]
    public GameObject platform;
    //Used to remove platform to prevent excess platforms
    GameObject lastPlatform;
    [Tooltip("Indicator of where & if platform will be spawned")]
    public GameObject indicator;
    GameObject _indicator;
    [Tooltip("Indicator for when platform cannot be placed")]
    public GameObject indicatorR;
    GameObject _indicatorR;
    [Tooltip("Bounding box size of platform")]
    Vector3 platformSize;
    [Tooltip("Aiming reticle on HUD; white/colorless image preferred, since image color will be changed"), Header("Reticle")]
    public GameObject reticle;
    public Color green;
    public Color red;
    Color original;
    bool aiming;

    void Start()
    {
        _indicator = Instantiate(indicator, Vector3.down, transform.rotation);
        _indicator.GetComponent<BoxCollider>().enabled = true;
        platformSize = _indicator.GetComponent<BoxCollider>().bounds.extents;
        _indicator.GetComponent<BoxCollider>().enabled = false;
        _indicator.SetActive(false);
        _indicatorR = Instantiate(indicatorR, Vector3.down, transform.rotation); 
        _indicatorR.SetActive(false);
        original = reticle.GetComponent<Image>().color;
    }

    private void OnEnable()
    {
        if (lastPlatform != null)
            lastPlatform.transform.GetChild(0).GetComponent<Animator>().speed = 1;

        if(Input.GetMouseButton(1))
            aiming = true;
        reticle.SetActive(true);
    }

    void Update()
    {
        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range, targetableLayers);

        AimPlatform();
    }

    void AimPlatform()
    {
        //Aim with RMB
        if (Input.GetMouseButtonDown(1))
            aiming = true;
        if (hit.collider != null && aiming)
        {
            _indicator.SetActive(true);
            _indicatorR.SetActive(false);
            reticle.GetComponent<Image>().color = green;

            _indicator.transform.rotation = Quaternion.LookRotation(hit.normal, -hit.transform.up/*Quaternion.AngleAxis(-90, -hit.normal) * hit.transform.right*/);

            //For when object is rotated
            #region Indicator Rotation
            if(hit.normal.normalized == hit.transform.up.normalized)
                _indicator.transform.rotation = Quaternion.LookRotation(hit.normal, Quaternion.AngleAxis(-90, -hit.normal) * hit.transform.right);
            #endregion

            if (hit.transform.gameObject.tag == "Round") //Round Objects
            {
                _indicator.transform.position = (hit.point - hit.normal * 0.1f);
            }
            else //Other Objects (Squares)
            {
                #region Square Orientation

                //Check that corners are attached to target object
                RaycastHit hitTR;
                RaycastHit hitBR;
                RaycastHit hitBL;
                RaycastHit hitTL;

                //Create offset to line up raycast with corners of indicator
                Vector3 offset = Quaternion.LookRotation(-hit.normal, hit.transform.up) * new Vector3(platformSize.x, platformSize.y, 0);
                //bool isOffset = false;
                if (hit.normal.normalized == hit.transform.up.normalized && hit.transform.rotation.eulerAngles.z > 0 || -hit.normal.normalized == hit.transform.up.normalized && hit.transform.rotation.eulerAngles.z > 0)
                {
                    //isOffset = true;
                    Vector3 upDir = Quaternion.AngleAxis(90, -hit.normal) * hit.transform.right;
                    offset = Quaternion.LookRotation(-hit.normal, upDir) * new Vector3(platformSize.x, platformSize.y, 0);
                }
                if (hit.normal.normalized == hit.transform.up.normalized && hit.transform.rotation.eulerAngles.y > 0 || -hit.normal.normalized == hit.transform.up.normalized && hit.transform.rotation.eulerAngles.y > 0)
                {
                    Vector3 upDir = Quaternion.AngleAxis(90, -hit.normal) * hit.transform.right;
                    /*if(isOffset)
                        offset += Quaternion.LookRotation(-hit.normal, upDir) * new Vector3(platformSize.x, platformSize.y, 0);
                    else*/
                        offset = Quaternion.LookRotation(-hit.normal, upDir) * new Vector3(platformSize.x, platformSize.y, 0);
                }
                //Formula = Raycast Impact Point + Offset to line up with corner + normal/10 so it doesn't clip into the collision box
                Physics.Raycast(hit.point + offset + hit.normal * 0.09f, -hit.normal, out hitTR, 0.1f, targetableLayers);
                Physics.Raycast(hit.point - offset + hit.normal * 0.09f, -hit.normal, out hitBL, 0.1f, targetableLayers);
                if(hitTR.collider != null) { Debug.DrawRay(hit.point + offset, -hit.normal, Color.green); } else { Debug.DrawRay(hit.point + offset, -hit.normal, Color.red); }
                if (hitBL.collider != null) { Debug.DrawRay(hit.point - offset, -hit.normal, Color.green); } else { Debug.DrawRay(hit.point - offset, -hit.normal, Color.red); }

                offset = Quaternion.LookRotation(-hit.normal, hit.transform.up) * new Vector3(platformSize.x, -platformSize.y, 0); //reuse offset
                //isOffset = false;
                if (hit.normal.normalized == hit.transform.up.normalized && hit.transform.rotation.eulerAngles.z > 0 || -hit.normal.normalized == hit.transform.up.normalized && hit.transform.rotation.eulerAngles.z > 0)
                {
                    Vector3 upDir = Quaternion.AngleAxis(90, -hit.normal) * hit.transform.right;
                    offset = Quaternion.LookRotation(-hit.normal, upDir) * new Vector3(platformSize.x, -platformSize.y, 0);
                }
                if (hit.normal.normalized == hit.transform.up.normalized && hit.transform.rotation.eulerAngles.y > 0 || -hit.normal.normalized == hit.transform.up.normalized && hit.transform.rotation.eulerAngles.y > 0)
                {
                    Vector3 upDir = Quaternion.AngleAxis(90, -hit.normal) * hit.transform.right;
                    /*if (isOffset)
                        offset += Quaternion.LookRotation(-hit.normal, upDir) * new Vector3(platformSize.x, -platformSize.y, 0);
                    else*/
                        offset = Quaternion.LookRotation(-hit.normal, upDir) * new Vector3(platformSize.x, -platformSize.y, 0);
                }
                Physics.Raycast(hit.point + offset + hit.normal * 0.09f, -hit.normal, out hitBR, 0.1f, targetableLayers);
                Physics.Raycast(hit.point - offset + hit.normal * 0.09f, -hit.normal, out hitTL, 0.1f, targetableLayers);
                if (hitBR.collider != null) { Debug.DrawRay(hit.point + offset, -hit.normal, Color.green); } else { Debug.DrawRay(hit.point + offset, -hit.normal, Color.red); }
                if (hitTL.collider != null) { Debug.DrawRay(hit.point - offset, -hit.normal, Color.green); } else { Debug.DrawRay(hit.point - offset, -hit.normal, Color.red); }


                bool touchingRight = true;
                bool touchingBottom = true;
                bool touchingLeft = true;
                bool touchingTop = true;
                //Right of indicator
                if (hitTR.collider == null && hitBR.collider == null) { touchingRight = false; }
                //Bottom of indicator
                if (hitBR.collider == null && hitBL.collider == null) { touchingBottom = false; }
                //Left of indicator
                if (hitBL.collider == null && hitTL.collider == null) { touchingLeft = false; }
                //Top of indicator
                if (hitTL.collider == null && hitTR.collider == null) { touchingTop = false; }


                //Snap indicator to target's edge
                offset = Vector3.zero;
                if (!touchingRight && !touchingLeft || !touchingTop && !touchingBottom) {
                    _indicator.SetActive(false);
                    _indicatorR.transform.position = (hit.point - hit.normal * 0.1f);
                    _indicatorR.transform.rotation = Quaternion.AngleAxis(-hit.transform.rotation.eulerAngles.z, -hit.normal) * _indicator.transform.rotation;
                    _indicatorR.SetActive(true);
                    reticle.GetComponent<Image>().color = red;
                }
                else
                {
                    if (!touchingRight)
                    {
                        Vector3 edgeDir = Quaternion.AngleAxis(90, _indicator.transform.up) * hit.normal;
                        RaycastHit rightEdge;
                        Physics.Raycast(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), -edgeDir, out rightEdge, 0.52f, targetableLayers);
                        Debug.DrawRay(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), -edgeDir * 0.52f, Color.blue); //Max Range of Raycast
                        Debug.DrawLine(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), rightEdge.point, Color.yellow); //Distance of Raycast to Collision Point
                        offset += rightEdge.point - (hit.point + (edgeDir * 0.5f) - (hit.normal * 0.03f));
                    }
                    if (!touchingBottom)
                    {
                        Vector3 edgeDir = Quaternion.AngleAxis(-90, _indicator.transform.right) * hit.normal;
                        RaycastHit BottomEdge;
                        Physics.Raycast(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), -edgeDir, out BottomEdge, 0.52f, targetableLayers);
                        Debug.DrawRay(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), -edgeDir * 0.52f, Color.blue); //Max Range of Raycast
                        Debug.DrawLine(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), BottomEdge.point, Color.yellow); //Distance of Raycast to Collision Point
                        offset += BottomEdge.point - (hit.point + (edgeDir * 0.5f) - (hit.normal * 0.03f));
                    }
                    if (!touchingLeft)
                    {
                        Vector3 edgeDir = Quaternion.AngleAxis(-90, _indicator.transform.up) * hit.normal;
                        RaycastHit leftEdge;
                        Physics.Raycast(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), -edgeDir, out leftEdge, 0.52f, targetableLayers);
                        Debug.DrawRay(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), -edgeDir * 0.52f, Color.blue); //Max Range of Raycast
                        Debug.DrawLine(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), leftEdge.point, Color.yellow); //Distance of Raycast to Collision Point
                        offset += leftEdge.point - (hit.point + (edgeDir * 0.5f) - (hit.normal * 0.03f));
                    }
                    if (!touchingTop)
                    {
                        Vector3 edgeDir = Quaternion.AngleAxis(90, _indicator.transform.right) * hit.normal;
                        RaycastHit TopEdge;
                        Physics.Raycast(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), -edgeDir, out TopEdge, 0.52f, targetableLayers);
                        Debug.DrawRay(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), -edgeDir * 0.52f, Color.blue); //Max Range of Raycast
                        Debug.DrawLine(hit.point + (edgeDir * 0.52f) - (hit.normal * 0.03f), TopEdge.point, Color.yellow); //Distance of Raycast to Collision Point
                        offset += TopEdge.point - (hit.point + (edgeDir * 0.5f) - (hit.normal * 0.03f));
                    }
                }

                _indicator.transform.position = hit.point+offset;

                #endregion
            }

            //Spawn Platform
            if (Input.GetKeyDown(KeyCode.Mouse0) && _indicator.activeSelf && _cooldown <= 0)
            {
                DestroyPlatform(lastPlatform);
                lastPlatform = Instantiate(platform, _indicator.transform.position, _indicator.transform.rotation);
                lastPlatform.transform.parent = hit.transform;
                _cooldown = cooldown;
            }
            _cooldown -= Time.deltaTime;
        }
        else
        {
            _indicator.SetActive(false);
            _indicatorR.SetActive(false);
            if(aiming)
                reticle.GetComponent<Image>().color = red;
            else
                reticle.GetComponent<Image>().color = original;
        }
        if (Input.GetMouseButtonUp(1))
        {
            aiming = false;
            _indicator.SetActive(false);
            _indicatorR.SetActive(false);
            reticle.GetComponent<Image>().color = original;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (hit.collider != null)
            Gizmos.DrawLine(Camera.main.transform.position, hit.point);
        else
            Gizmos.DrawLine(Camera.main.transform.position, Camera.main.transform.position + Camera.main.transform.forward.normalized * range);

    }

    public void Pause()
    {
        aiming = false;
        _indicator.SetActive(false);
        _indicatorR.SetActive(false);
        reticle.GetComponent<Image>().color = original;
        reticle.SetActive(false);
        if(lastPlatform!=null)
            lastPlatform.transform.GetChild(0).GetComponent<Animator>().speed = 0; ///Unpausing is handled in OnEnable()
        this.enabled = false;
    }

    void DestroyPlatform(GameObject selectedPlatform)
    {
        Destroy(selectedPlatform);
        //Add effect for destroying platform
    }
}
