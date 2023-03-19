using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HierarchyAnalys : EditorWindow
{
    const string stringCountText     = "GameObjects Count  : ";
    const string stringVertCountText = "Vertex Count              : ";
    StringBuilder countText =     new StringBuilder(stringCountText);
    StringBuilder vertCountText = new StringBuilder(stringVertCountText);

    Label ObjCount;
    Label ObjVertCount;
    ListView leftPane;
    List<string> rows = new();
    List<GameObject> objs = new();

    DataSet dataSet = new DataSet("dataSet");
    DataTable data = new DataTable("Data");

    [MenuItem("Window/Analysis/Hierarchy Analysator")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<HierarchyAnalys>();
        wnd.titleContent = new GUIContent("Hierarchy Analysator");
        wnd.maxSize = new Vector2(600, 11080);
        wnd.minSize = new Vector2(450, 150);
    }

    public void CreateGUI()
    {
        leftPane = new ListView();

        Label InfoH = new Label("Base Info");

        ObjCount = new Label(countText.ToString());
        ObjVertCount = new Label(vertCountText.ToString());

        ObjVertCount.style.top = 20;
        ObjVertCount.style.left = 10;

        ObjCount.style.top = 15;
        ObjCount.style.left = 10;

        InfoH.style.top = 5;
        InfoH.style.left = 5;
        InfoH.style.unityFontStyleAndWeight = FontStyle.Bold;


        Button Update = new Button(onStat);
        Update.text = "Update Data";
        Update.style.top = 30;

        leftPane.style.top = 40;
        leftPane.style.alignContent = Align.Stretch;
        leftPane.style.alignItems = Align.Stretch;
        leftPane.style.unityTextAlign = TextAnchor.MiddleCenter;

        rootVisualElement.Add(InfoH);

        rootVisualElement.Add(ObjCount);
        rootVisualElement.Add(ObjVertCount);

        rootVisualElement.Add(Update);

        rootVisualElement.Add(leftPane);

        leftPane.onSelectionChange += OnSelect;
    }

    private void OnGUI() {
        leftPane.style.left = 10;
        leftPane.style.height = new StyleLength(position.height - 110);
        leftPane.style.width = new StyleLength(position.width - 15);
    }

    void OnSelect(IEnumerable<object> a) {
        string sel = a.ToArray()[0].ToString();

        string stringId = sel.Substring(0, sel.IndexOf(' '));
        int id = int.TryParse(stringId, out int sid) ? sid : -1;

        if (id == -1) return;

        if (id > objs.Count - 1 || id < 0) {
            Debug.LogWarning("Data is not actual, will be updated!");
            onStat();
            return;
        }

        Selection.activeGameObject = objs[id];
    }
    
    void onStat() {
        dataSet = new DataSet("BookStore");
        data = new DataTable("Books");

        dataSet.Tables.Add(data);

        DataColumn key = new DataColumn("Key", System.Type.GetType("System.Int32"));
            key.Unique = true; // столбец будет иметь уникальное значение
            key.AllowDBNull = false; // не может принимать null
            key.AutoIncrement = true; // будет автоинкрементироваться
            key.AutoIncrementSeed = 1; // начальное значение
            key.AutoIncrementStep = 1; // приращении при добавлении новой строки

        DataColumn idColumn =   new DataColumn("Id", System.Type.GetType("System.String"));
        DataColumn nameColumn = new DataColumn("Name", System.Type.GetType("System.String"));
        DataColumn vertCount = new DataColumn("VertCount", System.Type.GetType("System.String"));

        data.Columns.Add(idColumn);
        data.Columns.Add(nameColumn);
        data.Columns.Add(vertCount);

        data.PrimaryKey = new DataColumn[] { data.Columns["Key"] };

        objs = GetAllObjectsOnlyInScene();

        GameObject objI;

        StringBuilder stringItem = new();

        int[] counts = new int[objs.Count];

        List<GameObjectType> objects = new();
        int vertSum = 0;

        for (int i = 0; i < objs.Count; i++)
        {
            objI = objs[i];

            if (objI.TryGetComponent(out MeshFilter meshF)) {
                GameObjectType e = new();
                e.id = i;
                e.name = objI.name;
                e.vertCount = meshF.sharedMesh.vertexCount;

                vertSum += meshF.sharedMesh.vertexCount;

                objects.Add(e);
            }
        }

        objects.Sort((x, y) => x.vertCount.CompareTo(y.vertCount) );
        DataRow aa= data.NewRow();
        aa.ItemArray = new object[] { "ID", "Name", "Vert Count"};
        data.Rows.Add(aa);

        for (int i = objects.Count - 1; i > 0; i--)
        {
            // stringItem.AppendFormat("{0,-35} {1,-20} {2, -10} \n", objects[i].id, objects[i].name, objects[i].vertCount);
            DataRow row = data.NewRow();
            row.ItemArray = new object[] { objects[i].id.ToString(), objects[i].name, objects[i].vertCount.ToString()};
            data.Rows.Add(row);

            // a.Add(stringItem.ToString());

            // stringItem.Clear();
            // rootVisualElement.Add(new Label(meshF.sharedMesh.vertexCount.ToString()));
            
        }

        rows.Clear();

        foreach (DataRow r in data.Rows)
        {

            object alt = null;
            foreach (var cell in r.ItemArray)
            {
                stringItem.Append(cell.ToString().PadLeft(alt != null ? 50 - alt.ToString().Length : 0));
                alt = cell;
            }
            rows.Add(stringItem.ToString());
            stringItem.Clear();
        }
        
        leftPane.itemsSource = rows;

        countText.Clear();
        vertCountText.Clear();

        countText.Append(stringCountText);
        vertCountText.Append(stringVertCountText);

        countText.Append(GetAllObjectsOnlyInScene().Count.ToString());
        vertCountText.Append(vertSum);

        ObjCount.text = countText.ToString();
        ObjVertCount.text = vertCountText.ToString();

    }

    

    List<GameObject> GetAllObjectsOnlyInScene()
    {
        List<GameObject> objectsInScene = new List<GameObject>();
 
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                objectsInScene.Add(go);
        }
 
        return objectsInScene;
    }
}

class GameObjectType
{
    public int id;
    public string name;
    public int vertCount;

}


