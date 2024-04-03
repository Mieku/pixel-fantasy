/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace Databrain.Modules.Import
{
	public class DatabrainCSVConverter
	{

		static string ignoreKey = "IGNORE";
		static string fieldNameKey = "FIELD_NAMES";
		static string fieldTypeKey = "FIELD_TYPES";

		[System.Serializable]
		public class Entry
		{
			public string entryName = "";
			public List<string> fields = new List<string>();
			public List<string> types = new List<string>();
			public List<string> values = new List<string>();

			public Entry(string _entryName)
			{
				entryName = _entryName;
			}
		}


		public static List<Entry> ConvertCSV(string _input)
		{
			var _entries = new List<Entry>();

			var _output = DatabrainCSVReader.SplitCSV(_input);
			var _fieldTypesIndex = 0;
			var _fieldNamesIndex = 0;

			foreach (var row in _output.Keys)
			{
				for (int i = 0; i < _output[row].Count; i++)
				{

					if (_output[row][i] == fieldTypeKey)
					{
						_fieldTypesIndex = i;
					}

					if (_output[row][i] == fieldNameKey)
					{
						_fieldNamesIndex = i;
					}

					//// Entry names
					if (row == 0 && i != _fieldNamesIndex && i != _fieldTypesIndex)
					{

						var _entryIndex = i;

						if (!string.IsNullOrEmpty(_output[row][i]) && _output[row][i] != ignoreKey)
						{
							_entries.Add(new Entry(_output[row][i]));

							foreach (var row2 in _output.Keys)
							{
								for (int i2 = 0; i2 < _output[row2].Count; i2++)
								{
									// field names
									if (row2 > 0 && i2 == _fieldNamesIndex)
									{
										if (!string.IsNullOrEmpty(_output[row2][i2]))
										{
											_entries[_entries.Count - 1].fields.Add(_output[row2][i2]);
										}
									}

									// field types
									if (row2 > 0 && i2 == _fieldTypesIndex)
									{
										if (!string.IsNullOrEmpty(_output[row2][i2]))
										{
											_entries[_entries.Count - 1].types.Add(_output[row2][i2]);
										}
									}
									// values
									if (row2 > 0 && i2 == _entryIndex && i2 != _fieldTypesIndex && i2 != _fieldNamesIndex)
									{
										_entries[_entries.Count - 1].values.Add(_output[row2][i2]);
									}
								}
							}

						}
					}
				}
			}


			// cleanup
			for (int e = 0; e < _entries.Count; e++)
			{
				for (int f = 0; f < _entries[e].fields.Count; f++)
				{
					if (_entries[e].fields[f] == ignoreKey)
					{
						_entries[e].types[f] = ignoreKey;
						_entries[e].values[f] = ignoreKey;
						_entries[e].fields[f] = ignoreKey;
					}
				}

				for (int v = 0; v < _entries[e].values.Count; v++)
				{
					if (string.IsNullOrEmpty(_entries[e].values[v]))
					{
						if (v < _entries[e].types.Count)
						{
							_entries[e].types[v] = ignoreKey;
						}

						if (v < _entries[e].fields.Count)
						{
							_entries[e].fields[v] = ignoreKey;
						}

						if (v < _entries[e].values.Count)
						{
							_entries[e].values[v] = ignoreKey;
						}
					}
				}
			}


			// remove all entries which contains the ignore key
			for (int e = 0; e < _entries.Count; e++)
			{
				for (int v = _entries[e].values.Count - 1; v >= 0; v--)
				{
					if (_entries[e].values[v].Contains(ignoreKey))
					{
						_entries[e].values.RemoveAt(v);
					}
				}

				for (int t = _entries[e].types.Count - 1; t >= 0; t--)
				{
					if (_entries[e].types[t].Contains(ignoreKey))
					{

						_entries[e].types.RemoveAt(t);
					}
				}

				for (int f = _entries[e].fields.Count - 1; f >= 0; f--)
				{
					if (_entries[e].fields[f].Contains(ignoreKey))
					{
						_entries[e].fields.RemoveAt(f);
					}
				}
			}

			_entries.Reverse();

			return _entries;
		}


		public static void BuildDataFromCSV(DataLibrary _dataContainer, List<Entry> _entries, string _typeName, string _importType)
		{
#if UNITY_EDITOR

			for (int e = 0; e < _entries.Count; e++)
			{
                Dictionary<string, System.Type> _collectedTypes = new Dictionary<string, System.Type>();

				foreach (System.Reflection.Assembly b in System.AppDomain.CurrentDomain.GetAssemblies())
				{
					System.Type[] assemblyTypes = b.GetTypes();
					for (int j = 0; j < assemblyTypes.Length; j++)
					{
						var _str = _typeName.Trim();
						if (assemblyTypes[j].Name == _str)
						{

							string _tmpNamespace = "base";
							if (!string.IsNullOrEmpty(assemblyTypes[j].Namespace))
							{
								_tmpNamespace = assemblyTypes[j].Namespace;
							}

							//Debug.Log(assemblyTypes[j].Namespace + " :: " + assemblyTypes[j].Name);

							if (!_collectedTypes.ContainsKey(_tmpNamespace))
							{
								_collectedTypes.Add(_tmpNamespace, assemblyTypes[j]);
							}
							else
							{
								//Debug.Log("already added");
							}
						}
					}
				}


				//// There could be more than one type with the same name in different namespaces
				//// In this case we use the most obvious one which is in no namespace
				//// Occurs with ResourceType. Which conflicts with:
				////.....
				//// ResourceType in namespace System.Security.AccessControl
				//// ResourceType in namespace Mono.Cecil
				////.....
				System.Type _foundType = null;

				if (_collectedTypes.Count > 1)
				{
					foreach (var _n in _collectedTypes.Keys)
					{
						if (_n.Contains("base") || _n.Contains("Databrain"))
						{
							_foundType = _collectedTypes[_n];
						}
					}
				}
				else
				{
					_foundType = _collectedTypes.FirstOrDefault().Value;
				}


                // Create Instance

                DataObject _dataObject = null;
				//Debug.Log(_importType);
				if (_importType == "Append")
				{
					_dataObject = DataObjectCreator.CreateNewDataObject(_dataContainer, _foundType);
				}
				else
				{
					// Search for existing data object so we can replace it.
                    _dataObject = _dataContainer.GetInitialDataObjectByTitle(_entries[e].entryName, _foundType);
					if (_dataObject == null)
					{
						_dataObject = DataObjectCreator.CreateNewDataObject(_dataContainer, _foundType);
					}
					
				}

				
				_dataObject.title = _entries[e].entryName;

                for (int a = 0; a < _entries[e].values.Count; a++)
				{
					var _so = new SerializedObject(_dataObject);
					SerializedProperty _iterator = _so.GetIterator();
					_iterator.NextVisible(true);


					while (_iterator.NextVisible(false))
					{

                        var _fieldName = System.Text.RegularExpressions.Regex.Replace(_entries[e].fields[a], @"\s+", "");
						//Debug.Log(_iterator.propertyPath + " __ " + _fieldName.ToLower());

						if (_iterator.propertyPath.ToLower() == _fieldName.ToLower())
						{
							bool _defaultTypesAssigned = false;

							switch (_entries[e].types[a].ToLower())
							{
								case "string":

									_iterator.stringValue = _entries[e].values[a];
									_defaultTypesAssigned = true;

                                    break;
								case "int":
								case "integer":

									_iterator.intValue = int.Parse(_entries[e].values[a]);
                                    _defaultTypesAssigned = true;
                                    break;
								case "float":

									_iterator.floatValue = float.Parse(_entries[e].values[a]);
                                    _defaultTypesAssigned = true;
                                    break;
								case "bool":
								case "boolean":

									_iterator.boolValue = bool.Parse(_entries[e].values[a]);
                                    _defaultTypesAssigned = true;
                                    break;
								case "double":

									_iterator.doubleValue = double.Parse(_entries[e].values[a]);
                                    _defaultTypesAssigned = true;
                                    break;
								case "vector4":

									_iterator.vector4Value = ConvertToVector4FromString(_entries[e].values[a]);
                                    _defaultTypesAssigned = true;
                                    break;
								case "vector3":

									_iterator.vector3Value = ConvertToVector3FromString(_entries[e].values[a]);
                                    _defaultTypesAssigned = true;
                                    break;
								case "vector2":

									_iterator.vector2Value = ConvertToVector2FromString(_entries[e].values[a]);
                                    _defaultTypesAssigned = true;
                                    break;
								case "quaternion":

									_iterator.quaternionValue = ConvertToQuaternionFromString(_entries[e].values[a]);
                                    _defaultTypesAssigned = true;
                                    break;
								case "rect":

									_iterator.rectValue = ConvertToRectFromString(_entries[e].values[a]);
                                    _defaultTypesAssigned = true;
                                    break;				
							}


							// Lists
							if (!_defaultTypesAssigned)
							{
                                var _field = _dataObject.GetType().GetField(_iterator.propertyPath);

                                switch (_entries[e].types[a].ToLower())
                                {                                  
									case "list<string>":
										var stringList = _entries[e].values[a].Split(',')
											.ToList();

										_field.SetValue(_dataObject, stringList);

										break;
									case "list<int>":
                                        var intList = _entries[e].values[a].Split(',')
											.Where(m => int.TryParse(m, out _))
											.Select(m => int.Parse(m))
											.ToList();

                                        _field.SetValue(_dataObject, intList);
                                        break;
                                    case "list<float>":
                                        var floatList = _entries[e].values[a].Split(',')
                                          .Where(m => float.TryParse(m, out _))
                                          .Select(m => float.Parse(m))
                                          .ToList();

                                        _field.SetValue(_dataObject, floatList);
                                        break;
									case "list<bool>":
                                        var boolList = _entries[e].values[a].Split(',')
                                         .Where(m => bool.TryParse(m, out _))
                                         .Select(m => bool.Parse(m))
                                         .ToList();

                                        _field.SetValue(_dataObject, boolList);
                                        break;
                                }
							}

							_iterator.serializedObject.ApplyModifiedProperties();
						}
					}

					//Debug.Log(_entries[e].types[a]);
					//Debug.Log(_entries[e].entryName + "\n" + _entries[e].values[a] + "\n" + _entries[e].fields[a]);
				}

				//Debug.Log("CONVERT CSV " + _dataObject.name + " " + _dataObject.GetType().Name);
				_dataObject.ConvertFromCSV(_entries[e]);
			}
#endif
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
