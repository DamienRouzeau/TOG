namespace FindMissingScriptsRecursively.Editor
{
    using UnityEngine;
    using UnityEditor;

    using System.Collections;
    using System.IO;

    public class FindMissingScriptsRecursively : EditorWindow 
	{
		private static int go_count;
		private static int components_count;
		private static int missing_count;
 
		[MenuItem("Tools/Find/MissingScriptsRecursively %&f")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(FindMissingScriptsRecursively));
		}
 
		public void OnGUI()
		{
			if (GUILayout.Button("Find Missing Scripts in selected GameObjects"))
			{
				FindInSelected();
			}
		
			if (GUILayout.Button("Find Missing Scripts in all prefabs"))
			{
				FindInPrefabs();
			}
		}
		private static void FindInSelected()
		{
			GameObject[] go = Selection.gameObjects;
			go_count = 0;
			components_count = 0;
			missing_count = 0;
			foreach (GameObject g in go)
			{
   				FindInGO(g);
			}
			Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
		}
	
		void FindInPrefabs()
		{
			string[] files;
			GameObject obj;
			
			go_count = 0;
			components_count = 0;
			missing_count = 0;

			// Stack of folders:
			Stack stack = new Stack();

			// Add root directory:
			stack.Push(Application.dataPath);

			// Continue while there are folders to process
			while (stack.Count > 0)
			{
				// Get top folder:
				string dir = (string)stack.Pop();

				try
				{
					// Get a list of all prefabs in this folder:
					files = Directory.GetFiles(dir, "*.prefab");

					// Process all prefabs:
					for (int i = 0; i < files.Length; ++i)
					{
						// Make the file path relative to the assets folder:
						files[i] = files[i].Substring(Application.dataPath.Length - 6);

						obj = (GameObject)AssetDatabase.LoadAssetAtPath(files[i], typeof(GameObject));

						if (obj != null)
						{
							FindInGO(obj);
						}
					}

					// Add all subfolders in this folder:
					foreach (string dn in Directory.GetDirectories(dir))
					{
						stack.Push(dn);
					}
				}
				catch
				{
					// Error
					Debug.LogError("Could not access folder: \"" + dir + "\"");
				}
			}
			Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
		}
 
		private static void FindInGO(GameObject g)
		{
			go_count++;
			Component[] components = g.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				components_count++;
				if (components[i] == null)
				{
				missing_count++;
				Debug.Log(g.name + " has an empty script attached in position: " + i + " root " + g.transform.root.name, g );
				}
			}
		
			// Now recurse through each child GO (if there are any):
			foreach (Transform childT in g.transform)
			{
				FindInGO(childT.gameObject);
			}
		}
	}
}
