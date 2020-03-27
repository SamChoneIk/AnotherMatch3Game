using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Text;
using System.IO;

[CustomEditor(typeof(UIManager))]
public class UIManagerEditor : Editor
{
	private ReorderableList list;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		list.DoLayoutList();

		serializedObject.ApplyModifiedProperties();

		if (GUILayout.Button("Generate Menus Enum"))
		{
			var uIMgr = target as UIManager;

			var sb = new StringBuilder();
			sb.Append("public enum Menus\n{\n\tNone = -1,\n");
			for (var i = 0; i < uIMgr.menus.Length; i++)
			{
				sb.Append("\t");
				sb.Append(uIMgr.menus[i].name);
				sb.Append(",\n");
			}
			sb.Append("}\n");
			// 저장 대화상자
			var path = EditorUtility.SaveFilePanel("Save The Menus Enums", "", "Menus.cs", "cs");

			using (var fs = new FileStream(path, FileMode.Create))
			{
				using (var sw = new StreamWriter(fs))
				{
					sw.Write(sb.ToString());
				}
			}

			AssetDatabase.Refresh();
		}
	}

	private void OnEnable()
	{
		// windows 배열과 연관을 갖게됨
		list = new ReorderableList(serializedObject, serializedObject.FindProperty("menus")
			, true, true, true, true);
		// 델리게이트 형
		list.drawHeaderCallback = (Rect rect) =>
		{
			EditorGUI.LabelField(rect, "menus");
		};
		// 각각의 요소마다 호출
		// 배열하나에 대한 serializedProperty가 넘어온다.
		list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
		{
			var element = list.serializedProperty.GetArrayElementAtIndex(index);
			EditorGUI.PropertyField(new Rect(rect.x, rect.y, Screen.width - 75,
				EditorGUIUtility.singleLineHeight), element, GUIContent.none);
		};
	}
}
