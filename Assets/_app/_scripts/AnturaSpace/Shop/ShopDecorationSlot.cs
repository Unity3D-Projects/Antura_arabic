﻿using System;
using DG.DeExtensions;
using UnityEngine;

namespace Antura.AnturaSpace
{
    public class ShopDecorationSlot : MonoBehaviour
    {
        public enum SlotHighlight
        {
            Idle,
            Correct,
            Wrong
        }

        public ShopDecorationSlotType slotType;
        private bool assigned = false;
        private ShopDecorationObject _assignedDecorationObject;

        private bool highlighted = false;
        public GameObject highlightMeshGO;

        public event Action<ShopDecorationSlot> OnSelect;

        public bool Assigned
        {
            get { return assigned; }
        }

        public ShopDecorationObject AssignedDecorationObject
        {
            get { return _assignedDecorationObject; }
        }

        #region Game Logic

        void Awake()
        {
            Highlight(false);
        }

        #endregion

        #region Assignment

        public void Free()
        {
            if (!assigned) return;
            assigned = false;
            //assignedDecorationObject.currentSlot = null;
            _assignedDecorationObject = null;
        }

        public void Assign(ShopDecorationObject assignedDecorationObject)
        {
            if (assigned) return;
            assigned = true;
            //assignedDecorationObject.currentSlot = this;
            _assignedDecorationObject = assignedDecorationObject;
            _assignedDecorationObject.transform.SetParent(transform);
            _assignedDecorationObject.transform.localEulerAngles = Vector3.zero;
            _assignedDecorationObject.transform.localPosition = Vector3.zero;
            _assignedDecorationObject.transform.SetLocalScale(1);
        }

        public bool IsFreeAndAssignableTo(ShopDecorationObject decorationObject)
        {
            return !assigned && IsAssignableTo(decorationObject);
        }

        public bool IsAssignableTo(ShopDecorationObject decorationObject)
        {
            return slotType == decorationObject.slotType;
        }

        public bool HasCurrentlyAssigned(ShopDecorationObject decorationObject)
        {
            return _assignedDecorationObject == decorationObject;
        }

        #endregion


        #region Highlight

        public Material slotHighlightIdleMat;
        public Material slotHighlightCorrectMat;
        public Material slotHighlightWrongMat;

        public void Highlight(bool choice, SlotHighlight slotHighlight =SlotHighlight.Idle)
        {
            highlighted = choice;
            highlightMeshGO.SetActive(choice);
            var renderer = highlightMeshGO.GetComponent<MeshRenderer>();
            switch (slotHighlight)
            {
                case SlotHighlight.Idle:
                    renderer.material = slotHighlightIdleMat;
                    break;
                case SlotHighlight.Correct:
                    renderer.material = slotHighlightCorrectMat;
                    break;
                case SlotHighlight.Wrong:
                    renderer.material = slotHighlightWrongMat;
                    break;
            }
        }

        #endregion

        public void OnMouseUpAsButton()
        {
            if (!highlighted) return;
            if (OnSelect != null) OnSelect.Invoke(this);
        }

    }
}