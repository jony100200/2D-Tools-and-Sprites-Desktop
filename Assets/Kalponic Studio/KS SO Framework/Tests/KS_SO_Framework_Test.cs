using UnityEngine;
using UnityEngine.UIElements;
using KalponicStudio.SO_Framework;

/// <summary>
/// Comprehensive test script to validate KS SO Framework v3.0 functionality
/// Attach this to a GameObject in a test scene to verify all framework features work
/// </summary>
public class KS_SO_Framework_Test : MonoBehaviour
{
    [Header("Test Variables")]
    [SerializeField] private IntVariable intVar;
    [SerializeField] private FloatVariable floatVar;
    [SerializeField] private BoolVariable boolVar;
    [SerializeField] private StringVariable stringVar;

    [Header("Test Events")]
    [SerializeField] private IntEvent intEvent;
    [SerializeField] private VoidEvent voidEvent;

    [Header("Test Lists")]
    [SerializeField] private IntList intList;
    [SerializeField] private StringList stringList;

    [Header("Enhanced Variables")]
    [SerializeField] private IntRangeVariable intRangeVar;
    [SerializeField] private ColorVariable colorVar;
    [SerializeField] private Vector3Variable vector3Var;
    [SerializeField] private Vector2Variable vector2Var;
    [SerializeField] private Vector2IntVariable vector2IntVar;
    [SerializeField] private QuaternionVariable quaternionVar;
    [SerializeField] private ComponentVariable componentVar;
    [SerializeField] private LayerMaskVariable layerMaskVar;

    [Header("UI Toolkit Bindings")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private LabelBinding labelBinding;
    [SerializeField] private SliderBinding sliderBinding;
    [SerializeField] private ToggleBinding toggleBinding;
    [SerializeField] private TextFieldBinding textFieldBinding;

    [Header("Event Listeners")]
    [SerializeField] private IntEventListener intEventListener;
    [SerializeField] private VoidEventListener voidEventListener;
    [SerializeField] private AnimationEventListener animationEventListener;
    [SerializeField] private AudioEventListener audioEventListener;

    [Header("Variable References")]
    [SerializeField] private IntVariableReference intReference;
    [SerializeField] private FloatVariableReference floatReference;
    [SerializeField] private DynamicVariableReference dynamicReference;

    [Header("Advanced Data Structures")]
    [SerializeField] private StringDictionary stringDictionary;
    [SerializeField] private StringIntDictionary stringIntDictionary;
    [SerializeField] private ScriptableEnum gameStateEnum;
    [SerializeField] private ItemRarityEnum itemRarityEnum;

    private void Start()
    {
        Debug.Log("=== KS SO Framework v3.0 Test Started ===");

        TestBasicVariables();
        TestEvents();
        TestLists();
        TestEnhancedVariables();
        TestUIBindings();
        TestEventListeners();
        TestVariableReferences();
        TestScriptableSingletons();
        TestAdvancedDataStructures();

        Debug.Log("=== KS SO Framework v3.0 Test Completed ===");
    }

    private void TestBasicVariables()
    {
        Debug.Log("Testing Basic Variables...");

        if (intVar != null)
        {
            intVar.Value = 42;
            Debug.Log($"Int Variable: {intVar.Value}");
        }

        if (floatVar != null)
        {
            floatVar.Value = 3.14f;
            Debug.Log($"Float Variable: {floatVar.Value}");
        }

        if (boolVar != null)
        {
            boolVar.Value = true;
            Debug.Log($"Bool Variable: {boolVar.Value}");
        }

        if (stringVar != null)
        {
            stringVar.Value = "Hello KS SO Framework v3.0!";
            Debug.Log($"String Variable: {stringVar.Value}");
        }
    }

    private void TestEvents()
    {
        Debug.Log("Testing Events...");

        if (intEvent != null)
        {
            intEvent.RegisterListener(OnIntEventRaised);
            intEvent.Raise(123);
            intEvent.UnregisterListener(OnIntEventRaised);
        }

        if (voidEvent != null)
        {
            voidEvent.RegisterListener(OnVoidEventRaised);
            voidEvent.Raise();
            voidEvent.UnregisterListener(OnVoidEventRaised);
        }
    }

    private void TestLists()
    {
        Debug.Log("Testing Lists...");

        if (intList != null)
        {
            intList.Add(1);
            intList.Add(2);
            intList.Add(3);
            Debug.Log($"Int List Count: {intList.Count}, Contains 2: {intList.Contains(2)}");
            intList.Remove(2);
            Debug.Log($"Int List Count after remove: {intList.Count}");
        }

        if (stringList != null)
        {
            stringList.Add("Apple");
            stringList.Add("Banana");
            stringList.Add("Cherry");
            Debug.Log($"String List Count: {stringList.Count}, Index 1: {stringList[1]}");
        }
    }

    private void TestEnhancedVariables()
    {
        Debug.Log("Testing Enhanced Variables...");

        if (intRangeVar != null)
        {
            intRangeVar.Value = 75;
            Debug.Log($"Int Range Variable: {intRangeVar.Value} (Min: {intRangeVar.MinValue}, Max: {intRangeVar.MaxValue})");
        }

        if (colorVar != null)
        {
            colorVar.Value = Color.blue;
            Debug.Log($"Color Variable: {colorVar.Value}");
        }

        if (vector3Var != null)
        {
            vector3Var.Value = Vector3.up;
            Debug.Log($"Vector3 Variable: {vector3Var.Value}");
        }

        if (vector2Var != null)
        {
            vector2Var.Value = Vector2.one;
            Debug.Log($"Vector2 Variable: {vector2Var.Value}");
        }

        if (vector2IntVar != null)
        {
            vector2IntVar.Value = new Vector2Int(5, 10);
            Debug.Log($"Vector2Int Variable: {vector2IntVar.Value}");
        }

        if (quaternionVar != null)
        {
            quaternionVar.Value = Quaternion.Euler(45f, 0f, 0f);
            Debug.Log($"Quaternion Variable: {quaternionVar.Value.eulerAngles}");
        }

        if (componentVar != null)
        {
            componentVar.Value = GetComponent<Transform>();
            Debug.Log($"Component Variable: {componentVar.Value}");
        }

        if (layerMaskVar != null)
        {
            layerMaskVar.Value = LayerMask.GetMask("Default");
            Debug.Log($"LayerMask Variable: {layerMaskVar.Value.value}");
        }
    }

    private void TestUIBindings()
    {
        Debug.Log("Testing UI Toolkit Bindings...");

        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            // Note: UI binding tests require manual setup in inspector
            // The binding components should be configured in the editor
            Debug.Log("UI Document found - bindings should be configured in inspector");
        }
        else
        {
            Debug.LogWarning("UI Document not found or not initialized for binding tests");
        }
    }

