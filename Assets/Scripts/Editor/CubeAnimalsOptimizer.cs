using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/// <summary>
/// Công cụ tự động tối ưu hóa Model CuteMagic Cube Animals (Fox & T-Rex):
/// 1. Tối ưu Texture: Point filter, không Mipmaps -> sắc nét từng khối Voxel pixel, không bị mờ nhòe.
/// 2. Tối ưu Material: Chuyển sang Universal Render Pipeline/Lit -> sáng đẹp, bắt ánh sáng, đổ bóng mềm, không bị xỉn màu.
/// 3. Scale x2 chuẩn xác bằng khối Cube Unity và căn chân chạm đất Y = 0 (offset -0.5).
/// 4. Tạo Animator Controller lặp hoạt ảnh chạy (root_Walk) nhún nhảy cực kỳ đáng yêu.
/// 5. Tạo sẵn Prefab Model_Fox và Model_TRex hoàn chỉnh trong Assets/Prefabs/Characters/.
/// </summary>
[InitializeOnLoad]
public static class CubeAnimalsOptimizer
{
    private const string FOX_TEXTURE_PATH = "Assets/CuteMagic_CubeAnimals_Free/CubeAnimals_Free/ShaderTexture/Materials/ColorMap_Texture.png";
    private const string TREX_TEXTURE_PATH = "Assets/CuteMagic_CubeAnimals_T-REX_Free/CubeAnimals_T-REX_Free/ShaderTexture/Materials/ColorMap_Texture.png";
    
    private const string FOX_MAT_PATH = "Assets/CuteMagic_CubeAnimals_Free/CubeAnimals_Free/ShaderTexture/Materials/Shader_Texture.mat";
    private const string TREX_MAT_PATH = "Assets/CuteMagic_CubeAnimals_T-REX_Free/CubeAnimals_T-REX_Free/ShaderTexture/Materials/Standard_Shader.mat";

    private const string FOX_PREFAB_ORIGINAL = "Assets/CuteMagic_CubeAnimals_Free/CubeAnimals_Free/Prefab_1/Fox.prefab";
    private const string TREX_PREFAB_ORIGINAL = "Assets/CuteMagic_CubeAnimals_T-REX_Free/CubeAnimals_T-REX_Free/Prefab/Animals/T_Rex.prefab";

    private const string FOX_WALK_ANIM = "Assets/CuteMagic_CubeAnimals_Free/CubeAnimals_Free/Animations/root_Walk.anim";
    private const string FOX_IDLE_ANIM = "Assets/CuteMagic_CubeAnimals_Free/CubeAnimals_Free/Animations/root_Idle.anim";
    private const string TREX_WALK_ANIM = "Assets/CuteMagic_CubeAnimals_T-REX_Free/CubeAnimals_T-REX_Free/Animations/root_Walk.anim";
    private const string TREX_IDLE_ANIM = "Assets/CuteMagic_CubeAnimals_T-REX_Free/CubeAnimals_T-REX_Free/Animations/root_Idle.anim";

    private const string CHAR_PREFABS_FOLDER = "Assets/Prefabs/Characters";
    private const string CONTROLLER_PATH = "Assets/Prefabs/Characters/CubeAnimals_Runner_AC.controller";
    private const string FOX_OUTPUT_PREFAB = "Assets/Prefabs/Characters/Model_Fox.prefab";
    private const string TREX_OUTPUT_PREFAB = "Assets/Prefabs/Characters/Model_TRex.prefab";

