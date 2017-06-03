#if UNITY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
#define COHERENT_UNITY_STANDALONE
#endif

#if UNITY_NACL || UNITY_WEBPLAYER
#define COHERENT_UNITY_UNSUPPORTED_PLATFORM
#endif

using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE || COHERENT_UNITY_UNSUPPORTED_PLATFORM
namespace Coherent.UI
#elif UNITY_IPHONE || UNITY_ANDROID
namespace Coherent.UI.Mobile
#endif
{
	class UnityFileHandler : FileHandler
	{
		private Regex m_RangeRequestValue =
			new Regex("bytes=(?<From>\\d+)\\-(?<To>\\d*)",
					  RegexOptions.ExplicitCapture);

		private string GetFilepath(string url)
		{
			var asUri = new Uri(url);
			string cleanUrl;
			if(asUri.Scheme != "file") {
#if UNITY_EDITOR
			cleanUrl = asUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
			// Read resources from the project folder
			var uiResources = PlayerPrefs.GetString("CoherentUIResources");
			if (uiResources == string.Empty)
			{
				Debug.LogError("Missing path for Coherent Browser resources. Please select path to your resources via Edit -> Project Settings -> Coherent Browser -> Select UI Folder");
				// Try to fall back to the default location
				uiResources = Path.Combine(Path.Combine(Application.dataPath, "WebPlayerTemplates"), "uiresources");
				Debug.LogWarning("Falling back to the default location of the UI Resources in the Unity Editor: " + uiResources);
				PlayerPrefs.SetString("CoherentUIResources",
					"WebPlayerTemplates/uiresources");
			} else {
				uiResources = Path.Combine(Application.dataPath, uiResources);
			}
			cleanUrl = cleanUrl.Insert(0, uiResources + '/');
#else
			cleanUrl = asUri.GetComponents(UriComponents.Host | UriComponents.Path, UriFormat.Unescaped);
			// Read resources from the <executable>_Data folder
			cleanUrl = Application.dataPath + '/' + cleanUrl;
#endif
			}
			else {
				cleanUrl = asUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
			}
			return cleanUrl;
		}

#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE || COHERENT_UNITY_UNSUPPORTED_PLATFORM
		public override void ReadFile(string url, URLRequestBase request,
			ResourceResponse response)
		{
		#if COHERENT_UNITY_UNSUPPORTED_PLATFORM
			throw new ApplicationException("Coherent Browser doesn't support the target platform!");
		#else
			string cleanUrl = GetFilepath(url);

			if (!File.Exists(cleanUrl))
			{
				response.SignalFailure();
				return;
			}

			if (request.GetExtraHeaderIndex("Range") < 0)
			{
				DoCompleteRead(cleanUrl, request, response);
			}
			else
			{
				DoPartialRead(cleanUrl, request, response);
			}
		#endif
		}

		private void DoCompleteRead(string cleanUrl, URLRequestBase request,
									ResourceResponse response)
		{
			byte[] bytes = File.ReadAllBytes(cleanUrl);

			IntPtr buffer = response.GetBuffer((uint)bytes.Length);
			if (buffer == IntPtr.Zero)
			{
				response.SignalFailure();
				return;
			}

			Marshal.Copy(bytes, 0, buffer, bytes.Length);

			response.SetStatus(200);
			response.SignalSuccess();
		}

		private void DoPartialRead(string cleanUrl, URLRequestBase request,
								   ResourceResponse response)
		{
			string rangeValue = request.GetExtraHeader("Range");
			Match match = m_RangeRequestValue.Match (rangeValue);
			if (!match.Success)
			{
				response.SignalFailure();
				return;
			}

			long fileSize = new FileInfo(cleanUrl).Length;

			long startByte = long.Parse (match.Groups ["From"].Value);
			string endByteString = match.Groups ["To"].Value;
			long endByte = fileSize - 1;
			if (string.IsNullOrEmpty(endByteString))
			{
				// Clamp to a maximum chunk size
				const long MaxPartialReadSize = 16 * 1024 * 1024;
				if (endByte - startByte > MaxPartialReadSize)
				{
					endByte = startByte + MaxPartialReadSize;
				}
			}
			else
			{
				endByte = long.Parse(endByteString);
			}

			// Clamp to int.MaxValue since that's the type BinaryReader
			// allows us to read; if it could read more bytes, then we would
			// clamp the size to uint.MaxValue since ResourceResponse.GetBuffer
			// expects an uint value.
			long bufferSize = Math.Min((long)int.MaxValue,
									   endByte - startByte + 1);

			byte[] bytes = new byte[bufferSize];
			using (BinaryReader reader = new BinaryReader(
				new FileStream(cleanUrl, FileMode.Open)))
			{
				reader.BaseStream.Seek(startByte, SeekOrigin.Begin);
				reader.Read(bytes, 0, (int)bufferSize);
			}

			IntPtr buffer = response.GetBuffer((uint)bytes.Length);
			if (buffer == IntPtr.Zero)
			{
				response.SignalFailure();
				return;
			}

			Marshal.Copy(bytes, 0, buffer, bytes.Length);

			// Set required response headers
			response.SetStatus(206);
			response.SetResponseHeader("Accept-Ranges", "bytes");
			response.SetResponseHeader("Content-Range", "bytes " + startByte +
									   "-" + endByte + "/" + fileSize);
			response.SetResponseHeader("Content-Length",
									   bufferSize.ToString());

			response.SignalSuccess();
		}
#elif UNITY_IPHONE || UNITY_ANDROID
		public override void ReadFile(string url, ResourceResponse response)
		{
			string cleanUrl = GetFilepath(url);

			if (!File.Exists(cleanUrl))
			{
				response.SignalFailure();
				return;
			}

			byte[] bytes = File.ReadAllBytes(cleanUrl);

			IntPtr buffer = response.GetBuffer((uint)bytes.Length);
			if (buffer == IntPtr.Zero)
			{
				response.SignalFailure();
				return;
			}

			Marshal.Copy(bytes, 0, buffer, bytes.Length);

			response.SignalSuccess();
		}
#endif

		#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
		public override void WriteFile(string url, ResourceData resource)
		{
		#if COHERENT_UNITY_UNSUPPORTED_PLATFORM
			throw new ApplicationException("Coherent Browser doesn't support the target platform!");
		#else
			IntPtr buffer = resource.GetBuffer();
			if (buffer == IntPtr.Zero)
			{
				resource.SignalFailure();
				return;
			}

			byte[] bytes = new byte[resource.GetSize()];
			Marshal.Copy(buffer, bytes, 0, bytes.Length);

			string cleanUrl = GetFilepath(url);

			try
			{
				File.WriteAllBytes(cleanUrl, bytes);
			}
			catch (IOException ex)
			{
				Console.Error.WriteLine(ex.Message);
				resource.SignalFailure();
				return;
			}

			resource.SignalSuccess();
		#endif
		}
		#endif
	}
}
