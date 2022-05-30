# RDCMan v2.90.1420.0
微软远程桌面管理工具  
Remote Desktop Connection Manager  

reflect base on v2.90.1420.0  
[Official download link 发布时间： 2022 年 1 月 27 日](https://docs.microsoft.com/zh-cn/sysinternals/downloads/rdcman)

汉化,及本土优化

Use .Net Framework 4.8

## 个性化更改的记录  
  没找到 支持 RdpClient10 的 lib , 注销掉了此部分支持代码
  
0. ICO 资源文件统一放到 Resources 文件夹  
影响文件:  
  MainForm.cs::InitComp    line:437  
  ServerTree.cs::Init    line:223  
  
1. 密码框的密码不用 占位字符代替  
影响文件:  
  CredentialsUI.cs::InitPassword     line:121    并 注释常量 DummyPassword  
  
2. 增加汉化说明
影响文件:  
  About.cs::InitializeComponent    line:20/35/42  

3. 右键点击选中 服务器/组  
影响文件:  
  ServerTree.cs::OnContextMenu    line:315  
  
4. 服务器配置-显示名称位置上升  
影响文件:  
  ServerPropertiesTabPage.cs::ServerPropertiesTabPage    line:26  
  
5. 备注框扩大  
影响文件:  
  NodePropertiesPage.cs::AddComment    line:58  
  
6. `VM console connect` 设置界面优化  
影响文件:  
  ServerPropertiesTabPage.cs::ServerPropertiesTabPage    line:41  
  ServerPropertiesTabPage.cs::VMConsoleConnectCheckBox_CheckedChanged    line:116  
  
7. `选项->客户区->缩略图单位大小` 界面优化  
影响文件:  
  ServerPropertiesTabPage.cs::CreateClientAreaPage    line:327  
  ServerPropertiesTabPage.cs::ThumbnailPercentageRadioCheckedChanged    line:457  
  
7. `选项->树` 界面优化  
影响文件:  
  GlobalOptionsDialog.cs::CreateServerTreePage    line:352/367/373/385  
  
999. 内置组  (配置完成,功能未完成)  
影响文件:  
  GlobalSettings.cs<>ShowBuiltInGroup    line:437  
  Internal\*.cs  

