// // using CardWar.Interfaces;
// // using UnityEngine;
// // using UnityEngine.EventSystems;

// namespace CardWar.Pointer
// {
// //     public class PointerInteract : MonoBehaviour
// //     {
// //         public IInteractable Interactable { get; set; }
// //         public bool IsPressed { get; set; }
// //         public bool IsHovering { get; set; }

// //         private float pressStartTime;
// //         private bool hasTriggeredHold;

// //         private Vector3 _pointerPos => Input.mousePosition;
// //         private Camera _mainCam => Camera.main;

// //         private void CheckHover()
// //         {
// //             Ray ray = _mainCam.ScreenPointToRay(_pointerPos);
// //             if (Physics.Raycast(ray, out RaycastHit hit))
// //             {
// //                 if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
// //                 {
// //             Debug.Log(1);
// //                     if (Interactable != interactable)
// //                     {
// //                         // rời khỏi object cũ
// //                         Interactable?.OnHoverExit(this);

// //                         // gán object mới
// //                         Interactable = interactable;
// //                         Interactable.OnHoverEnter(this);
// //                     }

// //                     IsHovering = true;
// //                     return;
// //                 }
// //             }

// //             // nếu không hit gì hoặc không phải IInteractable
// //             if (IsHovering && Interactable != null)
// //             {
// //                 Interactable.OnHoverExit(this);
// //                 Interactable = null;
// //             }

// //             IsHovering = false;
// //         }

// //         private void CheckPress()
// //         {
// //             if (Interactable == null) return;

// //             if (Input.GetKeyDown(KeyCode.Mouse0))
// //             {
// //             Debug.Log(2);
// //                 IsPressed = true;
// //                 pressStartTime = Time.time;
// //                 hasTriggeredHold = false;
// //             }

// //             if (IsPressed && !hasTriggeredHold)
// //             {
// //                 if (Time.time - pressStartTime >= 0.8f)
// //                 {
// //                     hasTriggeredHold = true;
// //                     Interactable.OnPressed(this);
// //                 }
// //             }

// //             if (Input.GetKeyUp(KeyCode.Mouse0))
// //             {
// //                 if (!hasTriggeredHold)
// //                 {
// //                     Interactable.OnClicked(this); 
// //                 }

// //                 IsPressed = false;
// //             }
// //         }

// //         void Update()
// //         {
// //             CheckHover();
// //             CheckPress();
// //         }
// //     }
//     using UnityEngine;
//     using UnityEngine.EventSystems;
//     using UnityEngine.UI;
//     using System.Collections.Generic;
//     using CardWar.Interfaces;
//     using UnityEngine.InputSystem;

//     public class PointerInteract : MonoBehaviour
//     {
//         public ISelectable currentInteractable;
//         public float holdThreshold = 0.8f;

//         private float pressStartTime;
//         private bool isHolding;
//         private bool hasTriggeredHold;

//         private GraphicRaycaster uiRaycaster;
//         private EventSystem eventSystem;

//         void Awake()
//         {
//             uiRaycaster = FindFirstObjectByType<GraphicRaycaster>();
//             eventSystem = EventSystem.current;
//         }

//         void Update()
//         {
//             CheckHover();
//             CheckPress();
//         }

//         private void CheckHover()
//         {
//             // --- 1. Check UI bằng GraphicRaycaster ---
//             PointerEventData pointerData = new PointerEventData(eventSystem)
//             {
//                 position = Input.mousePosition
//             };

//             List<RaycastResult> results = new List<RaycastResult>();
//             uiRaycaster.Raycast(pointerData, results);

//             if (results.Count > 0)
//             {
//                 var uiObj = results[0].gameObject.GetComponentInParent<ISelectable>();
//                 if (uiObj != null)
//                 {
//                     if (currentInteractable != uiObj)
//                     {
//                         currentInteractable?.OnHoverExit(this);
//                         currentInteractable = uiObj;
//                         currentInteractable.OnHoverEnter(this);
//                     }
//                     return; // Ưu tiên UI, không cần check 3D nữa
//                 }
//             }

//             // --- 2. Nếu không có UI -> check world object bằng Physics ---
//             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 var interactable = hit.collider.GetComponent<ISelectable>();
//                 if (interactable != null)
//                 {
//                     if (currentInteractable != interactable)
//                     {
//                         currentInteractable?.OnHoverExit(this);
//                         currentInteractable = interactable;
//                         currentInteractable.OnHoverEnter(this);
//                     }
//                     return;
//                 }
//             }

//             // --- 3. Nếu không trúng gì ---
//             currentInteractable?.OnHoverExit(this);
//             currentInteractable = null;
//         }

//         private void CheckPress()
//         {
//             if (currentInteractable == null) return;

//             if (Input.GetKeyDown(KeyCode.Mouse0))
//             {
//                 // Debug.Log(2);
//                 pressStartTime = Time.time;
//                 isHolding = true;
//                 hasTriggeredHold = false;
//             }

//             if (isHolding && !hasTriggeredHold)
//             {
//                 if (Time.time - pressStartTime >= holdThreshold)
//                 {
//                     hasTriggeredHold = true;
//                     currentInteractable.OnPressed(this);
//                 }
//             }

//             if (Input.GetKeyUp(KeyCode.Mouse0))
//             {
//                 if (!hasTriggeredHold)
//                 {
//                     currentInteractable.OnClicked(this);
//                 }

//                 isHolding = false;
//             }
//         }
//     }
// }
