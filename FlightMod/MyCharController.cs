using PiTung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

namespace FlightMod
{
    public class MyCharController : MonoBehaviour
    {
        public static MyCharController Instance;
        
        public MouseLook m_MouseLook;
        public Camera m_Camera;

        public float cameraSensitivity = 90;
        public float climbSpeed = 10;
        public float moveSpeed = 10;
        public float fastMoveFactor = 3;
        public float slowMoveFactor = 0.25f;

        public bool m_Enabled = true;
        private bool fixedYAxis = false;

        public void Start()
        {
            Instance = this;
        }

        public void Update()
        {
            if (ModInput.GetKeyDown("ToggleFlight"))
            {
                m_Enabled = !m_Enabled;
                FirstPersonController.Instance.enabled = !m_Enabled;
            }

            if (!m_Enabled)
                return;

            if (ModInput.GetKeyDown("ToggleAxisLock"))
            {
                fixedYAxis = !fixedYAxis;
            }

            this.m_MouseLook.LookRotation(base.transform, this.m_Camera.transform);
            this.m_MouseLook.UpdateCursorLock();

            float speedMult = 1;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                speedMult = fastMoveFactor;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                speedMult = slowMoveFactor;
            }
                        
            var trans = m_Camera.transform;

            float vertical = Input.GetAxisRaw("Vertical");
            float horizontal = Input.GetAxisRaw("Horizontal");

            Vector3 forward = new Vector3(trans.forward.x, fixedYAxis ? 0 : trans.forward.y, trans.forward.z);

            transform.position += forward * moveSpeed * speedMult * vertical * Time.deltaTime;
            transform.position += trans.right * moveSpeed * speedMult * horizontal * Time.deltaTime;

            if (Input.GetKey(KeyCode.Space))
                transform.position += Vector3.up * climbSpeed * speedMult * Time.deltaTime;
            if (Input.GetKey(KeyCode.C))
                transform.position -= Vector3.up * climbSpeed * speedMult * Time.deltaTime;
        }
    }
}
