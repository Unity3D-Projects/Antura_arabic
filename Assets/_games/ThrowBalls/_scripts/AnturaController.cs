﻿using UnityEngine;
using System.Collections;

namespace EA4S.ThrowBalls
{
    public class AnturaController : MonoBehaviour
    {
        public static AnturaController instance;

        private const float RUNNING_SPEED = 15f;
        private const float JUMP_INIT_VELOCITY = 60f;

        private Vector3 velocity;
        private Vector3 jumpPoint;
        private AnturaAnimationController animator;
        private bool jumped;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            animator = GetComponent<AnturaAnimationController>();


            Disable();
        }

        public void EnterScene()
        {
            if (!gameObject.activeSelf)
            {
                Enable();
            }

            Vector3 ballPosition = BallController.instance.transform.position;
            Vector3 anturaPosition = ballPosition;
            anturaPosition.y = GroundController.instance.transform.position.y + 0.1f;

            float frustumHeight = 2.0f * Mathf.Abs(anturaPosition.z - Camera.main.transform.position.z) * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * Camera.main.aspect;

            anturaPosition.x = frustumWidth / 2 + 2f;

            if (Random.Range(0, 40) % 2 == 0)
            {
                anturaPosition.x *= -1;
            }

            velocity.x = ballPosition.x - anturaPosition.x;
            velocity.Normalize();
            velocity *= RUNNING_SPEED;

            if (velocity.x > 0)
            {
                transform.rotation = Quaternion.Euler(0, 270, 0);
            }

            else
            {
                transform.rotation = Quaternion.Euler(0, 90, 0);
            }

            transform.position = anturaPosition;
            jumpPoint = anturaPosition;

            float velocityFactor = (-1 * JUMP_INIT_VELOCITY) - Mathf.Sqrt(Mathf.Pow(JUMP_INIT_VELOCITY, 2) - (2 * (anturaPosition.y - ballPosition.y) * 0.5f * Constants.GRAVITY.y));
            velocityFactor = Mathf.Pow(velocityFactor, -1);
            velocityFactor *= Constants.GRAVITY.y;

            jumpPoint.x = (ballPosition.x + 5f * Mathf.Sign(velocity.x)) - (velocity.x / velocityFactor);

            jumped = false;
        }

        void Update()
        {
            Vector3 position = transform.position;
            position += velocity * Time.deltaTime;
            transform.position = position;

            if ((jumpPoint.x - position.x) * velocity.x <= 0 && !jumped)
            {
                velocity.y = JUMP_INIT_VELOCITY;

                jumped = true;
            }

            if (IsOffScreen() && velocity.x * transform.position.x > 0)
            {
                Disable();
            }
        }

        private bool IsOffScreen()
        {
            float frustumHeight = 2.0f * Mathf.Abs(transform.position.z - Camera.main.transform.position.z) * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * Camera.main.aspect;
            float halfFrustumWidth = frustumWidth / 2;

            if (Mathf.Abs(transform.position.x) - 6f > halfFrustumWidth)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public void Reset()
        {
            velocity = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        public void Enable()
        {
            gameObject.SetActive(true);
            animator.State = AnturaAnimationStates.walking;
            animator.SetWalkingSpeed(1f);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}