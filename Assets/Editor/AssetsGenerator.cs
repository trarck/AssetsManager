using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEditor;
using UnityEngine;
using YH;

public class StringGenerator
{
    private const string BasicString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const int BasicStringLen = 62;
    public static string RandomString(int len)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < len; ++i)
        {
            sb.Append(BasicString[Random.Range(0, BasicStringLen)]);
        }
        return sb.ToString();
    }
}

[System.Serializable]
public class GenerateAssetConfig
{
    public enum ItemType
    {
        Image,
        Material,
        Prefab
    }

    public string name;
    public ItemType itemType;
    public string baseFolder;
    public string outFolder;
    public int generateCount;
    public bool foreceRegenerate = false;

    public string fileFilter = "*";

    public bool useSubFolder = true;
    public int maxFilePerFolder = 1000;

    public int folderRandomMinLen = 4;
    public int folderRandomMaxLen = 8;

    public int fileRandomMinLen = 6;
    public int fileRandomMaxLen = 20;

    public string needResultName;
}

public class AssetGenerator
{
    public GenerateAssetConfig config;

    protected List<string> m_Items;

    public System.Action<float,int> onProgress;

    public List<string> items
    {
        get
        {
            return m_Items;
        }
        set
        {
            m_Items = value;
        }
    }

    public AssetGenerator()
    {

    }

    public AssetGenerator(GenerateAssetConfig config)
    {
        this.config = config;
    }

    public virtual void BeforeGenerate()
    {
        if (m_Items == null)
        {
            m_Items = new List<string>();
        }
        else
        {
            m_Items.Clear();
        }

        if (!config.foreceRegenerate)
        {
            LoadItems();
        }

        AssetDatabase.StartAssetEditing();
    }

    public virtual void GenerateStep(string sourceFile, string outFile)
    {
        m_Items.Add(outFile);
    }

    public virtual void AfterGenerate()
    {
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();
    }

    public virtual void Generate()
    {
        string[] origFiles = Directory.GetFiles(config.baseFolder, config.fileFilter, SearchOption.AllDirectories);
        int originCount = origFiles.Length;

        BeforeGenerate();

        string folderName = "";
        int folderFileCount = 0;
        int needGenerateCount = config.foreceRegenerate ? config.generateCount : (config.generateCount - m_Items.Count);

        for (int i = 0; i < needGenerateCount; ++i)
        {
            string sourceFile = origFiles[i % originCount];
            string ext = Path.GetExtension(sourceFile);


            if (config.useSubFolder)
            {
                if (folderFileCount == 0)
                {
                    folderName = GetFolderRandomString();
                }

                if (++folderFileCount > config.maxFilePerFolder)
                {
                    folderFileCount = 0;
                }
            }

            string outFile = Path.Combine(config.outFolder, folderName, GetFileRandomString()) + ext;
            string outDir = Path.GetDirectoryName(outFile);
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }
            GenerateStep(sourceFile,outFile);
            if (onProgress != null)
            {
                onProgress(i / (float)config.generateCount, i);
            }
        }
        AfterGenerate();
    }

    public void LoadItems()
    {
        if (m_Items == null)
        {
            m_Items = new List<string>();
        }
        else
        {
            m_Items.Clear();
        }
        string[] generateFiles = Directory.GetFiles(config.outFolder, config.fileFilter, SearchOption.AllDirectories);
        m_Items.AddRange(generateFiles);
    }

    protected string GetFolderRandomString()
    {
        int len = Random.Range(config.fileRandomMinLen, config.folderRandomMaxLen + 1);
        return StringGenerator.RandomString(len);
    }

    protected string GetFileRandomString()
    {
        int len = Random.Range(config.fileRandomMinLen, config.fileRandomMaxLen + 1);
        return StringGenerator.RandomString(len);
    }
}

public class TextureGenerator : AssetGenerator
{

    public TextureGenerator(GenerateAssetConfig config) : base(config)
	{

	}

	public override void GenerateStep(string sourceFile, string outFile)
	{
        File.Copy(sourceFile, outFile);
        base.GenerateStep(sourceFile, outFile);
	}
}

public class MaterialGenerator : AssetGenerator
{
    private List<string> m_TempTexturePropertyNames = new List<string>();
    private List<string> m_ImageFiles;

    public MaterialGenerator(GenerateAssetConfig config, Dictionary<string,List<string>> results)
    {
        this.config = config;
        m_ImageFiles = results[config.needResultName];
    }

    public override void GenerateStep(string sourceFile, string outFile)
    {
        string assetPath = FileSystem.AddAssetPrev(FileSystem.Relative(Application.dataPath, sourceFile));
        Material baseMat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

        Material outMat = new Material(baseMat);
        SetMaterialTexture(outMat);
        AssetDatabase.CreateAsset(outMat, outFile);
        base.GenerateStep(sourceFile, outFile);
    }

    private Texture GetRandomTexture()
    {
        int i = Random.Range(0, m_ImageFiles.Count);
        string imageFile = m_ImageFiles[i];
        string assetPath = FileSystem.AddAssetPrev(FileSystem.Relative(Application.dataPath, imageFile));
        Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
        return tex;
    }

    private void SetMaterialTexture(Material mat)
    {
        m_TempTexturePropertyNames.Clear();
        mat.GetTexturePropertyNames(m_TempTexturePropertyNames);
        foreach (var texName in m_TempTexturePropertyNames)
        {
            if (texName.Contains("NormalMap") || texName.Contains("BumpMap"))
            {
                continue;
            }
            mat.SetTexture(texName, GetRandomTexture());
        }
    }
}

