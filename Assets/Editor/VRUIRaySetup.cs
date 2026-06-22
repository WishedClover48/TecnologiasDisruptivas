using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Surfaces;

/// <summary>
/// Editor tools for the Meta Interaction SDK ray UI.
///
/// CAMINO B2 (lo que usamos) - botones 3D seleccionables por el ray:
///   Select the button GameObjects (PlayAgain / Quit / Entendido) and run
///   "Tools/VR UI/Convert Hold Buttons To Ray Buttons". Swaps the HoldToActivate
///   + grab components for ColliderSurface + RayInteractable + an
///   InteractableUnityEventWrapper, and auto-wires When Select -> the panel method
///   (PlayAgain / Quit / Acknowledge). No canvas/EventSystem changes needed.
///   The visible ray + reticle comes from the Controller Ray Interactor in the rig.
///
/// CAMINO B1 (opcional, para UGUI buttons dentro de un Canvas, estilo PagueSusCuentas):
///   "Tools/VR UI/Setup Ray On Selected Canvas" + "Tools/VR UI/Fix EventSystem".
///   No hace falta para B2.
/// </summary>
public static class VRUIRaySetup
{
    private const string Menu = "Tools/VR UI/";

    [MenuItem(Menu + "Setup Ray On Selected Canvas")]
    private static void SetupSelectedCanvases()
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection == null || selection.Length == 0)
        {
            EditorUtility.DisplayDialog("VR UI Ray Setup",
                "Seleccioná uno o más GameObjects que tengan un Canvas (World Space).", "OK");
            return;
        }

        int done = 0;
        foreach (GameObject go in selection)
            if (TrySetupCanvas(go)) done++;

        Debug.Log($"[VRUIRaySetup] Canvas configurados: {done}/{selection.Length}");
    }

    [MenuItem(Menu + "Setup Ray On Selected Canvas", true)]
    private static bool SetupSelectedCanvasesValidate() => Selection.gameObjects.Length > 0;

    private static bool TrySetupCanvas(GameObject go)
    {
        Canvas canvas = go.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning($"[VRUIRaySetup] '{go.name}' no tiene Canvas. Salteado.", go);
            return false;
        }

        if (canvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogWarning($"[VRUIRaySetup] '{go.name}': el Canvas NO está en World Space. " +
                             "El ray del SDK solo funciona con World Space. Cambialo y reintentá.", go);
            return false;
        }

        Undo.RegisterFullObjectHierarchyUndo(go, "Setup VR UI Ray");

        // Unity UI necesita un GraphicRaycaster para resolver qué elemento (botón) se tocó.
        if (go.GetComponent<GraphicRaycaster>() == null)
            Undo.AddComponent<GraphicRaycaster>(go);

        // 1) PointableCanvas -> el Canvas
        PointableCanvas pointable = GetOrAdd<PointableCanvas>(go);
        SetRef(pointable, "_canvas", canvas);

        // 2) _surface: ColliderSurface respaldada por un BoxCollider del tamaño del rect
        BoxCollider box = GetOrAdd<BoxCollider>(go);
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            Rect r = rt.rect;
            box.center = new Vector3(r.center.x, r.center.y, 0f);
            box.size   = new Vector3(Mathf.Max(r.width, 0.01f), Mathf.Max(r.height, 0.01f), 0.01f);
        }
        box.isTrigger = true; // no debe chocar con físicas; el ray usa Collider.Raycast directo

        ColliderSurface colliderSurface = GetOrAdd<ColliderSurface>(go);
        SetRef(colliderSurface, "_collider", box);

        // 3) _selectSurface: PlaneSurface (plano infinito para selección estable)
        PlaneSurface planeSurface = GetOrAdd<PlaneSurface>(go);

        // 4) RayInteractable: ata pointable + ambas surfaces
        RayInteractable rayInteractable = GetOrAdd<RayInteractable>(go);
        SetRef(rayInteractable, "_pointableElement", pointable);
        SetRef(rayInteractable, "_surface", colliderSurface);
        SetRef(rayInteractable, "_selectSurface", planeSurface);

        EditorUtility.SetDirty(go);
        Debug.Log($"[VRUIRaySetup] '{go.name}': PointableCanvas + RayInteractable + Surfaces listo.", go);
        return true;
    }

    [MenuItem(Menu + "Fix EventSystem (active scene)")]
    private static void FixEventSystem()
    {
        EventSystem es = Object.FindAnyObjectByType<EventSystem>();
        if (es == null)
        {
            EditorUtility.DisplayDialog("VR UI Ray Setup",
                "No encontré un EventSystem en la escena activa.", "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(es.gameObject, "Fix EventSystem Module");

        // Solo puede haber un BaseInputModule: sacamos los que no sean el de Meta.
        foreach (BaseInputModule module in es.GetComponents<BaseInputModule>())
            if (!(module is PointableCanvasModule))
                Undo.DestroyObjectImmediate(module);

        if (es.GetComponent<PointableCanvasModule>() == null)
            Undo.AddComponent<PointableCanvasModule>(es.gameObject);

        EditorUtility.SetDirty(es.gameObject);
        Debug.Log($"[VRUIRaySetup] EventSystem '{es.name}': ahora usa PointableCanvasModule.", es.gameObject);
    }

    // ---------------------------------------------------------------------
    //  Camino B2: convertir un HoldToActivateButton (grab/aim+hold) en un
    //  botón 3D del SDK: RayInteractable + ColliderSurface + Event Wrapper,
    //  seleccionable por el ray con retícula. When Select -> método.
    // ---------------------------------------------------------------------

    [MenuItem(Menu + "Convert Hold Buttons To Ray Buttons")]
    private static void ConvertSelectedButtons()
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection == null || selection.Length == 0)
        {
            EditorUtility.DisplayDialog("VR UI Ray Setup",
                "Seleccioná los GameObjects de los botones (PlayAgain / Quit / Entendido).", "OK");
            return;
        }

        int done = 0;
        foreach (GameObject go in selection)
            if (TryConvertButton(go)) done++;

        Debug.Log($"[VRUIRaySetup] Botones convertidos a Ray Button: {done}/{selection.Length}");
    }

    [MenuItem(Menu + "Convert Hold Buttons To Ray Buttons", true)]
    private static bool ConvertSelectedButtonsValidate() => Selection.gameObjects.Length > 0;

    private static bool TryConvertButton(GameObject go)
    {
        Undo.RegisterFullObjectHierarchyUndo(go, "Convert Hold Button To Ray Button");

        // Capturamos el hold ANTES de sacarlo, para preservar su acción (_onActivated).
        HoldToActivateButton hold = go.GetComponent<HoldToActivateButton>();

        // 1) Sacar la interacción de grab (el HoldToActivateButton lo sacamos al final).
        RemoveIfPresent<HandGrabInteractable>(go);
        RemoveIfPresent<GrabInteractable>(go);
        RemoveIfPresent<Grabbable>(go);

        // El Rigidbody del grab ya no hace falta: dejarlo kinematic para que no "caiga".
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Undo.RecordObject(rb, "Make button kinematic");
            rb.isKinematic = true;
            rb.useGravity  = false;
        }

        // 2) El ColliderSurface necesita un Collider (reusamos el del grab; si no hay, Box).
        Collider col = go.GetComponent<Collider>();
        if (col == null) col = Undo.AddComponent<BoxCollider>(go);

        // 3) ColliderSurface -> ese collider (es el _surface del RayInteractable).
        ColliderSurface colliderSurface = GetOrAdd<ColliderSurface>(go);
        SetRef(colliderSurface, "_collider", col);

        // 4) RayInteractable -> _surface = ColliderSurface. _selectSurface es opcional: lo dejamos vacío.
        RayInteractable rayInteractable = GetOrAdd<RayInteractable>(go);
        SetRef(rayInteractable, "_surface", colliderSurface);

        // 5) Event Wrapper: When Select -> la acción original del botón (o el método por nombre).
        InteractableUnityEventWrapper wrapper = GetOrAdd<InteractableUnityEventWrapper>(go);
        SetRef(wrapper, "_interactableView", rayInteractable);
        WireWhenSelect(go, wrapper, hold);

        // 6) Ahora sí sacamos el HoldToActivateButton (ya copiamos su acción).
        RemoveIfPresent<HoldToActivateButton>(go);

        EditorUtility.SetDirty(go);
        Debug.Log($"[VRUIRaySetup] '{go.name}': RayInteractable + ColliderSurface + Event Wrapper listo.", go);
        return true;
    }

    private static void WireWhenSelect(GameObject go, InteractableUnityEventWrapper wrapper, HoldToActivateButton hold)
    {
        // 1) Preservar la acción original del hold (_onActivated) si tenía algo cableado.
        if (hold != null)
        {
            int copied = CopyPersistentCalls(hold, "_onActivated", wrapper, "_whenSelect");
            if (copied > 0)
            {
                Debug.Log($"[VRUIRaySetup] '{go.name}': preservé {copied} acción(es) de 'On Activated' -> 'When Select'.", go);
                return;
            }
        }

        // 2) Fallback por nombre (SampleScene: PlayAgain / Quit / Acknowledge).
        UnityEvent evt = GetOrCreateUnityEvent(wrapper, "_whenSelect");
        if (evt == null)
        {
            Debug.LogWarning($"[VRUIRaySetup] '{go.name}': no pude acceder a WhenSelect. " +
                             "Cablealo a mano en el Inspector (When Select -> método).", go);
            return;
        }

        // Limpiar listeners persistentes previos para no duplicar.
        for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(evt, i);

        string n = go.name.ToLowerInvariant();
        UI_GameEndFeedback feedback = go.GetComponentInParent<UI_GameEndFeedback>(true);
        MinigameTutorial    tutorial = go.GetComponentInParent<MinigameTutorial>(true);

        if (feedback != null && (n.Contains("quit") || n.Contains("salir") || n.Contains("exit")))
        {
            UnityEventTools.AddPersistentListener(evt, feedback.Quit);
            Debug.Log($"[VRUIRaySetup] '{go.name}'.WhenSelect -> UI_GameEndFeedback.Quit()", go);
        }
        else if (feedback != null && (n.Contains("again") || n.Contains("play") || n.Contains("retry")))
        {
            UnityEventTools.AddPersistentListener(evt, feedback.PlayAgain);
            Debug.Log($"[VRUIRaySetup] '{go.name}'.WhenSelect -> UI_GameEndFeedback.PlayAgain()", go);
        }
        else if (tutorial != null)
        {
            UnityEventTools.AddPersistentListener(evt, tutorial.Acknowledge);
            Debug.Log($"[VRUIRaySetup] '{go.name}'.WhenSelect -> MinigameTutorial.Acknowledge()", go);
        }
        else
        {
            Debug.LogWarning($"[VRUIRaySetup] '{go.name}': no pude deducir el método. " +
                             "Cablealo a mano en el Inspector (When Select -> target + método).", go);
        }

        EditorUtility.SetDirty(wrapper);
    }

    // El UnityEvent privado del wrapper puede estar null recién agregado: lo aseguramos por reflexión.
    private static UnityEvent GetOrCreateUnityEvent(Component comp, string fieldName)
    {
        FieldInfo f = comp.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (f == null)
        {
            Debug.LogError($"[VRUIRaySetup] El campo '{fieldName}' no existe en {comp.GetType().Name}. " +
                           "Puede ser otra versión del Meta SDK; avisame y lo ajusto.", comp);
            return null;
        }
        UnityEvent evt = f.GetValue(comp) as UnityEvent;
        if (evt == null)
        {
            evt = new UnityEvent();
            f.SetValue(comp, evt);
        }
        return evt;
    }

    // Copia los persistent calls de un UnityEvent serializado a otro, preservando
    // target + método + argumentos exactos. Devuelve cuántos copió.
    private static int CopyPersistentCalls(Component src, string srcField, Component dst, string dstField)
    {
        SerializedProperty srcCalls = new SerializedObject(src)
            .FindProperty(srcField + ".m_PersistentCalls.m_Calls");
        if (srcCalls == null || srcCalls.arraySize == 0) return 0;

        SerializedObject dstSO = new SerializedObject(dst);
        SerializedProperty dstCalls = dstSO.FindProperty(dstField + ".m_PersistentCalls.m_Calls");
        if (dstCalls == null) return 0;

        dstCalls.ClearArray();
        for (int i = 0; i < srcCalls.arraySize; i++)
        {
            dstCalls.InsertArrayElementAtIndex(i);
            SerializedProperty s = srcCalls.GetArrayElementAtIndex(i);
            SerializedProperty d = dstCalls.GetArrayElementAtIndex(i);

            CopyProp(s, d, "m_Target");
            CopyProp(s, d, "m_TargetAssemblyTypeName");
            CopyProp(s, d, "m_MethodName");
            CopyProp(s, d, "m_Mode");
            CopyProp(s, d, "m_CallState");
            CopyProp(s, d, "m_Arguments.m_ObjectArgument");
            CopyProp(s, d, "m_Arguments.m_ObjectArgumentAssemblyTypeName");
            CopyProp(s, d, "m_Arguments.m_IntArgument");
            CopyProp(s, d, "m_Arguments.m_FloatArgument");
            CopyProp(s, d, "m_Arguments.m_StringArgument");
            CopyProp(s, d, "m_Arguments.m_BoolArgument");
        }
        dstSO.ApplyModifiedProperties();
        return srcCalls.arraySize;
    }

    private static void CopyProp(SerializedProperty s, SerializedProperty d, string rel)
    {
        SerializedProperty sp = s.FindPropertyRelative(rel);
        SerializedProperty dp = d.FindPropertyRelative(rel);
        if (sp == null || dp == null) return;

        switch (sp.propertyType)
        {
            case SerializedPropertyType.ObjectReference: dp.objectReferenceValue = sp.objectReferenceValue; break;
            case SerializedPropertyType.String:          dp.stringValue          = sp.stringValue;          break;
            case SerializedPropertyType.Float:           dp.floatValue           = sp.floatValue;           break;
            case SerializedPropertyType.Boolean:         dp.boolValue            = sp.boolValue;             break;
            case SerializedPropertyType.Integer:
            case SerializedPropertyType.Enum:            dp.intValue             = sp.intValue;              break;
        }
    }

    private static void RemoveIfPresent<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        if (c != null) Undo.DestroyObjectImmediate(c);
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c != null ? c : Undo.AddComponent<T>(go);
    }

    private static void SetRef(Component target, string field, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(field);
        if (prop == null)
        {
            Debug.LogError($"[VRUIRaySetup] El campo '{field}' no existe en {target.GetType().Name}. " +
                           "Puede ser otra versión del Meta SDK; avisame y lo ajusto.", target);
            return;
        }
        prop.objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }
}