    private void TestEventListeners()
    {
        Debug.Log("Testing Event Listeners...");

        // Note: Event listeners require manual setup in inspector
        // The listener components should be configured in the editor
        Debug.Log("Event listeners should be configured in inspector");
    }

    private void TestVariableReferences()
    {
        Debug.Log("Testing Variable References...");

        // Note: Variable references require manual setup in inspector
        // The reference components should be configured in the editor
        if (intReference != null)
        {
            Debug.Log($"Int Reference configured - Effective Value: {intReference.EffectiveValue}");
        }

        if (floatReference != null)
        {
            Debug.Log($"Float Reference configured - Effective Value: {floatReference.EffectiveValue}");
        }

        if (dynamicReference != null)
        {
            Debug.Log($"Dynamic Reference configured - Effective Value: {dynamicReference.EffectiveValue}");
        }
    }

    private void TestScriptableSingletons()
    {
        Debug.Log("Testing Scriptable Singletons...");

        // Test Game Settings
        GameSettings.Instance.SetMasterVolume(0.8f);
        Debug.Log($"Game Settings Master Volume: {GameSettings.Instance.MasterVolume}");

        // Test Game State
        GameState.Instance.AddGold(100);
        Debug.Log($"Game State Gold: {GameState.Instance.Gold}");

        // Test Input Settings
        InputSettings.Instance.SetMouseSensitivity(1.5f);
        Debug.Log($"Input Settings Mouse Sensitivity: {InputSettings.Instance.MouseSensitivity}");
    }

    private void TestAdvancedDataStructures()
    {
        Debug.Log("Testing Advanced Data Structures...");

        // Test String Dictionary
        if (stringDictionary != null)
        {
            stringDictionary.Add("Key1", "Value1");
            stringDictionary.Add("Key2", "Value2");
            Debug.Log($"String Dictionary Count: {stringDictionary.Count}");
            Debug.Log($"String Dictionary Key1: {stringDictionary["Key1"]}");
        }

        // Test String-Int Dictionary
        if (stringIntDictionary != null)
        {
            stringIntDictionary.Add("Health", 100);
            stringIntDictionary.Add("Mana", 50);
            Debug.Log($"String-Int Dictionary Sum: {stringIntDictionary.Values[0] + stringIntDictionary.Values[1]}");
        }

        // Test Scriptable Enum
        if (gameStateEnum != null)
        {
            gameStateEnum.AddValue("Menu");
            gameStateEnum.AddValue("Playing");
            gameStateEnum.AddValue("Paused");
            Debug.Log($"Game State Enum Count: {gameStateEnum.Count}");
            Debug.Log($"Game State Enum Value 0: {gameStateEnum.GetValue(0)}");
        }

        // Test Item Rarity Enum
        if (itemRarityEnum != null)
        {
            itemRarityEnum.AddValue("Common");
            itemRarityEnum.AddValue("Rare");
            itemRarityEnum.SetData("Common", Color.gray);
            itemRarityEnum.SetData("Rare", Color.blue);
            Debug.Log($"Item Rarity Enum Count: {itemRarityEnum.Count}");
            Debug.Log($"Common Rarity Color: {itemRarityEnum.GetData("Common")}");
        }
    }

    private void OnIntEventRaised(int value)
    {
        Debug.Log($"Int Event Raised with value: {value}");
    }

    private void OnVoidEventRaised()
    {
        Debug.Log("Void Event Raised");
    }

    [ContextMenu("Run Tests")]
    private void RunTests()
    {
        Start();
    }

    [ContextMenu("Test UI Updates")]
    private void TestUIUpdates()
    {
        // Modify variables to test UI binding updates
        if (intVar != null) intVar.Value = Random.Range(0, 100);
        if (floatVar != null) floatVar.Value = Random.Range(0f, 10f);
        if (boolVar != null) boolVar.Value = !boolVar.Value;
        if (stringVar != null) stringVar.Value = "Updated: " + Random.Range(0, 1000);

        Debug.Log("Variables updated - check UI for binding updates");
    }

    [ContextMenu("Test Events")]
    private void TestEventTriggering()
    {
        // Trigger events to test listeners
        if (intEvent != null) intEvent.Raise(Random.Range(1, 100));
        if (voidEvent != null) voidEvent.Raise();

        Debug.Log("Events triggered - check listeners for responses");
    }
}
