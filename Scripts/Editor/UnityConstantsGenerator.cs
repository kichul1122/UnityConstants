using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Audio;

namespace UnityToolbag
{
	public static class UnityConstantsGenerator
	{
		[MenuItem("Edit/Generate Unity Constants", priority = 1000)]
		static void Generate()
		{
			string path = GetExistingFile();
			//If no such file exists already, use the save panel to get a folder in which the file will be placed.
			if (path == null)
			{
				string directory = EditorUtility.OpenFolderPanel("Choose location for UnityConstants.cs", Application.dataPath, "");
				if (string.IsNullOrEmpty(directory))
				{
					return;
				}
				path = Path.Combine(directory, "UnityConstants.cs");
			}
			//Write out our file
			using (var writer = new StreamWriter(path))
			{
				writer.WriteLine("// This file is auto-generated. Modifications are not saved.");
				writer.WriteLine();
				writer.WriteLine("namespace UnityConstants");
				writer.WriteLine("{");
				WriteTags(writer);
				WriteSortingLayers(writer);
				WriteLayers(writer);
				WriteScenes(writer);
				WriteInputManagerAxes(writer);
				WriteAudioMixerParams(writer);
				WriteAnimatorParams(writer);
				//End of namespace UnityConstants
				writer.WriteLine("}");
				writer.WriteLine();
			}
			AssetDatabase.Refresh();
		}

		static string GetExistingFile()
		{
			//Try to find an existing file in the project called "UnityConstants.cs"
			foreach (var file in Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories))
			{
				if (Path.GetFileNameWithoutExtension(file) == "UnityConstants")
				{
					return file;
				}
			}
			return null;
		}

		static void WriteTags(StreamWriter writer)
		{
			//Write out the tags
			writer.WriteLine("    public static class Tags");
			writer.WriteLine("    {");
			foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
			{
				writer.WriteLine("        /// <summary>");
				writer.WriteLine("        /// Name of tag '{0}'.", tag);
				writer.WriteLine("        /// </summary>");
				writer.WriteLine("        public const string {0} = \"{1}\";", MakeSafeForCode(tag), tag);
			}
			writer.WriteLine("    }");
			writer.WriteLine();
		}

		static void WriteSortingLayers(StreamWriter writer)
		{
			//Write out sorting layers
			writer.WriteLine("    public static class SortingLayers");
			writer.WriteLine("    {");
			foreach (var layer in SortingLayer.layers)
			{
				writer.WriteLine("        /// <summary>");
				writer.WriteLine("        /// ID of sorting layer '{0}'.", layer.name);
				writer.WriteLine("        /// </summary>");
				writer.WriteLine("        public const int {0} = {1};", MakeSafeForCode(layer.name), layer.id);
			}
			writer.WriteLine("    }");
			writer.WriteLine();
		}

		static void WriteLayers(StreamWriter writer)
		{
			//Write out layers
			writer.WriteLine("    public static class Layers");
			writer.WriteLine("    {");
			for (int i = 0; i < 32; i++)
			{
				string layer = UnityEditorInternal.InternalEditorUtility.GetLayerName(i);
				if (!string.IsNullOrEmpty(layer))
				{
					writer.WriteLine("        /// <summary>");
					writer.WriteLine("        /// Index of layer '{0}'.", layer);
					writer.WriteLine("        /// </summary>");
					writer.WriteLine("        public const int {0} = {1};", MakeSafeForCode(layer), i);
				}
			}
			writer.WriteLine();
			for (int i = 0; i < 32; i++)
			{
				string layer = UnityEditorInternal.InternalEditorUtility.GetLayerName(i);
				if (!string.IsNullOrEmpty(layer))
				{
					writer.WriteLine("        /// <summary>");
					writer.WriteLine("        /// Bitmask of layer '{0}'.", layer);
					writer.WriteLine("        /// </summary>");
					writer.WriteLine("        public const int {0}Mask = 1 << {1};", MakeSafeForCode(layer), i);
				}
			}
			writer.WriteLine("    }");
			writer.WriteLine();
		}

