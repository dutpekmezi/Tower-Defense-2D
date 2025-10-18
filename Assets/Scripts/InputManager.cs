using UnityEngine;
using System.Collections.Generic;

namespace dutpekmezi
{
    public class InputManager : MonoBehaviour
    {
        [Header("Camera")]
        public Camera cam;

        [Header("Throw Settings")]
        public float power = 12f;
        public float minSpeed = 4f;
        public float maxSpeed = 18f;
        public float minDragToThrow = 0.15f;

        [Header("Trajectory Settings")]
        public float minTimeStep = 0.04f;
        public float maxTimeStep = 0.12f;
        public float maxDotDistance = 0.3f;

        [Header("Dot Settings")]
        public GameObject dotPrefab;
        public int maxDots = 40;
        public int minDots = 6;
        public float minDotScale = 0.15f;
        public float maxDotScale = 0.25f;
        public float minScaleFactor = 0.6f;

        [Header("Start Gap (by speed)")]
        public float minStartGap = 0.2f;
        public float maxStartGap = 0.5f;

        [Header("Air Drag")]
        public float linearDrag = 1.2f;
        public float stopSpeed = 0.25f;

        // --- Internal ---
        private EntityBase currentEntity;
        private Collider2D currentCol;
        private SpriteRenderer currentSprite;
        private Vector2 dragStartWorld;
        private bool isDragging;

        private List<GameObject> dots = new List<GameObject>();

        void Awake()
        {
            if (!cam) cam = Camera.main;

            // Dot pool
            for (int i = 0; i < maxDots; i++)
            {
                var dot = Instantiate(dotPrefab);
                dot.SetActive(false);
                dots.Add(dot);
            }
        }

        void Update()
        {
            HandleSelectionAndThrow();
        }

        void HandleSelectionAndThrow()
        {
            Vector2 pointerPos = ScreenToWorld2D(Input.mousePosition);
            bool down = Input.GetMouseButtonDown(0);
            bool hold = Input.GetMouseButton(0);
            bool up = Input.GetMouseButtonUp(0);

            if (down)
            {
                RaycastHit2D hit = Physics2D.Raycast(pointerPos, Vector2.zero);
                if (hit.collider)
                {
                    SelectEntity(hit.collider);
                }

                if (currentEntity != null)
                {
                    dragStartWorld = pointerPos;
                    isDragging = true;
                }
            }

            if (hold && isDragging && currentEntity != null)
            {
                Vector2 dragVec = dragStartWorld - pointerPos;
                float dragDist = dragVec.magnitude;

                if (dragDist < minDragToThrow)
                {
                    ShowDots(false);
                    return;
                }

                Vector2 dir = dragVec.normalized;
                float rawSpeed = Mathf.Min(dragDist * power, maxSpeed);
                float pull01 = Mathf.Clamp01(rawSpeed / Mathf.Max(0.0001f, maxSpeed));

                float launchSpeed = Mathf.Lerp(minSpeed, maxSpeed, pull01);
                Vector2 v0 = dir * launchSpeed;
                float dt = Mathf.Lerp(minTimeStep, maxTimeStep, pull01);
                float startGap = Mathf.Lerp(minStartGap, maxStartGap, pull01);
                int visibleDots = Mathf.RoundToInt(Mathf.Lerp(minDots, maxDots, pull01));

                float scaleFactor = Mathf.Lerp(minScaleFactor, 1f, pull01);
                float currMaxScale = maxDotScale * scaleFactor;
                float currMinScale = minDotScale * scaleFactor;

                Vector3 startPos = (Vector2)currentCol.bounds.center + dir * startGap;
                DrawTrajectory(startPos, v0, visibleDots, dt, currMinScale, currMaxScale);
            }

            if (up && isDragging)
            {
                isDragging = false;

                if (currentEntity != null)
                {
                    Vector2 dragVec = dragStartWorld - pointerPos;
                    if (dragVec.magnitude < minDragToThrow)
                    {
                        ShowDots(false);
                        return;
                    }

                    Vector2 dir = dragVec.normalized;
                    float rawSpeed = Mathf.Min(dragVec.magnitude * power, maxSpeed);
                    float pull01 = Mathf.Clamp01(rawSpeed / Mathf.Max(0.0001f, maxSpeed));
                    float launchSpeed = Mathf.Lerp(minSpeed, maxSpeed, pull01);
                    Vector2 v0 = dir * launchSpeed;

                    // 👇 Launch through EntityBase
                    currentEntity.Launch(v0, linearDrag);
                }

                ShowDots(false);
                DeselectCurrent();
            }
        }

        void DrawTrajectory(Vector2 startPos, Vector2 startVel, int count, float dt, float currMinScale, float currMaxScale)
        {
            foreach (var d in dots) d.SetActive(false);
            Vector2 vel = startVel;
            Vector3 pos = startPos;
            Vector3 prevPos = pos;
            int placed = 0;

            while (placed < count && placed < maxDots)
            {
                vel -= vel * (linearDrag * dt);
                if (vel.magnitude < stopSpeed) vel = Vector2.zero;

                Vector2 disp = vel * dt;
                pos += (Vector3)disp;

                float seg = Vector3.Distance(prevPos, pos);
                if (seg > maxDotDistance)
                {
                    float adjust = maxDotDistance / seg;
                    pos = Vector3.Lerp(prevPos, pos, adjust);
                }
                prevPos = pos;

                float t = (count > 1) ? (float)placed / (count - 1) : 0f;
                float scale = Mathf.Lerp(currMaxScale, currMinScale, t);

                var dot = dots[placed];
                dot.SetActive(true);
                dot.transform.position = pos;
                dot.transform.localScale = Vector3.one * scale;

                placed++;
            }
        }

        void SelectEntity(Collider2D c)
        {
            if (currentCol != null && c != currentCol)
            {
                if (currentSprite) currentSprite.color = Color.white;
            }

            currentCol = c;
            currentSprite = c.GetComponent<SpriteRenderer>();
            currentEntity = c.GetComponentInParent<EntityBase>();

            if (currentSprite) currentSprite.color = Color.yellow;
        }

        void DeselectCurrent()
        {
            if (currentSprite) currentSprite.color = Color.white;
            currentCol = null;
            currentSprite = null;
            currentEntity = null;
        }

        void ShowDots(bool on)
        {
            foreach (var d in dots) d.SetActive(on);
        }

        Vector2 ScreenToWorld2D(Vector3 screenPos)
        {
            var w = cam.ScreenToWorldPoint(screenPos);
            return new Vector2(w.x, w.y);
        }
    }
}
