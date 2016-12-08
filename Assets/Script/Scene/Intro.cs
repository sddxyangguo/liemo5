﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine.UI;
using Mono.Xml;

/// <summary>
/// 1,初始化服务器地址
/// 2,检查是否需要更新
/// 2.1 不需要更新则进入Loading场景。loading场景只做资源类文件加载更新进度使用。
/// 2.2 如果需要更新，则弹出更新框，点击确定，更新ab文件和配置文件。
/// 2.3 配置文件解压
/// 3 进入loading场景开始加载city资源

/// </summary>
public class Intro : MonoBehaviour
{
    public Slider slider;
    public string ServerAddress;
    private WWW xmlLoader;
    public UnityEngine.UI.Text label;
    public Texture2D[] CurBGs;
    private string[] versionCode;
    private string[] newversionCode;
    public string Baseurl = string.Empty;
    public string QiangGengAddress = string.Empty;
    private int totalMtoLoad=0;
    public GameObject NoticeUI;

    public delegate void OnItemLoadcomplete();

    public BundleExtractor bundleExtractor;
    private List<string> loadList= new List<string>();

    void Awake()
    { 
        ServerAddress = @"http://42.159.80.141/liemo/liemo.xml";
    }

    // Use this for initialization
    void Start()
    {
        slider.value = (float)0.8; 
        GoRequestXML();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void GoRequestXML()
    {
        StartCoroutine(RequestXml());
        Debug.Log("=====");
    }

    IEnumerator RequestXml()
    {
        this.xmlLoader = new WWW(URLAntiCacheRandomizer.RandomURL(ServerAddress));
        Debug.Log(xmlLoader.url);
        label.text = @"连接服务器中.....";
        yield return this.xmlLoader;
        if (this.xmlLoader.error == null && this.xmlLoader.isDone)
        {
            SecurityParser xml = new SecurityParser();
            xml.LoadXml(this.xmlLoader.text);
            Debug.Log(this.xmlLoader.text);
            label.text = @"服务器地址加载中";
            SecurityElement root = xml.ToXml();
            AllAdrList.InitList(root);
            StartCoroutine(check_update());
        }
        else
        {
            label.text = @"服务器地址加载出错";
            Debug.Log(this.xmlLoader.error);
        }
        StopCoroutine(RequestXml());
    }

    private IEnumerator check_update()
    {
        label.text = @"检查更新中...";
        versionCode = LMVersion.getVersion_list().Split('.');
        Baseurl = AllAdrList.list["Android"];
        QiangGengAddress = AllAdrList.list["PackagePath"];
        string bigVersionPath = URLAntiCacheRandomizer.RandomURL(Baseurl + "Version.txt");
        WWW versionLoader = new WWW(bigVersionPath);
        yield return versionLoader;
        if (versionLoader.error == null && versionLoader.isDone)
        {
            Debug.Log("==>" + versionLoader.url);
            newversionCode = versionLoader.text.Split('.');
            Debug.Log(versionLoader.text);
            if (versionCode[0].Equals(newversionCode[0]) && versionCode[1].Equals(newversionCode[1]))
            {
                bundleExtractor.StartLoading(OnCopyToCacheEnd);
            }else
            {
                bundleExtractor.StartLoading(OnCopyToCacheEnd);
            }
        }  
        StopCoroutine("check_update");   


    }

    public void OnclieckConfirmUpdate()
    {
        NoticeUI.SetActive(false);
        StartCoroutine(BeginDownRes());
    }

    private IEnumerator BeginDownRes()
    {
        yield return new WaitForEndOfFrame();
    #if HOT
        BundleManager.getIns().SetLoadList(loadList);
        BundleManager.getIns().StartLoadBundle();
    #endif


    }

    private void OnCopyToCacheEnd()
    {
        StartCoroutine(HashCodeLoad());
    }

    private IEnumerator HashCodeLoad()
    {
        Debug.Log("copyFileEnd !!!!");
        WWW versionLoader = new WWW(URLAntiCacheRandomizer.RandomURL( Baseurl+"/v.txt"));
        yield return versionLoader;
        if(versionLoader.error==null && versionLoader.isDone)
        {
            string[] result = versionLoader.text.Split('\n');
            foreach(string line in result)
            {
                if(line.Length>0)
                {
                    loadList.Add(line);
                }
            } 
            totalMtoLoad =  BundleManager.getIns().TotalBytesToload(loadList);
            if(totalMtoLoad>0){
                NoticeUI.SetActive(true);
            }
        }
        Debug.Log("totalMtoLoad="+totalMtoLoad);
    }
}
