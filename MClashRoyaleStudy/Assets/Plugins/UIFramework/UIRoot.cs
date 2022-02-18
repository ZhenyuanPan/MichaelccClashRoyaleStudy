namespace MCUIFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// 
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        private static UIRoot instance = null;
        public static UIRoot Instance 
        {
            get 
            {
                if (instance == null)
                {
                    InitRoot();
                    GameObject.DontDestroyOnLoad(instance.gameObject);
                }
                return instance;
            }
        }

        public Transform root;
        public Camera uiCamera;
        public Transform normalRoot;
        public Transform fixedRoot;
        public Transform popupRoot;
        //默认UI分辨率
        private static Vector2 resolution = new Vector2(900,1600);

        /// <summary>
        /// 设置UI分辨率, 适配游戏
        /// </summary>
        public static void SetInitResolutionParmas(Vector2 resolution) 
        {
            UIRoot.resolution = resolution;
        }

        /// <summary>
        /// 用来初始化UI根目录
        /// </summary>
        private static void InitRoot()
        {
            GameObject go = new GameObject("UIRoot");//设置根节点 UIRoot
            go.layer = LayerMask.NameToLayer("UI");
            instance = go.AddComponent<UIRoot>();//给UI添加脚本,并给字段赋值为该脚本(对象)
            go.AddComponent<RectTransform>();
            Canvas can = go.AddComponent<Canvas>();
            //2019UnityBUG, 不能选择相机模式
            //can.renderMode = RenderMode.ScreenSpaceCamera;
            can.renderMode = RenderMode.ScreenSpaceOverlay;
            can.pixelPerfect = true;
            go.AddComponent<GraphicRaycaster>();
            instance.root = go.transform; //m_UIRootInstance实例化对象持有根节点UIRoot引用
            GameObject camObj = new GameObject("UICamera");//设置相机模式的相机
            camObj.layer = LayerMask.NameToLayer("UI");
            camObj.transform.parent = go.transform;
            camObj.transform.localPosition = new Vector3(0,0,-100f);
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Depth;
            cam.orthographic = true;//正交摄像机
            cam.farClipPlane = 200f;
            can.worldCamera = cam;//给 canvas设置UIcamera
            cam.cullingMask = 1 << 5;//开启第5层也就是UI层
            cam.nearClipPlane = -50f;
            cam.farClipPlane = 50f;
            instance.uiCamera = cam;
            CanvasScaler cs = go.AddComponent<CanvasScaler>();//设置Canvas画板大小
            cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;//设置缩放, 这一块的坑, 一定要选这个才能保持UI缩放比例是对的
            cs.referenceResolution = resolution;
            print($"参考分辨率为：{cs.referenceResolution}");
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            //设置其他子节点 NormalRoot, FixedRoot, PopupRoot
            GameObject subRoot;
            subRoot = CreateSubCanvasForRoot(go.transform,0);
            subRoot.name = "NormalRoot";
            instance.normalRoot = subRoot.transform;
            instance.normalRoot.transform.localScale = Vector3.one;

            subRoot = CreateSubCanvasForRoot(go.transform,250);
            subRoot.name = "FixedRoot";
            instance.fixedRoot = subRoot.transform;
            instance.fixedRoot.transform.localScale = Vector3.one;

            subRoot = CreateSubCanvasForRoot(go.transform,500);
            subRoot.name = "PopupRoot";
            instance.popupRoot = subRoot.transform;
            instance.transform.localScale = Vector3.one;

            //添加EventSystem
            GameObject esObj = GameObject.Find("EventSystem");
            if (esObj != null) 
            {
                GameObject.DestroyImmediate(esObj);
            }
            GameObject eventObj = new GameObject("EventSystem");
            eventObj.layer = LayerMask.NameToLayer("UI");
            eventObj.transform.SetParent(go.transform);
            eventObj.AddComponent<EventSystem>();
            eventObj.AddComponent<StandaloneInputModule>();
        }

        /// <summary>
        /// 生成Canvas下的子节点
        /// </summary>
        /// <returns></returns>
        private static GameObject CreateSubCanvasForRoot(Transform root, int sort)
        {
            GameObject go = new GameObject();
            go.transform.parent = root;
            go.layer = LayerMask.NameToLayer("UI");
            RectTransform rect = go.AddComponent<RectTransform>();
            //代码设置锚点
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            return go;
        }

        private void OnDestroy()
        {
            instance = null;
        }
    }
}

