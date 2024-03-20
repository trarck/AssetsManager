using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ModelMaterialTest 
{
    public static void SetModelMaterialRemap(string modelPath, Material mat)
    {
        var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer != null)
        {
            bool needRefresh = false;

            using (var serializedObject = new SerializedObject(importer))
            {
                var externalObjects = serializedObject.FindProperty("m_ExternalObjects");
                var materials = serializedObject.FindProperty("m_Materials");

                for (int materialIdx = 0; materialIdx < materials.arraySize; ++materialIdx)
                {
                    var id = materials.GetArrayElementAtIndex(materialIdx);
                    var name = id.FindPropertyRelative("name").stringValue;
                    var type = id.FindPropertyRelative("type").stringValue;
                    var assembly = id.FindPropertyRelative("assembly").stringValue;

                    SerializedProperty materialProp = null;
                    Material material = null;
                    var propertyIdx = 0;

                    for (int externalObjectIdx = 0, count = externalObjects.arraySize; externalObjectIdx < count; ++externalObjectIdx)
                    {
                        var pair = externalObjects.GetArrayElementAtIndex(externalObjectIdx);
                        var externalName = pair.FindPropertyRelative("first.name").stringValue;
                        var externalType = pair.FindPropertyRelative("first.type").stringValue;

                        if (externalName == name && externalType == type)
                        {
                            materialProp = pair.FindPropertyRelative("second");
                            material = materialProp != null ? materialProp.objectReferenceValue as Material : null;
                            propertyIdx = externalObjectIdx;
                            break;
                        }
                    }

                    if (materialProp != null)
                    {
                        if ((material == null && mat!=null) || AssetDatabase.GetAssetPath(material) == "Resources/unity_builtin_extra")
                        {
                            materialProp.objectReferenceValue = mat;
                            needRefresh = true;
                        }
                    }
                    else
                    {
                        var newIndex = externalObjects.arraySize++;
                        var pair = externalObjects.GetArrayElementAtIndex(newIndex);
                        pair.FindPropertyRelative("first.name").stringValue = name;
                        pair.FindPropertyRelative("first.type").stringValue = type;
                        pair.FindPropertyRelative("first.assembly").stringValue = assembly;
                        pair.FindPropertyRelative("second").objectReferenceValue = mat;
                        needRefresh = true;
                    }
                }

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            if (needRefresh)
            {
                Debug.Log("Refresh");
                importer.SaveAndReimport();
            }
        }
    }

    [MenuItem("Tools/Model/ClearModelMaterialRemap")]
    public static void ClearModelMaterialRemap()
    {
        string selectPath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        SetModelMaterialRemap(selectPath, null);
    }
}
