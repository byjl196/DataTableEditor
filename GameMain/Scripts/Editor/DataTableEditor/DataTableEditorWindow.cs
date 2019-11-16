﻿//------------------------------------------------------------
//
//@Author : Jrimmmmmrz
//
//@Verison : 
//
//@Description : 数据表格编辑器窗口
//
//------------------------------------------------------------
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System;
using UnityGameFramework.Editor.DataTableTools;
using UnityEditorInternal;
using System.IO;

namespace GameFramework.DataTableTools
{
    /// <summary>
    /// 启动窗口
    /// </summary>
    public class DataTableEditorLaunchWindow : EditorWindow
    {

        private static Vector2 windowSize = new Vector2(300, 400);
        public static Rect WindowRect;
        GUIStyle fontStyle = new GUIStyle();
        GUIStyle titleStyle = new GUIStyle();

        [MenuItem("Game Framework/DataTable Editor")]
        public static void OpenWindow()
        {
            WindowRect = new Rect((Screen.currentResolution.width / 2) - (windowSize.x / 2), (Screen.currentResolution.height / 2) - (windowSize.y / 2), windowSize.x, windowSize.y);
            var window = DataTableEditorLaunchWindow.GetWindowWithRect<DataTableEditorLaunchWindow>(WindowRect, true, DataTableEditorConfig.GetConfig().WindowTitle);
            window.minSize = windowSize;
            window.maxSize = windowSize;
            window.ShowUtility();
        }

        public static void OpenWindow(Vector2 position)
        {
            Rect rect = new Rect(WindowRect);
            rect.position = position;
            var window = DataTableEditorLaunchWindow.GetWindowWithRect<DataTableEditorLaunchWindow>(rect, true, DataTableEditorConfig.GetConfig().WindowTitle);
            window.minSize = windowSize;
            window.maxSize = windowSize;
            window.position = rect;
            window.ShowUtility();
        }

        /// <summary>
        /// 新建表格
        /// </summary>
        private void CreateDataTable()
        {

            OSPlatformWindow.OpenFileName openFileName = new OSPlatformWindow.OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            openFileName.filter = DataTableEditorConfig.Filter;//文件格式过滤
            openFileName.file = new string(new char[256]);
            openFileName.maxFile = openFileName.file.Length;
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
            openFileName.title = "Save";
            openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

            if (OSPlatformWindow.LocalDialog.GetSaveFileName(openFileName))
            {
                this.Close();
                DataTableEditorWindow.SaveWindow(openFileName.fileTitle.Replace(".txt", ""), this.position.position);
            }
        }

        /// <summary>
        /// 打开表格
        /// </summary>
        private void OpenDataTable()
        {

            OSPlatformWindow.OpenFileName openFileName = new OSPlatformWindow.OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            openFileName.filter = DataTableEditorConfig.Filter;
            openFileName.file = new string(new char[256]);
            openFileName.maxFile = openFileName.file.Length;
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
            openFileName.title = "Load";
            openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

            if (OSPlatformWindow.LocalDialog.GetSaveFileName(openFileName))
            {
                this.Close();
                DataTableEditorWindow.OpenWindow(openFileName.fileTitle.Replace(".txt", ""), this.position.position);
            }
        }


