﻿using APKRepackageSDKTool.src;
using APKRepackageSDKTool.src.YML;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using YamlDotNet.Serialization.NamingConventions;

namespace APKRepackageSDKTool
{
    public class ChannelTool
    {
        OutPutCallBack callBack;
        OutPutCallBack errorCallBack;

        CompileTool compileTool;
        public ChannelTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
        {
            this.callBack = callBack;
            this.errorCallBack = errorCallBack;

            compileTool = new CompileTool(callBack, errorCallBack);
        }

        public void ChannelLogic(string filePath, ChannelInfo info)
        {
            if (!string.IsNullOrEmpty(info.PackageName))
            {
                OutPut("替换包名");
                ChangePackageName(filePath, info.PackageName);
            }

            if (!string.IsNullOrEmpty(info.AppName))
            {
                OutPut("替换appName");
                ChangeAppName(filePath, info.AppName);
            }

            if (!string.IsNullOrEmpty(info.AppIcon))
            {
                OutPut("替换appIcon");
                ChangeAppIcon(filePath, info.AppIcon);
            }

            if (!string.IsNullOrEmpty(info.AppBanner))
            {
                OutPut("替换AppBanner");
                ChangeAppBanner(filePath, info.AppBanner);
            }

            OutPut("替换MainActity");
            ChangeMainActity(filePath);

            if (info.sdkList.Count > 0)
            {
                OutPut("放入SDK接口 ");
                PutSDKInterface(filePath);

                for (int i = 0; i < info.sdkList.Count; i++)
                {
                    OutPut("放入SDK " + info.sdkList[i].sdkName);
                    PutSDK(filePath, info.sdkList[i], info);
                }
            }

            OutPut("写配置清单");
            SaveSDKManifest(filePath, info);

            if(info.propertiesList.Count > 0)
            {
                OutPut("写入配置");
                SaveProperties(filePath, info);
            }

            OutPut("整合权限");
            PermissionLogic(filePath, info);

            OutPut("整合SDKVsersion");
            SDKVersion(filePath, info);
        }

        public void OutPut(string content)
        {
            callBack?.Invoke(content);
        }

        public void ErrorOutPut(string content)
        {
            errorCallBack?.Invoke(content);
        }

        #region AndroidManifest 修改

        private void ChangeMainActity(string filePath)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            xml = xml.Replace("com.unity3d.player.UnityPlayerActivity", "sdkInterface.activity.MainActivity");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            xmlDoc.Save(xmlPath);
        }

        public void ChangePackageName(string filePath, string packageName)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            packageName = compileTool.RemoveSpecialCode(packageName);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlElement nodeEle = (XmlElement)manifest;

