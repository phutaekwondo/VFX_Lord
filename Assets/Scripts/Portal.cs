using System;
using UnityEngine;
using UnityEngine.Rendering;
using RenderPipelineManager = UnityEngine.Rendering.RenderPipelineManager;

public class Portal : MonoBehaviour
{
    [SerializeField] Portal m_pairPortal;
    [SerializeField] Camera m_lookThroughCamera;
    [SerializeField] GameObject m_portalHole;
    [SerializeField] float nearClipOffset = 0.05f;
    // Collider m_collider;
    bool m_ableToTeleport = true;
    float nearClipLimit = 0.2f;
    float m_screenZOffset = 0.16f;
    float m_currentResolutionWidth;
    float m_currentResolutionHeight;
    Camera m_playerCam;

    private void Start() 
    {
        m_playerCam = Camera.main;
        if (m_pairPortal != null)
        {
            m_pairPortal.m_pairPortal = this;
        }
        // m_collider = GetComponent<Collider>();

        SetupTextureAndMaterial();
    }

    private void Update() 
    {
        if (m_currentResolutionWidth != Screen.width || m_currentResolutionHeight != Screen.height)
        {
            m_currentResolutionWidth = Screen.width;
            m_currentResolutionHeight = Screen.height;
            SetupTextureAndMaterial();
        }
    }
    

    private void OnEnable() 
    {
        RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
    }
    private void OnDisable() 
    {
        RenderPipelineManager.beginFrameRendering -= OnBeginFrameRendering;
    }

    private void OnBeginFrameRendering(ScriptableRenderContext context, Camera[] arg2)
    {
        SetHoleViewScreenToBehindTheGate();
        UpdateCameraTransform();
    }

    //Teleport
    private void OnTriggerEnter(Collider other) 
    {
        if (other.tag == "Player" && m_ableToTeleport)
        {
            //set up destination portal
            m_pairPortal.m_ableToTeleport = false;
            m_pairPortal.SetEnableHoleScreen(false);

            //tele the player to there
            Vector3 telePos = GetTeleportPosition(other.transform.position);
            Vector3 teleForw = GetTeleportForward(other.transform.forward);
            other.transform.position = telePos;
            other.transform.forward = teleForw;
        }
    }
    private void OnTriggerExit(Collider other) 
    {
        if (other.tag == "Player" && !m_ableToTeleport)
        {
            m_ableToTeleport = true;
            m_pairPortal.m_ableToTeleport = true;
            m_pairPortal.SetEnableHoleScreen(true);
        }
    }

    public void SetEnableHoleScreen(bool enable)
    {
        //if player enter hole, the hole should be disable til player exit the collision 
        m_portalHole.SetActive(enable);
        // m_collider.enabled = enable;
        if (m_pairPortal != null)
        {
            m_pairPortal.m_portalHole.SetActive(enable);
            // m_pairPortal.m_collider.enabled = enable;
        }
    }

    public Vector3 GetTeleportPosition(Vector3 position)
    {
        Transform thisTrans = this.transform;
        Vector2 A = new Vector2(thisTrans.position.x, thisTrans.position.z);
        Vector2 C = new Vector2(position.x, position.z);
        Vector2 F = new Vector2(thisTrans.forward.x, thisTrans.forward.z);
        F.Normalize();

        Vector2 pairPos2D = new Vector2(m_pairPortal.transform.position.x,m_pairPortal.transform.position.z);
        Vector2 pairForw2D = new Vector2(m_pairPortal.transform.forward.x,m_pairPortal.transform.forward.z);
        pairForw2D.Normalize();

        float angle = Vector2.SignedAngle(F, C-A);
        Vector2 dir = Quaternion.Euler(0, 0, angle) * pairForw2D;
        Vector2 relativePos = pairPos2D + (dir * Vector2.Distance(A, C));

        return new Vector3(relativePos.x, position.y, relativePos.y);
    }

    public Vector3 GetTeleportForward(Vector3 forward)
    {
        Vector2 thisForw2D = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 pairPortalForw2D = new Vector2(m_pairPortal.transform.forward.x, m_pairPortal.transform.forward.z);
        Vector2 forw2D = new Vector2(forward.x, forward.z);

        float angle = Vector2.SignedAngle(thisForw2D, forw2D);
        Vector2 dir = Quaternion.Euler(0, 0, angle) * thisForw2D;
        dir *= forw2D.magnitude/dir.magnitude;

        return new Vector3(dir.x, forward.y, dir.y);
    }

    public Matrix4x4 PairPortalRelavetiveMatrix(Transform from)
    {
        return this.transform.localToWorldMatrix * m_pairPortal.transform.worldToLocalMatrix * from.localToWorldMatrix;
    }


    //Camera
    private void SetHoleViewScreenToBehindTheGate()
    {
        //make sure the hole view screen always behind the gate from the player's perspective
        Vector3 thisForward = transform.forward;
        Vector3 this2Player = m_playerCam.transform.position - transform.position;
        Vector3 m_portalHoleLocalPos = m_portalHole.transform.localPosition;
        float dot = Vector3.Dot(thisForward, this2Player);
        if (dot > 0)
        {
            m_portalHole.transform.localPosition = new Vector3(m_portalHoleLocalPos.x, m_portalHoleLocalPos.y, -m_screenZOffset);
        }
        else
        {
            m_portalHole.transform.localPosition = new Vector3(m_portalHoleLocalPos.x, m_portalHoleLocalPos.y, m_screenZOffset);
        }
    }

    private void UpdateCameraTransform()
    {
        Matrix4x4 relative = PairPortalRelavetiveMatrix(m_playerCam.transform);

        //update the camera position and rotation
        m_lookThroughCamera.transform.SetPositionAndRotation(relative.GetColumn(3), relative.rotation);

        //update the projection matrix
        UpdateCameraClipPlane(m_playerCam);
    }

    private void UpdateCameraClipPlane(Camera viewCamera)
    {
        // m_lookThroughCamera.projectionMatrix = viewCamera.projectionMatrix;
        // return;
        Transform clipPlane = transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - m_lookThroughCamera.transform.position));

        Vector3 camSpacePos = m_lookThroughCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = m_lookThroughCamera.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;

        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        if (Math.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            m_lookThroughCamera.projectionMatrix = viewCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            m_lookThroughCamera.projectionMatrix = viewCamera.projectionMatrix;
        }
    }

    //Texture and Material
    private void SetupTextureSize()
    {
        if (m_lookThroughCamera.targetTexture != null)
        {
            m_lookThroughCamera.targetTexture.Release();
        }
        m_lookThroughCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 32);
    }

    private void SetupTextureAndMaterial()
    {
        SetupTextureSize();
        //generate material and render it to the pair portal

        //generate material that contain render texture from this portal camera
        Material material = new Material(Shader.Find("Shader Graphs/PortalShaderGraph"));

        if (material == null)
        {
            Debug.LogError("Portal material is null");
            return;
        }

        //set the render texture to the material
        material.SetTexture("_PortalTexture", m_lookThroughCamera.targetTexture);

        //set material to the pair portal
        m_pairPortal.SetTheMaterialForHoleView(material);
    }

    private void SetTheMaterialForHoleView(Material material)
    {
        m_portalHole.GetComponent<MeshRenderer>().material = material;
    }

}