        void OnGUI()
        {
            //居中样式
            fontStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            fontStyle.fontSize = 10;
            titleStyle.fontSize = 20;


            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            GUILayout.Label(DataTableEditorConfig.GetConfig().MainTitle, titleStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(DataTableEditorConfig.GetConfig().SubTitle, fontStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(30);

            //按钮
            if (GUILayout.Button(DataTableEditorConfig.GetConfig().New, GUILayout.Height(50)))
            {
                CreateDataTable();
            }

            if (GUILayout.Button(DataTableEditorConfig.GetConfig().Load, GUILayout.Height(50)))
            {
                OpenDataTable();
            }

            if (GUILayout.Button(DataTableEditorConfig.GetConfig().Setting, GUILayout.Height(50)))
            {
                this.Close();
                DataTableEditorSettingWindow.OpenWindow(this.position.position);
            }

            if (GUILayout.Button(DataTableEditorConfig.GetConfig().Generate, GUILayout.Height(50)))
            {
                this.Close();
                DataTableGenerateWindow.OpenWindow(this.position.position);
            }

            GUILayout.Space(60);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Author：Jrimmmmmrz", fontStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label(DataTableEditorConfig.GetConfig().Verision, fontStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

        }
    }

    /// <summary>
    /// 编辑窗口
    /// </summary>
    public class DataTableEditorWindow : EditorWindow
    {
        private class Row
        {
            public List<string> RowData = new List<string>();
        }

        private static List<Row> rows;

        private static float TextFieldWidth = 80f;
        private static float TextFieldHeight = 30f;

        private static string m_fileName;
        private static DataTableProcessor m_tableProcessor;

        /// <summary>
        /// 打开编辑窗口
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="position">位置</param>
        public static void OpenWindow(string fileName, Vector2 position)
        {
            m_fileName = fileName;
            m_tableProcessor = DataTableGenerator.CreateDataTableProcessor(fileName);
            if (!DataTableGenerator.CheckRawData(m_tableProcessor, fileName))
            {
                Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", fileName));
                return;
            }
            LoadDataTable();

            Rect rect = new Rect(DataTableEditorLaunchWindow.WindowRect);
            rect.position = position;
            var window = DataTableEditorWindow.GetWindowWithRect<DataTableEditorWindow>(rect, true, DataTableEditorConfig.GetConfig().WindowTitle);
            window.position = rect;
            window.minSize = new Vector2(m_tableProcessor.RawColumnCount * (TextFieldWidth + 4) + 5, m_tableProcessor.RawRowCount * (TextFieldHeight + 2) + 215);
            window.maxSize = new Vector2(m_tableProcessor.RawColumnCount * (TextFieldWidth + 4) + 5, m_tableProcessor.RawRowCount * (TextFieldHeight + 2) + 215);
            window.ShowUtility();

        }

        /// <summary>
        /// 用于刷新窗口
        /// </summary>
        /// <param name="position">位置</param>
        public static void OpenWindow(Vector2 position)
        {
            Rect rect = new Rect(DataTableEditorLaunchWindow.WindowRect);
            rect.position = position;
            var window = DataTableEditorWindow.GetWindowWithRect<DataTableEditorWindow>(rect, true, DataTableEditorConfig.GetConfig().WindowTitle);
            window.position = rect;
            window.minSize = new Vector2(rows[0].RowData.Count * (TextFieldWidth + 4) + 5, rows.Count * (TextFieldHeight + 2) + 215);
            window.maxSize = new Vector2(rows[0].RowData.Count * (TextFieldWidth + 4) + 5, rows.Count * (TextFieldHeight + 2) + 215);
            window.ShowUtility();
        }

        /// <summary>
        /// 新建
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="position">窗户位置</param>
        public static void SaveWindow(string fileName, Vector2 position)
        {
            m_fileName = fileName;

            rows = new List<Row>();
            Row row1 = new Row();
            Row row2 = new Row();
            Row row3 = new Row();
            Row row4 = new Row();
            Row row5 = new Row();

            row1.RowData = new List<string>()
            {
                "#","默认配置",""
            };
            row2.RowData = new List<string>()
            {
                "#","ID",""
            };
            row3.RowData = new List<string>()
            {
                "#","int",""
            };
            row4.RowData = new List<string>()
            {
                "#","Id",""
            };
            row5.RowData = new List<string>()
            {
                "","0",""
            };

            rows.Add(row1);
            rows.Add(row2);
            rows.Add(row3);
            rows.Add(row4);
            rows.Add(row5);

            FileStream file = new FileStream(Path.Combine(DataTableGenerator.DataTablePath, fileName + ".txt"), FileMode.CreateNew);

            string line = "";
            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; j < rows[i].RowData.Count; j++)
                {
                    //保留第一行注释以及制表符
                    if (rows[i].RowData[j] != "#" && j != rows[i].RowData.Count - 1)
                    {
                        line += rows[i].RowData[j] + "\t";
                        continue;
                    }

                    //保留原来的数据
                    line += rows[i].RowData[j];
                    if (string.IsNullOrEmpty(rows[i].RowData[j]))
                    {
                        line += "\t";
                    }

                    //制表符会被跳过，而且行的尾部不添加
                    if (j != rows[i].RowData.Count - 1)
                    {
                        line += "\t";
                    }
                }
                if (i != rows.Count - 1)
                {
                    line += "\n";
                }
            }
            byte[] bts = System.Text.Encoding.Unicode.GetBytes(line);

            file.Write(bts, 0, bts.Length);
            file.Flush();
            file.Close();
            file.Dispose();

            OpenWindow(m_fileName, position);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 保存数据表文件
        /// </summary>
        private void SaveDataTable()
        {
            FileStream file = new FileStream(Path.Combine(DataTableGenerator.DataTablePath, m_fileName + ".txt"), FileMode.Create);

            string line = "";
            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; j < rows[i].RowData.Count; j++)
                {
                    //保留第一行注释以及制表符
                    if (rows[i].RowData[j] != "#" && j != rows[i].RowData.Count - 1)
                    {
                        line += rows[i].RowData[j] + "\t";
                        continue;
                    }

                    //保留原来的数据
                    line += rows[i].RowData[j];

                    //制表符会被跳过，而且行的尾部不添加
                    if (j != rows[i].RowData.Count - 1)
                    {
                        line += "\t";
                    }
                }
                if (i != rows.Count - 1)
                {
                    line += "\n";
                }
            }
            byte[] bts = System.Text.Encoding.Unicode.GetBytes(line);
            file.Write(bts, 0, bts.Length);
            file.Flush();
            file.Close();
            file.Dispose();

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 加载数据表文件
        /// </summary>
        private static void LoadDataTable()
        {
            rows = new List<Row>();

            for (int i = 0; i < m_tableProcessor.RawRowCount; i++)
            {
                Row newRow = new Row();
                for (int j = 0; j < m_tableProcessor.RawColumnCount; j++)
                {
                    newRow.RowData.Add(m_tableProcessor.GetValue(i, j));
                }
                rows.Add(newRow);
            }
        }