            nodeEle.SetAttribute("package", packageName);
            xmlDoc.Save(xmlPath);
        }

        public void ChangeAppName(string filePath, string appName)
        {
            string xmlPath = filePath + "\\res\\values\\strings.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode resources =  xmlDoc.SelectSingleNode("resources");

            //遍历String表，替换App_name
            for (int i = 0; i < resources.ChildNodes.Count; i++)
            {
                XmlNode node = resources.ChildNodes[i];
                XmlElement nodeEle = (XmlElement)node;
                if (node.Name == "string"
                    && nodeEle.GetAttribute("name") == "app_name" )
                {
                    nodeEle.InnerText = appName;
                    break;
                }
            }

            xmlDoc.Save(xmlPath);
        }

        /// <summary>
        /// 添加权限
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="Permission"></param>
        void AddPermission(string filePath, string permission)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");

            //权限判重
            for (int i = 0; i < manifest.ChildNodes.Count; i++)
            {
                XmlNode node = manifest.ChildNodes[i];
                XmlElement ele = (XmlElement)node;
                if (ele.GetAttribute("name", "http://schemas.android.com/apk/res/android") == "android.permission." + permission)
                {
                    return;
                }
            }

            XmlElement nd = null;

            string[] info = permission.Split('|');

            if (info.Length == 1)
            {
                nd = xmlDoc.CreateElement("uses-permission");
                //如果有“.”则认为是自定义权限，全文输入，否则补全权限
                if (info[0].Contains("."))
                {
                    nd.SetAttribute("name", "http://schemas.android.com/apk/res/android", info[0]);
                }
                else
                {
                    nd.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.permission." + info[0]);
                }
            }
            else
            {
                nd = xmlDoc.CreateElement("uses-permission-sdk-" + info[1]);

                //如果有“.”则认为是自定义权限，全文输入，否则补全权限
                if (info[0].Contains("."))
                {
                    nd.SetAttribute("name", "http://schemas.android.com/apk/res/android", info[0]);
                }
                else
                {
                    nd.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.permission." + info[0]);
                }
            }

            manifest.AppendChild(nd);
            xmlDoc.Save(xmlPath);
        }

        void ChangeSDKVersion(string filePath, int minSDKVersion,int targetSDKVersion)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlNode useSdk = GetNode(manifest, "uses-sdk");

            if (useSdk == null)
            {
                useSdk = xmlDoc.CreateElement("uses-sdk");
                manifest.AppendChild(useSdk);
            }

            XmlElement sdkEle = (XmlElement)useSdk;

            if (minSDKVersion > 0)
            {
                sdkEle.SetAttribute("minSdkVersion", "http://schemas.android.com/apk/res/android", minSDKVersion.ToString());
            }

            if (targetSDKVersion > 0)
            {
                sdkEle.SetAttribute("targetSdkVersion", "http://schemas.android.com/apk/res/android", targetSDKVersion.ToString());
            }

            xmlDoc.Save(xmlPath);
        }

        #region 添加Activity与Service

        void RemoveErrordManifest(string filePath)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";

            string xml = FileTool.ReadStringByFile(xmlPath);

            xml = xml.Replace("android:compileSdkVersion=\"28\"", "");
            xml = xml.Replace("android:compileSdkVersionCodename=\"9\"", "");

            //直接保存
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            xmlDoc.Save(xmlPath);
        }

        void AddXMLHead(string filePath, KeyValue info, SDKInfo sdkInfo, ChannelInfo channelInfo)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";

            //替换关键字
            string newContent = compileTool.ReplaceKeyWord(info.value, channelInfo);
            newContent = compileTool.ReplaceKeyWordbySDKInfo(newContent, sdkInfo);

            string xml = FileTool.ReadStringByFile(xmlPath);
            int index = xml.IndexOf("<manifest") + 10;
            xml = xml.Insert(index, newContent + " "); //最后加一个空格

            //直接保存
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            xmlDoc.Save(xmlPath);

        }

        void AddActivity(string filePath, ActivityInfo info,SDKInfo sdkInfo,ChannelInfo channelInfo)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";

            //移除旧MainActivity
            if (info.MainActivity)
            {
                RemoveOldMainActivity(filePath);
            }

            //替换关键字
            string newContent = compileTool.ReplaceKeyWord(info.content, channelInfo);
            newContent = compileTool.ReplaceKeyWordbySDKInfo(newContent, sdkInfo);

            string xml = FileTool.ReadStringByFile(xmlPath);
            int index = xml.IndexOf("</application>");
            xml = xml.Insert(index, newContent);

            //直接保存
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            xmlDoc.Save(xmlPath);

            //添加新MainActivity
            if (info.MainActivity)
            {
                AddMainActivity(filePath, info);
            }
        }

        void AddMainActivityProperty(string filePath, KeyValue kv, SDKInfo sdkInfo, ChannelInfo channelInfo)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";

            string xml = FileTool.ReadStringByFile(xmlPath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            //替换关键字
            string newValue = compileTool.ReplaceKeyWord(kv.value, channelInfo);
            newValue = compileTool.ReplaceKeyWordbySDKInfo(newValue, sdkInfo);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlNode app = GetNode(manifest, "application");

            //获取主Activity
            for (int i = 0; i < app.ChildNodes.Count; i++)
            {
                XmlNode node = app.ChildNodes[i];
                XmlElement ele = (XmlElement)node;
                for (int j = 0; j < ele.ChildNodes.Count; j++)
                {
                    XmlNode node2 = ele.ChildNodes[j];

                    if (node2.Name == "intent-filter")
                    {
                        XmlNode action = GetNode(node2, "category");
                        XmlElement ele2 = (XmlElement)action;

                        if (ele2.GetAttribute("name", "http://schemas.android.com/apk/res/android") == "android.intent.category.LAUNCHER")
                        {
                            //增加属性
                            ele.SetAttribute(kv.key, "http://schemas.android.com/apk/res/android", newValue);

                            break;
                        }
                    }
                }
            }

            //保存
            xmlDoc.Save(xmlPath);
        }

        void RemoveOldMainActivity(string filePath)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            //直接保存
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlNode app = GetNode(manifest, "application");

            //获取主Activity
            for (int i = 0; i < app.ChildNodes.Count; i++)
            {
                XmlNode node = app.ChildNodes[i];
                XmlElement ele = (XmlElement)node;
                for (int j = 0; j < ele.ChildNodes.Count; j++)
                {
                    XmlNode node2 = ele.ChildNodes[j];

                    if (node2.Name == "intent-filter")
                    {
                        XmlNode action = GetNode(node2, "category");
                        XmlElement ele2 = (XmlElement)action;

                        if (ele2.GetAttribute("name", "http://schemas.android.com/apk/res/android") == "android.intent.category.LAUNCHER")
                        {
                            ele.RemoveChild(node2);
                            break;
                        }
                    }
                }
            }

            xmlDoc.Save(xmlPath);
        }

        XmlNode GetNode(XmlNode parent, string name)
        {
            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                if (parent.ChildNodes[i].Name == name)
                {
                    return parent.ChildNodes[i];
                }
            }

            return null;
        }

        void AddMainActivity(string filePath, ActivityInfo info)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            //直接保存
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlNode app = GetNode(manifest, "application");

            //权限判重
            for (int i = 0; i < app.ChildNodes.Count; i++)
            {
                XmlNode node = app.ChildNodes[i];
                XmlElement ele = (XmlElement)node;

                string Attribute = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                if (Attribute.Contains(info.name))
                {
                    XmlElement nd = xmlDoc.CreateElement("intent-filter");
                    node.AppendChild(nd);

                    XmlElement nd1 = xmlDoc.CreateElement("action");
                    nd1.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.action.MAIN");

                    XmlElement nd2 = xmlDoc.CreateElement("category");
                    nd2.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.category.LAUNCHER");
                    XmlElement nd3 = xmlDoc.CreateElement("category");
                    nd3.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.category.LEANBACK_LAUNCHER");

                    nd.AppendChild(nd1);
                    nd.AppendChild(nd2);
                    nd.AppendChild(nd3);
                    break;
                }
            }

            xmlDoc.Save(xmlPath);
        }

        void AddService(string filePath, ServiceInfo info, ChannelInfo channelInfo, SDKInfo SDKinfo)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            string content = compileTool.ReplaceKeyWord(info.content, channelInfo);
            content = compileTool.ReplaceKeyWordbySDKInfo(content, SDKinfo);

            int index = xml.IndexOf("</application>");
            xml = xml.Insert(index, content);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            xmlDoc.Save(xmlPath);
        }

        void AddProvider(string filePath, ProviderInfo info,ChannelInfo channelInfo, SDKInfo SDKinfo)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            int index = xml.IndexOf("</application>");

            string content = compileTool.ReplaceKeyWord(info.content, channelInfo);
            content = compileTool.ReplaceKeyWordbySDKInfo(content, SDKinfo);

            xml = xml.Insert(index, content);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            xmlDoc.Save(xmlPath);
        }

        void AddMeta(string filePath, KeyValue kv, ChannelInfo channelInfo,SDKInfo info)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            int index = xml.IndexOf("</application>");

            //替换关键字和配置
            string content = compileTool.ReplaceKeyWord(kv.value, channelInfo);
            content = compileTool.ReplaceKeyWordbySDKInfo(content, info);

            xml = xml.Insert(index, content);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            xmlDoc.Save(xmlPath);
        }

        void AddUses(string filePath, KeyValue kv, ChannelInfo channelInfo, SDKInfo info)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            int index = xml.IndexOf("</manifest>");

            //替换关键字和配置
            string content = compileTool.ReplaceKeyWord(kv.value, channelInfo);
            content = compileTool.ReplaceKeyWordbySDKInfo(content, info);

            xml = xml.Insert(index, content);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            xmlDoc.Save(xmlPath);
        }

        void ChangeApplicationName(string filePath,string applicationName)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlNode app = GetNode(manifest, "application");
            XmlElement ele = (XmlElement)app;

            ele.SetAttribute("name", "http://schemas.android.com/apk/res/android", applicationName);

            xmlDoc.Save(xmlPath);

        }

        #endregion

        #endregion

        #region 重新生成R表

        public void Rebuild_R_Table(string aimPath)
        {
            OutPut("创建临时目录");
            String R_Path = PathTool.GetCurrentPath() + "/R_path/";

            FileTool.CreatPath(R_Path);
            //OutPut("生成R文件");
            //OutPut("生成的R文件的jar");
            //OutPut("生成 dex文件");
            //OutPut("生成smali文件");
            //OutPut("替换smali文件");

            //String androidPath = @"D:\AndroidSDK\platforms\android-28\android.jar";
            string manifest = aimPath + "/AndroidManifest.xml";
            string resPath = aimPath + "/res";

            CmdService cmd = new CmdService(OutPut, errorCallBack);

            OutPut("生成R.java文件");

            OutPut("R_Path " + R_Path + " resPath " + resPath + " manifest " + manifest);

            //生成R文件
            cmd.Execute("aapt package -f -I android.jar -m -J " + R_Path + " -S " + resPath + " -M " + manifest + "");

            if(FindRPath(R_Path) != null)
            {
                //GBK转码
                //string java = FileTool.ReadStringByFile(FindRPath(R_Path));
                //java = compileTool.RemoveSpecialCode(java);
                //FileTool.WriteStringByFile(FindRPath(R_Path),java);

                //编译R.java文件
                cmd.Execute("javac -encoding UTF-8 -source 1.6 -target 1.6 " + FindRPath(R_Path), true, true);

                //生成的R文件的jar
                cmd.Execute("jar cvf ./R.jar ./com", path: R_Path);

                //Jar to dex
                cmd.Execute("java -jar dx.jar --dex --output=./R_path/classes.dex ./R_path/R.jar ", true, true);

                //dex to smali
                cmd.Execute("java -jar baksmali-2.1.3.jar --o=" + aimPath + "/smali ./R_path/classes.dex");
            }
            else
            {
                throw new Exception("R文件生成失败！ 请检查清单文件是否正确！");
            }


            FileTool.SafeDeleteDirectory(R_Path);
            Directory.Delete(R_Path);

            //cmd.Execute("java -jar baksmali-2.1.3.jar classes.dex");
        }

        String FindRPath(string path)
        {
            try
            {
                //递归寻找目标文件路径并输出
                return FileTool.GetAllFileNamesByPath(path, new string[] { "java" })[0];
            }
            catch(Exception e)
            {
                return null;
            }

        }


        #endregion

        #region 替换图片

        void ChangeAppIcon(string filePath, string appIcon)
        {
            string exportPath = filePath + "\\res\\drawable-hdpi-v4\\app_icon.png";
            if (File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 72, 72);

            exportPath = filePath + "\\res\\drawable-ldpi-v4\\app_icon.png";
            if (File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 36, 36);

            exportPath = filePath + "\\res\\drawable-mdpi-v4\\app_icon.png";
            if (File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 48, 48);

            exportPath = filePath + "\\res\\drawable-xhdpi-v4\\app_icon.png";
            if (File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 96, 96);

            exportPath = filePath + "\\res\\drawable-xxhdpi-v4\\app_icon.png";
            if(File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 144, 144);

            exportPath = filePath + "\\res\\drawable-xxxhdpi-v4\\app_icon.png";
            if (File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 192, 192);
        }

        void ChangeAppBanner(string filePath, string appBanner)
        {
            string exportPath = filePath + "\\res\\drawable-xhdpi-v4\\app_banner.png";

            ExportImage(exportPath, appBanner, 320, 180);
        }

        void ExportImage(string exportPath,string scorePath, int width,int height)
        {
            System.Drawing.Size size = new System.Drawing.Size(width, height);

            Image img = Image.FromFile(scorePath);

            img = ResizeImage(img, size);

            img.Save(exportPath);
        }

        private Image ResizeImage(Image imgToResize, System.Drawing.Size size)
        {
            Bitmap b = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //绘制图像
            g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
            g.Dispose();
            return b;
        }

        #endregion

        #region 放入SDK

        void PutSDKInterface(string filePath)
        {
            string interfacePath = EditorData.SdkLibPath + "\\Interface\\SdkInterface.jar";
            compileTool.Jar2Smali(interfacePath, filePath);
        }

        void SaveSDKManifest(string filePath, ChannelInfo info)
        {
            string path = filePath + "\\assets\\SdkManifest.properties";
            string content = "";

            foreach (SDKType item in Enum.GetValues(typeof(SDKType)))
            {
                string key = item.ToString();
                string value = "";

                for (int i = 0; i < info.sdkList.Count; i++)
                {
                    SDKInfo si = info.sdkList[i];
                    SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(si.sdkName);
                    if ((config.sdkType & item) != 0)
                    {
                        if(config.className == null)
                        {
                            config.className = config.SdkName + "<NullClassName>";
                        }

                        if(value =="")
                        {
                            value += config.className;
                        }
                        else
                        {
                            value += "|" + config.className;
                        }
                    }
                }
                content += key + "=" + value + "\n";
            }

            //content += "\n";

            for (int i = 0; i < info.sdkList.Count; i++)
            {
                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkList[i].sdkName);
                content += config.className + "=" + config.sdkName + "\n";
            }

            if(info.isLog)
            {
                content += "IsLog=true\n";
            }

            FileTool.WriteStringByFile(path, content);
        }

        void SaveProperties(string filePath, ChannelInfo info)
        {
            string path = filePath + "\\assets\\Channel.properties";
            string content = "";

            for (int i = 0; i < info.propertiesList.Count; i++)
            {
                content += info.propertiesList[i].key + "=" + info.propertiesList[i].value + "\n";
            }

            FileTool.WriteStringByFile(path, content);
        }

        void PutSDK(string filePath,SDKInfo info, ChannelInfo channelInfo)
        {
            SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkName);

            //添加Jar
            OutPut("添加Jar " + info.sdkName);
            PutJar(filePath, info);

            //手动移除无法编译通过的字段
            RemoveErrordManifest(filePath);

            //自动编译类
            if (config.useCustomJavaClass)
            {
                OutPut("自动编译 " );
                compileTool.Compile(config, channelInfo, filePath);
            }

            //拷贝资源文件
            OutPut("拷贝资源文件 " + info.sdkName);
            CopyFile(filePath,info, channelInfo);

            //添加标签头
            for (int i = 0; i < config.XmlHeadList.Count; i++)
            {
                OutPut("添加AddXMLHead " + info.sdkName + " " + config.ActivityInfoList[i].name);
                AddXMLHead(filePath, config.XmlHeadList[i], info, channelInfo);
            }

            //添加Activity
            for (int i = 0; i < config.ActivityInfoList.Count; i++)
            {
                OutPut("添加Activity " + info.sdkName + " " + config.ActivityInfoList[i].name);
                AddActivity(filePath, config.ActivityInfoList[i], info, channelInfo);
            }

            //添加MainActivityProperty
            for (int i = 0; i < config.mainActivityPropertyList.Count; i++)
            {
                OutPut("添加mainActivityProperty " + info.sdkName + " " + config.mainActivityPropertyList[i].key);
                AddMainActivityProperty(filePath, config.mainActivityPropertyList[i], info, channelInfo);
            }

            //添加Service
            for (int i = 0; i < config.serviceInfoList.Count; i++)
            {
                OutPut("添加Service " + info.sdkName + " " + config.serviceInfoList[i].name);
                AddService(filePath, config.serviceInfoList[i], channelInfo , info);
            }

            //添加Provider
            for (int i = 0; i < config.providerInfoList.Count; i++)
            {
                OutPut("添加Provider " + info.sdkName + " " + config.providerInfoList[i].name);
                AddProvider(filePath, config.providerInfoList[i], channelInfo, info);
            }

            //添加Meta字段
            for (int i = 0; i < config.metaInfoList.Count; i++)
            {
                OutPut("添加Meta " + info.sdkName + " " + config.metaInfoList[i].key);
                AddMeta(filePath, config.metaInfoList[i], channelInfo, info);
            }

            //添加Uses字段
            for (int i = 0; i < config.usesList.Count; i++)
            {
                OutPut("添加Uses " + info.sdkName + " " + config.usesList[i].key);
                AddUses(filePath, config.usesList[i], channelInfo, info);
            }

            //修改ApplicationName
            if (!string.IsNullOrEmpty(config.ApplicationName))
            {
                OutPut("修改ApplicationName " + config.ApplicationName);
                ChangeApplicationName(filePath, config.ApplicationName);
            }

            //添加配置文件
            OutPut("添加配置文件 " + info.sdkName);
            SaveSDKConfigFile(filePath, info);
        }

        void PutJar(string filePath, SDKInfo info)
        {
            string libPath = EditorData.SdkLibPath + "\\" + info.sdkName;

            List<string> jarList = FileTool.GetAllFileNamesByPath(libPath, new string[] { "jar" }, false);

            for (int i = 0; i < jarList.Count; i++)
            {
                compileTool.Jar2Smali(jarList[i], filePath);
            }
        }

        void SaveSDKConfigFile(string filePath , SDKInfo info)
        {
            //TODO 加密此处以免破解
            SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkName);
            string path = filePath + "\\assets\\"+ config.sdkName+ ".properties";

            string content = "";
            for (int i = 0; i < info.sdkConfig.Count; i++)
            {
                KeyValue kv = info.sdkConfig[i];

                content += kv.key + "=" + kv.value + "\n";
            }

            FileTool.WriteStringByFile(path, content);
        }

        //整合权限
        void PermissionLogic(string filePath, ChannelInfo info)
        {
            List<string> permissionList = new List<string>();

            for (int i = 0; i < info.sdkList.Count; i++)
            {
                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkList[i].sdkName);

                for (int j = 0; j < config.permissionList.Count; j++)
                {
                    //权限去重
                    string permission = config.permissionList[j];

                    //替换关键字
                    permission = compileTool.ReplaceKeyWord(permission, info);
                    permission = compileTool.ReplaceKeyWordbySDKInfo(permission, info.sdkList[i]);

                    if (!permissionList.Contains(permission))
                    {
                        permissionList.Add(permission);
                    }
                }
            }

            for (int i = 0; i < permissionList.Count; i++)
            {
                OutPut("权限 " + permissionList[i]);

                AddPermission(filePath, permissionList[i]);
            }
        }

        void CopyFile(string filePath,SDKInfo info, ChannelInfo channelInfo)
        {
            string SDKPath = EditorData.SdkLibPath + "\\" + info.sdkName;

            DirectoryInfo directoryInfo = new DirectoryInfo(SDKPath);
            DirectoryInfo[] directoryInfoArray = directoryInfo.GetDirectories();
            foreach (DirectoryInfo dir in directoryInfoArray)
            {
                string dirName = FileTool.GetDirectoryName(dir.FullName);

                //只拷贝这三个目录
                if(dirName.Contains("assets")
                    || dirName.Contains("lib")
                    )
                {
                    FileTool.CopyDirectory(dir.FullName, filePath + "\\" + dirName);
                }

                //合并res文件
                if(dirName.Contains("res"))
                {
                    FileTool.CopyDirectory(dir.FullName, filePath + "\\" + dirName, RepeatHandle);

                    //递归替换关键字
                    FileTool.RecursionFileExecute(filePath + "\\" + dirName, "xml", (file) =>
                    {
                        String content = FileTool.ReadStringByFile(file);
                        content = compileTool.ReplaceKeyWord(content, channelInfo);

                        FileTool.WriteStringByFile(file,content);
                    });
                }
            }
        }

        public void MergeXMLFile(string PathA, string PathB)
        {
            FileTool.CopyDirectory(PathA, PathB , RepeatHandle);
        }

        void RepeatHandle(string fileA,string fileB)
        {
            OutPut("合并文件 " + fileA + " ->" + fileB);

            //只支持合并xml
            if(fileA.Contains("xml") && fileB.Contains("xml"))
            {
                AppadnXML(fileA, fileB);
            }
            else
            {
                ErrorOutPut("不支持的合并类型" + fileA + " ->" + fileB);
            }
        }

        void AppadnXML(string fileA, string fileB)
        {
            XmlDocument doca = new XmlDocument();
            doca.Load(fileA);

            XmlDocument docb = new XmlDocument();
            docb.Load(fileB);

            // 分别获取两个文档的根元素，以便于合并
            XmlElement rootA = doca.DocumentElement;
            XmlElement rootB = docb.DocumentElement;

            // 创建一个合并的 document
            XmlDocument result = new XmlDocument();

            // 创建根元素
            XmlElement root = result.CreateElement(rootB.Name);
            result.AppendChild(root);

            foreach (XmlNode node in rootA.ChildNodes)
            {
                // 先导入节点
                XmlNode n = result.ImportNode(node, true);
                //if(root.SelectNodes())
                // 然后，插入指定的位置
                root.AppendChild(n);
            }

            // 同上
            foreach (XmlNode node in rootB.ChildNodes)
            {
                XmlNode n = result.ImportNode(node, true);
                root.AppendChild(n);
            }

            result.Save(fileB);
        }

        #endregion

        #region 整合SDK

        public void SDKVersion(string filePath,ChannelInfo info)
        {
            int minSDKVersion = 0;
            int targetSDKVersion = 0;

            for (int i = 0; i < info.sdkList.Count; i++)
            {
                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkList[i].sdkName);
                if(config.minSDKversion != 0)
                {
                    if(minSDKVersion == 0)
                    {
                        minSDKVersion = config.minSDKversion;
                    }
                    else
                    {
                        //最小SDKVersion取所有SDK设置中最大的
                        if(config.minSDKversion > minSDKVersion)
                        {
                            minSDKVersion = config.minSDKversion;
                        }
                    }
                }

                if(config.targetSDKVersion != 0)
                {
                    if (targetSDKVersion == 0)
                    {
                        targetSDKVersion = config.targetSDKVersion;
                    }
                    else
                    {
                        //目标SDKVersion取所有SDK设置中最大的
                        if (config.targetSDKVersion > targetSDKVersion)
                        {
                            targetSDKVersion = config.targetSDKVersion;
                        }
                    }
                }
            }
            ChangeSDKVersion(filePath, minSDKVersion, targetSDKVersion);
        }

        #endregion

        #region YML


        public void YMLLogic(string filePath)
        {
            string path = filePath + @"\apktool.yml";
            var input = new StringReader(path);

            YML yml = new YML(path);

            yml.modify("doNotCompress", "null");
            yml.DeleteAllChildNode("doNotCompress");
            yml.save();
        }

        #endregion

        #region 混淆Dll

        public void ConfusionDLL(string filePath)
        {
            string dllPath = filePath + @"\assets\bin\Data\Managed\Assembly-CSharp.dll";
            string outPath = filePath + @"\assets\bin\Data\Managed\Assembly-CSharp_Secure\Assembly-CSharp.dll";
            string outDir = filePath + @"\assets\bin\Data\Managed\Assembly-CSharp_Secure";

            CmdService cmd = new CmdService(OutPut, errorCallBack);
            //混淆
            cmd.Execute(@"DotNETReactor\dotNET_Reactor.exe -file " + dllPath);

            //覆盖
            File.Copy(outPath, dllPath,true);

            //删除旧文件
            File.Delete(outPath);
            Directory.Delete(outDir);
        }

        #endregion 

    }

}