public class PrefabGenerator : AssetGenerator
{
    private List<string> m_MaterialFiles;

    public PrefabGenerator(GenerateAssetConfig config, Dictionary<string,List<string>> results)
    {
        this.config = config;
        m_MaterialFiles = results[config.needResultName];
    }

	public override void GenerateStep(string sourceFile, string outFile)
    {
        string assetPath = FileSystem.AddAssetPrev(FileSystem.Relative(Application.dataPath, sourceFile));
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

        GameObject newPrefab = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;

        Renderer[] renderers = newPrefab.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < r.sharedMaterials.Length; ++i)
            {
                mats[i] = GetRandomMaterial();
            }
            r.sharedMaterials = mats;
        }

        PrefabUtility.SaveAsPrefabAsset(newPrefab, outFile);
        GameObject.DestroyImmediate(newPrefab);
        base.GenerateStep(sourceFile, outFile);
    }

    private Material GetRandomMaterial()
    {
        int i = Random.Range(0, m_MaterialFiles.Count);
        string imageFile = m_MaterialFiles[i];
        string assetPath = FileSystem.AddAssetPrev(FileSystem.Relative(Application.dataPath, imageFile));
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        return mat;
    }

}


public class AssetsGenerator 
{
    public List<GenerateAssetConfig> generateAssetConfigs;

    public Dictionary<string, List<string>> results;

    public System.Action<string,float, int> onProgress;

    public void GenerateAll()
    {
        results = new Dictionary<string, List<string>>();

        foreach (var config in generateAssetConfigs)
        {
            AssetGenerator generator = null;

            switch (config.itemType)
            {
                case GenerateAssetConfig.ItemType.Image:
                    generator = new TextureGenerator(config);
                    break;
                case GenerateAssetConfig.ItemType.Material:
                    generator = new MaterialGenerator(config,results);
                    break;
                case GenerateAssetConfig.ItemType.Prefab:
                    generator = new PrefabGenerator(config, results);
                    break;
            }


            if (onProgress != null)
            {
                generator.onProgress += (p, i) =>
                {
                    onProgress(generator.config.name,p, i);
                };
            }

            generator.Generate();
            results[config.name] = generator.items;
        }
    }
}

public class GenerateAssetsWindow : EditorWindow
{
    public class GenerateConfigs : ScriptableObject
    {
        public List<GenerateAssetConfig> configs=new List<GenerateAssetConfig>();
    }

    [System.Serializable]
    public class SerializeableConfigs 
    {
        public List<GenerateAssetConfig> configs = new List<GenerateAssetConfig>();
    }

    SerializedObject m_ConfigsObj;

    [MenuItem("Tools/GenerateLargeAmountAssets")]
    public static void Open()
    {
        GetWindow<GenerateAssetsWindow>(false, "Generate Assets");
    }

	private void OnEnable()
	{
        GenerateConfigs obj = ScriptableObject.CreateInstance<GenerateConfigs>();
        m_ConfigsObj = new SerializedObject(obj);
    }

	private void OnGUI()
	{
        m_ConfigsObj.Update();

        SerializedProperty prop = m_ConfigsObj.GetIterator();

        while (prop.NextVisible(true))
        {
            if (prop.depth != 0)
                continue;

            if (prop.name.EndsWith("Script"))
                continue;

            EditorGUILayout.PropertyField(prop, true);
        }
        m_ConfigsObj.ApplyModifiedProperties();


        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Save"))
            {
                SaveConfigs();
            }

            if (GUILayout.Button("Load"))
            {
                LoadConfigs();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
    }

    private string GetSaveConfigFile()
    {
        string dir = Path.Combine(Application.dataPath, "../AssetDatabase");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return Path.Combine(dir, "GenerateConfig.json");
    }

    private void SaveConfigs()
    {
        GenerateConfigs generateConfigs = m_ConfigsObj.targetObject as GenerateConfigs;
        if (generateConfigs)
        {
            SerializeableConfigs saveConfigs = new SerializeableConfigs();
            saveConfigs.configs = generateConfigs.configs;
            string context = JsonUtility.ToJson(saveConfigs);
            File.WriteAllText(GetSaveConfigFile(), context);
        }
    }

    private void LoadConfigs()
    {
        string context = File.ReadAllText(GetSaveConfigFile());
        SerializeableConfigs saveConfigs = JsonUtility.FromJson<SerializeableConfigs>(context);
        GenerateConfigs generateConfigs = m_ConfigsObj.targetObject as GenerateConfigs;
        generateConfigs.configs = saveConfigs.configs;
    }

    private void Generate()
    {
        GenerateConfigs generateConfigs = m_ConfigsObj.targetObject as GenerateConfigs;
        if (generateConfigs)
        {
            AssetsGenerator assetsGenerator = new AssetsGenerator();
            assetsGenerator.generateAssetConfigs = generateConfigs.configs;
            assetsGenerator.onProgress += ShowGenerateProgress;
            assetsGenerator.GenerateAll();
            EditorUtility.ClearProgressBar();
        }
    }

    private void ShowGenerateProgress(string name, float percent, int idx)
    {
        EditorUtility.DisplayProgressBar("Genereate " + name, idx.ToString(), percent);
    }
}
