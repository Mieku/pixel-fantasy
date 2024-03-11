/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace Databrain.Modules.Import
{
	public class DatabrainGoogleSheetDownloader
	{
		public static void Download(DataLibrary _dataContainer, string _namespace, ImportModule.GoogleImportSettings.ImportType _importType, Action _onCompleteCallback)
		{
#if UNITY_EDITOR
            Databrain.Helpers.EditorCoroutines.Execute(DownloadIE(_dataContainer, _namespace, _importType, _onCompleteCallback));
#endif
		}

		static IEnumerator DownloadIE(DataLibrary _dataContainer, string _namespace, ImportModule.GoogleImportSettings.ImportType _importType, Action _onCompleteCallback)
		{

			var _importModule = _dataContainer.GetModule("ImportModule") as ImportModule;

			for (int n = 0; n < _importModule.googleImportSettings.Count; n++)
			{
				if (_importModule.googleImportSettings[n].nameSpace == _namespace)
				{

					for (int i = 0; i < _importModule.googleImportSettings[n].googleWorksheets.Count; i++)
					{
						if (string.IsNullOrEmpty(_importModule.googleImportSettings[n].googleWorksheets[i].worksheetID))
							continue;

						var _url = _importModule.googleImportSettings[n].googleShareURL;

						_url = FixURL(_url, _importModule.googleImportSettings[n].googleWorksheets[i].worksheetID);


						UnityWebRequest _download = UnityWebRequest.Get(_url);

						yield return _download.SendWebRequest();

						while (!_download.isDone)
							yield return null;

						if (_download.result == UnityWebRequest.Result.ConnectionError)
						{

						}
						else
						{
							if (_download.downloadHandler.text.Contains("google-site-verification"))
							{
								continue;
							}

							List<DatabrainCSVConverter.Entry> _entries = DatabrainCSVConverter.ConvertCSV(_download.downloadHandler.text);
                            DatabrainCSVConverter.BuildDataFromCSV(_dataContainer, _entries, _importModule.googleImportSettings[n].googleWorksheets[i].worksheetTypeName, _importModule.googleImportSettings[n].importType.ToString());

						}

						_download.Dispose();
					}
				}
			}

			_onCompleteCallback.Invoke();
		}


		public static Rect ConvertToRectFromString(string _value)
		{
			System.Globalization.CultureInfo _ci = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
			_ci.NumberFormat.CurrencyDecimalSeparator = ".";


			string[] _chars = _value.Split(char.Parse(","));
			Rect _rect = new Rect(float.Parse(_chars[0], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[1], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[2], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[3], System.Globalization.NumberStyles.Any, _ci));

			return _rect;
		}


		public static Quaternion ConvertToQuaternionFromString(string _value)
		{
			System.Globalization.CultureInfo _ci = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
			_ci.NumberFormat.CurrencyDecimalSeparator = ".";


			string[] _chars = _value.Split(char.Parse(","));
			Quaternion _q = new Quaternion(float.Parse(_chars[0], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[1], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[2], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[3], System.Globalization.NumberStyles.Any, _ci));

			return _q;
		}

		public static Vector2 ConvertToVector2FromString(string _value)
		{
			System.Globalization.CultureInfo _ci = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
			_ci.NumberFormat.CurrencyDecimalSeparator = ".";

			string[] _chars = _value.Split(char.Parse(","));

			Vector2 value = new Vector2(float.Parse(_chars[0], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[1], System.Globalization.NumberStyles.Any, _ci));

			return value;
		}

		public static Vector3 ConvertToVector3FromString(string _value)
		{
			System.Globalization.CultureInfo _ci = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
			_ci.NumberFormat.CurrencyDecimalSeparator = ".";

			string[] _chars = _value.Split(char.Parse(","));

			Vector3 value = new Vector3(float.Parse(_chars[0], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[1], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[2], System.Globalization.NumberStyles.Any, _ci));

			return value;
		}

		public static Vector4 ConvertToVector4FromString(string _value)
		{
			System.Globalization.CultureInfo _ci = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture.Clone();
			_ci.NumberFormat.CurrencyDecimalSeparator = ".";

			string[] _chars = _value.Split(char.Parse(","));

			Vector4 value = new Vector4(float.Parse(_chars[0], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[1], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[2], System.Globalization.NumberStyles.Any, _ci),
				float.Parse(_chars[3], System.Globalization.NumberStyles.Any, _ci));

			return value;
		}

		public static string FixURL(string url, string gId)
		{
			// if it's a Google Docs URL, then grab the document ID and reformat the URL
			if (url.StartsWith("https://docs.google.com/document/d/"))
			{
				var docID = url.Substring("https://docs.google.com/document/d/".Length, 44);
				return string.Format("https://docs.google.com/document/export?gid={1}&format=txt&id={0}&includes_info_params=true", docID, gId);
			}
			if (url.StartsWith("https://docs.google.com/spreadsheets/d/"))
			{
				var docID = url.Substring("https://docs.google.com/spreadsheets/d/".Length, 44);
				return string.Format("https://docs.google.com/spreadsheets/export?gid={1}&format=csv&id={0}", docID, gId);
			}

			return url;
		}

	}
}
#endif