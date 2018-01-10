using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace UnityEngine.AssetBundles
{

    public class AssetBundleBrowserMain : EditorWindow, IHasCustomMenu, ISerializationCallbackReceiver
    {

        public const float kButtonWidth = 150;

        enum Mode
        {
            Browser,
            Builder,
            Inspect,
        }
        [SerializeField]
        Mode m_Mode;

        [SerializeField]
        int m_DataSourceIndex;

        [SerializeField]
        public AssetBundleManageTab m_ManageTab;

        [SerializeField]
        public AssetBundleBuildTab m_BuildTab;

        [SerializeField]
        public AssetBundleInspectTab m_InspectTab;

        private Texture2D m_RefreshTexture;

        const float k_ToolbarPadding = 15;
        const float k_MenubarPadding = 32;

        BuildMainData m_Data;

        [MenuItem("Window/AssetBundle Browser", priority = 2050)]
        static void ShowWindow()
        {
            var window = GetWindow<AssetBundleBrowserMain>();
            window.titleContent = new GUIContent("AssetBundles");
            window.Show();
        }

        [SerializeField]
        public bool multiDataSource = false;
        List<AssetBundleDataSource.ABDataSource> m_DataSourceList = null;
        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Custom Sources"), multiDataSource, FlipDataSource);
        }
        public void FlipDataSource()
        {
            multiDataSource = !multiDataSource;
        }

        private void OnEnable()
        {
            LoadData();

            Rect subPos = GetSubWindowArea();
            if(m_ManageTab == null)
                m_ManageTab = new AssetBundleManageTab();
            m_ManageTab.OnEnable(subPos, this);
            if(m_BuildTab == null)
                m_BuildTab = new AssetBundleBuildTab();
            m_BuildTab.OnEnable(subPos, this);
            if (m_InspectTab == null)
                m_InspectTab = new AssetBundleInspectTab();
            m_InspectTab.OnEnable(subPos, this);

            m_RefreshTexture = EditorGUIUtility.FindTexture("Refresh");
            
            InitDataSources();
        } 
        private void InitDataSources()
        {
            //determine if we are "multi source" or not...
            multiDataSource = false;
            m_DataSourceList = new List<AssetBundleDataSource.ABDataSource>();
            foreach (var info in AssetBundleDataSource.ABDataSourceProviderUtility.CustomABDataSourceTypes)
            {
                m_DataSourceList.AddRange(info.GetMethod("CreateDataSources").Invoke(null, null) as List<AssetBundleDataSource.ABDataSource>);
            }
             
            if (m_DataSourceList.Count > 1)
            {
                multiDataSource = true;

                if (!string.IsNullOrEmpty(m_Data.dataSource))
                {
                    for(int i = 0; i < m_DataSourceList.Count; ++i)
                    {
                        if (m_DataSourceList[i].Name.Equals(m_Data.dataSource,System.StringComparison.CurrentCultureIgnoreCase))
                        {
                            m_DataSourceIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    m_DataSourceIndex = 0;
                    m_Data.dataSource = m_DataSourceList[0].Name;
                }

                if (m_DataSourceIndex >= m_DataSourceList.Count)
                    m_DataSourceIndex = 0;
                AssetBundleModel.Model.DataSource = m_DataSourceList[m_DataSourceIndex];
            }
        }

        private void OnDisable()
        {
            SaveData();

            if (m_BuildTab != null)
                m_BuildTab.OnDisable();
            if (m_InspectTab != null)
                m_InspectTab.OnDisable();
        }

        public void OnBeforeSerialize()
        {
        }
        public void OnAfterDeserialize()
        {
            InitDataSources();
        }

        private Rect GetSubWindowArea()
        {
            float padding = k_MenubarPadding;
            if (multiDataSource)
                padding += k_MenubarPadding * 0.5f;
            Rect subPos = new Rect(0, padding, position.width, position.height - padding);
            return subPos;
        }

        private void Update()
        {
            switch (m_Mode)
            {
                case Mode.Builder:
                    break;
                case Mode.Inspect:
                    break;
                case Mode.Browser:
                default:
                    m_ManageTab.Update();
                    break;
            }
        }

        private void OnGUI()
        {
            ModeToggle();

            switch(m_Mode)
            {
                case Mode.Builder:
                    m_BuildTab.OnGUI(GetSubWindowArea());
                    break;
                case Mode.Inspect:
                    m_InspectTab.OnGUI(GetSubWindowArea());
                    break;
                case Mode.Browser:
                default:
                    m_ManageTab.OnGUI(GetSubWindowArea());
                    break;
            }
        }

        void ModeToggle()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(k_ToolbarPadding);
            bool clicked = false;
            switch(m_Mode)
            {
                case Mode.Browser:
                    clicked = GUILayout.Button(m_RefreshTexture);
                    if (clicked)
                        m_ManageTab.ForceReloadData();
                    break;
                case Mode.Builder:
                    GUILayout.Space(m_RefreshTexture.width + k_ToolbarPadding);
                    break;
                case Mode.Inspect:
                    clicked = GUILayout.Button(m_RefreshTexture);
                    if (clicked)
                        m_InspectTab.RefreshBundles();
                    break;
            }

            float toolbarWidth = position.width - k_ToolbarPadding * 4 - m_RefreshTexture.width;
            //string[] labels = new string[2] { "Configure", "Build"};
            string[] labels = new string[3] { "Configure", "Build", "Inspect" };
            m_Mode = (Mode)GUILayout.Toolbar((int)m_Mode, labels, "LargeButton", GUILayout.Width(toolbarWidth) );
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if(multiDataSource)
            {
                //GUILayout.BeginArea(r);
                GUILayout.BeginHorizontal();

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    GUILayout.Label("Bundle Data Source:");
                    GUILayout.FlexibleSpace();
                    var c = new GUIContent(string.Format("{0} ({1})", AssetBundleModel.Model.DataSource.Name, AssetBundleModel.Model.DataSource.ProviderName), "Select Asset Bundle Set");
                    if (GUILayout.Button(c , EditorStyles.toolbarPopup) )
                    {
                        GenericMenu menu = new GenericMenu();

                        for (int index = 0; index < m_DataSourceList.Count; index++)
                        {
                            var ds = m_DataSourceList[index];
                            if (ds == null)
                                continue;

                            if (index > 0)
                                menu.AddSeparator("");
                             
                            var counter = index;
                            menu.AddItem(new GUIContent(string.Format("{0} ({1})", ds.Name, ds.ProviderName)), false,
                                () =>
                                {
                                    var thisDataSource = ds;
                                    AssetBundleModel.Model.DataSource = thisDataSource;
                                    m_Data.dataSource = thisDataSource.Name;
                                    m_ManageTab.ForceReloadData();
                                }
                            );
                        }

                        menu.ShowAsContext();
                    }

                    GUILayout.FlexibleSpace();
                    if (AssetBundleModel.Model.DataSource.IsReadOnly())
                    {
                        GUIStyle tbLabel = new GUIStyle(EditorStyles.toolbar);
                        tbLabel.alignment = TextAnchor.MiddleRight;

                        GUILayout.Label("Read Only", tbLabel);
                    }
                }

                GUILayout.EndHorizontal();
                //GUILayout.EndArea();
            }
        }

        void SaveData()
        {
            var dataPath = System.IO.Path.GetFullPath(".");
            dataPath = dataPath.Replace("\\", "/");
            dataPath += "/"+AssetBundleConstans.MainSetting;

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(dataPath);

            bf.Serialize(file, m_Data);
            file.Close();
        }

        void LoadData()
        {
            var dataPath = System.IO.Path.GetFullPath(".");
            dataPath = dataPath.Replace("\\", "/");
            dataPath += "/"+AssetBundleConstans.MainSetting;

            if (File.Exists(dataPath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(dataPath, FileMode.Open);
                var data = bf.Deserialize(file) as BuildMainData;
                if (data != null)
                    m_Data = data;
                file.Close();
            }
        }

        [System.Serializable]
        public class BuildMainData
        {
            public string dataSource;
        }

    }
}
