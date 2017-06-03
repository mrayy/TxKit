#if UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1
// Coherent Browser supports Linux for Unity3D 4.2+
#define COHERENT_UNITY_PRE_4_2
#endif

using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;

public partial class CoherentPostProcessor {

	public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);
		DirectoryInfo[] dirs = dir.GetDirectories();

		if (!dir.Exists)
		{
			throw new DirectoryNotFoundException(
				"Source directory does not exist or could not be found: "
				+ sourceDirName);
		}

		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}

		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files)
		{
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, true);
		}

		if (copySubDirs)
		{
			foreach (DirectoryInfo subdir in dirs)
			{
				string temppath = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, copySubDirs);
			}
		}
	}
	
	private static void DeleteFileIfExists(string path)
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}
	
	private static void PackageActivator(BuildTarget target, string outDir, string projName)
	{
		switch (target)
		{
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			{
				string activatorExecutable = Path.Combine(Application.dataPath, "CoherentUI/Activator/Activator.exe");
				if (File.Exists(activatorExecutable))
				{
					string targetDir = string.Format("{0}/{1}_Data", outDir, projName);
					File.Copy(activatorExecutable, Path.Combine(targetDir, "Activator.exe"));
				}
			}
			break;
#if !COHERENT_UNITY_PRE_4_2
		case BuildTarget.StandaloneOSXIntel64:
		case BuildTarget.StandaloneOSXUniversal:
#endif
		case BuildTarget.StandaloneOSXIntel:
			{
				string activatorExecutable = Path.Combine(Application.dataPath, "CoherentUI/Activator/Activator.app");
				if (Directory.Exists(activatorExecutable))
				{
					string targetDir = string.Format("{0}/{1}.app/Contents/Activator.app", outDir, projName);
					DirectoryCopy(activatorExecutable, targetDir, true);
				}
			}
			break;
#if !COHERENT_UNITY_PRE_4_2
		case BuildTarget.StandaloneLinux64:
			{
				string activatorExecutable = Path.Combine(Application.dataPath, "CoherentUI/Activator/Activator");
				if (File.Exists(activatorExecutable))
				{
					string targetDir = string.Format("{0}/{1}_Data", outDir, projName);
					File.Copy(activatorExecutable, Path.Combine(targetDir, "Activator"));
				}
			}
			break;
#endif
		}
	}
	
	private static bool IsTargetPlatformSupported(BuildTarget target)
	{
		return target == BuildTarget.Android ||
			target == BuildTarget.iOS ||
#if !COHERENT_UNITY_PRE_4_2
			target == BuildTarget.StandaloneLinux64 ||
			target == BuildTarget.StandaloneOSXIntel64 ||
			target == BuildTarget.StandaloneOSXUniversal ||
#endif
			target == BuildTarget.StandaloneOSXIntel ||
			target == BuildTarget.StandaloneWindows ||
			target == BuildTarget.StandaloneWindows64;
	}
	
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
		if (!IsTargetPlatformSupported(target))
		{
			Debug.Log("Trying to build Coherent Browser"
						+ " for Unsupported target (" + target + ")!");
			return;
		}

		var outDir = Path.GetDirectoryName(pathToBuiltProject);
		var projName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
		var resourcesFolder = PlayerPrefs.GetString("CoherentUIResources");

		if (target != BuildTarget.Android && target != BuildTarget.iOS)
		{
			//Copy the host directory from CoherentUI/Binaries
			CopyPlatformResources(target, outDir, projName);
		}

#if !COHERENT_UNITY_PRE_4_2
		if (target == BuildTarget.Android)
		{
			outDir = pathToBuiltProject;
			projName = PlayerSettings.productName;
		}
