namespace MCUIFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    #region UI预定义
    /// <summary>
    /// UI类型: Nomal|Fixed|PopUp|Any
    /// Normal是大类, 其下又分为DoNothing|HideOther|NeedBack|NoNeedBack四小类
    /// </summary>
    public enum UIType
    {
        Normal = 1, //0001
        Fixed = 1<<1, // 0010
        Popup = 1<<2, // 0100
        Any = Normal | Fixed | Popup//独立窗口 0111
    }

    public enum UIMode 
    {
        DoNothing,//不关闭其他页面
        HideOther,//关闭其他窗口
        NeedBack,//点击返回按钮,不关闭其他页面
    
    }

    public enum UICollider 
    {
        None,//显示该页面不包含碰撞背景
        Normal,//碰撞透明背景
        WithBG//碰撞非透明背景
    }

    #endregion

    public abstract class UIPage:MyComponent
    {
		protected UIType type = UIType.Normal;
		protected UIMode mode = UIMode.DoNothing;
		protected UICollider uiCollider = UICollider.None;
		private string uiName = string.Empty;

		protected bool isActived = false;	//页面是否开启标志值
												
		private object m_data = null;	//页面数据字段
		protected object Data => m_data;	//页面数据只读属性

		protected abstract string UIPath { get; }

		private bool isAsyncUI = false;

		private static List<UIPage> m_currentPageNodes;//控制打开页面的层级

		private static Dictionary<string, UIPage> m_allPages;

		//delegate load UI function


		#region virtual api

		/// <summary>
		/// When Instance UI Ony Once.只会在初始化的时候执行一次
		/// </summary>
		protected virtual void OnAwake() { }

		///Show UI Refresh Eachtime.
		protected virtual void OnRefresh() { }

		/// <summary>
		/// 每次激活都会执行一次
		/// </summary>
		protected virtual void OnActive() { }

		protected virtual void OnHide() { }

		/// <summary>
		/// 当数据改变时，刷新UI组件显示
		/// </summary>
		public void Refresh()
		{
			OnRefresh();
		}

		/// <summary>
		/// 激活自身页面，做UI动画
		/// </summary>
		protected void Active()
		{
			this.uigameObject.SetActive(true);
			isActived = true;

			OnActive();
		}

		/// <summary>
		/// 失活自身页面，但不清理数据
		/// </summary>
		protected void Hide()
		{
			OnHide();

			this.uigameObject.SetActive(false);
			isActived = false;
			//set this page's data null when hide.
			this.m_data = null;

			Debug.Log($"页面[{this.uiName}]已关闭");
		}

		#endregion

		private UIPage() { }
		public UIPage(UIType type, UIMode mod, UICollider col) 
		{
			this.type = type;
			this.mode = mod;
			this.uiCollider = col;
			this.uiName = this.GetType().ToString();
			//TODO绑定页面
			//UIBind.Bind();
		}
		/// <summary>
		/// 使用Resources协程异步显示界面
		/// </summary>
		/// <param name="callback"></param>
		protected void AsyncShowUIWithResources(Action callback) 
		{
			UIRoot.Instance.StartCoroutine(AsyncShowUIenumerator(callback));
		}

		//383
		IEnumerator AsyncShowUIenumerator(Action callback) 
		{
			Debug.Log($"准备异步显示[{this.uiName}]页面=>Resources方式");
			if (this.uigameObject == null && string.IsNullOrEmpty(UIPath) == false)
            {
				GameObject go = null;
				var rs = Resources.LoadAsync(UIPath);
				yield return rs;
				go = GameObject.Instantiate(rs.asset as GameObject);
				AnchorUIGameObject(go);
				OnAwake();
				isAsyncUI = true;
				Active();
				Refresh();
				//TODO UI栈设定
				PushTop(this);
				if (callback != null)
				{
					callback();
				}
            }
            else 
			{
				Active();
				Refresh();
				PushTop(this);
				if (callback != null)
				{
					callback();
				}
			}
		}


		/// <summary>
		/// 使用Addressables异步显示界面
		/// </summary>
		protected void AsyncShowUIWithAddressables(Action callback) 
		{
			Debug.Log($"准备异步显示[{this.uiName}]页面=>Addressables方式");
            if (this.uigameObject == null && string.IsNullOrEmpty(this.uiName) == false)
            {
				GameObject go = null;
				float _t0 = Time.realtimeSinceStartup;
				Addressables.InstantiateAsync(this.uiName).Completed += (obj) =>
				{
                    if (obj.IsDone)
                    {
						go = obj.Result;
						//TODO UI定锚点
						AnchorUIGameObject(go);
						OnAwake();
						isAsyncUI = true;
						Active();
						Refresh();
						//TODO UI栈设定
						PushTop(this);
                        if (callback!=null)
                        {
							callback();
                        }
                    }
                    else
                    {
                        if (Time.realtimeSinceStartup - _t0 >= 10f)
                        {
							Debug.LogError("[UI] async load your UI prefab timeout, place check out");
							return;
                        }
                    }
				};
            }
            else
            {
				Active();
				Refresh();
				PushTop(this);
                if (callback != null)
                {
					callback();
                }
            }
		}

		/// <summary>
		/// 处理Page页面栈层级关系
		/// 根据枚举类型 
		/// ---NeedBack 将其在页面栈中置顶
		/// ---HideOther 将其他页面隐藏
		/// </summary>
		/// <param name="page"></param>
		private static void PushTop(UIPage page) 
		{
            if (m_currentPageNodes == null)
            {
				m_currentPageNodes = new List<UIPage>();
            }
            if (page == null)
            {
				Debug.LogError($"页面[{page.uiName}为空]");
				return;
            }
            if (page.BackOrHide() == false)
            {
				return;
            }
			//将page调到栈顶
			bool _isFound = false;
            for (int i = m_currentPageNodes.Count -1; i >=0 ; i--)
            {
                if (m_currentPageNodes[i].Equals(page))
                {
					m_currentPageNodes.RemoveAt(i);
					m_currentPageNodes.Add(page);
					_isFound = true;
					break;
                }
            }
            //若UI栈中没有该页面, 则是一个新页面, 将其入栈
            if (!_isFound)
            {
				m_currentPageNodes.Add(page);
            }
			//如果栈顶page的mode == UIMode.HideOther 则显示该栈顶元素的同时, 将栈中其他UIpage全部隐藏
			HideOldNodes();
		}

		/// <summary>
		/// 隐藏其他页面的方法
		/// </summary>
		private static void HideOldNodes() 
		{
            if (m_currentPageNodes.Count<0) 
            {
				return;
            }
			UIPage topage = m_currentPageNodes[m_currentPageNodes.Count - 1];
            if (topage.mode == UIMode.HideOther)
            {
                for (int i = m_currentPageNodes.Count-2; i >=0 ; i--)
                {
                    if (m_currentPageNodes[i].IsActive())
                    {
						m_currentPageNodes[i].Hide();
                    }
                }
            }
		}

		/// <summary>
		/// 获取页面是否是激活状态
		/// </summary>
		/// <returns></returns>
		public bool IsActive() 
		{
			bool ret = uigameObject != null && uigameObject.activeSelf;
			return ret || isActived;
		}


		/// <summary>
		/// internal访问权限标识同程序集下的类都可以使用
		/// </summary>
		/// <returns></returns>
		internal bool BackOrHide() 
		{
            if (type == UIType.Normal && (mode == UIMode.HideOther || mode == UIMode.NeedBack))
            {
				return true;
            }
			return false;
		}

		protected void AnchorUIGameObject(GameObject ui) 
		{
            if (UIRoot.Instance == null || ui == null)
            {
				Debug.LogError("UIRoot单例为空");
				return;
            }
			this.uigameObject = ui;
			this.uitransform = ui.transform;
			Vector3 anchorPos = Vector3.zero;
			Vector2 sizeDel = Vector2.zero;
			Vector3 scale = Vector3.one;
			if (ui.GetComponent<RectTransform>() != null) 
            {
				var uiRectTransform = ui.GetComponent<RectTransform>();
				anchorPos = uiRectTransform.anchoredPosition;
				sizeDel = uiRectTransform.sizeDelta;
				scale = uiRectTransform.localScale;
            }
            else
            {
				anchorPos = ui.transform.localPosition;
				scale = ui.transform.localScale;
            }

            if (type == UIType.Fixed)
            {
				ui.transform.SetParent(UIRoot.Instance.fixedRoot);
            }
            else if (type == UIType.Normal)
            {
				ui.transform.SetParent(UIRoot.Instance.normalRoot);
            }
            else if (type == UIType.Popup)
            {
				ui.transform.SetParent(UIRoot.Instance.popupRoot);
            }

            //Question:没看懂这步赋值操作, 上面取出来了, 然后又赋值一遍, 不知道在干嘛
			//Answer: 放到指定的节点处, 再根据储存的锚点定位
            if (ui.GetComponent<RectTransform>() != null)
            {
                ui.GetComponent<RectTransform>().anchoredPosition = anchorPos;
                ui.GetComponent<RectTransform>().sizeDelta = sizeDel;
                ui.GetComponent<RectTransform>().localScale = scale;
            }
            else
            {
                ui.transform.localPosition = anchorPos;
                ui.transform.localScale = scale;
            }

        }
		/// <summary>
		/// 使用Resources异步显示页面方法
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="callback"></param>
		private static void ShowPageAsyncWithResources<T>(Action callback = null) where T : UIPage, new()
		{
			_ShowPage<T>(false,null,callback);
		}


		/// <summary>
		/// 使用Addressables异步显示页面方法
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="callback"></param>
		private static void ShowPageAsyncWithAddressables<T>(Action callback = null) where T:UIPage,new() 
		{
			_ShowPage<T>(true,null,callback);
		}

		public static void _ShowPage<T>(bool isAddressable, object pageData = null, Action callback = null) where T : UIPage, new() 
		{
			string pageName = typeof(T).Name;
            if (m_allPages != null && m_allPages.ContainsKey(pageName))
            {
				_ShowPage(pageName, m_allPages[pageName], isAddressable,pageData, callback);
            }
            else 
			{
				T instance = new T();
				Debug.Log(instance);
				_ShowPage(pageName,instance,isAddressable,pageData,callback);
			}
		}
		private static void _ShowPage(string pageName, UIPage pageInstance, bool isAddressable,object pageData = null, Action callback = null) 
		{
            if (string.IsNullOrEmpty(pageName) || pageInstance == null)
            {
				Debug.LogError($"[UI] show page error with [{pageName}]" );
				return;
            }
            if (m_allPages == null)
            {
				m_allPages = new Dictionary<string, UIPage>();
            }
			UIPage page = null;
            if (m_allPages.ContainsKey(pageName))
            {
				page = m_allPages[pageName];
            }
            else 
			{
				m_allPages.Add(pageName,pageInstance);
				page = pageInstance;
			}
            if (pageData!= null)
            {
				page.m_data = pageData;
			}
			if (isAddressable)
            {
				page.AsyncShowUIWithAddressables(callback);
            }
            else 
			{
				//TODO 使用Resources目录异步加载
				page.AsyncShowUIWithResources(callback);
			}

		}


		/// <summary>
		/// 关闭页面栈的顶层页面
		/// 注意: 关闭页面时虽然从页面栈移除了页面对象, 但其仍然位于页面字典m_allPages中,在用于调用ShowPage方法时可以立即从缓存中取出
		/// Addressables 版本
		/// </summary>
		public static void ClosePage()
		{
            if (m_currentPageNodes == null || m_currentPageNodes.Count <1)
            {
				return;
            }

			UIPage clasePage = m_currentPageNodes[m_currentPageNodes.Count - 1];
			m_currentPageNodes.RemoveAt(m_currentPageNodes.Count-1);
			clasePage.Hide();

            //显示之前的page
            if (m_currentPageNodes.Count>0)
            {
				UIPage page = m_currentPageNodes[m_currentPageNodes.Count - 1];
				_ShowPage(page.uiName,page,true);
            }
		}

		/// <summary>
		/// 关闭指定页面
		/// Addressables版本
		/// </summary>
		public static void ClosePage(UIPage target, bool showPrevPage = true) 
		{
            if (target == null)
            {
				return;
            }
			//如果页面已经通过Hide()隐藏了, 将其从页面栈中移除即可返回
            if (target.IsActive() == false)
            {
                if (m_currentPageNodes != null)
                {
                    for (int i = m_currentPageNodes.Count-1; i >=0 ; i--)
                    {
                        if (m_currentPageNodes[i] == target)
                        {
							m_currentPageNodes.RemoveAt(i);
							break;
                        }
                    }
                }
				return;
            }
			//若当前页面时正在显示的栈顶页面, 则将其从页面栈移除, 并显示移除后的栈顶页面
            if (m_currentPageNodes != null && m_currentPageNodes.Count >=1 && m_currentPageNodes[m_currentPageNodes.Count-1] == target)
            {
				m_currentPageNodes.RemoveAt(m_currentPageNodes.Count-1);
                //显示之前的页面
                if (m_currentPageNodes.Count>0)
                {
					UIPage page = m_currentPageNodes[m_currentPageNodes.Count - 1];
                    if (page.isAsyncUI)
                    {
						if (showPrevPage)
						{
							_ShowPage(page.uiName, page, true, null, target.Hide);//委托是一种方法的类型, 需要传入一个方法, 不能传入方法的执行(返回值)
						}
						else 
						{
							target.Hide();
						}
                    }
					return;
                }
            }
            //如果目标页面不是栈顶页面, 且目标页面设置了返回或独占显示, 则将其从页面栈移除,并隐藏显示
            if (target.BackOrHide())
            {
                for (int i = m_currentPageNodes.Count-1; i >= 0; i--)
                {
                    if (m_currentPageNodes[i] == target)
                    {
						m_currentPageNodes.RemoveAt(i);
						break;
                    }
                }
            }
			target.Hide();
		}

		public static void ClosePage<T>() where T : UIPage 
		{
			string pageName = typeof(T).Name;
			ClosePage(pageName);
		}


		public static void ClosePage(string pageName) 
		{
            if (m_allPages!=null && m_allPages.ContainsKey(pageName))
            {
				ClosePage(m_allPages[pageName]);
            }
            else 
			{
				Debug.LogError($"页面[{pageName}尚未显示, 无法关闭!]");
			}
		}


		/// <summary>
		/// 关闭页面栈中所有页面
		/// </summary>
		public static void CloseAllPages(UIType uiType = UIType.Any) 
		{
            for (int i = m_allPages.Count-1; i >=0; i--)
            {
				var page = m_allPages.ElementAt(i);
                if (uiType == UIType.Any || uiType == page.Value.type)
                {
					ClosePage(page.Value,false);
                }
            }
		}

		public static Tpage GetPage<Tpage>() where Tpage: UIPage 
		{
			m_allPages.TryGetValue(typeof(Tpage).Name,out UIPage page);
			return page as Tpage;
		}










	}



	

}