        private void OnGUI()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < rows[i].RowData.Count; j++)
                {
                    rows[i].RowData[j] = GUILayout.TextField(rows[i].RowData[j], GUILayout.Width(TextFieldWidth), GUILayout.Height(TextFieldHeight));
                }
                GUILayout.EndHorizontal();
            }

            //添加列按钮
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(DataTableEditorConfig.GetConfig().AddColumn, GUILayout.Height(50)))
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    rows[i].RowData.Add("");
                }
                ReOpenWindow();
            }
            if (GUILayout.Button(DataTableEditorConfig.GetConfig().RemoveColumn, GUILayout.Height(50)))
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    rows[i].RowData.RemoveAt(rows[i].RowData.Count - 1);
                }
                ReOpenWindow();
            }
            GUILayout.EndHorizontal();

            //添加行按钮
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(DataTableEditorConfig.GetConfig().AddRow, GUILayout.Height(50)))
            {
                Row row = new Row();
                for (int i = 0; i < rows[0].RowData.Count; i++)
                {
                    row.RowData.Add("");
                }
                rows.Add(row);
                ReOpenWindow();
            }
            if (GUILayout.Button(DataTableEditorConfig.GetConfig().RemoveRow, GUILayout.Height(50)))
            {
                rows.RemoveAt(rows.Count - 1);
                ReOpenWindow();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button(DataTableEditorConfig.GetConfig().Apply, GUILayout.Height(50)))
            {
                this.Close();
                DataTableEditorLaunchWindow.OpenWindow(this.position.position);
                SaveDataTable();
            }
            if (GUILayout.Button(DataTableEditorConfig.GetConfig().Back, GUILayout.Height(50)))
            {
                this.Close();
                DataTableEditorLaunchWindow.OpenWindow(this.position.position);
            }
        }

        private void ReOpenWindow()
        {
            this.Close();
            OpenWindow(this.position.position);
        }
    }

    /// <summary>
    /// 生成窗口
    /// </summary>
    public class DataTableGenerateWindow : EditorWindow
    {
        [System.Serializable]
        public class DataTableName
        {
            [SerializeField] public string Name = "";
            [SerializeField] public bool IsOn = false;
        }

        private static Vector2 windowSize = new Vector2(300, 400);

        public static ReorderableList m_datatableNames;

        /// <summary>
        /// 打开生成窗口
        /// </summary>
        /// <param name="position">窗口位置</param>
        public static void OpenWindow(Vector2 position)
        {
            Rect rect = new Rect(DataTableEditorLaunchWindow.WindowRect);
            rect.position = position;
            var window = DataTableGenerateWindow.GetWindowWithRect<DataTableGenerateWindow>(DataTableEditorLaunchWindow.WindowRect, true, DataTableEditorConfig.GetConfig().WindowTitle);
            window.minSize = windowSize;
            window.maxSize = windowSize;
            window.position = rect;
            window.ShowUtility();
        }

        private void CheckNull()
        {
            if (m_datatableNames == null)
            {
                m_datatableNames = new ReorderableList(DataTableEditorConfig.GetConfig().DataTableNamesList, typeof(DataTableName), true, false, true, true);
                m_datatableNames.elementHeight = 40;
                m_datatableNames.drawElementCallback = (rectt, index, isActive, isFocused) =>
                {
                    DataTableEditorConfig.GetConfig().DataTableNamesList[index].Name = EditorGUI.TextField(rectt, "Data Table Name", DataTableEditorConfig.GetConfig().DataTableNamesList[index].Name);

                    rectt.position = new Vector2(20, rectt.y + 20);
                    rectt.size = new Vector2(rectt.width, 20);

                    if (DataTableEditorConfig.GetConfig().DataTableNamesList[index].IsOn)
                    {
                        DataTableEditorConfig.GetConfig().DataTableNamesList[index].IsOn = EditorGUI.ToggleLeft(rectt, "On", DataTableEditorConfig.GetConfig().DataTableNamesList[index].IsOn);
                    }
                    else
                    {
                        DataTableEditorConfig.GetConfig().DataTableNamesList[index].IsOn = EditorGUI.ToggleLeft(rectt, "Off", DataTableEditorConfig.GetConfig().DataTableNamesList[index].IsOn);
                    }
                };

                m_datatableNames.drawHeaderCallback = (rectt) =>
                    EditorGUI.LabelField(rectt, "Data Table");

                m_datatableNames.onAddCallback = (list) =>
                {
                    DataTableName dataTable = new DataTableName();
                    dataTable.IsOn = false;
                    dataTable.Name = "";
                    DataTableEditorConfig.GetConfig().DataTableNamesList.Add(dataTable);
                };
            }
        }

        private void OnGUI()
        {

            GUILayout.Space(10);

            CheckNull();
            m_datatableNames.DoLayoutList();

            GUILayout.Space(10);

            if (GUILayout.Button(DataTableEditorConfig.GetConfig().Generate, GUILayout.Height(50)))
            {
                DataTableGeneratorMenu.GenerateDataTables(DataTableEditorConfig.GetConfig().DataTableNamesList);
            }

            if (GUILayout.Button(DataTableEditorConfig.GetConfig().Back, GUILayout.Height(50)))
            {
                this.Close();
                DataTableEditorLaunchWindow.OpenWindow(this.position.position);
            }
        }

    }

    /// <summary>
    /// 设置窗口
    /// </summary>
    public class DataTableEditorSettingWindow : EditorWindow
    {

        private static Vector2 windowSize = new Vector2(300, 400);

        private static int m_datatableIDNameRow = 1;
        private static int m_datatableIDTypeRow = 2;
        private static int m_datatableCommentRow = 3;
        private static int m_datatableCommentStartRow = 3;
        private static int m_datatableIdColumn = 3;

        private static string m_datatablePath = "";
        private static string m_CSharpCodePath = "";
        private static string m_CSharpCodeTemplateFileName = "";
        private static string m_namespace = "";

        /// <summary>
        /// 打开设置窗口
        /// </summary>
        /// <param name="position">窗口位置</param>
        public static void OpenWindow(Vector2 position)
        {

            GetProcessorData();

            Rect rect = new Rect(DataTableEditorLaunchWindow.WindowRect);
            rect.position = position;
            var window = DataTableEditorSettingWindow.GetWindowWithRect<DataTableEditorSettingWindow>(rect, true, DataTableEditorConfig.GetConfig().Setting);
            window.minSize = windowSize;
            window.maxSize = windowSize;
            window.position = rect;
            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Label(DataTableEditorConfig.GetConfig().IDNameRow, GUILayout.Width(120));

            m_datatableIDNameRow = EditorGUILayout.IntField(m_datatableIDNameRow, GUILayout.Width(60));

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Label(DataTableEditorConfig.GetConfig().IDTypeRow, GUILayout.Width(120));

            m_datatableIDTypeRow = EditorGUILayout.IntField(m_datatableIDTypeRow, GUILayout.Width(60));

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Label(DataTableEditorConfig.GetConfig().CommentRow, GUILayout.Width(120));

            m_datatableCommentRow = EditorGUILayout.IntField(m_datatableCommentRow, GUILayout.Width(60));

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Label(DataTableEditorConfig.GetConfig().ContentStartRow, GUILayout.Width(120));

            m_datatableCommentStartRow = EditorGUILayout.IntField(m_datatableCommentStartRow, GUILayout.Width(60));

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Label(DataTableEditorConfig.GetConfig().IDColumn, GUILayout.Width(120));

            m_datatableIdColumn = EditorGUILayout.IntField(m_datatableIdColumn, GUILayout.Width(60));

            GUILayout.EndHorizontal();


            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Label(DataTableEditorConfig.GetConfig().Language, GUILayout.Width(120));

            DataTableEditorConfig.DefaultLanguage = EditorGUILayout.Popup(DataTableEditorConfig.DefaultLanguage, DataTableEditorConfig.LanguageFlags);

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            m_namespace = EditorGUILayout.TextField(DataTableEditorConfig.GetConfig().NameSpace, m_namespace);

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            m_datatablePath = EditorGUILayout.TextField(DataTableEditorConfig.GetConfig().DataTablePath, m_datatablePath);

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            m_CSharpCodePath = EditorGUILayout.TextField(DataTableEditorConfig.GetConfig().CSharpCodePath, m_CSharpCodePath);

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            m_CSharpCodeTemplateFileName = EditorGUILayout.TextField(DataTableEditorConfig.GetConfig().CSharpCodeTemplateFileName, m_CSharpCodeTemplateFileName);

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button(DataTableEditorConfig.GetConfig().Apply, GUILayout.Height(50)))
            {
                this.Close();
                DataTableEditorLaunchWindow.OpenWindow(this.position.position);
                SetProcessorData();
            }

            if (GUILayout.Button(DataTableEditorConfig.GetConfig().Back, GUILayout.Height(50)))
            {
                this.Close();
                DataTableEditorLaunchWindow.OpenWindow(this.position.position);
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        private static void GetProcessorData()
        {
            m_datatableIDNameRow = DataTableProcessor.NameRow;
            m_datatableIDTypeRow = DataTableProcessor.TypeRow;
            m_datatableCommentRow = DataTableProcessor.CommentRow;
            m_datatableCommentStartRow = DataTableProcessor.ContentStartRow;
            m_datatableIdColumn = DataTableProcessor.IdColumn;
            m_datatablePath = DataTableGenerator.DataTablePath;
            m_CSharpCodePath = DataTableGenerator.CSharpCodePath;
            m_CSharpCodeTemplateFileName = DataTableGenerator.CSharpCodeTemplateFileName;
            m_namespace = DataTableGenerator.NameSpace;
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        private static void SetProcessorData()
        {
            DataTableProcessor.NameRow = m_datatableIDNameRow;
            DataTableProcessor.TypeRow = m_datatableIDTypeRow;
            DataTableProcessor.CommentRow = m_datatableCommentRow;
            DataTableProcessor.ContentStartRow = m_datatableCommentStartRow;
            DataTableProcessor.IdColumn = m_datatableIdColumn;
            DataTableGenerator.DataTablePath = m_datatablePath;
            DataTableGenerator.CSharpCodePath = m_CSharpCodePath;
            DataTableGenerator.CSharpCodeTemplateFileName = m_CSharpCodeTemplateFileName;
            DataTableGenerator.NameSpace = m_namespace;
        }

    }

    /// <summary>
    /// Windows对话框保存
    /// </summary>
    public class OSPlatformWindow
    {

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public String filter = null;
            public String customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public String file = null;
            public int maxFile = 0;
            public String fileTitle = null;
            public int maxFileTitle = 0;
            public String initialDir = null;
            public String title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public String defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public String templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

        public class LocalDialog
        {
            //链接指定系统函数       打开文件对话框
            [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
            public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
            public static bool GetOFN([In, Out] OpenFileName ofn)
            {
                return GetOpenFileName(ofn);
            }

            //链接指定系统函数        另存为对话框
            [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
            public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
            public static bool GetSFN([In, Out] OpenFileName ofn)
            {
                return GetSaveFileName(ofn);
            }
        }

    }
}
#endif