#endif

		// check for per-project override
		if(string.IsNullOrEmpty(resourcesFolder))
		{
			FieldInfo projectUIResourcesStr = typeof(CoherentPostProcessor).GetField("ProjectUIResources", BindingFlags.Public | BindingFlags.Static);
			if(projectUIResourcesStr != null) 
			{
				string projectResFolder = (string)projectUIResourcesStr.GetValue(null);
				Debug.Log(String.Format("[Coherent Browser]: Found project resources folder: {0}", projectResFolder));
				resourcesFolder = projectResFolder;
			}
		}
		
		bool buildingAndroidApk = false;
		string androidUnpackDir = Path.Combine(Application.dataPath, "../Temp/CouiApkRepack");
		if (target == BuildTarget.Android && pathToBuiltProject.EndsWith(".apk"))
		{
			buildingAndroidApk = true;
		}
		
		if (buildingAndroidApk)
		{
			AndroidPostProcessor.FindAndCopySdkAapt();
			AndroidPostProcessor.UnpackAPK(pathToBuiltProject, androidUnpackDir);
		}

		// copy the UI resources
		if(!string.IsNullOrEmpty(resourcesFolder))
		{
			var lastDelim = resourcesFolder.LastIndexOf('/');
			string folderName = lastDelim != -1 ? resourcesFolder.Substring(lastDelim) : resourcesFolder;
		
			StringBuilder outputDir = new StringBuilder(outDir);
			string uiResourcesFormat = null;
			switch(target)
			{
#if !COHERENT_UNITY_PRE_4_2
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.StandaloneOSXUniversal:
#endif
			case BuildTarget.StandaloneOSXIntel:
				uiResourcesFormat = "/{0}.app/Contents/{1}";
				break;
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
#if !COHERENT_UNITY_PRE_4_2
			case BuildTarget.StandaloneLinux64:
#endif
				uiResourcesFormat = "/{0}_Data/{1}";
				break;
			case BuildTarget.iOS:
				uiResourcesFormat = "/{0}/Data/{1}";
				break;
			case BuildTarget.Android:
				uiResourcesFormat = "/{0}/assets/{1}"; // Format for exported Eclipse project
				break;
			default:
				throw new System.ApplicationException
					("Unsupported by Coherent Browser build target");
			}

			var inDir = Path.Combine(Application.dataPath, resourcesFolder);

			if (!Directory.Exists(inDir))
			{
				resourcesFolder = Path.Combine("..", resourcesFolder);
				inDir = Path.Combine(Application.dataPath, resourcesFolder);
			}
			
			if (buildingAndroidApk)
			{
				outputDir = new StringBuilder(Path.Combine(androidUnpackDir, "assets/" + folderName));
			}
			else
			{
				outputDir.AppendFormat(uiResourcesFormat, projName, folderName);
			}
			
			DirectoryCopy(inDir.ToString(), outputDir.ToString(), true);
		}
		
		#if COHERENT_UI_EVALUATION_UNITY3D
			PackageActivator(target, outDir, projName);
		#endif

		switch (target)
		{
		case BuildTarget.iOS:
			IOSPostProcessor.PostProcess(pathToBuiltProject);
			break;
		case BuildTarget.Android:
			if (buildingAndroidApk)
			{
				AndroidPostProcessor.RepackAPK
						(pathToBuiltProject, androidUnpackDir);
			}
			else
			{
				bool apiLevel11OrGreater = (PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel11);
				AndroidPostProcessor.ModifyManifestFile(string.Format("{0}/{1}/AndroidManifest.xml", outDir, projName), apiLevel11OrGreater);
				AndroidPostProcessor.CleanUpForAndroid
					(string.Format("{0}/{1}/Plugins", outDir, projName));
			}
			break;
		}
	}

	private static void CopyPlatformResources(BuildTarget target,
												string pathToBuiltProject,
												string projName)
	{
		string hostDir = "Binaries/CoherentUI_Host/";
		string outDataFormat = "";

		switch (target)
		{
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			hostDir = Path.Combine(hostDir, "windows");
			outDataFormat = "/{0}_Data/";
			break;
		#if !COHERENT_UNITY_PRE_4_2
		case BuildTarget.StandaloneLinux64:
			hostDir = Path.Combine(hostDir, "linux");
			outDataFormat += "/{0}_Data/";
			break;
		case BuildTarget.StandaloneOSXIntel64:
		case BuildTarget.StandaloneOSXUniversal:
			hostDir = Path.Combine(hostDir, "macosx");
			outDataFormat += "/{0}.app/Contents/";
			break;
		#endif
		case BuildTarget.StandaloneOSXIntel:
			hostDir = Path.Combine(hostDir, "macosx");
			outDataFormat += "/{0}.app/Contents/";
			break;
		default:
			throw new System.ApplicationException
					("Unsupported by Coherent Browser build target");
		}

		StringBuilder outputDir = new StringBuilder("");
		outputDir.AppendFormat(outDataFormat, projName, hostDir);

		string finalInHostDir = Application.dataPath + "/Libraries/CoherentUI/" + hostDir;
		string finalOutHostDir = "";

		//Note: finalOutHostDir is made of concantenated strings because:
		//1. Path.Combine takes only two arguments
		//2. Path.Combine does not concantenate strings that start with a
		//slash.

		if(target == BuildTarget.StandaloneOSXIntel
		#if !COHERENT_UNITY_PRE_4_2
		   || target == BuildTarget.StandaloneOSXIntel64
		   || target == BuildTarget.StandaloneOSXUniversal
		#endif
		   )
		{
			finalOutHostDir = pathToBuiltProject
								+ outputDir.ToString()
								+ "Data/CoherentUI_Host/macosx";
		}
		else
		{
			string hostBinaryDirLinux = "CoherentUI_Host/linux";
			string hostBinaryDirWin = "CoherentUI_Host/windows";

		#if COHERENT_UNITY_PRE_4_2
			if(target == BuildTarget.StandaloneLinux)
		#else
			if(target == BuildTarget.StandaloneLinux64)
		#endif
			{
				finalOutHostDir = pathToBuiltProject
									+ outputDir.ToString()
									+ hostBinaryDirLinux;
			}
			else
			{
				finalOutHostDir = pathToBuiltProject
									+ outputDir.ToString()
									+ hostBinaryDirWin;
			}
		}

		DirectoryCopy( finalInHostDir, finalOutHostDir, true);
	}
}