    static CubeAnimalsOptimizer()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorPrefs.GetBool("CubeAnimals_Optimized_V2", false))
            {
                OptimizeAllCharacters();
                EditorPrefs.SetBool("CubeAnimals_Optimized_V2", true);
            }
        };
    }

    [MenuItem("Jelly Runner/🦊 Tối Ưu & Tạo Nhân Vật Cute Magic (Fox & T-Rex)")]
    public static void OptimizeAllCharacters()
    {
        Debug.Log("<color=#FF9900><b>[CubeAnimalsOptimizer] Đang tối ưu hóa Model Fox & T-Rex...</b></color>");

        if (!Directory.Exists(CHAR_PREFABS_FOLDER))
            Directory.CreateDirectory(CHAR_PREFABS_FOLDER);

        // 1. Tối ưu Texture (Point filter, sắc nét)
        OptimizeTexture(FOX_TEXTURE_PATH);
        OptimizeTexture(TREX_TEXTURE_PATH);

        // 2. Tối ưu Material (URP Lit)
        Material foxMat = SetupURPLitMaterial(FOX_MAT_PATH, FOX_TEXTURE_PATH);
        Material trexMat = SetupURPLitMaterial(TREX_MAT_PATH, TREX_TEXTURE_PATH);

        // 3. Tạo Animator Controller chạy mượt mà
        RuntimeAnimatorController runAnimController = CreateOrGetRunnerAnimatorController();

        // 4. Tạo Prefab hoàn chỉnh cho Fox (Scale x2, Offset -0.5)
        CreateOptimizedCharacterPrefab(FOX_PREFAB_ORIGINAL, FOX_OUTPUT_PREFAB, foxMat, runAnimController, "Model_Fox");

        // 5. Tạo Prefab hoàn chỉnh cho T-Rex (Scale x2, Offset -0.5)
        CreateOptimizedCharacterPrefab(TREX_PREFAB_ORIGINAL, TREX_OUTPUT_PREFAB, trexMat, runAnimController, "Model_TRex");

        // 6. Cập nhật vào JellyPlayer trên SampleScene nếu có
        AttachToScenePlayer();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#00FF99><b>[CubeAnimalsOptimizer] HOÀN TẤT TỐI ƯU HÓA! Model Fox & T-Rex đã sẵn sàng chơi cực đẹp!</b></color>");
    }

    private static void OptimizeTexture(string texPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null)
        {
            bool modified = false;
            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                modified = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                modified = true;
            }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                modified = true;
            }
            if (modified)
            {
                importer.SaveAndReimport();
                Debug.Log($"[CubeAnimalsOptimizer] Đã tối ưu Texture sắc nét Point filter: {texPath}");
            }
        }
    }

    private static Material SetupURPLitMaterial(string matPath, string texPath)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");

        if (urpLitShader != null)
        {
            if (mat == null)
            {
                mat = new Material(urpLitShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = urpLitShader;
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTexture("_MainTex", tex);
            }

            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", 0.25f);
            mat.SetFloat("_Metallic", 0.0f);
            EditorUtility.SetDirty(mat);
            Debug.Log($"[CubeAnimalsOptimizer] Đã cập nhật Material URP Lit: {matPath}");
        }

        return mat;
    }

    private static RuntimeAnimatorController CreateOrGetRunnerAnimatorController()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);
        }

        // Tải clip Walk và Idle
        AnimationClip walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(FOX_WALK_ANIM)
                              ?? AssetDatabase.LoadAssetAtPath<AnimationClip>(TREX_WALK_ANIM);
        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(FOX_IDLE_ANIM)
                              ?? AssetDatabase.LoadAssetAtPath<AnimationClip>(TREX_IDLE_ANIM);

        if (controller.layers.Length > 0 && walkClip != null)
        {
            var sm = controller.layers[0].stateMachine;

            // Xóa states cũ nếu có để tạo chuẩn
            for (int i = sm.states.Length - 1; i >= 0; i--)
            {
                sm.RemoveState(sm.states[i].state);
            }

            // State Run (root_Walk)
            var runState = sm.AddState("Run");
            runState.motion = walkClip;
            runState.speed = 1.85f; // Chạy nhanh nhịp nhàng tương thích với tốc độ 8m/s

            // State Idle (root_Idle)
            if (idleClip != null)
            {
                var idleState = sm.AddState("Idle");
                idleState.motion = idleClip;
                idleState.speed = 1.0f;
            }

            sm.defaultState = runState;
            EditorUtility.SetDirty(controller);
            Debug.Log($"[CubeAnimalsOptimizer] Đã thiết lập Runner Animator Controller: {CONTROLLER_PATH}");
        }

        return controller;
    }

    private static void CreateOptimizedCharacterPrefab(string originalPrefabPath, string outputPath, Material mat, RuntimeAnimatorController animCtrl, string charName)
    {
        GameObject orig = AssetDatabase.LoadAssetAtPath<GameObject>(originalPrefabPath);
        if (orig == null)
        {
            Debug.LogWarning($"[CubeAnimalsOptimizer] Không tìm thấy file prefab gốc: {originalPrefabPath}");
            return;
        }

        GameObject instance = Object.Instantiate(orig);
        instance.name = charName;

        // Đảm bảo scale x2 chuẩn bằng khối cube Unity
        instance.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        instance.transform.localScale = new Vector3(2f, 2f, 2f);
        instance.transform.localRotation = Quaternion.identity;

        // Gán Material URP Lit cho toàn bộ SkinnedMeshRenderer / MeshRenderer
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (mat != null) r.material = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows = true;
        }

        // Cập nhật Animator
        Animator anim = instance.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            if (animCtrl != null) anim.runtimeAnimatorController = animCtrl;
            anim.applyRootMotion = false;
            anim.speed = 1.85f;
        }

        // Gắn CubeAnimalCharacter helper
        if (instance.GetComponent<CubeAnimalCharacter>() == null)
            instance.AddComponent<CubeAnimalCharacter>();

        // Xóa Prefab cũ nếu đã tồn tại và lưu Prefab mới
        PrefabUtility.SaveAsPrefabAsset(instance, outputPath);
        Object.DestroyImmediate(instance);

        Debug.Log($"[CubeAnimalsOptimizer] Đã tạo thành công Prefab tối ưu: {outputPath}");
    }

    private static void AttachToScenePlayer()
    {
        JellyPlayer player = Object.FindAnyObjectByType<JellyPlayer>();
        if (player != null)
        {
            SerializedObject so = new SerializedObject(player);
            SerializedProperty foxProp = so.FindProperty("foxModelPrefab");
            SerializedProperty trexProp = so.FindProperty("trexModelPrefab");

            GameObject foxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FOX_OUTPUT_PREFAB);
            GameObject trexPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TREX_OUTPUT_PREFAB);

            if (foxProp != null && foxPrefab != null) foxProp.objectReferenceValue = foxPrefab;
            if (trexProp != null && trexPrefab != null) trexProp.objectReferenceValue = trexPrefab;

            so.ApplyModifiedProperties();

            // Áp dụng model hiển thị ngay lập tức
            player.ApplyCharacterModel();
            EditorUtility.SetDirty(player.gameObject);
            Debug.Log("[CubeAnimalsOptimizer] Đã kết nối Model Fox & T-Rex vào JellyPlayer trong Scene!");
        }
    }
}
