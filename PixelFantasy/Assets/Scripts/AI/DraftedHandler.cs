using System;
using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace AI
{
    public class DraftedHandler : MonoBehaviour
    {
        [SerializeField] private Kinling _kinling;
        [SerializeField] private Transform _draftedIndicator;
        
        private KinlingData _kinlingData => _kinling.RuntimeData;
        private KinlingAgent _agent => _kinling.KinlingAgent;
        private Order _currentOrder;

        public void SetDrafted(bool isDrafted)
        {
            if (!isDrafted)
            {
                ClearOrders();
            }
            
            ShowDraftedIndicator(isDrafted);
        }

        public void ShowDraftedIndicator(bool showIndicator)
        {
            _draftedIndicator.gameObject.SetActive(showIndicator);
        }

        private void Update()
        {
            if (!_kinlingData.IsDrafted) return;

            if (_currentOrder == null && _kinlingData.DraftedOrders.Count > 0)
            {
                _currentOrder = _kinlingData.DraftedOrders.Peek();
                StartOrder(_currentOrder);
            }
        }

        private void StartOrder(Order order)
        {
            switch (order.OrderType)
            {
                case EOrderType.Move:
                    StartMovementOrder(order);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StartMovementOrder(Order order)
        {
            Vector2 movePos = (Vector2)order.OrderData["MovePos"];
            _agent.SetMovePosition(movePos, OnOrderComplete);
        }

        private void OnOrderComplete()
        {
            _kinlingData.DraftedOrders.Dequeue();
            _currentOrder = null;
        }

        private void ClearOrders()
        {
            _kinlingData.DraftedOrders.Clear();
            _currentOrder = null;
            _agent.SetMovePosition(_kinling.transform.position);
        }

        public void AssignMoveOrder(Vector2 targetPosition)
        {
            var moveOrder = CreateMoveOrder(targetPosition);

            if (!SelectionManager.Instance.IncludeIsActive)
            {
                ClearOrders();
            }

            _kinlingData.DraftedOrders.Enqueue(moveOrder);
        }

        private Order CreateMoveOrder(Vector2 targetPosition)
        {
            var order = new Order
            {
                OrderType = EOrderType.Move,
            };
            
            order.OrderData.Add("MovePos", targetPosition);
            return order;
        }
    }

    [Serializable]
    public class Order
    {
        public EOrderType OrderType;
        public Dictionary<string, object> OrderData = new Dictionary<string, object>();
    }

    [Serializable]
    public enum EOrderType
    {
        Move,
    }
}
