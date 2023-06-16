//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;
//using Newtonsoft.Json.Linq;

//public class WebGet : MonoBehaviour
//{
//	[ContextMenu("Do Get")]
//    void DoGet() {
//		StartCoroutine(GetCoroutine());
//	}

//	IEnumerator GetCoroutine() {
//		string url = "https://api.github.com/repos/hku-ect/gazebosc/pulls?state=all";

//		using (UnityWebRequest www = UnityWebRequest.Get(url)) {
//			yield return www.SendWebRequest();

//			if (www.error == null) {				
//				Debug.Log(www.downloadHandler.text);

//				// Parse Array instance (github returns an array of json objects
//				JArray arr = JArray.Parse(www.downloadHandler.text);

//				Debug.Log(arr.Count);
				
//				// Parse each object
//				for( int i = 0; i < arr.Count; ++i ) {
//					JObject json = JObject.Parse(arr[i].ToString());
					
//					// Check if this pull request is open
//					if ( (string)json["state"] == "open" ) {
//						Debug.Log($"Found Open Pull: {json["title"]}, at: {json["html_url"]}");
//					}
//				}
//			}
//		}

//		yield return null;
//	}

//	[ContextMenu("Do Post")]
//	void DoPost() {
//		StartCoroutine(PostCoroutine());
//	}

//	IEnumerator PostCoroutine() {
//		// TODO: Server Authentication...

//		// yield op die return...
		
//		// TODO: Dit via een UI

//		//Use WWWForm with Post request
		
//		// WWWForm form = new WWWForm();
//		// form.AddField("username", "test");
//		// form.AddField("password", "test");
//		// // TODO: valid id from Server Auth
//		// form.AddField("sessionid", "cmj8fqtdh50fborr4tl63b46tdhord057uu19nhlvkrgqmfikdd1");

//		string userName = "Pietje";
//		string password = "Geheim123!";

//		using (UnityWebRequest www = UnityWebRequest.Get($"https://studenthome.hku.nl/~voornaam.achternaam/authenticate.php?userName={userName}&password={password.Length}")) {
//			yield return www.SendWebRequest();

//			if (www.result != UnityWebRequest.Result.Success) {
//				Debug.Log(www.error);
//			}
//			else {
//				Debug.Log(www.downloadHandler.text);

//				// We're probably expexting something in return:
//				//JObject json = JObject.Parse(www.downloadHandler.text);
//				//int sessionID = (int)json["sessionid"];
//				//Debug.Log("Login Complete!");
//			}
//		}

//		yield return null;
//	}
//}
