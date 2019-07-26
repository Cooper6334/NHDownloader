using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class NHParser : MonoBehaviour
{
    public InputField tagI;
    public InputField startI;
    public InputField delayI;

    public Text downloadCntText;
    public Text stateInfoText;

    public GameObject startBtn;
    public GameObject stopBtn;



    int total = 0;
    int startPage = 0;
    int currentPage = 0;
    int delaySec = 3;
    int downloadCnt = 0;
    ArrayList idList;
    ArrayList nameList;
    bool working = false;
    bool downloading = false;
    string tags = "";
    // Use this for initialization
    void Start()
    {
        Screen.SetResolution(600, 360, false);
        idList = new ArrayList();
        nameList = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //working = true;
        //}
        if (working && !downloading)
        {
            downloading = true;
            if (idList.Count <= 0)
            {
                if (total != 0 && (currentPage - 1) * 25 > total)
                {
                    stateInfoText.text = "Download finish";
                    working = false;
                    total = 0;
                    currentPage = 0;
                    downloadCnt = 0;
                    startBtn.SetActive(true);
                    stopBtn.SetActive(false);

                    downloading = false;
                }
                else
                {
                    StartCoroutine(StartQuery("https://nhentai.net/search/?q=" + tags));
                    currentPage++;
                }
            }
            else
            {
                string id = idList[0] as string;
                idList.RemoveAt(0);
                string name = nameList[0] as string;
                nameList.RemoveAt(0);
                StartCoroutine(StartDownload(id, name));
            }
        }
    }

    public void StartDownload()
    {
        total = 0;
        downloadCnt = 0;
        tags = tagI.text;
        if (tags.Length <= 0)
        {
            stateInfoText.text = "tags can not be null";
            return;
        }
        tags = tags.Replace(" ", "+");
        startPage = startI.text.Length <= 0 ? 0 : int.Parse(startI.text);
        if (startPage < 1)
        {
            startPage = 1;
        }
        currentPage = startPage;
        Debug.Log("currentPage:" + currentPage);
        delaySec = delayI.text.Length <= 0 ? 3 : int.Parse(delayI.text);
        delaySec = Mathf.Clamp(delaySec, 1, 10);
        stateInfoText.text = "Downloading";
        startBtn.SetActive(false);
        stopBtn.SetActive(true);

        working = true;
    }

    public void StopDownload()
    {
        startBtn.SetActive(true);
        stopBtn.SetActive(false);
        stateInfoText.text = "Download stop";
        working = false;
    }

    private IEnumerator StartQuery(string baseuri)
    {
        stateInfoText.text = "Query page " + currentPage;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(baseuri + "&page=" + currentPage))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                working = false;
                Debug.LogError("Error: " + webRequest.error);
                stateInfoText.text = webRequest.error;
            }
            else
            {
                //Debug.Log("Received: " + webRequest.downloadHandler.text);
                string[] result = webRequest.downloadHandler.text.Split('\n');
                for (int i = 0; i < result.Length; i++)
                {
                    ParseResult(result[i]);
                }
            }
        }
        downloading = false;
        yield break;
    }

    private void ParseResult(string result)
    {
        if (result.Contains("Result"))
        {
            int start = 0;
            int end = result.IndexOf("Result") - 1;
            for (int i = end; i >= 0; i--)
            {
                if (result[i] == '>')
                {
                    start = i + 1;
                }
            }
            string cnt = result.Substring(start, end - start + 1);
            cnt = cnt.Replace(",", "");
            Debug.Log("Get total cnt " + cnt);
            total = int.Parse(cnt);
        }
        else if (result.Contains("/g/"))
        {
            int start = result.IndexOf("/g/") + "/g/".Length;
            int end = start;
            for (int i = start; i < result.Length; i++)
            {
                if (result[i] == '/')
                {
                    end = i - 1;
                    break;
                }
            }
            string id = result.Substring(start, end - start + 1);
            idList.Add(id);

            string startTag = "<div class=\"caption\">";
            string endTag = "</div>";
            start = result.IndexOf(startTag) + startTag.Length;
            string cutStart = result.Substring(start);
            end = cutStart.IndexOf(endTag);
            string name = cutStart.Substring(0, end);
            nameList.Add(name);
            //Debug.Log("Get " + id + "," + name);
        }
    }

    char[] invalidChars = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
    private IEnumerator StartDownload(string id, string name)
    {
        downloadCnt++;
        downloadCntText.text = downloadCnt + "/" + (total - 25 * (startPage - 1));
        Application.OpenURL("https://nhentai.net/g/" + id + "/download");
        yield return new WaitForSeconds(delaySec);
        downloading = false;
        yield break;
        /*
        Debug.Log(id + "," + name);
        foreach (char c in invalidChars)
        {
            if (name.Contains("" + c))
            {
                name = name.Replace("" + c, "");
            }
        }

        string url = "https://nhentai.net/g/" + id + "/download";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.Send();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                string savePath = string.Format("{0}/{1}.torrent", downloadFolder, name);
                System.IO.File.WriteAllText(savePath, www.downloadHandler.text);
            }
        }
        downloading = false;
        yield break;
        */
    }
}