		static void WriteScenes(StreamWriter writer)
		{
			//Write out scenes
			writer.WriteLine("    public static class Scenes");
			writer.WriteLine("    {");
			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				string scene = Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);
				writer.WriteLine("        /// <summary>");
				writer.WriteLine("        /// ID of scene '{0}'.", scene);
				writer.WriteLine("        /// </summary>");
				writer.WriteLine("        public const int {0} = {1};", MakeSafeForCode(scene), i);
			}
			writer.WriteLine("    }");
			writer.WriteLine();
		}

		static void WriteInputManagerAxes(StreamWriter writer)
		{
			//Write out Input axes
			writer.WriteLine("    public static class Axes");
			writer.WriteLine("    {");
			HashSet<string> axes = new HashSet<string>();
			var inputManagerProp = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
			foreach (SerializedProperty axe in inputManagerProp.FindProperty("m_Axes"))
			{
				var name = axe.FindPropertyRelative("m_Name").stringValue;
				var variableName = MakeSafeForCode(name);
				if (!axes.Contains(variableName))
				{
					writer.WriteLine("        /// <summary>");
					writer.WriteLine("        /// Input axis '{0}'.", name);
					writer.WriteLine("        /// </summary>");
					writer.WriteLine("        public const string {0} = \"{1}\";", variableName, name);
					axes.Add(variableName);
				}
			}
			writer.WriteLine("    }");
			writer.WriteLine();
		}

		static void WriteAudioMixerParams(StreamWriter writer)
		{
			//Write out Audio mixer exposed parameters
			writer.WriteLine("    public static class AudioMixerParams");
			writer.WriteLine("    {");
			string[] mixers = AssetDatabase.FindAssets("t:AudioMixer");
			HashSet<string> exposedParameters = new HashSet<string>();
			foreach (string guid in mixers)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				SerializedObject mixer = new SerializedObject(AssetDatabase.LoadAssetAtPath<AudioMixer>(path));
				foreach (SerializedProperty param in mixer.FindProperty("m_ExposedParameters"))
				{
					var name = param.FindPropertyRelative("name").stringValue;
					var variableName = MakeSafeForCode(name);
					if (!exposedParameters.Contains(variableName))
					{
						writer.WriteLine("        /// <summary>");
						writer.WriteLine("        /// Audio mixer exposed parameter '{0}'.", name);
						writer.WriteLine("        /// </summary>");
						writer.WriteLine("        public const string {0} = \"{1}\";", variableName, name);
						exposedParameters.Add(variableName);
					}
				}
			}
			writer.WriteLine("    }");
			writer.WriteLine();
		}

		static void WriteAnimatorParams(StreamWriter writer)
		{
			//Write out Audio mixer exposed parameters
			writer.WriteLine("    public static class AnimatorParams");
			writer.WriteLine("    {");
			string[] animators = AssetDatabase.FindAssets("t:AnimatorController");
			HashSet<string> enimatorParameters = new HashSet<string>();
			foreach (string guid in animators)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				SerializedObject mixer = new SerializedObject(AssetDatabase.LoadAssetAtPath<AnimatorController>(path));
				foreach (SerializedProperty param in mixer.FindProperty("m_AnimatorParameters"))
				{
					var name = param.FindPropertyRelative("m_Name").stringValue;
					var variableName = MakeSafeForCode(name);
					if (!enimatorParameters.Contains(variableName))
					{
						writer.WriteLine("        /// <summary>");
						writer.WriteLine("        /// Animator controller exposed parameter '{0}'.", name);
						writer.WriteLine("        /// </summary>");
						writer.WriteLine("        public const string {0} = \"{1}\";", variableName, name);
						enimatorParameters.Add(variableName);
					}
				}
			}
			writer.WriteLine("    }");
		}

		static string MakeSafeForCode(string str)
		{
			str = Regex.Replace(str, "[^a-zA-Z0-9_]", "_", RegexOptions.Compiled);
			if (char.IsDigit(str[0]))
			{
				str = "_" + str;
			}
			return str;
		}
	}
}